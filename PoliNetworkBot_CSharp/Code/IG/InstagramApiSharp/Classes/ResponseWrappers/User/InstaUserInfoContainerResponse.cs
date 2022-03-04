﻿#region

using InstagramApiSharp.Classes.ResponseWrappers.BaseResponse;
using Newtonsoft.Json;

#endregion

namespace InstagramApiSharp.Classes.ResponseWrappers
{
    public class InstaUserInfoContainerResponse : BaseStatusResponse
    {
        [JsonProperty("user")] public InstaUserInfoResponse User { get; set; }
    }
}