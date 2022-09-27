#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.BanUnban;
using PoliNetworkBot_CSharp.Code.Objects.Files;
using PoliNetworkBot_CSharp.Code.Objects.TelegramMedia;
using Telegram.Bot.Types.Enums;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils;

internal static class NotifyUtil
{
    private const string DefaultLang = "en";

    /// <summary>
    ///     Used to notify of permitted spam in the permitted spam group.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="messageEventArgs"></param>
    internal static async Task NotifyOwnersPermittedSpam(TelegramBotAbstract? sender,
        MessageEventArgs? messageEventArgs)
    {
        var title = messageEventArgs?.Message?.Chat.Title;
        if (messageEventArgs is { Message: { } })
        {
            var text = messageEventArgs.Message.Text ?? messageEventArgs.Message.Caption;
            if (text == null)
            {
                var ex = new Exception("text null and caption null in permitted spam notification");
                await NotifyOwners(ex, sender, messageEventArgs);
                return;
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
            message += "#IDGroup_" + (messageEventArgs.Message.Chat.Id > 0
                ? messageEventArgs.Message.Chat.Id.ToString()
                : "n" + -1 * messageEventArgs.Message.Chat.Id);
            message += "\n" + "#IDUser_" + messageEventArgs.Message.From?.Id;
            message += "\n\n";
            message += "Message tag: #" + hashText;

            const string? langCode = "it";
            var text2 = new Language(new Dictionary<string, string?>
            {
                { "it", message }
            });
            Logger.Logger.WriteLine(text2.Select("it"), LogSeverityLevel.ALERT);
            await SendMessage.SendMessageInAGroup(sender, langCode, text2, messageEventArgs,
                Data.Constants.Groups.PermittedSpamGroup,
                ChatType.Group,
                ParseMode.Html, null, true);
        }
    }

    internal static async Task<List<MessageSentResult>?> NotifyOwners(ExceptionNumbered exception,
        TelegramBotAbstract? sender, MessageEventArgs? messageEventArgs, int loopNumber = 0, string? extrainfo = null,
        string? langCode = DefaultLang,
        long? replyToMessageId2 = null)
    {
        if (sender == null)
            return null;

        var message3 = exception.GetMessageAsText(extrainfo, messageEventArgs, false);
        return await message3.Send(sender, loopNumber, langCode, replyToMessageId2, messageEventArgs);
    }

    internal static Task NotifyOwners(string? v, TelegramBotAbstract? telegramBotAbstract,
        MessageEventArgs? messageEventArgs)
    {
        return NotifyOwners12(new Language(new Dictionary<string, string?> { { "it", v } }), telegramBotAbstract,
            null, null, messageEventArgs, null);
    }

    private static async Task<List<MessageSentResult>?> NotifyOwners12(Language text2, TelegramBotAbstract? sender,
        long? replyToMessageId, string? langCode, MessageEventArgs? messageEventArgs, StringJson? fileContent)
    {
        Logger.Logger.WriteLine(text2.Select(langCode), LogSeverityLevel.ERROR);

        var text = GetNotifyText(langCode, text2);

        return await SendString(fileContent, messageEventArgs, sender, "stack.json", text.Select(langCode),
            replyToMessageId);

        /*
        return await SendMessage.SendMessageInAGroup(sender, langCode, text, messageEventArgs,
            Data.Constants.Groups.GroupException,
            ChatType.Group, ParseMode.Html, replyToMessageId, true, v);
            */
    }

    private static Language GetNotifyText(string? langCode, Language text2)
    {
        var text = new Language(new Dictionary<string, string?>
        {
            { langCode ?? "en", HttpUtility.HtmlEncode(text2.Select(langCode)) }
        });
        return text;
    }

    internal static async Task NotifyOwners(Exception? e, TelegramBotAbstract? telegramBotAbstract,
        MessageEventArgs? messageEventArgs, int loopNumber = 0)
    {
        await NotifyOwners(new ExceptionNumbered(e), telegramBotAbstract, messageEventArgs, loopNumber);
        Logger.Logger.WriteLine(e);
    }

    public static async Task<List<MessageSentResult>?> NotifyOwners7(Language text, TelegramBotAbstract? sender,
        string? langCode, long? replyto, MessageEventArgs? messageEventArgs, StringJson? fileContent = null)
    {
        return await NotifyOwners12(text, sender, replyto, langCode, messageEventArgs, fileContent);
    }

    internal static async Task NotifyIfFalseAsync(Tuple<bool?, string, long>? r1, string extraInfo,
        TelegramBotAbstract? sender)
    {
        if (r1?.Item1 == null)
            return;

        if (r1.Item1.Value)
            return;

        var error = "Error (notifyIfFalse): ";
        error += "\n";
        error += "String: " + r1.Item2 + "\n";
        error += "Long: " + r1.Item3 + "\n";
        error += "Extra: " + extraInfo;
        error += "\n";

        var exception = new ExceptionNumbered(error);
        await NotifyOwners(exception, sender, null);
    }

    internal static async Task NotifyOwnersAsync5(Tuple<List<ExceptionNumbered>, int> exceptions,
        TelegramBotAbstract? sender, MessageEventArgs? messageEventArgs, string v, string? langCode,
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
            m = await NotifyOwners7(text, sender, langCode, replyToMessageId, messageEventArgs);
        }
        catch
        {
            // ignored
        }

        var (exceptionNumbereds, item2) = exceptions;
        try
        {
            var text = new Language(new Dictionary<string, string?>
            {
                { "en", "Number of exceptions: " + item2 + " - " + exceptionNumbereds.Count }
            });
            _ = await NotifyOwners7(text, sender, langCode, replyToMessageId, messageEventArgs);
        }
        catch
        {
            // ignored
        }

        await SendNumberedExceptionsAsFile(exceptionNumbereds, sender, messageEventArgs, filename, replyToMessageId);


        try
        {
            var text2 = new Language(new Dictionary<string, string?>
            {
                { "en", "---End---" }
            });

            long? replyto = null;

            if (m != null)
                replyto = m.First().GetMessageId();
            await NotifyOwners7(text2, sender, langCode, replyto, messageEventArgs);
        }
        catch
        {
            // ignored
        }
    }

    private static async Task SendNumberedExceptionsAsFile(IEnumerable<ExceptionNumbered> exceptionNumbereds,
        TelegramBotAbstract? sender,
        MessageEventArgs? messageEventArgs, string filename, long? replyToMessageId)
    {
        try
        {
            var toSend = exceptionNumbereds.Select(variable => variable.GetMessageAsText(null, messageEventArgs, true))
                .Select(x => x.GetFileContentStringJson()).ToList();
            var toSendString = GetSerialized(toSend);
            await SendString(toSendString, messageEventArgs, sender, filename, "", replyToMessageId);
        }
        catch (Exception e)
        {
            try
            {
                _ = NotifyOwners(e, sender);
            }
            catch
            {
                //ignored
            }
        }
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

    public static async Task<List<MessageSentResult>?> SendString(StringJson? toSendString,
        MessageEventArgs? messageEventArgs,
        TelegramBotAbstract? sender, string filename, string? caption, long? replyToMessageId,
        ParseMode parseMode = ParseMode.Html)
    {
        var stream = GenerateStreamFromString(toSendString);
        return await SendFiles(messageEventArgs, sender, filename, stream, caption, parseMode, replyToMessageId);
    }

    private static Stream GenerateStreamFromString(StringJson? s)
    {
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        if (s != null) 
            writer.Write(s.GetStringAsJson());
        writer.Flush();
        stream.Position = 0;
        return stream;
    }

    private static async Task<List<MessageSentResult>?> SendFiles(MessageEventArgs? messageEventArgs,
        TelegramBotAbstract? telegramBotAbstract,
        string filename, Stream stream, string? caption, ParseMode parseModeCaption, long? replyToMessageId)
    {
        var peer = new PeerAbstract(Data.Constants.Groups.GroupException, ChatType.Group);
        var destinatari = new List<PeerAbstract> { peer };
        return await SendFiles2(
            stream, filename, caption, telegramBotAbstract,
            messageEventArgs?.Message?.From?.Username, destinatari, parseModeCaption, replyToMessageId
        );
    }

    private static async Task<List<MessageSentResult>?> SendFiles2(Stream stream, string filename, string? caption,
        TelegramBotAbstract? telegramBotAbstract, string? fromUsername, List<PeerAbstract> peerAbstracts,
        ParseMode parseModeCaption, long? replyToMessageId)
    {
        var file = new TelegramFile(stream, filename, caption, "application/json");


        //var peer = new PeerAbstract(e?.Message?.From?.Id, message.Chat.Type);
        var text = new Language(new Dictionary<string, string?>
        {
            { "en", caption }
        });

        var done = new List<MessageSentResult>();

        foreach (var peer in peerAbstracts)
        {
            var b = await SendMessage.SendFileAsync(file, peer, text, TextAsCaption.AS_CAPTION,
                telegramBotAbstract, fromUsername, "en",
                replyToMessageId, true, parseModeCaption);
            done.Add(new MessageSentResult(b, null, peer.Type));
        }

        return done;
    }

    public static async void NotifyOwnersBanAction(TelegramBotAbstract? sender, MessageEventArgs? messageEventArgs,
        RestrictAction restrictAction, BanUnbanAllResultComplete? done,
        TargetUserObject finalTarget,
        string? reason)
    {
        try
        {
            {
                if (messageEventArgs is not { Message: { } }) return;

                var message = "Restrict action: " + restrictAction;
                message += "\n";
                message += "Restricted by: " + (messageEventArgs.Message.From?.Username != null
                               ? "@" + messageEventArgs.Message.From?.Username
                               : "Unknown") + " [" +
                           "<a href=\"tg://user?id=" + messageEventArgs.Message.From?.Id + "\">" +
                           messageEventArgs.Message.From?.Id + "</a>" + "]";
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
                    Data.Constants.Groups.BanNotificationGroup,
                    ChatType.Group,
                    ParseMode.Html, null, true);
            }
        }
        catch (Exception? e)
        {
            Logger.Logger.WriteLine(e);
        }
    }

    public static async void NotifyOwnersBanAction(TelegramBotAbstract? sender, MessageEventArgs? messageEventArgs,
        long? target, string? username)
    {
        try
        {
            {
                if (messageEventArgs is not { Message: { } }) return;
                var message = "Restrict action: " + "Simple Ban";
                message += "\n";
                message += "Restricted user: " + target + "[" +
                           (string.IsNullOrEmpty(username) ? "Unknown" : " @" + username) + " ]" + " in group: " +
                           messageEventArgs.Message.Chat.Id + " [" + messageEventArgs.Message.Chat.Title + "]";
                message += "\n";
                message += "Restricted by: " + (messageEventArgs.Message.From?.Username != null
                               ? "@" + messageEventArgs.Message.From?.Username
                               : "Unknown") + " [" +
                           "<a href=\"tg://user?id=" + messageEventArgs.Message.From?.Id + "\">" +
                           messageEventArgs.Message.From?.Id + "</a>" + "]";

                const string? langCode = "it";
                var text2 = new Language(new Dictionary<string, string?>
                {
                    { "it", message }
                });
                Logger.Logger.WriteLine(text2.Select("it"), LogSeverityLevel.ALERT);
                await SendMessage.SendMessageInAGroup(sender, langCode, text2, messageEventArgs,
                    Data.Constants.Groups.BanNotificationGroup,
                    ChatType.Group,
                    ParseMode.Html, null, true);
            }
        }
        catch (Exception? e)
        {
            Logger.Logger.WriteLine(e);
        }
    }

    public static async Task NotifyOwners(Exception? exception, TelegramBotAbstract? telegramBotAbstract,
        int loopNumber = 0)
    {
        await NotifyOwners(new ExceptionNumbered(exception), telegramBotAbstract, null, loopNumber);
        Logger.Logger.WriteLine(exception);
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
        MessageEventArgs? messageEventArgs,
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
            Data.Constants.Groups.PermittedSpamGroup,
            ChatType.Group,
            ParseMode.Html, null, true);

        return message;
    }

    public static string CreatePermittedSpamMessage(MessageEventArgs? messageEventArgs,
        string? text, string? groups, string? messageType, string? assoc)
    {
        var hashAssoc = HashUtils.GetHashOf(assoc)?[..8];
        var hashText = HashUtils.GetHashOf(text)?[..20];

        var message = "#Allowed spam in groups: " + groups;
        message += "\n\n";
        message += "Allowed by: " + (messageEventArgs?.Message?.From?.Username != null
                       ? "@" + messageEventArgs.Message.From?.Username
                       : "Unknown") + " [" +
                   "<a href=\"tg://user?id=" + messageEventArgs?.Message?.From?.Id + "\">" +
                   messageEventArgs?.Message?.From?.Id + "</a>" + "]";
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


    public static async Task SendReportOfSuccessAndFailures(TelegramBotAbstract? sender, MessageEventArgs? e,
        BanUnbanAllResultComplete? done)
    {
        try
        {
            if (done != null)
            {
                var (banUnbanAllResult, _) = done;
                await SendReportOfSuccessAndFailures2(
                    StreamSerialization.SerializeToStream(banUnbanAllResult.GetSuccess()),
                    "success.bin", sender, e);
                await SendReportOfSuccessAndFailures2(
                    StreamSerialization.SerializeToStream(banUnbanAllResult.GetFailed()),
                    "failed.bin", sender, e);
            }
        }
        catch
        {
            // ignored
        }
    }


    private static async Task SendReportOfSuccessAndFailures2(Stream? stream, string filename,
        TelegramBotAbstract? sender, MessageEventArgs? e)
    {
        var file = new TelegramFile(stream, filename, "", "application/octet-stream");
        var message = e?.Message;
        if (message != null)
        {
            var peer = new PeerAbstract(e?.Message?.From?.Id, message.Chat.Type);
            var text = new Language(new Dictionary<string, string?>
            {
                { "en", "" }
            });
            await SendMessage.SendFileAsync(file, peer, text, TextAsCaption.AS_CAPTION,
                sender, e?.Message?.From?.Username, e?.Message?.From?.LanguageCode,
                null, true);
        }
    }
}