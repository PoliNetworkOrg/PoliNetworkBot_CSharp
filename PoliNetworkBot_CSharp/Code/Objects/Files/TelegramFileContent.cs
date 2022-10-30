#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JsonPolimi_Core_nf.Tipi;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PoliNetworkBot_CSharp.Code.Bots.Anon;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Enums.Action;
using PoliNetworkBot_CSharp.Code.Objects.Exceptions;
using PoliNetworkBot_CSharp.Code.Utils;
using PoliNetworkBot_CSharp.Code.Utils.CallbackUtils;
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
        long? replyToMessageId2, EventArgsContainer? eventArgsContainer, FileTypeJsonEnum whatWeWant)
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
                eventArgsContainer,
                _fileContent, whatWeWant, SendActionEnum.SEND_TEXT);
            return r11;
        }


        if (string.IsNullOrEmpty(_caption))
            await NotifyUtil.SendString(
                _fileContent, eventArgsContainer, sender,
                "ex.json", "", replyToMessageId2, ParseMode.Html, whatWeWant);

        var text = new Language(new Dictionary<string, string?>
        {
            { "it", "Eccezione! " + _caption },
            { "en", "Exception! " + _caption }
        });

        var r1 = await NotifyUtil.NotifyOwners_AnError_AndLog2(text, sender, langCode, replyToMessageId2,
            eventArgsContainer, null,
            null, SendActionEnum.SEND_TEXT);
        return r1;
    }

    public StringJson? GetFileContentStringJson()
    {
        return _fileContent;
    }

    public static TelegramFileContent? GetStack(ExtraInfo? extraInfo, EventArgsContainer? eventArgsContainer)
    {
        try
        {
            var stack = Environment.StackTrace;
            var strings = GetLines(stack);
            var stackJ = new JObject
            {
                ["currStack"] = strings,
                ["extraInfo"] = extraInfo?.GetJToken(),
                ["EventArgsContainer"] = GetEventArgsContainerAsJToken(eventArgsContainer)
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

    private static JToken? GetEventArgsContainerAsJToken(EventArgsContainer? eventArgsContainer)
    {
        if (eventArgsContainer == null)
            return null;
    
        var result = new JObject
        {
            ["MessageEventArgs"] = GetMessageEventArgsAsJToken(eventArgsContainer.MessageEventArgs),
            ["CallbackGenericData"] = GetGenericCallDataAsJToken(eventArgsContainer.CallbackGenericData),
            ["CallbackQueryEventArgs"] = GetCallbackQueryEventArgsAsJToken( eventArgsContainer.CallbackQueryEventArgs)
        };

        return result;
    }

    private static JToken? GetCallbackQueryEventArgsAsJToken(CallbackQueryEventArgs? callbackQueryEventArgs)
    {
        if (callbackQueryEventArgs == null)
            return null;
    
        var result = new JObject
        {
            ["CallbackQuery"] = GetCallbackQueryAsJToken(callbackQueryEventArgs.CallbackQuery)
        };

        return result;
    }

    private static JToken? GetCallbackQueryAsJToken(CallbackQuery? callbackQuery)
    {
        if (callbackQuery == null)
            return null;
    
        var result = new JObject
        {
            ["Data"] = callbackQuery.Data,
            ["Id"] = callbackQuery.Id,
            ["ChatInstance"] = callbackQuery.ChatInstance,
            ["GameShortName"] = callbackQuery.GameShortName,
            ["InlineMessageId"] = callbackQuery.InlineMessageId,
            ["IsGameQuery"] = callbackQuery.IsGameQuery,
            ["From"] = GetUserAsJToken(callbackQuery.From),
            ["Message"] = GetMessageAsJToken(callbackQuery.Message)
        };

        return result;
    }

    private static JToken? GetGenericCallDataAsJToken(CallbackGenericData? callbackGenericData)
    {
        if (callbackGenericData == null)
            return null;
    
        var result = new JObject
        {
            ["id"] = callbackGenericData.Id,
            ["InsertedTime"] = callbackGenericData.InsertedTime,
            ["SelectedAnswer"] = callbackGenericData.SelectedAnswer,
            ["Options"] = GetOptionsAsJToken(callbackGenericData.Options)
        };

        return result;
    }

    private static JToken? GetOptionsAsJToken(IReadOnlyCollection<CallbackOption>? options)
    {
        if (options == null)
            return null;

        var result = new JArray();
        foreach (var callbackOptionAsJToken in options.Select(GetCallbackOptionAsJToken))
        {
            if (callbackOptionAsJToken != null) 
                result.Add(callbackOptionAsJToken);
        }

        return result;
    }

    private static JToken? GetCallbackOptionAsJToken(CallbackOption? callbackOption)
    {
        if (callbackOption == null)
            return null;
    
        var result = new JObject
        {
            ["displayed"] = callbackOption.displayed,
            ["id"] = callbackOption.id,
            ["value"] = GetObjectAsJToken(callbackOption.value)
        };

        return result;
    }

    private static JToken? GetObjectAsJToken(object? callbackOptionValue)
    {
        if (callbackOptionValue == null)
            return null;

        var result = new JObject();

        try
        {
            result["toString"] = callbackOptionValue.ToString();
        }
        catch
        {
            // ignored
        }
        
        try
        {
            result["json"] = JsonConvert.SerializeObject(callbackOptionValue);
        }
        catch
        {
            // ignored
        }

        return result;
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