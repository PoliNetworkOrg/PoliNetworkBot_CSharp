﻿#region

using InstagramApiSharp.Classes.ResponseWrappers.BaseResponse;
using Newtonsoft.Json;

#endregion

namespace InstagramApiSharp.Classes.ResponseWrappers;

public class BadStatusErrorsResponse : BaseStatusResponse
{
    [JsonProperty("message")] public MessageErrorsResponse Message { get; set; }
}