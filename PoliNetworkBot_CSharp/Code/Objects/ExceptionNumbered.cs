#region

using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects.Exceptions;
using PoliNetworkBot_CSharp.Code.Objects.Files;
using PoliNetworkBot_CSharp.Code.Utils.Notify;

#endregion

namespace PoliNetworkBot_CSharp.Code.Objects;

public class ExceptionNumbered : Exception
{
    private const int DefaultV = 1;
    private int _v;

    public ExceptionNumbered(Exception? item1, int v = DefaultV) : base(item1?.Message, item1)
    {
        _v = v;
    }

    public ExceptionNumbered(string message, int v = DefaultV) : base(message)
    {
        _v = v;
    }

    internal void Increment()
    {
        _v++;
    }


    internal bool AreTheySimilar(Exception? item2)
    {
        return Message == item2?.Message;
    }

    public int GetNumberOfTimes()
    {
        return _v;
    }

    internal static async Task<bool> SendExceptionAsync(Exception? e,
        SampleNuGet.Objects.TelegramBotAbstract? telegramBotAbstract,
        EventArgsContainer? eventArgsContainer)
    {
        if (telegramBotAbstract == null)
            return false;

        await NotifyUtil.NotifyOwnerWithLog2(e, telegramBotAbstract, eventArgsContainer);
        return true;
    }

    public TelegramFileContent GetMessageAsText(
        ExtraInfo? extraInfo,
        EventArgsContainer? messageEventArgs,
        bool json
    )
    {
        if (json)
        {
            var jObject = new JObject
            {
                ["number"] = GetNumberOfTimes(),
                ["message"] = Message,
                ["ExceptionToString"] = ToString(),
                ["StackTrace"] = GetStackTrace(StackTrace),
                ["MessageArgs"] = messageEventArgs == null ? null : JsonConvert.SerializeObject(messageEventArgs),
                ["extraInfo"] = extraInfo == null ? null : JsonConvert.SerializeObject(extraInfo)
            };
            var s2 = new StringJson(FileTypeJsonEnum.OBJECT, jObject);
            return new TelegramFileContent(s2, null);
        }

        string message3;
        try
        {
            message3 = "";
            try
            {
                message3 += "Number of times: ";
                message3 += GetNumberOfTimes();
                message3 += "\n\n";
            }
            catch
            {
                message3 += "\n\n";
            }

            try
            {
                message3 += "Message:\n";
                message3 += Message;
                message3 += "\n\n";
            }
            catch
            {
                message3 += "\n\n";
            }

            try
            {
                message3 += "ExceptionToString:\n";
                message3 += ToString();
                message3 += "\n\n";
            }
            catch
            {
                message3 += "\n\n";
            }


            if (messageEventArgs != null)
                try
                {
                    message3 += "MessageArgs:\n";
                    message3 += JsonConvert.SerializeObject(messageEventArgs);
                }
                catch
                {
                    message3 += "\n\n";
                }

            var genericInfo = extraInfo?.GenericInfo;
            if (!string.IsNullOrEmpty(genericInfo))
                message3 += "\n\n" + genericInfo;
        }
        catch (Exception e1)
        {
            message3 = "Error in sending exception: this exception occurred:\n\n" + e1.Message;
        }

        var serializeObject = "StackTrace:\n" + JsonConvert.SerializeObject(GetStackTrace(StackTrace));
        var serializeObject2 = new StringJson(FileTypeJsonEnum.SIMPLE_STRING, serializeObject);
        return new TelegramFileContent(serializeObject2, message3);
    }

    private static JObject GetStackTrace(string? stackTracePar)
    {
        var result = new JObject
        {
            ["eStack"] = stackTracePar,
            ["currStack"] = Environment.StackTrace
        };
        return result;
    }
}