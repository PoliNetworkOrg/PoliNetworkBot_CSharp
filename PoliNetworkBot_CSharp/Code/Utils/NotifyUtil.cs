#region

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects;
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

    internal static async Task<MessageSentResult?> NotifyOwners(ExceptionNumbered exception,
        TelegramBotAbstract? sender, MessageEventArgs? messageEventArgs, int loopNumber = 0, string? extrainfo = null,
        string? langCode = DefaultLang,
        long? replyToMessageId2 = null)
    {
        if (sender == null)
            return null;

        string message3;
        try
        {
            message3 = "";
            try
            {
                message3 += "Number of times: ";
                message3 += exception.GetNumberOfTimes();
                message3 += "\n\n";
            }
            catch
            {
                message3 += "\n\n";
            }

            try
            {
                message3 += "Message:\n";
                message3 += exception.Message;
                message3 += "\n\n";
            }
            catch
            {
                message3 += "\n\n";
            }

            try
            {
                message3 += "ExceptionToString:\n";
                message3 += exception.GetException().ToString();
                message3 += "\n\n";
            }
            catch
            {
                message3 += "\n\n";
            }

            try
            {
                message3 += "StackTrace:\n";
                message3 += exception.StackTrace;
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

            if (!string.IsNullOrEmpty(extrainfo)) message3 += "\n\n" + extrainfo;
        }
        catch (Exception e1)
        {
            message3 = "Error in sending exception: this exception occurred:\n\n" + e1.Message;
        }

        var text = new Language(new Dictionary<string, string?>
        {
            { "it", "Eccezione! " + message3 },
            { "en", "Exception! " + message3 }
        });

        var r1 = await NotifyOwners2Async(text, sender, loopNumber, langCode, replyToMessageId2, messageEventArgs);
        return r1;
    }

    internal static Task NotifyOwners(string? v, TelegramBotAbstract? telegramBotAbstract,
        MessageEventArgs? messageEventArgs)
    {
        return NotifyOwners3(new Language(new Dictionary<string, string?> { { "it", v } }), telegramBotAbstract,
            null, 0, null, messageEventArgs);
    }

    private static async Task<MessageSentResult?> NotifyOwners3(Language text2, TelegramBotAbstract? sender,
        long? replyToMessageId, int v, string? langCode, MessageEventArgs? messageEventArgs)
    {
        Logger.Logger.WriteLine(text2.Select(langCode), LogSeverityLevel.ERROR);

        var text = GetNotifyText(langCode, text2);


        return await SendMessage.SendMessageInAGroup(sender, langCode, text, messageEventArgs,
            Data.Constants.Groups.GroupException,
            ChatType.Group, ParseMode.Html, replyToMessageId, true, v);
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

    private static async Task<MessageSentResult?> NotifyOwners2Async(Language text, TelegramBotAbstract? sender,
        int v, string? langCode, long? replyto, MessageEventArgs? messageEventArgs)
    {
        return await NotifyOwners3(text, sender, replyto, v, langCode, messageEventArgs);
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

    internal static async Task NotifyOwnersAsync(Tuple<List<ExceptionNumbered>, int> exceptions,
        TelegramBotAbstract? sender, MessageEventArgs? messageEventArgs, string v, string? langCode,
        long? replyToMessageId = null)
    {
        MessageSentResult? m = null;
        try
        {
            var text = new Language(new Dictionary<string, string?>
            {
                { "en", v }
            });
            m = await NotifyOwners2Async(text, sender, 0, langCode, replyToMessageId, messageEventArgs);
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
            _ = await NotifyOwners2Async(text, sender, 0, langCode, replyToMessageId, messageEventArgs);
        }
        catch
        {
            // ignored
        }

        try
        {
            foreach (var e1 in exceptionNumbereds)
                try
                {
                    await NotifyOwners(e1, sender, messageEventArgs);
                }
                catch
                {
                    // ignored
                }
        }
        catch
        {
            // ignored
        }

        try
        {
            var text2 = new Language(new Dictionary<string, string?>
            {
                { "en", "---End---" }
            });

            long? replyto = null;

            if (m != null) replyto = m.GetMessageId();
            await NotifyOwners2Async(text2, sender, 0, langCode, replyto, messageEventArgs);
        }
        catch
        {
            // ignored
        }
    }

    public static async void NotifyOwnersBanAction(TelegramBotAbstract? sender, MessageEventArgs? messageEventArgs,
        RestrictAction restrictAction, Tuple<BanUnbanAllResult, List<ExceptionNumbered>, long>? done,
        string? finalTarget,
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

                var (banUnbanAllResult, _, item3) = done;
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
}