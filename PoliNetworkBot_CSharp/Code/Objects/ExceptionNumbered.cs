#region

using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PoliNetworkBot_CSharp.Code.Utils;

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

    internal Exception GetException()
    {
        return this;
    }

    internal bool AreTheySimilar(Exception? item2)
    {
        return Message == item2?.Message;
    }

    internal int GetNumberOfTimes()
    {
        return _v;
    }

    internal static async Task<bool> SendExceptionAsync(Exception? e, TelegramBotAbstract? telegramBotAbstract,
        MessageEventArgs? messageEventArgs)
    {
        if (telegramBotAbstract == null)
            return false;

        await NotifyUtil.NotifyOwners(e, telegramBotAbstract, messageEventArgs);
        return true;
    }

    public string GetMessageAsText(
        string? extrainfo, 
        MessageEventArgs? messageEventArgs, 
        bool json
        )
    {
        if (json)
        {
            var jObject = new JObject
            {
                ["number"] = this.GetNumberOfTimes(),
                ["message"] = this.Message,
                ["ExceptionToString"] = this.GetException().ToString(),
                ["StackTrace"] = GetStackTrace(),
                ["MessageArgs"] = messageEventArgs == null ? null : JsonConvert.SerializeObject(messageEventArgs),
                ["extraInfo"] = string.IsNullOrEmpty(extrainfo) ? null: extrainfo
            };
            return JsonConvert.SerializeObject(jObject);
        }

        var message3 = "";
        try
        {
            message3 = "";
            try
            {
                message3 += "Number of times: ";
                message3 += this.GetNumberOfTimes();
                message3 += "\n\n";
            }
            catch
            {
                message3 += "\n\n";
            }

            try
            {
                message3 += "Message:\n";
                message3 += this.Message;
                message3 += "\n\n";
            }
            catch
            {
                message3 += "\n\n";
            }

            try
            {
                message3 += "ExceptionToString:\n";
                message3 += this.GetException().ToString();
                message3 += "\n\n";
            }
            catch
            {
                message3 += "\n\n";
            }

            try
            {
                message3 += "StackTrace:\n";
                message3 += JsonConvert.SerializeObject( GetStackTrace());
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

            if (!string.IsNullOrEmpty(extrainfo))
                message3 += "\n\n" + extrainfo;
        }
        catch (Exception e1)
        {
            message3 = "Error in sending exception: this exception occurred:\n\n" + e1.Message;
        }
            
        return message3;

    }

    private JObject GetStackTrace()
    {
        var result = new JObject
        {
            ["eStack"] = this.StackTrace,
            ["currStack"] = System.Environment.StackTrace
        };
        return result;
    }
}