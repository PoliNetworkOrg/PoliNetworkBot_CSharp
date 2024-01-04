#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PoliNetworkBot_CSharp.Code.Data.Constants;
using PoliNetworkBot_CSharp.Code.Data.Variables;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Enums.Action;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.AbstractBot;
using PoliNetworkBot_CSharp.Code.Objects.BanUnban;
using PoliNetworkBot_CSharp.Code.Objects.Exceptions;
using PoliNetworkBot_CSharp.Code.Objects.Files;
using PoliNetworkBot_CSharp.Code.Objects.Log;
using PoliNetworkBot_CSharp.Code.Objects.TelegramMedia;
using Telegram.Bot.Types.Enums;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils.Notify;

internal static class NotifyUtil
{
    private const string DefaultLang = "en";

    /// <summary>
    ///     Used to notify of permitted spam in the permitted spam group.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="messageEventArgs"></param>
    internal static async Task<bool> NotifyOwnersPermittedSpam(TelegramBotAbstract? sender,
        EventArgsContainer? messageEventArgs)
    {
        var title = messageEventArgs?.MessageEventArgs?.Message.Chat.Title;
        if (messageEventArgs is not { MessageEventArgs.Message: not null })
            return false;

        var text = messageEventArgs.MessageEventArgs.Message.Text ?? messageEventArgs.MessageEventArgs.Message.Caption;
        if (text == null)
        {
            var ex = new Exception("text null and caption null in permitted spam notification");
            await NotifyOwnerWithLog2(ex, sender, messageEventArgs);
            return false;
        }

        var hashText = HashUtils.GetHashOf(text)?[..20];

        var message = "#Permitted spam in group: ";
        message += "\n";
        message += title;
        message += "\n\n";
        message += "@@@@@@@";
        message += "\n\n";
        message += text;
        message += "\n\n";
        message += "@@@@@@@";
        message += "\n\n";
        message += "#IDGroup_" + (messageEventArgs.MessageEventArgs.Message.Chat.Id > 0
            ? messageEventArgs.MessageEventArgs.Message.Chat.Id.ToString()
            : "n" + -1 * messageEventArgs.MessageEventArgs.Message.Chat.Id);
        message += "\n" + "#IDUser_" + messageEventArgs.MessageEventArgs.Message.From?.Id;
        message += "\n\n";
        message += "Message tag: #" + hashText;

        const string? langCode = "it";
        var text2 = new Language(new Dictionary<string, string?>
        {
            { "it", message }
        });
        Logger.Logger.WriteLine(text2.Select("it"), LogSeverityLevel.ALERT);
        var m = await SendMessage.SendMessageInAGroup(sender, langCode, text2, messageEventArgs,
            GroupsConstants.PermittedSpamGroup.FullLong(),
            ChatType.Group,
            ParseMode.Html, null, true);
        return m != null && m.IsSuccess();
    }

    internal static List<MessageSentResult?>? NotifyOwnersClassic(ExceptionNumbered exception,
        TelegramBotAbstract? sender, EventArgsContainer? messageEventArgs, ExtraInfo? extraInfo = null,
        string? langCode = DefaultLang,
        long? replyToMessageId2 = null)
    {
        if (sender == null)
            return null;

        var r = new List<MessageSentResult?>();

        lock (Locks.LockObjectExceptionGroup)
        {
            try
            {
                var message3 = exception.GetMessageAsText(extraInfo, messageEventArgs, false);
                var r1 = message3.SendToOwners(sender, langCode, replyToMessageId2, messageEventArgs,
                    FileTypeJsonEnum.SIMPLE_STRING, new LogFileInfo { filename = "ex.json" });

                if (r1 != null)
                    r.AddRange(r1);

                var r4 = SendStack(sender, langCode,
                    replyToMessageId2, messageEventArgs, extraInfo, exception).Result;

                if (r4 != null)
                    r.AddRange(r4);

                return r;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        return null;
    }

    private static Task<List<MessageSentResult>?> SendStack(TelegramBotAbstract sender, string? langCode,
        long? replyToMessageId2, EventArgsContainer? messageEventArgs, ExtraInfo? extraInfo,
        ExceptionNumbered exception)
    {
        try
        {
            var telegramFileContent = TelegramFileContent.GetStack(extraInfo, messageEventArgs, exception);

            if (telegramFileContent == null)
                return Task.FromResult<List<MessageSentResult>?>(null);

            var r4 = telegramFileContent.SendToOwners(
                sender, langCode, replyToMessageId2,
                messageEventArgs, FileTypeJsonEnum.SIMPLE_STRING, new LogFileInfo { filename = "stack.json" });

            return Task.FromResult(r4);
        }
        catch
        {
            // ignored
        }

        return Task.FromResult<List<MessageSentResult>?>(null);
    }

    internal static Task NotifyOwners_AnError_AndLog3(string? v, TelegramBotAbstract? telegramBotAbstract,
        EventArgsContainer? messageEventArgs, FileTypeJsonEnum whatWeWant, SendActionEnum sendActionEnum)
    {
        return NotifyOwners_AnError_AndLog(new Language(new Dictionary<string, string?> { { "it", v } }),
            telegramBotAbstract,
            null, null, messageEventArgs, null, whatWeWant, sendActionEnum, TextAsCaption.AS_CAPTION);
    }

    private static async Task<List<MessageSentResult>?> NotifyOwners_AnError_AndLog(Language text2,
        TelegramBotAbstract? sender,
        long? replyToMessageId, string? langCode, EventArgsContainer? messageEventArgs, StringJson? fileContent,
        FileTypeJsonEnum? whatWeWant, SendActionEnum sendActionEnum, TextAsCaption textAsCaptionParam)
    {
        Logger.Logger.WriteLine(text2.Select(langCode), LogSeverityLevel.ERROR);

        var text = GetNotifyText(langCode, text2);

        switch (sendActionEnum)
        {
            case SendActionEnum.SEND_FILE:
                return SendString(fileContent, messageEventArgs, sender, "stack.json", text,
                    replyToMessageId, ParseMode.Html, whatWeWant, textAsCaptionParam);

            case SendActionEnum.SEND_TEXT:
                var x = await SendMessage.SendMessageInAGroup(sender, langCode, text, messageEventArgs,
                    GroupsConstants.GroupException.FullLong(),
                    ChatType.Group, ParseMode.Html, replyToMessageId, true);
                return x != null ? new List<MessageSentResult> { x } : new List<MessageSentResult>();
        }

        return null;
    }

    private static Language GetNotifyText(string? langCode, Language text2)
    {
        var text = new Language(new Dictionary<string, string?>
        {
            { langCode ?? "en", HttpUtility.HtmlEncode(text2.Select(langCode)) }
        });
        return text;
    }

    internal static Task<List<MessageSentResult?>?> NotifyOwnerWithLog2(Exception? e,
        TelegramBotAbstract? telegramBotAbstract,
        EventArgsContainer? messageEventArgs)
    {
        try
        {
            var x = NotifyOwnersClassic(new ExceptionNumbered(e), telegramBotAbstract, messageEventArgs);
            Logger.Logger.WriteLine(e);
            return Task.FromResult(x);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }

        return Task.FromResult<List<MessageSentResult?>?>(null);
    }

    internal static Task<List<MessageSentResult?>?> NotifyOwnerWithLog25(Exception? e,
        TelegramBotAbstract? telegramBotAbstract,
        EventArgsContainer? messageEventArgs)
    {
        try
        {
            Logger.Logger.WriteLine(e);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }

        try
        {
            Console.WriteLine(e);
            Console.WriteLine(messageEventArgs);
            Console.WriteLine(telegramBotAbstract);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }

        return Task.FromResult<List<MessageSentResult?>?>(null);
    }


    public static async Task<List<MessageSentResult>?> NotifyOwners_AnError_AndLog2(Language text,
        TelegramBotAbstract? sender,
        string? langCode, long? replyTo, EventArgsContainer? messageEventArgs, StringJson? fileContent,
        FileTypeJsonEnum? whatWeWant, SendActionEnum sendActionEnum)
    {
        return await NotifyOwners_AnError_AndLog(text, sender, replyTo, langCode, messageEventArgs, fileContent,
            whatWeWant, sendActionEnum, TextAsCaption.AS_CAPTION);
    }

    internal static void NotifyIfFalseAsync(Tuple<bool?, string, long>? r1, string extraInfo,
        TelegramBotAbstract? sender)
    {
        if (r1?.Item1 == null) return;

        if (r1.Item1.Value) return;

        var error = "Error (notifyIfFalse): ";
        error += "\n";
        error += "String: " + r1.Item2 + "\n";
        error += "Long: " + r1.Item3 + "\n";
        error += "Extra: " + extraInfo;
        error += "\n";

        var exception = new ExceptionNumbered(error);
        NotifyOwnersClassic(exception, sender, null);
    }

    internal static async Task NotifyOwnersAsync5(Tuple<List<ExceptionNumbered>, int> exceptions,
        TelegramBotAbstract? sender, EventArgsContainer? messageEventArgs, string v, string? langCode,
        string filename,
        long? replyToMessageId = null)
    {
        List<MessageSentResult>? m = null;
        try
        {
            var text = new Language(new Dictionary<string, string?>
            {
                { "en", v }
            });
            m = await NotifyOwners_AnError_AndLog2(text, sender, langCode, replyToMessageId, messageEventArgs, null,
                null, SendActionEnum.SEND_TEXT);
        }
        catch
        {
            // ignored
        }

        var (exceptionsNumbered, item2) = exceptions;
        try
        {
            var text = new Language(new Dictionary<string, string?>
            {
                { "en", "Number of exceptions: " + item2 + " - " + exceptionsNumbered.Count }
            });
            _ = await NotifyOwners_AnError_AndLog2(text, sender, langCode, replyToMessageId, messageEventArgs, null,
                null, SendActionEnum.SEND_FILE);
        }
        catch
        {
            // ignored
        }

        await SendNumberedExceptionsAsFile(exceptionsNumbered, sender, messageEventArgs, filename, replyToMessageId);


        try
        {
            var text2 = new Language(new Dictionary<string, string?>
            {
                { "en", "---End---" }
            });

            long? replyTo = null;

            if (m != null)
            {
                var messageSentResult = m.First();
                replyTo = messageSentResult.GetMessageId();
            }

            await NotifyOwners_AnError_AndLog2(text2, sender, langCode, replyTo, messageEventArgs, null, null,
                SendActionEnum.SEND_TEXT);
        }
        catch
        {
            // ignored
        }
    }

    private static Task SendNumberedExceptionsAsFile(IEnumerable<ExceptionNumbered> exceptionsNumbered,
        TelegramBotAbstract? sender,
        EventArgsContainer? messageEventArgs, string filename, long? replyToMessageId)
    {
        try
        {
            var toSend = exceptionsNumbered.Select(variable => variable.GetMessageAsText(null, messageEventArgs, true))
                .Select(x => x.GetFileContentStringJson()).ToList();
            var toSendString = GetSerialized(toSend);
            SendString(toSendString, messageEventArgs, sender, filename, new L(), replyToMessageId, ParseMode.Html,
                FileTypeJsonEnum.STRING_JSONED, TextAsCaption.AS_CAPTION);
        }
        catch (Exception e)
        {
            try
            {
                _ = NotifyOwnersWithLog(e, sender, null, messageEventArgs);
            }
            catch
            {
                //ignored
            }
        }

        return Task.CompletedTask;
    }

    private static StringJson GetSerialized(IEnumerable<StringJson?> toSend)
    {
        var toSend2 = toSend.Where(x => x != null).ToList();
        var r = "[";
        for (var i = 0; i < toSend2.Count; i++)
        {
            r += toSend2[i]?.GetStringAsJson();

            if (i == toSend2.Count - 1)
                r += ",";
        }

        r += "]";
        return new StringJson(FileTypeJsonEnum.STRING_JSONED, r);
    }

    public static List<MessageSentResult> SendString(StringJson? toSendString,
        EventArgsContainer? messageEventArgs,
        TelegramBotAbstract? sender, string filename, Language? caption, long? replyToMessageId,
        ParseMode parseMode, FileTypeJsonEnum? whatWeWant, TextAsCaption textAsCaptionParam)
    {
        var stream = GenerateStreamFromString(toSendString, whatWeWant);
        return SendFiles(messageEventArgs, sender, filename, stream, caption, parseMode, replyToMessageId,
            textAsCaptionParam);
    }

    private static Stream GenerateStreamFromString(StringJson? s, FileTypeJsonEnum? whatWeWant)
    {
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        if (s != null)
            writer.Write(s.Get(whatWeWant));
        writer.Flush();
        stream.Position = 0;
        return stream;
    }

    private static List<MessageSentResult> SendFiles(EventArgsContainer? messageEventArgs,
        TelegramBotAbstract? telegramBotAbstract,
        string filename, Stream stream, Language? caption, ParseMode parseModeCaption, long? replyToMessageId,
        TextAsCaption textAsCaptionParam)
    {
        var peer = new PeerAbstract(GroupsConstants.GroupException.FullLong(), ChatType.Group);
        var destinatari = new List<PeerAbstract> { peer };
        return SendFiles2(
            stream, filename, caption, telegramBotAbstract,
            messageEventArgs?.MessageEventArgs?.Message.From?.Username, destinatari, parseModeCaption, replyToMessageId,
            textAsCaptionParam);
    }

    private static List<MessageSentResult> SendFiles2(Stream stream, string filename, Language? caption,
        TelegramBotAbstract? telegramBotAbstract, string? fromUsername, IEnumerable<PeerAbstract> peerAbstracts,
        ParseMode parseModeCaption, long? replyToMessageId, TextAsCaption textAsCaptionParam)
    {
        var file = TelegramFile.FromStreamJson(stream, filename, caption, textAsCaptionParam);


        //var peer = new PeerAbstract(e?.Message?.From?.Id, message.Chat.Type);


        return peerAbstracts.Select(peer =>
                SendFiles3(peer, file, telegramBotAbstract, fromUsername, replyToMessageId, parseModeCaption))
            .ToList();
    }

    private static MessageSentResult SendFiles3(PeerAbstract peer, TelegramFile file,
        TelegramBotAbstract? telegramBotAbstract, string? fromUsername, long? replyToMessageId,
        ParseMode parseModeCaption)
    {
        var b = SendMessage.SendFileAsync(file, peer,
            telegramBotAbstract, fromUsername, "en",
            replyToMessageId, true, parseModeCaption);
        return new MessageSentResult(b, null, peer.Type);
    }

    public static async void NotifyOwnersBanAction(TelegramBotAbstract? sender, EventArgsContainer? messageEventArgs,
        RestrictAction restrictAction, BanUnbanAllResultComplete? done,
        TargetUserObject finalTarget,
        string? reason)
    {
        try
        {
            {
                if (messageEventArgs is not { MessageEventArgs.Message: not null }) return;

                var message = "Restrict action: " + restrictAction;
                message += "\n";
                message += "Restricted by: " +
                           UserbotPeer.GetHtmlStringWithUserLink(messageEventArgs.MessageEventArgs.Message.From);
                message += "\n";
                message += "For reason: \n";
                message += reason;
                message += "\n";
                message += "-----";
                message += "\n";
                if (done == null)
                    return;

                var (banUnbanAllResult, item3) = done;
                message += banUnbanAllResult.GetLanguage(restrictAction, finalTarget, item3)?.Select("it");

                const string? langCode = "it";
                var text2 = new Language(new Dictionary<string, string?>
                {
                    { "it", message }
                });
                Logger.Logger.WriteLine(text2.Select("it"), LogSeverityLevel.ALERT);
                await SendMessage.SendMessageInAGroup(sender, langCode, text2, messageEventArgs,
                    GroupsConstants.BanNotificationGroup.FullLong(),
                    ChatType.Group,
                    ParseMode.Html, null, true);
            }
        }
        catch (Exception? e)
        {
            Logger.Logger.WriteLine(e);
        }
    }

    public static async Task<bool> NotifyOwnersBanAction(TelegramBotAbstract? sender,
        EventArgsContainer? messageEventArgs,
        long? target, string? username)
    {
        try
        {
            {
                if (messageEventArgs is not { MessageEventArgs.Message: not null }) return false;
                var message = "Restrict action: " + "Simple Ban";
                message += "\n";
                message += "Restricted user: " + target + "[" +
                           (string.IsNullOrEmpty(username) ? "Unknown" : " @" + username) + " ]" + " in group: " +
                           messageEventArgs.MessageEventArgs.Message.Chat.Id + " [" +
                           messageEventArgs.MessageEventArgs.Message.Chat.Title + "]";
                message += "\n";
                message += "Restricted by: " +
                           UserbotPeer.GetHtmlStringWithUserLink(messageEventArgs.MessageEventArgs.Message.From);

                const string? langCode = "it";
                var text2 = new Language(new Dictionary<string, string?>
                {
                    { "it", message }
                });
                Logger.Logger.WriteLine(text2.Select("it"), LogSeverityLevel.ALERT);
                var m = await SendMessage.SendMessageInAGroup(sender, langCode, text2, messageEventArgs,
                    GroupsConstants.BanNotificationGroup.FullLong(),
                    ChatType.Group,
                    ParseMode.Html, null, true);
                return m != null && m.IsSuccess();
            }
        }
        catch (Exception? e)
        {
            Logger.Logger.WriteLine(e);
        }

        return false;
    }

    public static Task NotifyOwnersWithLog(Exception? exception, TelegramBotAbstract? telegramBotAbstract,
        string? stackTrace, EventArgsContainer? eventArgsContainer)
    {
        var extraInfo = new ExtraInfo
        {
            StackTrace = stackTrace
        };
        NotifyOwnersClassic(new ExceptionNumbered(exception), telegramBotAbstract,
            eventArgsContainer, extraInfo);
        Logger.Logger.WriteLine(exception);
        return Task.CompletedTask;
    }

    /// <summary>
    ///     Notifies Council and Permitted spam group of the addiction of a new Allowed Message
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="messageEventArgs"></param>
    /// <param name="text"></param>
    /// <param name="groups"></param>
    /// <param name="messageType"></param>
    /// <param name="assoc"></param>
    /// <returns>Language with his language code</returns>
    public static async Task<string?> NotifyAllowedMessage(TelegramBotAbstract? sender,
        EventArgsContainer? messageEventArgs,
        string? text, string? groups, string? messageType, string? assoc)
    {
        var message = CreatePermittedSpamMessage(messageEventArgs, text, groups, messageType, assoc);

        const string? langCode = "en";
        var text2 = new Language(new Dictionary<string, string?>
        {
            { "en", message }
        });

        Logger.Logger.WriteLine(text2.Select("en"), LogSeverityLevel.ALERT);
        await SendMessage.SendMessageInAGroup(sender, langCode, text2, messageEventArgs,
            GroupsConstants.PermittedSpamGroup.FullLong(),
            ChatType.Group,
            ParseMode.Html, null, true);

        return message;
    }

    private static string CreatePermittedSpamMessage(EventArgsContainer? messageEventArgs,
        string? text, string? groups, string? messageType, string? assoc)
    {
        var hashAssoc = HashUtils.GetHashOf(assoc)?[..8];
        var hashText = HashUtils.GetHashOf(text)?[..20];

        var message = "#Allowed spam in groups: " + groups;
        message += "\n\n";
        message += "Allowed by: " +
                   UserbotPeer.GetHtmlStringWithUserLink(messageEventArgs?.MessageEventArgs?.Message.From);
        message += "\n\n";
        message += "Association: " + assoc;
        message += " #" + hashAssoc;
        message += "\n\n";
        message += "Message type: " + messageType;
        message += "\n\n";
        message += "@@@@@@@";
        message += "\n\n";
        message += text;
        message += "\n\n";
        message += "@@@@@@@";
        message += "\n\n";
        message += "Message tag: #" + hashText;
        return message;
    }


    public static void SendReportOfSuccessAndFailures(TelegramBotAbstract? sender, MessageEventArgs? e,
        BanUnbanAllResultComplete? done)
    {
        try
        {
            if (done == null)
                return;

            var (banUnbanAllResult, _) = done;
            SendReportOfSuccessAndFailures2(
                StreamSerialization.SerializeToStream(banUnbanAllResult.GetSuccess()),
                "success.bin", sender, e);
            SendReportOfSuccessAndFailures2(
                StreamSerialization.SerializeToStream(banUnbanAllResult.GetFailed()),
                "failed.bin", sender, e);
        }
        catch
        {
            // ignored
        }
    }


    private static void SendReportOfSuccessAndFailures2(Stream? stream, string filename,
        TelegramBotAbstract? sender, MessageEventArgs? e)
    {
        var file = new TelegramFile(stream, filename, new L(), "application/octet-stream", TextAsCaption.AS_CAPTION);
        var message = e?.Message;
        if (message == null)
            return;

        var peer = new PeerAbstract(e?.Message.From?.Id, message.Chat.Type);
        SendMessage.SendFileAsync(file, peer,
            sender, e?.Message.From?.Username, e?.Message.From?.LanguageCode,
            null, true);
    }

    public static bool SendReportOfExecution(
        MessageEventArgs? messageEventArgs,
        TelegramBotAbstract telegramBotAbstract,
        List<long?> longs,
        string s,
        JToken? extraValues = null)
    {
        var stack = Environment.StackTrace;
        var x = new JObject
        {
            ["messageEventArgs"] = TelegramFileContent.GetMessageEventArgsAsJToken(messageEventArgs),
            ["telegramBotAbstract"] = telegramBotAbstract.GetId(),
            ["sendTo"] = ToJArray(longs),
            ["message"] = s,
            ["stacktrace"] = LogObject.GetJArray(stack),
            ["extra"] = extraValues
        };
        var sTosend = JsonConvert.SerializeObject(x);

        var r = new List<bool>();
        foreach (var toSend in longs)
            try
            {
                var r2 = SendReportSingle(sTosend, toSend, telegramBotAbstract);
                r.Add(r2);
            }
            catch
            {
                r.Add(false);
            }

        return r.All(b => b);
    }

    private static bool SendReportSingle(string sTosend, long? toSendUser, TelegramBotAbstract telegramBotAbstract)
    {
        var language = sTosend.Length < 1024 ? new L(sTosend) : new L();
        var documentInput =
            TelegramFile.FromString(sTosend, "report_execution.json", language, TextAsCaption.AS_CAPTION);
        var peer = new PeerAbstract(toSendUser, ChatType.Private);

        var messageOptions = new MessageOptions
        {
            ChatId = peer.GetUserId(),
            Peer = peer,

            DocumentInput = documentInput,
            DisablePreviewLink = false
        };
        var r2 = telegramBotAbstract.SendFileAsync(messageOptions);
        return r2;
    }

    private static JArray ToJArray(List<long?> longs)
    {
        var x = new JArray();
        foreach (var i in longs) x.Add(i);

        return x;
    }
}