#region

using InstagramApiSharp.API.Versions;
using InstagramApiSharp.Helpers;
using Newtonsoft.Json;

#endregion

namespace PoliNetworkBot_CSharp.Code.IG.InstagramApiSharp.Classes.Android.DeviceInfo;

internal class ApiTwoFactorRequestMessage
{
    internal ApiTwoFactorRequestMessage(string verificationCode, string username, string deviceId,
        string twoFactorIdentifier)
    {
        VerificationCode = verificationCode;
        Username = username;
        DeviceId = deviceId;
        TwoFactorIdentifier = twoFactorIdentifier;
    }

    public string VerificationCode { get; set; }
    public string Username { get; set; }
    public string DeviceId { get; set; }
    public string TwoFactorIdentifier { get; set; }

    internal string GenerateSignature(InstaApiVersion apiVersion, string signatureKey)
    {
        if (string.IsNullOrEmpty(signatureKey))
            signatureKey = apiVersion.SignatureKey;
        return CryptoHelper.CalculateHash(signatureKey,
            JsonConvert.SerializeObject(this));
    }

    internal string GetMessageString()
    {
        return JsonConvert.SerializeObject(this);
    }
}