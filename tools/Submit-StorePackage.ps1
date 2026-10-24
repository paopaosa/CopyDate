# Submit a prebuilt .msixupload to Partner Center for Copy Date.
# Requires PowerShell 7+ so ConvertFrom-Json keeps camelCase property names.
param(
    [Parameter(Mandatory = $true)]
    [string] $MsixUploadPath,
    [string] $AppId = '9NMF25PCV1PF',
    [string] $TenantId = '5d7e0cab-4f1d-4070-9309-88493c82389d',
    [string] $ClientId = 'b44225e4-9ebd-41e7-932e-416aa6f0a5bc',
    [string] $ReleaseNotesJson = ''
)

$ErrorActionPreference = 'Stop'
if ($PSVersionTable.PSVersion.Major -lt 7) {
    throw 'Run this script with pwsh (PowerShell 7+).'
}
if (-not (Test-Path $MsixUploadPath)) { throw "Package not found: $MsixUploadPath" }
$packageFileName = [IO.Path]::GetFileName($MsixUploadPath)

if (-not $ReleaseNotesJson) {
    $ReleaseNotesJson = Join-Path $PSScriptRoot 'store-release-notes-1.2.2.json'
}
$notes = Get-Content -LiteralPath $ReleaseNotesJson -Raw -Encoding UTF8 | ConvertFrom-Json
$ReleaseNotesZh = [string]$notes.zh
$ReleaseNotesEn = [string]$notes.en
$ReleaseNotesHant = [string]$notes.'zh-hant'

Add-Type -TypeDefinition @'
using System;
using System.Runtime.InteropServices;
public class StoreCredUtil {
  [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
  public struct CREDENTIAL {
    public uint Flags; public uint Type; public string TargetName; public string Comment;
    public System.Runtime.InteropServices.ComTypes.FILETIME LastWritten; public uint CredentialBlobSize;
    public IntPtr CredentialBlob; public uint Persist; public uint AttributeCount; public IntPtr Attributes;
    public string TargetAlias; public string UserName;
  }
  [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
  public static extern bool CredRead(string target, uint type, int reservedFlag, out IntPtr credentialPtr);
  [DllImport("advapi32.dll")] public static extern void CredFree(IntPtr cred);
  public static string ReadSecret(string target) {
    IntPtr p; if (!CredRead(target, 1, 0, out p)) return null;
    try {
      var c = Marshal.PtrToStructure<CREDENTIAL>(p);
      if (c.CredentialBlob == IntPtr.Zero || c.CredentialBlobSize == 0) return null;
      return Marshal.PtrToStringUni(c.CredentialBlob, (int)c.CredentialBlobSize / 2);
    } finally { CredFree(p); }
  }
}
'@

$secret = [StoreCredUtil]::ReadSecret("MicrosoftStoreCli:user=$ClientId")
if ([string]::IsNullOrEmpty($secret)) { throw 'Cannot read client secret from Credential Manager (MicrosoftStoreCli).' }

$tokenResp = Invoke-RestMethod -Method Post -Uri "https://login.microsoftonline.com/$TenantId/oauth2/token" -Body @{
    grant_type    = 'client_credentials'
    client_id     = $ClientId
    client_secret = $secret
    resource      = 'https://manage.devcenter.microsoft.com'
}
$headers = @{ Authorization = "Bearer $($tokenResp.access_token)" }

$appUri = "https://manage.devcenter.microsoft.com/v1.0/my/applications/$AppId"
$app = Invoke-RestMethod -Method Get -Uri $appUri -Headers $headers
if ($app.pendingApplicationSubmission -and $app.pendingApplicationSubmission.id) {
    $pendingId = $app.pendingApplicationSubmission.id
    Write-Host "Deleting pending submission $pendingId"
    Invoke-RestMethod -Method Delete -Uri "$appUri/submissions/$pendingId" -Headers $headers | Out-Null
    Start-Sleep -Seconds 5
}

Write-Host 'Creating submission...'
$submission = Invoke-RestMethod -Method Post -Uri "$appUri/submissions" -Headers ($headers + @{ 'Content-Type' = 'application/json' }) -Body ''
$submissionId = $submission.id
$uploadUrl = $submission.fileUploadUrl
Write-Host "SubmissionId=$submissionId"

foreach ($lang in @($submission.listings.PSObject.Properties.Name)) {
    if (-not $submission.listings.$lang.baseListing) { continue }
    if ($lang -match '^(zh-hans|zh-cn)') { $submission.listings.$lang.baseListing.releaseNotes = $ReleaseNotesZh }
    elseif ($lang -match '^(zh-hant|zh-tw|zh-hk)') { $submission.listings.$lang.baseListing.releaseNotes = $ReleaseNotesHant }
    elseif ($lang -match '^en') { $submission.listings.$lang.baseListing.releaseNotes = $ReleaseNotesEn }
}

$packages = [System.Collections.Generic.List[object]]::new()
foreach ($pkg in @($submission.applicationPackages)) {
    $pkg.fileStatus = 'PendingDelete'
    $packages.Add($pkg)
}
$packages.Add([pscustomobject]@{
    fileName              = $packageFileName
    fileStatus            = 'PendingUpload'
    minimumDirectXVersion = 'None'
    minimumSystemRam      = 'None'
})
$submission.applicationPackages = $packages.ToArray()

$putBody = $submission | ConvertTo-Json -Depth 100 -Compress
Write-Host 'Updating submission metadata...'
Invoke-RestMethod -Method Put -Uri "$appUri/submissions/$submissionId" -Headers ($headers + @{ 'Content-Type' = 'application/json' }) -Body $putBody | Out-Null
Write-Host 'Metadata updated (packages + release notes)'

$zipPath = Join-Path ([IO.Path]::GetDirectoryName($MsixUploadPath)) ('_store_upload_' + [IO.Path]::GetFileNameWithoutExtension($packageFileName) + '.zip')
if (Test-Path $zipPath) { Remove-Item $zipPath -Force }
Compress-Archive -LiteralPath $MsixUploadPath -DestinationPath $zipPath -Force
# Compress-Archive names entry with full filename already
Write-Host "Uploading zip ($((Get-Item $zipPath).Length) bytes)..."
Invoke-RestMethod -Method Put -Uri $uploadUrl -Headers @{ 'x-ms-blob-type' = 'BlockBlob'; 'Content-Type' = 'application/octet-stream' } -InFile $zipPath | Out-Null
Write-Host 'Package uploaded'

Invoke-RestMethod -Method Post -Uri "$appUri/submissions/$submissionId/commit" -Headers ($headers + @{ 'Content-Type' = 'application/json' }) -Body '{}' | Out-Null
Write-Host 'Commit requested; polling...'

$status = $null
for ($i = 0; $i -lt 36; $i++) {
    Start-Sleep -Seconds 5
    $status = Invoke-RestMethod -Method Get -Uri "$appUri/submissions/$submissionId/status" -Headers $headers
    Write-Host ("[{0}] {1}" -f $i, $status.status)
    if ($status.status -in @('CommitFailed', 'Failed', 'Canceled', 'Rejected')) {
        $status | ConvertTo-Json -Depth 10 | Write-Host
        throw "Submission failed: $($status.status)"
    }
    if ($status.status -in @('PreProcessing', 'Certification', 'PendingCertification', 'InProgress', 'Published', 'CommitSucceeded')) {
        break
    }
}

Write-Host 'Submission accepted by Partner Center.'
$status | ConvertTo-Json -Depth 6
