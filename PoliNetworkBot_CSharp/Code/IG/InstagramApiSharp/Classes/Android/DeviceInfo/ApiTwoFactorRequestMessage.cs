#region

using InstagramApiSharp.API.Versions;
using InstagramApiSharp.Helpers;
using Newtonsoft.Json;

#endregion

namespace InstagramApiSharp.Classes.Android.DeviceInfo
{
    internal class ApiTwoFactorRequestMessage
    {
        internal ApiTwoFactorRequestMessage(string verificationCode, string username, string deviceId,
            string twoFactorIdentifier)
        {
            Verification_code = verificationCode;
            this.Username = username;
            Device_id = deviceId;
            Two_factor_identifier = twoFactorIdentifier;
        }

        public string Verification_code { get; set; }
        public string Username { get; set; }
        public string Device_id { get; set; }
        public string Two_factor_identifier { get; set; }

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
}