using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Objects;
using Telegram.Bot.Types.Enums;

namespace PoliNetworkBot_CSharp.Code.Utils
{
    internal class NotifyUtil
    {
        private const long group_exception = -438352042;
        private const long permitted_spam_group = 736428640;
        private const string default_lang = "en";

        internal static async Task NotifyOwnersPermittedSpam(string message, TelegramBotAbstract sender)
        {
            string langCode = "it";
            var text2 = new Language(new Dictionary<string, string>
            {
                {"it", message},
            });
            await SendMessage.SendMessageInAGroup(sender, langCode, text2, permitted_spam_group, ChatType.Group,
                ParseMode.Default, group_exception, false, 0);
        }
        internal static async Task NotifyOwners(ExceptionNumbered exception,
            TelegramBotAbstract sender, int v = 0, string extrainfo = null, string langCode = default_lang,
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

            var r1 = await NotifyOwners2Async(text, sender, v, langCode, replyToMessageId2);
            if (r1 == null)
                return;
        }

        private static async Task<MessageSentResult> NotifyOwners3(Language text2, TelegramBotAbstract sender,
            long? replyToMessageId, int v, string langCode)
        {
            return await SendMessage.SendMessageInAGroup(sender, langCode, text2, group_exception,
                ChatType.Group, ParseMode.Html, replyToMessageId, true, v);
        }

        internal static async Task NotifyOwners(Exception e, TelegramBotAbstract telegramBotAbstract, int v = 0)
        {
            await NotifyOwners(new ExceptionNumbered(e), telegramBotAbstract, v);
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