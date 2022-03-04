#region

using InstagramApiSharp.Classes.ResponseWrappers.BaseResponse;
using Newtonsoft.Json;

#endregion

namespace InstagramApiSharp.Classes.ResponseWrappers
{
    public class DeleteResponse : BaseStatusResponse
    {
        [JsonProperty("did_delete")] public bool IsDeleted { get; set; }
    }
}