#region

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JsonPolimi_Core_nf.Tipi;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Enums.Action;
using PoliNetworkBot_CSharp.Code.Utils;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

#endregion

namespace PoliNetworkBot_CSharp.Code.Objects.Files;

public class TelegramFileContent
{
    private readonly string? _caption;
    private readonly StringJson? _fileContent;

    public TelegramFileContent(StringJson? fileContent, string? caption)
    {
        _fileContent = fileContent;
        _caption = caption;
    }

    public async Task<List<MessageSentResult?>?> SendToOwners(TelegramBotAbstract sender, string? langCode,
        long? replyToMessageId2, MessageEventArgs? messageEventArgs, FileTypeJsonEnum whatWeWant)
    {
        if ((_fileContent == null || _fileContent.IsEmpty()) && string.IsNullOrEmpty(_caption)) return null;

        if (_fileContent == null || _fileContent.IsEmpty())
        {
            var text1 = new Language(new Dictionary<string, string?>
            {
                { "it", "Eccezione! " + _caption },
                { "en", "Exception! " + _caption }
            });

            var r11 = await NotifyUtil.NotifyOwners_AnError_AndLog2(text1, sender, langCode, replyToMessageId2,
                messageEventArgs,
                _fileContent, whatWeWant, SendActionEnum.SEND_TEXT);
            return r11;
        }


        if (string.IsNullOrEmpty(_caption))
            await NotifyUtil.SendString(
                _fileContent, messageEventArgs, sender,
                "ex.json", "", replyToMessageId2, ParseMode.Html, whatWeWant);

        var text = new Language(new Dictionary<string, string?>
        {
            { "it", "Eccezione! " + _caption },
            { "en", "Exception! " + _caption }
        });

        var r1 = await NotifyUtil.NotifyOwners_AnError_AndLog2(text, sender, langCode, replyToMessageId2,
            messageEventArgs, null,
            null, SendActionEnum.SEND_TEXT);
        return r1;
    }

    public StringJson? GetFileContentStringJson()
    {
        return _fileContent;
    }

    public static TelegramFileContent? GetStack(ExtraInfo? extraInfo, MessageEventArgs? messageEventArgs)
    {
        try
        {
            var stack = Environment.StackTrace;
            JArray strings = GetLines(stack);
            var stackJ = new JObject
            {
                ["currStack"] = strings,
                ["extraInfo"] = extraInfo?.GetJToken(),
                ["messageEventArgs"] = GetMessageEventArgsAsJToken(messageEventArgs)
            };
            var stringToSend = JsonConvert.SerializeObject(stackJ);
            var fileContent = new StringJson(FileTypeJsonEnum.SIMPLE_STRING, stringToSend);
            return new TelegramFileContent(fileContent, null);
        }
        catch
        {
            // ignored
        }

        return null;
    }

    private static JArray GetLines(string stack)
    {
        var s = stack.Replace("\\n", "\n");
        var s2 = s.Split('\n');
        return new JArray() { s2 };
    }

    private static JToken? GetMessageEventArgsAsJToken(MessageEventArgs? messageEventArgs)
    {
        if (messageEventArgs == null)
            return null;
    
        var result = new JObject
        {
            ["message"] = GetMessageAsJToken(messageEventArgs.Message)
        };

        return result;
    }

    private static JToken? GetMessageAsJToken(Message? message)
    {
        if (message == null)
            return null;
        
        var result = new JObject
        {
            ["text"] = message.Text,
            ["from"] = GetUserAsJToken(message.From),
            ["replyTo"] = GetMessageAsJToken(message.ReplyToMessage),
            ["chat"] = GetChatAsJToken(message.Chat)
        };

        return result;
    }

    private static JToken? GetChatAsJToken(Chat? messageChat)
    {
        if (messageChat == null)
            return null;
        
        var result = new JObject
        {
            ["title"] = messageChat.Title,
            ["bio"] = messageChat.Bio,
            ["description"] = messageChat.Description,
            ["id"] = messageChat.Id,
            ["inviteLink"] = messageChat.InviteLink,
            ["username"] = messageChat.Username,
            ["type"] = messageChat.Type.ToString()
        };
        return result;
    }

    private static JToken? GetUserAsJToken(User? messageFrom)
    {
        if (messageFrom == null)
            return null;
        
        var result = new JObject
        {
            ["id"] = messageFrom.Id,
            ["username"] = messageFrom.Username,
            ["langCode"] = messageFrom.LanguageCode,
            ["firstName"] = messageFrom.FirstName,
            ["lastName"] = messageFrom.LastName
        };
        return result;
    }
}