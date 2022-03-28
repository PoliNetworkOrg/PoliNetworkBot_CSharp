#region

using System.Collections.Generic;
using InstagramApiSharp.Classes.ResponseWrappers.BaseResponse;

#endregion

namespace InstagramApiSharp.Classes.ResponseWrappers;

public class InstaSendDirectMessageResponse : BaseStatusResponse
{
    public List<InstaDirectInboxThreadResponse> Threads { get; set; } = new();
}