#region

using Newtonsoft.Json;

#endregion

namespace InstagramApiSharp.Classes.Models
{
    public class InstaAccountProfilPicUpload
    {
        [JsonProperty("upload_id")] public string UploadId { get; set; }

        [JsonProperty("xsharing_nonces")] public XsharingNonces XsharingNonces { get; set; }

        [JsonProperty("status")] public string Status { get; set; }
    }

    public class XsharingNonces
    {
    }
}