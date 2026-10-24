param(
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
$headers = @{ Authorization = "Bearer $token" }
$app = Invoke-RestMethod -Method Get -Uri "https://manage.devcenter.microsoft.com/v1.0/my/applications/$AppId" -Headers $headers
if ($app.pendingApplicationSubmission -and $app.pendingApplicationSubmission.id) {
    $id = $app.pendingApplicationSubmission.id
    Invoke-RestMethod -Method Delete -Uri "https://manage.devcenter.microsoft.com/v1.0/my/applications/$AppId/submissions/$id" -Headers $headers | Out-Null
    Write-Host "Deleted pending submission $id"
} else {
    Write-Host 'No pending submission'
}
