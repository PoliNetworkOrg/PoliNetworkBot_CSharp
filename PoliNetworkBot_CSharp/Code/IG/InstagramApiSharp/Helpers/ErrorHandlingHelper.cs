#region

using System;
using InstagramApiSharp.Classes.ResponseWrappers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

#endregion

namespace InstagramApiSharp.Helpers;

internal static class ErrorHandlingHelper
{
    internal static BadStatusResponse GetBadStatusFromJsonString(string json)
    {
        var badStatus = new BadStatusResponse();
        try
        {
            if (json.Contains("Oops, an error occurred"))
            {
                badStatus.Message = json;
            }
            else if (json.Contains("debug_info"))
            {
                var root = JObject.Parse(json);
                var debugInfo = root["debug_info"];
                var type = debugInfo["type"].ToString();
                var message = debugInfo["message"].ToString();

                badStatus = new BadStatusResponse { Message = message, ErrorType = type };
            }
            else
            {
                badStatus = JsonConvert.DeserializeObject<BadStatusResponse>(json);
            }
        }
        catch (Exception ex)
        {
            badStatus.Message = ex.Message;
        }

        return badStatus;
    }
}