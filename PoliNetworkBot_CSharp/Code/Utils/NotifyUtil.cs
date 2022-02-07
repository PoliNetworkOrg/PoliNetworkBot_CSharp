#region

using Newtonsoft.Json;
using PoliNetworkBot_CSharp.Code.Bots.Anon;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot.Types.Enums;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils
{
    internal class NotifyUtil
    {
        private const long group_exception = -1001456960264;
        private const long permitted_spam_group = -1001685451643;
        private const long ban_notification_group = -1001710276126;
        private const string default_lang = "en";

        internal static async Task NotifyOwnersPermittedSpam(TelegramBotAbstract sender,
            MessageEventArgs messageEventArgs)
        {
            var title = messageEventArgs.Message.Chat.Title;
            if (messageEventArgs is { Message: { } })
            {
                var message = "Permitted spam in group: ";
                message += "\n";
                message += title;
                message += "\n\n";
                message += "@@@@@@@";
                message += "\n\n";
                message += messageEventArgs.Message.Text;
                message += "\n\n";
                message += "@@@@@@@";
                message += "\n\n";
                message += "#IDGroup_" + (messageEventArgs.Message.Chat.Id > 0
                    ? messageEventArgs.Message.Chat.Id.ToString()
                    : "n" + -1 * messageEventArgs.Message.Chat.Id);
                message += "\n" + "#IDUser_" + messageEventArgs.Message.From?.Id;

                const string langCode = "it";
                var text2 = new Language(new Dictionary<string, string>
                {
                    { "it", message }
                });
                Logger.WriteLine(text2.Select("it"), LogSeverityLevel.ERROR);
                await SendMessage.SendMessageInAGroup(sender, langCode, text2, messageEventArgs, permitted_spam_group,
                    ChatType.Group,
                    ParseMode.Html, group_exception, true);
            }
        }

        internal static async Task NotifyOwners(ExceptionNumbered exception,
            TelegramBotAbstract sender, MessageEventArgs messageEventArgs, int loopNumber = 0, string extrainfo = null,
            string langCode = default_lang,
            long? replyToMessageId2 = null)
        {
            if (sender == null)
                return;

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

            var text = new Language(new Dictionary<string, string>
            {
                { "it", "Eccezione! " + message3 },
                { "en", "Exception! " + message3 }
            });

            var r1 = await NotifyOwners2Async(text, sender, loopNumber, langCode, replyToMessageId2, messageEventArgs);
        }

        internal static Task NotifyOwners(string v, TelegramBotAbstract telegramBotAbstract,
            MessageEventArgs messageEventArgs)
        {
            return NotifyOwners3(new Language(new Dictionary<string, string> { { "it", v } }), telegramBotAbstract,
                null, 0, null, messageEventArgs);
        }

        private static async Task<MessageSentResult> NotifyOwners3(Language text2, TelegramBotAbstract sender,
            long? replyToMessageId, int v, string langCode, MessageEventArgs messageEventArgs)
        {
            Logger.WriteLine(text2.Select(langCode), LogSeverityLevel.ERROR);
            return await SendMessage.SendMessageInAGroup(sender, langCode, text2, messageEventArgs, group_exception,
                ChatType.Group, ParseMode.Html, replyToMessageId, true, v);
        }

        internal static async Task NotifyOwners(Exception e, TelegramBotAbstract telegramBotAbstract,
            MessageEventArgs messageEventArgs, int loopNumber = 0)
        {
            await NotifyOwners(new ExceptionNumbered(e), telegramBotAbstract, messageEventArgs, loopNumber);
        }

        private static async Task<MessageSentResult> NotifyOwners2Async(Language text, TelegramBotAbstract sender,
            int v, string langCode, long? replyto, MessageEventArgs messageEventArgs)
        {
            return await NotifyOwners3(text, sender, replyto, v, langCode, messageEventArgs);
        }

        internal static async Task NotifyIfFalseAsync(Tuple<bool?, string, long> r1, string extraInfo,
            TelegramBotAbstract sender)
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

        internal static async Task NotifyOwners(Exception item2, string message, TelegramBotAbstract sender,
            string langCode, MessageEventArgs messageEventArgs, long? replyToMessageId = null)
        {
            var dict = new Dictionary<string, string>
            {
                { "en", message }
            };
            var text = new Language(dict);
            await NotifyOwners2Async(text, sender, 0, langCode, replyToMessageId, messageEventArgs);
        }

        internal static async Task NotifyOwnersAsync(Tuple<List<ExceptionNumbered>, int> exceptions,
            TelegramBotAbstract sender, MessageEventArgs messageEventArgs, string v, string langCode,
            long? replyToMessageId = null)
        {
            MessageSentResult m = null;
            try
            {
                var text = new Language(new Dictionary<string, string>
                {
                    { "en", v }
                });
                m = await NotifyOwners2Async(text, sender, 0, langCode, replyToMessageId, messageEventArgs);
            }
            catch
            {
                ;
            }

            var (exceptionNumbereds, item2) = exceptions;
            try
            {
                var text = new Language(new Dictionary<string, string>
                {
                    { "en", "Number of exceptions: " + item2 + " - " + exceptionNumbereds.Count }
                });
                _ = await NotifyOwners2Async(text, sender, 0, langCode, replyToMessageId, messageEventArgs);
            }
            catch
            {
                ;
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
                        ;
                    }
            }
            catch
            {
                ;
            }

            try
            {
                var text2 = new Language(new Dictionary<string, string>
                {
                    { "en", "---End---" }
                });

                long? replyto = null;
                ;

                if (m != null) replyto = m.GetMessageID();
                await NotifyOwners2Async(text2, sender, 0, langCode, replyto, messageEventArgs);
            }
            catch
            {
                ;
            }
        }

        public static async void NotifyOwnersBanAction(TelegramBotAbstract sender, MessageEventArgs messageEventArgs,
            RestrictAction restrictAction, Tuple<BanUnbanAllResult, List<ExceptionNumbered>, long> done,
            string finalTarget,
            string reason)
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
                    var (banUnbanAllResult, exceptionNumbereds, item3) = done;
                    message += banUnbanAllResult.GetLanguage(restrictAction, finalTarget, item3).Select("it");
                    ;

                    const string langCode = "it";
                    var text2 = new Language(new Dictionary<string, string>
                    {
                        { "it", message }
                    });
                    Logger.WriteLine(text2.Select("it"), LogSeverityLevel.ALERT);
                    await SendMessage.SendMessageInAGroup(sender, langCode, text2, messageEventArgs,
                        ban_notification_group,
                        ChatType.Group,
                        ParseMode.Html, group_exception, true);
                }
            }
            catch (Exception e)
            {
                Logger.WriteLine(e);
            }
        }

        public static async void NotifyOwnersBanAction(TelegramBotAbstract sender, MessageEventArgs messageEventArgs,
            long? target, string username)
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
                               "<a href=tg://user?id=" + messageEventArgs.Message.From?.Id + ">" +
                               messageEventArgs.Message.From?.Id + "</a>" + "]";

                    const string langCode = "it";
                    var text2 = new Language(new Dictionary<string, string>
                    {
                        { "it", message }
                    });
                    Logger.WriteLine(text2.Select("it"), LogSeverityLevel.ALERT);
                    await SendMessage.SendMessageInAGroup(sender, langCode, text2, messageEventArgs,
                        ban_notification_group,
                        ChatType.Group,
                        ParseMode.Html, group_exception, true);
                }
            }
            catch (Exception e)
            {
                Logger.WriteLine(e);
            }
        }
    }
}