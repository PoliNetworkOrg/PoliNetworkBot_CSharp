using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Utils;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
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
                            await StartMessageAsync(telegramBotAbstract, e);
                            return;
                        }

                    case "/help":
                        {
                            await HelpMessageAsync(telegramBotAbstract, e);
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
                await AskIdentityForMessageToSend2(telegramBotAbstract, e);
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

        private static async Task AskIdentityForMessageToSend2(TelegramBotAbstract telegramBotAbstract, MessageEventArgs e)
        {
            Language question = new Language(dict: new Dictionary<string, string>() {
                {
                    "it",
                    "Vuoi essere completamente anonimo o usare uno dei nostri pseudonimi per te riservati? (non sarai riconoscibile in nessun modo, " +
                    "tuttavia sarà chiaro a chi legge che l'autore è sempre lo stesso, se scegli lo stesso pseudonimo)"
                }
            });

            Language l1 = new Language(dict: new Dictionary<string, string>() { { "it", "Anonimo" } });
            Language l2 = new Language(dict: new Dictionary<string, string>() { { "it", "Pseudonimo" } });

            List<List<Language>> options = new List<List<Language>>
            {
                new List<Language>()
                {
                    l1, l2
                }
            };

            var r = await Utils.AskUser.AskBetweenRangeAsync(e.Message.From.Id, question, telegramBotAbstract, e.Message.From.LanguageCode, options, e.Message.From.Username, true, e.Message.MessageId);

            if (l1.Matches(r))
            {
                await AskForMessageToReplyTo(telegramBotAbstract, e, identity: 0);

                return;
            }

            await AskIdentityForMessageToSend(telegramBotAbstract, e);
        }

        private static async Task AskForMessageToReplyTo(TelegramBotAbstract telegramBotAbstract, MessageEventArgs e, int identity)
        {
            Language question = new Language(dict: new Dictionary<string, string>() {
                {
                    "it",
                    "Vuoi rispondere ad un messaggio già pubblicato sul canale principale o uncensored?"
                }
            });

            bool r = await Utils.AskUser.AskYesNo(e.Message.From.Id, question, false, telegramBotAbstract, e.Message.From.LanguageCode, e.Message.From.Username);

            if (r == false)
            {
                await PlaceMessageInQueue(telegramBotAbstract, new MessaggeAnonToSendInQueue(e), identity: 0, messageIdReplyTo: null);
                return;
            }

            await AskForMessageToReplyTo2(telegramBotAbstract, e, identity);
        }

        private static async Task AskForMessageToReplyTo2(TelegramBotAbstract telegramBotAbstract, MessageEventArgs e, int identity)
        {
            //ask link

            Language question = new Language(dict: new Dictionary<string, string>() {
                {"it", "Inserisci il link del messaggio a cui vuoi rispondere" }
            });
            var r = await AskUser.AskAsync(e.Message.From.Id, question, telegramBotAbstract, e.Message.From.LanguageCode, e.Message.From.Username);
            Tuple<long?, Anon.ResultQueueEnum?> tuple = GetMessageReply(r);
            if (r == null)
            {
                Language l2 = new Language(dict: new Dictionary<string, string>() {
                    {
                        "it",
                        "Errore, non siamo riusciti a comprendere il link che hai inviato\n" +
                        "Operazione annullata."
                    }
                });
            }

            await PlaceMessageInQueue(telegramBotAbstract, new MessaggeAnonToSendInQueue(e), identity, tuple);
        }

        private static Tuple<long?, ResultQueueEnum?> GetMessageReply(string r)
        {
            if (string.IsNullOrEmpty(r))
            {
                return null;
            }

            if (!r.Contains("/"))
                return null;

            r = r.Trim();

            var r2 = r.Split(r);

            long? f1 = null;
            try
            {
                f1 = Convert.ToInt64(r2[^1]);
            }
            catch
            {
                ;
            }

            Anon.ResultQueueEnum? f2 = GetMessageReply2(r, r2);

            if (f1 == null || f2 == null)
                return null;

            return new Tuple<long?, ResultQueueEnum?>(f1, f2);
        }

        private static ResultQueueEnum? GetMessageReply2(string r, string[] r2)
        {
            if (r2.Length > 1)
            {
                if (r2[^2] == ConfigAnon.WhereToPublishAnonMain.ToString())
                {
                    return ResultQueueEnum.APPROVED_MAIN;
                }
                else if (r2[^2] == ConfigAnon.WhereToPublishAnonUncensored.ToString())
                {
                    return ResultQueueEnum.GO_TO_UNCENSORED;
                }
            }

            return null;
        }

        private static async Task AskIdentityForMessageToSend(TelegramBotAbstract telegramBotAbstract, MessageEventArgs e)
        {
            Language question = new Language(dict: new Dictionary<string, string>() {
                {
                    "it",
                    "Quale identità vuoi usare? Ti ricordiamo che sono tutte anonime, con pseudonimi collegati all'identità che hai scelto. " +
                    "Se te ne servono altre, puoi digitare manualmente il numero che desideri\n" +
                    "I numeri consentiti vanno da 1 a 2147483647"
                }
            });

            List<List<Language>> options = new List<List<Language>>();

            List<Language> l2 = new List<Language>();
            for (int i = 1; i <= 8; i++)
            {
                l2.Add(new Language(dict: new Dictionary<string, string> {
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

            await AskForMessageToReplyTo(telegramBotAbstract, e, identity: chosen.Value);
        }

        internal static void CallbackMethod(object sender, CallbackQueryEventArgs e)
        {
            ;

            Thread thread = new Thread(() => _ = CallbackMethod3Async(sender, e));
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

                await telegramBotAbstract.EditText(ConfigAnon.ModAnonCheckGroup, (int)(e.CallbackQuery.Message.MessageId), "Hai scelto [" + x.resultQueueEnum?.ToString() + "]");
            }
            catch (Exception e1)
            {
                ;
                Console.WriteLine(e1.Message);
            }

            ;

            switch (x.resultQueueEnum)
            {
                case ResultQueueEnum.APPROVED_MAIN:
                    {
                        if (x.userId == null)
                            return;

                        string link = "";

                        try
                        {
                            MessageSentResult messageSentResult = await SendMessageToChannel(telegramBotAbstract, e, x);
                            if (messageSentResult != null)
                                link = messageSentResult.GetLink(ConfigAnon.WhereToPublishAnonMain.ToString(), true);
                        }
                        catch
                        {
                            ;
                        }

                        Language t1 = new Language(dict: new Dictionary<string, string>() {
                            {"it", "Il tuo post è stato approvato! Congratulazioni! " + link }
                        });
                        await telegramBotAbstract.SendTextMessageAsync(x.userId.Value, t1, Telegram.Bot.Types.Enums.ChatType.Private,
                            x.langUser, Telegram.Bot.Types.Enums.ParseMode.Html, new ReplyMarkupObject(Enums.ReplyMarkupEnum.REMOVE), x.username, x.messageIdUser);
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
                            x.langUser, Telegram.Bot.Types.Enums.ParseMode.Html, new ReplyMarkupObject(Enums.ReplyMarkupEnum.REMOVE), x.username, x.messageIdUser);
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
                            x.langUser, Telegram.Bot.Types.Enums.ParseMode.Html, new ReplyMarkupObject(Enums.ReplyMarkupEnum.REMOVE), x.username, x.messageIdUser);
                        break;
                    }
                default:
                    {
                        //todo: error
                        return;
                    }
            }
        }

        private static async Task<MessageSentResult> SendMessageToChannel(TelegramBotAbstract telegramBotAbstract, CallbackQueryEventArgs e, CallBackDataAnon x)
        {
            if (x.messageIdGroup != null)
            {
                Telegram.Bot.Types.Message r2 = e.CallbackQuery.Message.ReplyToMessage; //todo: fill this with the message to send

                ;

                //var r = await telegramBotAbstract.ForwardMessageAsync((int)x.messageIdGroup.Value, ConfigAnon.ModAnonCheckGroup, x.resultQueueEnum == ResultQueueEnum.APPROVED_MAIN ? ConfigAnon.WhereToPublishAnonMain : ConfigAnon.WhereToPublishAnonUncensored);
                var r = await telegramBotAbstract.ForwardMessageAnonAsync(
                    x.resultQueueEnum == ResultQueueEnum.APPROVED_MAIN ? ConfigAnon.WhereToPublishAnonMain : ConfigAnon.WhereToPublishAnonUncensored,
                    r2, messageIdToReplyToLong: x.messageIdToReplyTo);

                return r;
            }

            return null;
        }

        public static async Task<bool> PlaceMessageInQueue(TelegramBotAbstract telegramBotAbstract, MessaggeAnonToSendInQueue e, int identity, Tuple<long?, Anon.ResultQueueEnum?> messageIdReplyTo)
        {
            ;

            long? m3 = null;
            MessageSentResult x = null;
            MessageSentResult m2 = null;

            try
            {
                Language l4 = new Language(dict: new Dictionary<string, string>() {
                    {"it", "Il tuo messaggio è stato correttamente messo in coda. Attendi risposta" }
                });

                ;

                if (e.FromTelegram())
                {
                    x = await telegramBotAbstract.ForwardMessageAnonAsync(Anon.ConfigAnon.ModAnonCheckGroup, e.GetMessage(), null);
                }
                else
                {
                    x = await e.SendMessageInQueueAsync(telegramBotAbstract);
                }

                ;

                if (x == null && e.FromTelegram())
                {
                    Language l6 = new Language(dict: new Dictionary<string, string>()
                    {
                        {"it", "Non siamo riusciti a mettere il messaggio in coda!\n" +
                        "Prova con un tipo di messaggio più comune (testo, foto, video, ecc)\n\n" +
                        "Operazione annullata" }
                    });

                    await telegramBotAbstract.SendTextMessageAsync(e.GetFromUserId(), l6, Telegram.Bot.Types.Enums.ChatType.Private,
                        e.GetLanguageCode(), Telegram.Bot.Types.Enums.ParseMode.Default, new ReplyMarkupObject(Enums.ReplyMarkupEnum.REMOVE), e.GetUsername());

                    return false;
                }

                if (e.FromTelegram())
                {
                    m2 = await telegramBotAbstract.SendTextMessageAsync(e.GetFromUserId(), l4,
                        Telegram.Bot.Types.Enums.ChatType.Group, "it", Telegram.Bot.Types.Enums.ParseMode.Html, new ReplyMarkupObject(Enums.ReplyMarkupEnum.REMOVE), null, e.GetMessage().MessageId);
                }


                m3 = m2.GetMessageID();
            }
            catch (Exception e1)
            {
                ;
            }

            ;

            Language language = new Language(dict: new Dictionary<string, string>()
            {
                { "it", "Approvare? Identità ["+identity+"]"}
            });

            List<List<InlineKeyboardButton>> x2 = new List<List<InlineKeyboardButton>>();
            if (messageIdReplyTo == null || messageIdReplyTo.Item2 == null || messageIdReplyTo.Item2 == ResultQueueEnum.APPROVED_MAIN)
            {
                x2.Add(new List<InlineKeyboardButton>() {
                    new InlineKeyboardButton() { Text = "Si, principale", CallbackData =
                    FormatDataCallBack(ResultQueueEnum.APPROVED_MAIN,
                                       x.GetMessageID(),
                                       e.GetFromUserIdOrPostId(),
                                       identity,
                                       e.GetLanguageCode(),
                                       e.GetUsername(),
                                       m3,
                                       messageIdReplyTo: messageIdReplyTo,
                                       from_telegram: e.FromTelegram()) }
                });
            }
            if (messageIdReplyTo == null || messageIdReplyTo.Item2 == null || messageIdReplyTo.Item2 == ResultQueueEnum.GO_TO_UNCENSORED)
            {
                x2.Add(new List<InlineKeyboardButton>() {
                    new InlineKeyboardButton() { Text = "Si, uncensored", CallbackData =
                    FormatDataCallBack( ResultQueueEnum.GO_TO_UNCENSORED, x.GetMessageID(), e.GetFromUserIdOrPostId(), identity,
                    e.GetLanguageCode(), e.GetUsername(), m3, messageIdReplyTo: messageIdReplyTo, e.FromTelegram())}
                });
            }
            x2.Add(new List<InlineKeyboardButton>() {
                new InlineKeyboardButton() { Text = "No, elimina", CallbackData =
                FormatDataCallBack(ResultQueueEnum.DELETE, x.GetMessageID(), e.GetFromUserIdOrPostId(), identity,
                e.GetLanguageCode(), e.GetUsername(), m3, messageIdReplyTo: messageIdReplyTo, e.FromTelegram())}
            });
            InlineKeyboardMarkup inlineKeyboardMarkup = new InlineKeyboardMarkup(x2);
            ReplyMarkupObject r = new ReplyMarkupObject(inlineKeyboardMarkup);

            var m4 = await telegramBotAbstract.SendTextMessageAsync(Anon.ConfigAnon.ModAnonCheckGroup, language,
                Telegram.Bot.Types.Enums.ChatType.Group, "it", Telegram.Bot.Types.Enums.ParseMode.Html, r, null, x.GetMessageID());

            return m4 != null;
        }

        private static string FormatDataCallBack(ResultQueueEnum v, long? messageIdGroup, long? userId, int identity,
            string langcode, string username, long? messageIdUser, Tuple<long?, Anon.ResultQueueEnum?> messageIdReplyTo, bool from_telegram)
        {
            CallBackDataAnon callBackDataAnon = new CallBackDataAnon(v, messageIdGroup, userId, identity, langcode, username, messageIdUser, messageIdReplyTo, from_telegram);
            return callBackDataAnon.ToDataString();
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
                if (x >= 1)
                    return x;
            }
            catch
            {
                ;
            }

            return null;
        }

        private static async Task HelpMessageAsync(TelegramBotAbstract telegramBotAbstract, MessageEventArgs e)
        {
            Language text = new Language(dict: new System.Collections.Generic.Dictionary<string, string>() {
                {"it", "Scrivi il messaggio che vuoi inviare, il bot ti chiederà il resto.\n" +
                "Per inviare un messaggio con identità nascosta.\n\n" +
                "Nessun iscritto al canale sarà in grado di capire chi siete.\n" +
                "L'identità è stata introdotta per permettere a delle persone di scrivere sotto uno pseudonimo fisso, se lo desiderano.\n\n" +
                "Buon divertimento con questa funzione del nostro bot 😄!\n\n" +
                "Se dovesse esserci qualsiasi problema, scriveteci alla pagina Facebook di PoliNetwork" }
            });
            await telegramBotAbstract.SendTextMessageAsync(e.Message.From.Id, text, Telegram.Bot.Types.Enums.ChatType.Private,
                e.Message.From.LanguageCode, Telegram.Bot.Types.Enums.ParseMode.Html, new ReplyMarkupObject(Enums.ReplyMarkupEnum.REMOVE), e.Message.From.Username, null);
        }

        private static async Task StartMessageAsync(TelegramBotAbstract sender, MessageEventArgs e)
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