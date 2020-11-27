using PoliNetworkBot_CSharp.Code.Objects;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using System.Linq;
using PoliNetworkBot_CSharp.Code.Utils;
using Telegram.Bot.Types.ReplyMarkups;

namespace PoliNetworkBot_CSharp.Code.Bots.Anon
{
    internal class MainAnon
    {
        internal static void MainMethod(object sender, MessageEventArgs e)
        {
            var t = new Thread(() => _ = MainMethod2(sender, e));
            t.Start();
        }

        private static async Task MainMethod2(object sender, MessageEventArgs e)
        {
            ;

            if (e.Message == null)
                return;

            if (e.Message.Chat.Type != Telegram.Bot.Types.Enums.ChatType.Private)
            {
                return;
            }

            TelegramBotClient telegramBotClient = null;
            if (sender is TelegramBotClient t2)
            {
                telegramBotClient = t2;
            }
            else
            {
                return;
            }

            PoliNetworkBot_CSharp.Code.Objects.TelegramBotAbstract telegramBotAbstract = Objects.TelegramBotAbstract.GetFromRam(telegramBotClient);

            if (string.IsNullOrEmpty(e.Message.Text))
            {
                await DetectMessageAsync(telegramBotAbstract, e);
                return;
            }

          

            string textLower = e.Message.Text.ToLower();
            if (textLower.StartsWith("/"))
            {
                switch (textLower)
                {
                    case "/start":
                        {
                            await startMessageAsync(telegramBotAbstract, e);
                            return;
                        }

                    case "/help":
                        {
                            await helpMessageAsync(telegramBotAbstract, e);
                            return;
                        }
                    default:
                        {
                            ;
                            return;
                        }
                }
            }
            else
            {
                await DetectMessageAsync(telegramBotAbstract, e);
                return;
            }

            ;
        }

        private static async Task DetectMessageAsync(TelegramBotAbstract telegramBotAbstract, MessageEventArgs e)
        {
            ;

            long botId = telegramBotAbstract.GetId();

            if (AskUser.UserAnswers.ContainsUser(e.Message.From.Id, botId))
                if (AskUser.UserAnswers.GetState(e.Message.From.Id, botId) == AnswerTelegram.State.WAITING_FOR_ANSWER)
                {
                    AskUser.UserAnswers.RecordAnswer(e.Message.From.Id, botId, e.Message.Text);
                    return;
                }

            Language question = new Language(dict: new Dictionary<string, string>() {
                { "it", "Vuoi postare questo messaggio?"},
                { "en", "Do you want to post this message?"}
            });

            Language l1 = new Language(dict: new Dictionary<string, string>() { { "it", "Si" } });
            Language l2 = new Language(dict: new Dictionary<string, string>() { { "it", "No" } });
            List<List<Language>> options = new List<List<Language>>
            {
                new List<Language>()
                {
                    l1, l2
                }
            };

            var r = await AskUser.AskBetweenRangeAsync(e.Message.From.Id, question, telegramBotAbstract, e.Message.From.LanguageCode, options, e.Message.From.Username, true, e.Message.MessageId);
            if (l1.Matches(r))
            {
                //yes
                await AskIdentityForMessageToSend(telegramBotAbstract, e);
                return;
            }
            else
            {
                Language l3 = new Language(dict: new Dictionary<string, string>() {
                    { "it", "Va bene. Se ti serve aiuto usa /help"},
                    { "en", "Ok. If you need any help, use /help"}
                });
                await telegramBotAbstract.SendTextMessageAsync(e.Message.From.Id, l3, Telegram.Bot.Types.Enums.ChatType.Private, e.Message.From.LanguageCode, Telegram.Bot.Types.Enums.ParseMode.Html, null, e.Message.From.Username);
                return;
            }
        }

        private static async Task AskIdentityForMessageToSend(TelegramBotAbstract telegramBotAbstract, MessageEventArgs e)
        {
            Language question = new Language(dict: new Dictionary<string, string>() {
                {
                    "it",
                    "Quale identità vuoi usare? Ti ricordiamo che sono tutte anonime, con pseudonimi collegati all'identità che hai scelto. " +
                    "Se te ne servono altre, puoi digitare manualmente il numero che desideri"
                }
            });

            Language l1 = new Language(dict: new Dictionary<string, string>() { { "it", "DEFAULT (0)" } });

            
            List<List<Language>> options = new List<List<Language>>
            {
                new List<Language>()
                {
                    l1
                }
            };

            List<Language> l2 = new List<Language>();
            for (int i=1; i<= 8; i++)
            {
                l2.Add(new Language(dict:new Dictionary<string, string> {
                    {"it", i.ToString() }
                }));
            }
            List<List<Language>> x1 = Utils.KeyboardMarkup.ArrayToMatrixString(l2);
            options.AddRange(x1);

            ;

            var r = await Utils.AskUser.AskBetweenRangeAsync(e.Message.From.Id, question, telegramBotAbstract, e.Message.From.LanguageCode, options, e.Message.From.Username, true, e.Message.MessageId);

            int? chosen = GetIdentityFromReply(r);
            if (chosen == null)
            {
                Language l3 = new Language(dict: new Dictionary<string, string>() {
                    {"it", "L'identità non è stata riconosciuta. Operazione annullata" }
                });
                await telegramBotAbstract.SendTextMessageAsync(e.Message.From.Id, l3, 
                                Telegram.Bot.Types.Enums.ChatType.Private, e.Message.From.LanguageCode,
                                Telegram.Bot.Types.Enums.ParseMode.Html, new ReplyMarkupObject(Enums.ReplyMarkupEnum.REMOVE), e.Message.From.Username);
                return;
            }

            await PlaceMessageInQueue(telegramBotAbstract, e, identity: chosen.Value);
        }

        internal static void CallbackMethod(object sender, CallbackQueryEventArgs e)
        {
            ;

            Thread thread = new Thread(() => _ = CallbackMethod3Async(sender, e) );
            thread.Start();        
        }

        private static async Task CallbackMethod3Async(object sender, CallbackQueryEventArgs e)
        {
            TelegramBotClient s3 = null;
            if (sender is TelegramBotClient s2)
            {
                s3 = s2;
            }
            else
            {
                return;
            }

            TelegramBotAbstract telegramBotAbstract = TelegramBotAbstract.GetFromRam(s3);

            try
            {
                CallBackDataAnon x = new CallBackDataAnon(e.CallbackQuery.Data);
                await CallbackMethod2Async(telegramBotAbstract, e, x);
            }
            catch
            {
                return;
            }         
        }

        private static async Task CallbackMethod2Async(TelegramBotAbstract telegramBotAbstract, CallbackQueryEventArgs e, CallBackDataAnon x)
        {
            try
            {
                InlineKeyboardButton inlineKeyboard = new InlineKeyboardButton() { Text = "-", CallbackData = "-" };
                InlineKeyboardMarkup replyMarkup = new InlineKeyboardMarkup(inlineKeyboard);


                if (x.messageId != null)
                {
                    await telegramBotAbstract.EditText(ConfigAnon.ModAnonCheckGroup, x.messageId.Value + 1, "Hai scelto ["+x.ResultQueueEnum?.ToString()+"]");
                }
            }
            catch (Exception e1)
            {
                ;
            }

            ;

            
            switch (x.ResultQueueEnum)
            {
                case ResultQueueEnum.APPROVED_MAIN:
                    {
                        if (x.userId == null)
                            return;

                        Language t1 = new Language(dict: new Dictionary<string, string>() {
                            {"it", "Il tuo post è stato approvato! Congratulazioni!" }
                        });
                        await telegramBotAbstract.SendTextMessageAsync(x.userId.Value, t1, Telegram.Bot.Types.Enums.ChatType.Private, 
                            x.langUser, Telegram.Bot.Types.Enums.ParseMode.Html, new ReplyMarkupObject(Enums.ReplyMarkupEnum.REMOVE), x.username, null);
                        break;
                    }
                case ResultQueueEnum.GO_TO_UNCENSORED:
                    {
                        if (x.userId == null)
                            return;

                        Language t1 = new Language(dict: new Dictionary<string, string>() {
                            {"it", "Il tuo post è stato messo nella zona uncensored!" }
                        });
                        await telegramBotAbstract.SendTextMessageAsync(x.userId.Value, t1, Telegram.Bot.Types.Enums.ChatType.Private,
                            x.langUser, Telegram.Bot.Types.Enums.ParseMode.Html, new ReplyMarkupObject(Enums.ReplyMarkupEnum.REMOVE), x.username, null);
                        break;
                    }
                 
                case ResultQueueEnum.DELETE:
                    {
                        if (x.userId == null)
                            return;

                        Language t1 = new Language(dict: new Dictionary<string, string>() {
                            {"it", "Il tuo post è stato rifiutato!" }
                        });
                        await telegramBotAbstract.SendTextMessageAsync(x.userId.Value, t1, Telegram.Bot.Types.Enums.ChatType.Private,
                            x.langUser, Telegram.Bot.Types.Enums.ParseMode.Html, new ReplyMarkupObject(Enums.ReplyMarkupEnum.REMOVE), x.username, null);
                        break;
                    }
                default:
                    {
                        //todo: error
                        return;
                    }
            }
        }

        private static async Task PlaceMessageInQueue(TelegramBotAbstract telegramBotAbstract, MessageEventArgs e, int identity)
        {
            ;
            //todo: place message in queue

            MessageSentResult x = await telegramBotAbstract.ForwardMessageAsync(e.Message.MessageId, e.Message.From.Id, Anon.ConfigAnon.ModAnonCheckGroup);

            Language language = new Language(dict: new Dictionary<string, string>()
            {
                { "it", "Approvare? Identità ["+identity+"]"}
            });

            List<List<InlineKeyboardButton>> x2 = new List<List<InlineKeyboardButton>>();
            List<InlineKeyboardButton> x3 = new List<InlineKeyboardButton>() {
                new InlineKeyboardButton() { Text = "Si", CallbackData = 
                FormatDataCallBack(ResultQueueEnum.APPROVED_MAIN, x.GetMessageID(), e.Message.From.Id, identity, e.Message.From.LanguageCode, e.Message.From.Username) },

                new InlineKeyboardButton() { Text = "No", CallbackData =
                FormatDataCallBack(ResultQueueEnum.DELETE, x.GetMessageID(), e.Message.From.Id, identity, e.Message.From.LanguageCode, e.Message.From.Username)}
            };
            List<InlineKeyboardButton> x4 = new List<InlineKeyboardButton>() {
                new InlineKeyboardButton() { Text = "Uncensored", CallbackData =
                FormatDataCallBack( ResultQueueEnum.GO_TO_UNCENSORED, x.GetMessageID(), e.Message.From.Id, identity, e.Message.From.LanguageCode, e.Message.From.Username) }
            }; ;

            x2.Add(x3);   
            x2.Add(x4);
            InlineKeyboardMarkup inlineKeyboardMarkup = new InlineKeyboardMarkup(x2);
            ReplyMarkupObject r = new ReplyMarkupObject(inlineKeyboardMarkup);
            await telegramBotAbstract.SendTextMessageAsync(Anon.ConfigAnon.ModAnonCheckGroup, language, 
                Telegram.Bot.Types.Enums.ChatType.Group, "it", Telegram.Bot.Types.Enums.ParseMode.Html, r, null, x.GetMessageID());
        }

        
        private static string FormatDataCallBack(ResultQueueEnum v, long? messageId, int userId, int identity, string langcode, string username)
        {
            // split
            string r = "";
            switch (v)
            {
                case ResultQueueEnum.APPROVED_MAIN:
                    {
                        r += "a";
                        break;
                    }
                case ResultQueueEnum.GO_TO_UNCENSORED:
                    {
                        r += "u";
                        break;
                    }
                case ResultQueueEnum.DELETE:
                    {
                        r += "d";
                        break;
                    }
            }

            r += Anon.ConfigAnon.splitCallback;
            r += (messageId == null ? "null" : messageId.Value.ToString());
            r += Anon.ConfigAnon.splitCallback;
            r += userId;
            r += Anon.ConfigAnon.splitCallback;
            r += identity;
            r += Anon.ConfigAnon.splitCallback;
            r += langcode;
            r += Anon.ConfigAnon.splitCallback;
            r += username;

            return r;
        }

        private static int? GetIdentityFromReply(string r)
        {
            if (string.IsNullOrEmpty(r))
                return null;

            string r2 = r.ToLower();
            if (r2.StartsWith("default"))
                return 0;

            try
            {
                int x = Convert.ToInt32(r);
                if (x >= 0)
                    return x;
            }
            catch
            {
                ;
            }

            return null;
        }

        private static async Task helpMessageAsync(TelegramBotAbstract telegramBotAbstract, MessageEventArgs e)
        {
            Language text = new Language(dict: new System.Collections.Generic.Dictionary<string, string>() {
                {"it", "[UPDATE!] Ora basta inviare il messaggio al bot. Il bot chiederà il resto.\n\n" +
                "Scrivi il messaggio che vuoi inviare.\n" +
                "Rispondi a quel messaggio con /anon per richiederne la pubblicazione sul canale @PoliAnoniMi.\n\n" +
                "Devi indicare un'identità con la quale vuoi postare, 0 per identità nascosta.\n\n" +
                "Esempio:\n" +
                "/anon 1 [eventuale link del messaggio del canale a cui rispondere]\n" +
                "Per inviare un messaggio con la propria identità anonima 1\n\n" +
                "/anon 0 [eventuale link]\n" +
                "Per inviare un messaggio con identità nascosta.\n\n" +
                "In entrambi i casi(sia che si usi 0 come identità o un altro numero) nessun iscritto al canale sarà in grado di capire chi siete.\n" +
                "L'identità è stata introdotta per permettere a delle persone di scrivere sotto uno pseudonimo fisso, se lo desiderano.\n\n" +
                "Buon divertimento con questa funzione del nostro bot 😄!\n\n" +
                "Se dovesse esserci qualsiasi problema, scriveteci alla pagina Facebook di PoliNetwork" }

            });
            await telegramBotAbstract.SendTextMessageAsync(e.Message.From.Id, text, Telegram.Bot.Types.Enums.ChatType.Private,
                e.Message.From.LanguageCode, Telegram.Bot.Types.Enums.ParseMode.Html, null, e.Message.From.Username, null);
        }

        private static async Task startMessageAsync(TelegramBotAbstract sender, MessageEventArgs e)
        {
            Language text = new Language(dict: new System.Collections.Generic.Dictionary<string, string>() {
                {"it", "Ciao! 👋\n\n" +
                "Scrivi /help per la lista completa delle mie funzioni 👀\n\n" +
                "Visita anche il nostro sito https://polinetwork.github.io" }
            
            });
            await sender.SendTextMessageAsync(e.Message.From.Id, text, Telegram.Bot.Types.Enums.ChatType.Private,
                e.Message.From.LanguageCode, Telegram.Bot.Types.Enums.ParseMode.Html, null, e.Message.From.Username, null);
        }
    }
}