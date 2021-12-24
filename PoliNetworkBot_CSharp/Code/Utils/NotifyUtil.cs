using PoliNetworkBot_CSharp.Code.Bots.Anon;
using PoliNetworkBot_CSharp.Code.Objects;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Enums;
using Telegram.Bot.Types.Enums;

namespace PoliNetworkBot_CSharp.Code.Utils
{
    internal class NotifyUtil
    {
        private const long group_exception = -1001456960264;
        private const long permitted_spam_group = -1001685451643;
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
                message += "#IDGroup_" + (messageEventArgs.Message.Chat.Id > 0 ? messageEventArgs.Message.Chat.Id.ToString() : "n" + ((-1) * messageEventArgs.Message.Chat.Id));
                message += "\n" + "#IDUser_" + messageEventArgs.Message.From?.Id;

                var langCode = "it";
                var text2 = new Language(new Dictionary<string, string>
                {
                    {"it", message}
                });
                Logger.WriteLine(text2.Select("it"), LogSeverityLevel.ERROR);
                await SendMessage.SendMessageInAGroup(sender, langCode, text2, permitted_spam_group, ChatType.Group,
                    ParseMode.Html, group_exception, true);
            }
        }

        internal static async Task NotifyOwners(ExceptionNumbered exception,
            TelegramBotAbstract sender, int loopNumber = 0, string extrainfo = null, string langCode = default_lang,
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

                if (!string.IsNullOrEmpty(extrainfo)) message3 += "\n\n" + extrainfo;
            }
            catch (Exception e1)
            {
                message3 = "Error in sending exception: this exception occurred:\n\n" + e1.Message;
            }

            var text = new Language(new Dictionary<string, string>
            {
                {"it", "Eccezione! " + message3},
                {"en", "Exception! " + message3}
            });

            var r1 = await NotifyOwners2Async(text, sender, loopNumber, langCode, replyToMessageId2);
            if (r1 == null)
                return;
        }

        internal static Task NotifyOwners(string v, TelegramBotAbstract telegramBotAbstract)
        {
            return NotifyOwners3(new Language(new Dictionary<string, string> { { "it", v } }), telegramBotAbstract, null, 0, null);
        }

        private static async Task<MessageSentResult> NotifyOwners3(Language text2, TelegramBotAbstract sender,
            long? replyToMessageId, int v, string langCode)
        {
            Logger.WriteLine(text2.Select(langCode), LogSeverityLevel.ERROR);
            return await SendMessage.SendMessageInAGroup(sender, langCode, text2, group_exception,
                ChatType.Group, ParseMode.Html, replyToMessageId, true, v);
        }

        internal static async Task NotifyOwners(Exception e, TelegramBotAbstract telegramBotAbstract, int loopNumber = 0)
        {
            await NotifyOwners(new ExceptionNumbered(e), telegramBotAbstract, loopNumber);
        }

        private static async Task<MessageSentResult> NotifyOwners2Async(Language text, TelegramBotAbstract sender,
            int v, string langCode, long? replyto)
        {
            return await NotifyOwners3(text, sender, replyto, v, langCode);
        }

        internal static async Task NotifyIfFalseAsync(Tuple<bool?, string, long> r1, string extraInfo,
            TelegramBotAbstract sender)
        {
            if (r1 == null)
                return;

            if (r1.Item1 == null)
                return;

            if (r1.Item1.Value)
                return;

            var error = "Error (notifyIfFalse): ";
            error += "\n";
            error += "String: " + r1?.Item2 + "\n";
            error += "Long: " + r1.Item3 + "\n";
            error += "Extra: " + extraInfo;
            error += "\n";

            var exception = new ExceptionNumbered(error);
            await NotifyOwners(exception, sender);
        }

        internal static async Task NotifyOwners(Exception item2, string message, TelegramBotAbstract sender,
            string langCode, long? replyToMessageId = null)
        {
            var dict = new Dictionary<string, string>
            {
                {"en", message}
            };
            var text = new Language(dict);
            await NotifyOwners2Async(text, sender, 0, langCode, replyToMessageId);
        }

        internal static async Task NotifyOwnersAsync(Tuple<List<ExceptionNumbered>, int> exceptions,
            TelegramBotAbstract sender, string v, string langCode, long? replyToMessageId = null)
        {
            MessageSentResult m = null;
            try
            {
                var text = new Language(new Dictionary<string, string>
                {
                    {"en", v}
                });
                m = await NotifyOwners2Async(text, sender, 0, langCode, replyToMessageId);
            }
            catch
            {
                ;
            }

            try
            {
                var text = new Language(new Dictionary<string, string>
                {
                    {"en", "Number of exceptions: " + exceptions.Item2 + " - " + exceptions.Item1.Count}
                });
                _ = await NotifyOwners2Async(text, sender, 0, langCode, replyToMessageId);
            }
            catch
            {
                ;
            }

            try
            {
                foreach (var e1 in exceptions.Item1)
                    try
                    {
                        await NotifyOwners(e1, sender);
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
                    {"en", "---End---"}
                });

                long? replyto = null;
                ;

                if (m != null) replyto = m.GetMessageID();
                await NotifyOwners2Async(text2, sender, 0, langCode, replyto);
            }
            catch
            {
                ;
            }
        }
    }
}