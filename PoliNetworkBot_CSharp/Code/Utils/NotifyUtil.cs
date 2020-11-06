using PoliNetworkBot_CSharp.Code.Objects;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PoliNetworkBot_CSharp.Code.Utils
{
    internal class NotifyUtil
    {
        const long group_exception = -438352042;
        const string default_lang = "en";

        internal static async System.Threading.Tasks.Task NotifyOwners(ExceptionNumbered exception, 
            TelegramBotAbstract sender, int v = 0, string extrainfo = null, string langCode = default_lang,
            long ? replyToMessageId2 = null)
        {
            if (sender == null)
                return;

            string message = null;

            try
            {
                message = exception.Message + "\n\n" + exception.GetException().ToString() + "\n\n" + exception.StackTrace.ToString();
                if (!string.IsNullOrEmpty(extrainfo))
                {
                    message += "\n\n" + extrainfo;
                }
            }
            catch (Exception e1)
            {
                message = "Error in sending exception: this exception occurred:\n\n" + e1.Message;
                return;
            }

            Language text = new Language(dict: new Dictionary<string, string>() {
                    {"it", "Eccezione! " + message },
                    {"en", "Exception! " + message }
                });

            MessageSend r1 = await NotifyOwners2Async(text, sender, v, langCode, replyToMessageId2);
            if (r1 == null)
                return;

            if (r1.IsSuccess())
            {
                int v2 = exception.GetNumberOfTimes();
                if (v <= 1)
                {
                    return;
                }

                string message2 = v2.ToString();

                Language text2 = new Language(dict: new Dictionary<string, string>() {
                    {"it", "Numero di volte: " + message2 },
                    {"en", "Number of times: " + message2 }
                });

                long? replyToMessageId = r1.GetMessageID();

                await NotifyOwners3(text2, sender, replyToMessageId, v, langCode);
            }
        }

        private static async Task<MessageSend> NotifyOwners3(Language text2, TelegramBotAbstract sender, long? replyToMessageId, int v, string langCode)
        {
            return await SendMessage.SendMessageInAGroup(sender, langCode, text2, group_exception,
                    Telegram.Bot.Types.Enums.ChatType.Group, Telegram.Bot.Types.Enums.ParseMode.Html, replyToMessageId: replyToMessageId, true, v);
        }

        internal static async Task NotifyOwners(Exception e, TelegramBotAbstract telegramBotAbstract, int v = 0)
        {
            await NotifyOwners(new ExceptionNumbered(e), telegramBotAbstract, v);
        }

        private static async Task<MessageSend> NotifyOwners2Async(Language text, TelegramBotAbstract sender, int v, string langCode, long? replyto)
        {
            return await NotifyOwners3(text, sender, replyto, v, langCode );
        }

        internal static async System.Threading.Tasks.Task NotifyIfFalseAsync(Tuple<bool?, string, long> r1, string extraInfo, TelegramBotAbstract sender)
        {
            if (r1 == null)
                return;

            if (r1.Item1 == null)
                return;

            if (r1.Item1.Value)
                return;

            string error = "Error (notifyIfFalse): ";
            error += "\n";
            error += "String: " + r1?.Item2 + "\n";
            error += "Long: " + r1.Item3.ToString() + "\n";
            error += "Extra: " + extraInfo;
            error += "\n";

            ExceptionNumbered exception = new ExceptionNumbered(error);
            await NotifyOwners(exception, sender, 0);
        }

        internal static async Task NotifyOwners(Exception item2, string message, TelegramBotAbstract sender, string langCode, long? replyToMessageId = null)
        {
            System.Collections.Generic.Dictionary<string, string> dict = new System.Collections.Generic.Dictionary<string, string>() {
                { "en", message}
            };
            Language text = new Language(dict: dict);
            await NotifyOwners2Async(text, sender, 0, langCode, replyToMessageId);
        }

        internal static async Task NotifyOwnersAsync(Tuple<List<ExceptionNumbered>, int> exceptions, TelegramBotAbstract sender, string v, string langCode, long? replyToMessageId = null)
        {
            MessageSend m = null;
            try
            {
                Language text = new Language(dict: new Dictionary<string, string>() {
                { "en", v }
                });
                m =  await NotifyOwners2Async(text, sender, 0, langCode, replyToMessageId);
            }
            catch
            {
                ;
            }

            try
            {
                Language text = new Language(dict: new Dictionary<string, string>() {
                { "en", "Number of exceptions: " + exceptions.Item2 + " - " + exceptions.Item1.Count }
                });
                _ = await NotifyOwners2Async(text, sender, 0, langCode, replyToMessageId);
            }
            catch
            {
                ;
            }

            try
            {
                foreach (ExceptionNumbered e1 in exceptions.Item1)
                {
                    try
                    {
                        await NotifyOwners(e1, sender, 0);
                    }
                    catch
                    {
                        ;
                    }
                }
            }
            catch
            {
                ;
            }


            try
            {
                Language text2 = new Language(dict: new Dictionary<string, string>() {
                { "en", "---End---"}
                });

                long? replyto = null; ;

                if (m!= null)
                {
                    replyto = m.GetMessageID();
                }
                await NotifyOwners2Async(text2, sender, 0, langCode, replyto);
            }
            catch
            {
                ;
            }
        }
    }
}