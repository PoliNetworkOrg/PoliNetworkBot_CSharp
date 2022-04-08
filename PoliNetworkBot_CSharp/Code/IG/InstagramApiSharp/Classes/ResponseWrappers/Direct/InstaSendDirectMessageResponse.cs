#region

using InstagramApiSharp.Classes.ResponseWrappers.BaseResponse;
using System.Collections.Generic;

#endregion

namespace InstagramApiSharp.Classes.ResponseWrappers;

public class InstaSendDirectMessageResponse : BaseStatusResponse
{
    public List<InstaDirectInboxThreadResponse> Threads { get; set; } = new();
}