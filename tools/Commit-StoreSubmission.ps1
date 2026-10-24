param(
    [Parameter(Mandatory = $true)][string] $SubmissionId,
    [string] $AppId = '9NMF25PCV1PF',
    [string] $TenantId = '5d7e0cab-4f1d-4070-9309-88493c82389d',
    [string] $ClientId = 'b44225e4-9ebd-41e7-932e-416aa6f0a5bc'
)
$ErrorActionPreference = 'Stop'
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
$token = (Invoke-RestMethod -Method Post -Uri "https://login.microsoftonline.com/$TenantId/oauth2/token" -Body @{
    grant_type = 'client_credentials'; client_id = $ClientId; client_secret = $secret
    resource = 'https://manage.devcenter.microsoft.com'
}).access_token
$headers = @{ Authorization = "Bearer $token"; 'Content-Type' = 'application/json' }
$uri = "https://manage.devcenter.microsoft.com/v1.0/my/applications/$AppId/submissions/$SubmissionId/commit"
Invoke-RestMethod -Method Post -Uri $uri -Headers $headers -Body '{}' | Out-Null
Write-Host 'Commit requested'
for ($i = 0; $i -lt 36; $i++) {
    Start-Sleep -Seconds 5
    $status = Invoke-RestMethod -Method Get -Uri "https://manage.devcenter.microsoft.com/v1.0/my/applications/$AppId/submissions/$SubmissionId/status" -Headers @{ Authorization = "Bearer $token" }
    Write-Host ("[{0}] {1}" -f $i, $status.status)
    if ($status.status -in @('CommitFailed', 'Failed', 'Canceled', 'Rejected')) {
        $status | ConvertTo-Json -Depth 10
        throw "Submission failed: $($status.status)"
    }
    if ($status.status -in @('PreProcessing', 'Certification', 'PendingCertification', 'InProgress', 'Published', 'CommitSucceeded')) {
        Write-Host 'Submission accepted by Partner Center.'
        $status | ConvertTo-Json -Depth 6
        exit 0
    }
}
throw 'Timed out waiting for commit'
