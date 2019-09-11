using UnityEngine.Networking;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
// Based on https://www.owasp.org/index.php/Certificate_and_Public_Key_Pinning#.Net
class SSLHandler : CertificateHandler
{
    protected override bool ValidateCertificate(byte[] certificateData)
    {
    
        return true;
    }
}