#region

using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Utils;
using PoliNetworkBot_CSharp.Code.Utils.CallbackUtils;
using PoliNetworkBot_CSharp.Code.Utils.Logger;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

#endregion

namespace PoliNetworkBot_CSharp.Code.Bots.Anon;

internal static class MainAnon
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

        if (e.Message.Chat.Type != ChatType.Private) return;

        TelegramBotClient telegramBotClient = null;
        if (sender is TelegramBotClient t2)
            telegramBotClient = t2;
        else
            return;

        var telegramBotAbstract = TelegramBotAbstract.GetFromRam(telegramBotClient);

        if (string.IsNullOrEmpty(e.Message.Text))
        {
            await DetectMessageAsync(telegramBotAbstract, e);
            return;
        }

        var textLower = e.Message.Text.ToLower();
        if (textLower.StartsWith("/"))
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

        await DetectMessageAsync(telegramBotAbstract, e);

        ;
    }

    private static async Task DetectMessageAsync(TelegramBotAbstract telegramBotAbstract, MessageEventArgs e)
    {
        ;

        var botId = telegramBotAbstract.GetId();

        if (AskUser.UserAnswers.ContainsUser(e.Message.From?.Id, botId))
            if (AskUser.UserAnswers.GetState(e.Message.From?.Id, botId) == AnswerTelegram.State.WAITING_FOR_ANSWER)
            {
                AskUser.UserAnswers.RecordAnswer(e.Message.From?.Id, botId, e.Message.Text);
                return;
            }

        var question = new Language(new Dictionary<string, string>
        {
            { "it", "Vuoi postare questo messaggio?" },
            { "en", "Do you want to post this message?" }
        });

        var l1 = new Language(new Dictionary<string, string> { { "it", "Si" } });
        var l2 = new Language(new Dictionary<string, string> { { "it", "No" } });
        var options = new List<List<Language>>
        {
            new()
            {
                l1, l2
            }
        };

        var r = await AskUser.AskBetweenRangeAsync(e.Message.From.Id, question, telegramBotAbstract,
            e.Message.From.LanguageCode, options, e.Message.From.Username, true, e.Message.MessageId);
        if (l1.Matches(r))
        {
            //yes
            await AskIdentityForMessageToSend2(telegramBotAbstract, e);
            return;
        }

        var l3 = new Language(new Dictionary<string, string>
        {
            { "it", "Va bene. Se ti serve aiuto usa /help" },
            { "en", "Ok. If you need any help, use /help" }
        });
        await telegramBotAbstract.SendTextMessageAsync(e.Message.From.Id, l3, ChatType.Private,
            e.Message.From.LanguageCode, ParseMode.Html, null, e.Message.From.Username);
    }

    private static async Task AskIdentityForMessageToSend2(TelegramBotAbstract telegramBotAbstract,
        MessageEventArgs e)
    {
        var question = new Language(new Dictionary<string, string>
        {
            {
                "it",
                "Vuoi essere completamente anonimo o usare uno dei nostri pseudonimi per te riservati? (non sarai riconoscibile in nessun modo, " +
                "tuttavia sarà chiaro a chi legge che l'autore è sempre lo stesso, se scegli lo stesso pseudonimo)"
            }
        });

        var l1 = new Language(new Dictionary<string, string> { { "it", "Anonimo" } });
        var l2 = new Language(new Dictionary<string, string> { { "it", "Pseudonimo" } });

        var options = new List<List<Language>>
        {
            new()
            {
                l1, l2
            }
        };

        var r = await AskUser.AskBetweenRangeAsync(e.Message.From?.Id, question, telegramBotAbstract,
            e.Message.From?.LanguageCode, options,
            e.Message.From?.Username, true, e.Message.MessageId);

        if (l1.Matches(r))
        {
            await AskForMessageToReplyTo(telegramBotAbstract, e, 0);

            return;
        }

        await AskIdentityForMessageToSend(telegramBotAbstract, e);
    }

    private static async Task AskForMessageToReplyTo(TelegramBotAbstract telegramBotAbstract, MessageEventArgs e,
        long identity)
    {
        var question = new Language(new Dictionary<string, string>
        {
            {
                "it",
                "Vuoi rispondere ad un messaggio già pubblicato sul canale principale o uncensored?"
            }
        });

        var r = await AskUser.AskYesNo(e.Message.From?.Id, question, false, telegramBotAbstract,
            e.Message.From?.LanguageCode, e.Message.From?.Username);

        if (r == false)
        {
            await PlaceMessageInQueue(telegramBotAbstract, new MessaggeAnonToSendInQueue(e), 0, null);
            return;
        }

        await AskForMessageToReplyTo2(telegramBotAbstract, e, identity);
    }

    private static async Task AskForMessageToReplyTo2(TelegramBotAbstract telegramBotAbstract, MessageEventArgs e,
        long identity)
    {
        //ask link

        var question = new Language(new Dictionary<string, string>
        {
            { "it", "Inserisci il link del messaggio a cui vuoi rispondere" }
        });
        var r = await AskUser.AskAsync(e.Message.From?.Id, question, telegramBotAbstract,
            e.Message.From?.LanguageCode, e.Message.From?.Username);
        var tuple = GetMessageReply(r);
        if (r == null)
        {
            var l2 = new Language(new Dictionary<string, string>
            {
                {
                    "it",
                    "Errore, non siamo riusciti a comprendere il link che hai inviato\n" +
                    "Operazione annullata."
                }
            });
        }

        await PlaceMessageInQueue(telegramBotAbstract, new MessaggeAnonToSendInQueue(e), identity, tuple.Item1);
    }

    private static Tuple<int?, ResultQueueEnum?> GetMessageReply(string r)
    {
        if (string.IsNullOrEmpty(r)) return null;

        if (!r.Contains('/'))
            return null;

        r = r.Trim();

        var r2 = r.Split(r);

        int? f1 = null;
        try
        {
            f1 = Convert.ToInt32(r2[^1]);
        }
        catch
        {
            ;
        }

        var f2 = GetMessageReply2(r2);

        if (f1 == null || f2 == null)
            return null;

        return new Tuple<int?, ResultQueueEnum?>(f1, f2);
    }

    private static ResultQueueEnum? GetMessageReply2(IReadOnlyList<string> r2)
    {
        if (r2.Count <= 1) return null;
        if (r2[^2] == ConfigAnon.WhereToPublishAnonMain.ToString())
            return ResultQueueEnum.APPROVED_MAIN;
        if (r2[^2] == ConfigAnon.WhereToPublishAnonUncensored.ToString())
            return ResultQueueEnum.GO_TO_UNCENSORED;

        return null;
    }

    private static async Task AskIdentityForMessageToSend(TelegramBotAbstract telegramBotAbstract,
        MessageEventArgs e)
    {
        var question = new Language(new Dictionary<string, string>
        {
            {
                "it",
                "Quale identità vuoi usare? Ti ricordiamo che sono tutte anonime, con pseudonimi collegati all'identità che hai scelto. " +
                "Se te ne servono altre, puoi digitare manualmente il numero che desideri\n" +
                "I numeri consentiti vanno da 1 a 2147483647"
            }
        });

        var options = new List<List<Language>>();

        var l2 = new List<Language>();
        for (var i = 1; i <= 8; i++)
            l2.Add(new Language(new Dictionary<string, string>
            {
                { "it", i.ToString() }
            }));
        var x1 = KeyboardMarkup.ArrayToMatrixString(l2);
        options.AddRange(x1);

        ;

        var r = await AskUser.AskBetweenRangeAsync(e.Message.From?.Id, question, telegramBotAbstract,
            e.Message.From?.LanguageCode, options, e.Message.From?.Username,
            true, e.Message.MessageId);

        var chosen = GetIdentityFromReply(r);
        if (chosen == null)
        {
            var l3 = new Language(new Dictionary<string, string>
            {
                { "it", "L'identità non è stata riconosciuta. Operazione annullata" }
            });
            await telegramBotAbstract.SendTextMessageAsync(e.Message.From.Id, l3,
                ChatType.Private, e.Message.From.LanguageCode,
                ParseMode.Html, new ReplyMarkupObject(ReplyMarkupEnum.REMOVE), e.Message.From.Username);
            return;
        }

        await AskForMessageToReplyTo(telegramBotAbstract, e, chosen.Value);
    }

    private static async Task CallbackMethod2Async(CallbackGenericData cb)
    {
        if (cb is not CallBackDataAnon dataAnon) return;

        var e = new CallbackQueryEventArgs(dataAnon.CallBackQueryFromTelegram);
        var telegramBotAbstract = dataAnon.Bot;

        try
        {
            var inlineKeyboard = new InlineKeyboardButton("-") { CallbackData = "-" };
            var replyMarkup = new InlineKeyboardMarkup(inlineKeyboard);

            if (dataAnon.CallBackQueryFromTelegram.Message != null)
                await dataAnon.Bot.EditText(ConfigAnon.ModAnonCheckGroup,
                    dataAnon.CallBackQueryFromTelegram.Message.MessageId,
                    "Hai scelto [" + dataAnon.GetResultEnum() + "]");
        }
        catch (Exception e1)
        {
            ;
            Logger.WriteLine(e1.Message);
        }

        ;

        switch (dataAnon.GetResultEnum())
        {
            case ResultQueueEnum.APPROVED_MAIN:
                {
                    if (dataAnon.authorId == null)
                        return;

                    var link = "";

                    try
                    {
                        var messageSentResult = await SendMessageToChannel(dataAnon.Bot, e, dataAnon);
                        if (messageSentResult != null)
                            link = messageSentResult.GetLink(ConfigAnon.WhereToPublishAnonMain.ToString(), true);
                    }
                    catch
                    {
                        ;
                    }

                    if (dataAnon.from_telegram != null && dataAnon.from_telegram.Value)
                    {
                        var t1 = new Language(new Dictionary<string, string>
                    {
                        { "it", "Il tuo post è stato approvato! Congratulazioni! " + link }
                    });
                        await telegramBotAbstract.SendTextMessageAsync(dataAnon.authorId.Value, t1, ChatType.Private,
                            dataAnon.langUser, ParseMode.Html, new ReplyMarkupObject(ReplyMarkupEnum.REMOVE),
                            dataAnon.username,
                            dataAnon.messageIdUser);
                    }
                    else
                    {
                        await WebPost.SetApprovedStatusAsync(dataAnon);
                    }

                    break;
                }
            case ResultQueueEnum.GO_TO_UNCENSORED:
                {
                    if (dataAnon.authorId == null)
                        return;

                    var link = "";

                    try
                    {
                        var messageSentResult = await SendMessageToChannel(telegramBotAbstract, e, dataAnon);
                        if (messageSentResult != null)
                            link = messageSentResult.GetLink(ConfigAnon.WhereToPublishAnonUncensored.ToString(), true);
                    }
                    catch
                    {
                        ;
                    }

                    if (dataAnon.from_telegram != null && dataAnon.from_telegram.Value)
                    {
                        var t1 = new Language(new Dictionary<string, string>
                    {
                        { "it", "Il tuo post è stato messo nella zona uncensored! " + link }
                    });
                        await telegramBotAbstract.SendTextMessageAsync(dataAnon.authorId.Value, t1, ChatType.Private,
                            dataAnon.langUser, ParseMode.Html, new ReplyMarkupObject(ReplyMarkupEnum.REMOVE),
                            dataAnon.username,
                            dataAnon.messageIdUser);
                    }
                    else
                    {
                        await WebPost.SetApprovedStatusAsync(dataAnon);
                    }

                    break;
                }

            case ResultQueueEnum.DELETE:
                {
                    if (dataAnon.authorId == null)
                        return;

                    if (dataAnon.from_telegram != null && dataAnon.from_telegram.Value)
                    {
                        var t1 = new Language(new Dictionary<string, string>
                    {
                        { "it", "Il tuo post è stato rifiutato!" }
                    });
                        await telegramBotAbstract.SendTextMessageAsync(dataAnon.authorId.Value, t1, ChatType.Private,
                            dataAnon.langUser, ParseMode.Html, new ReplyMarkupObject(ReplyMarkupEnum.REMOVE),
                            dataAnon.username,
                            dataAnon.messageIdUser);
                    }
                    else
                    {
                        await WebPost.SetApprovedStatusAsync(dataAnon);
                    }

                    break;
                }
            default:
                {
                    //todo: error
                    return;
                }
        }
    }

    private static async Task<MessageSentResult> SendMessageToChannel(TelegramBotAbstract telegramBotAbstract,
        CallbackQueryEventArgs e, CallBackDataAnon x)
    {
        var r2 = e.CallbackQuery.Message?.ReplyToMessage; //todo: fill this with the message to send

        ;

        //var r = await telegramBotAbstract.ForwardMessageAsync((long)x.messageIdGroup.Value, ConfigAnon.ModAnonCheckGroup, x.resultQueueEnum == ResultQueueEnum.APPROVED_MAIN ? ConfigAnon.WhereToPublishAnonMain : ConfigAnon.WhereToPublishAnonUncensored);
        var r = await telegramBotAbstract.ForwardMessageAnonAsync(
            x.GetResultEnum() == ResultQueueEnum.APPROVED_MAIN
                ? ConfigAnon.WhereToPublishAnonMain
                : ConfigAnon.WhereToPublishAnonUncensored,
            r2, x.messageIdReplyTo);

        return r;
    }

    public static async Task<bool> PlaceMessageInQueue(TelegramBotAbstract telegramBotAbstract,
        MessaggeAnonToSendInQueue e, long identity, int? messageIdReplyTo)
    {
        ;

        long? m3 = null;
        MessageSentResult x = null;
        MessageSentResult m2 = null;

        try
        {
            var l4 = new Language(new Dictionary<string, string>
            {
                { "it", "Il tuo messaggio è stato correttamente messo in coda. Attendi risposta" }
            });

            ;

            if (e.FromTelegram())
                x = await telegramBotAbstract.ForwardMessageAnonAsync(ConfigAnon.ModAnonCheckGroup, e.GetMessage(),
                    null);
            else
                x = await e.SendMessageInQueueAsync(telegramBotAbstract);

            ;

            if (x == null && e.FromTelegram())
            {
                var l6 = new Language(new Dictionary<string, string>
                {
                    {
                        "it", "Non siamo riusciti a mettere il messaggio in coda!\n" +
                              "Prova con un tipo di messaggio più comune (testo, foto, video, ecc)\n\n" +
                              "Operazione annullata"
                    }
                });

                await telegramBotAbstract.SendTextMessageAsync(e.GetFromUserId(), l6, ChatType.Private,
                    e.GetLanguageCode(), ParseMode.Html, new ReplyMarkupObject(ReplyMarkupEnum.REMOVE),
                    e.GetUsername());

                return false;
            }

            if (e.FromTelegram())
                m2 = await telegramBotAbstract.SendTextMessageAsync(e.GetFromUserId(), l4,
                    ChatType.Group, "it", ParseMode.Html, new ReplyMarkupObject(ReplyMarkupEnum.REMOVE), null,
                    e.GetMessage().MessageId);

            m3 = m2?.GetMessageID();
        }
        catch (Exception e1)
        {
            Console.WriteLine(e1);
        }

        ;

        var language = new Language(new Dictionary<string, string>
        {
            { "it", "Approvare? Identità [" + identity + "]" }
        });

        List<CallbackOption> options = new()
        {
            new CallbackOption("Sì, principale", 0, ResultQueueEnum.APPROVED_MAIN),
            new CallbackOption("Sì, uncensored", 1, ResultQueueEnum.GO_TO_UNCENSORED),
            new CallbackOption("No, elimina", 2, ResultQueueEnum.DELETE)
        };
        CallBackDataAnon callBackDataAnon = new(options, cb => { _ = CallbackMethod2Async(cb); })
        {
            identity = identity,
            authorId = e.GetFromUserId(),
            langUser = e.GetLanguageCode(),
            username = e.GetUsername(),
            from_telegram = true,
            messageIdUser = e.GetMessage().MessageId,
            messageIdReplyTo = messageIdReplyTo
        };

        var m4 = await CallbackUtils.SendMessageWithCallbackQueryAsync(callBackDataAnon, ConfigAnon.ModAnonCheckGroup,
            language, telegramBotAbstract, ChatType.Group, "it", null, false, x?.GetMessageID());

        return m4 != null;
    }

    private static long? GetIdentityFromReply(string r)
    {
        if (string.IsNullOrEmpty(r))
            return null;

        var r2 = r.ToLower();
        if (r2.StartsWith("default"))
            return 0;

        try
        {
            var x = Convert.ToInt64(r);
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
        var text = new Language(new Dictionary<string, string>
        {
            {
                "it", "Scrivi il messaggio che vuoi inviare, il bot ti chiederà il resto.\n" +
                      "Per inviare un messaggio con identità nascosta.\n\n" +
                      "Nessun iscritto al canale sarà in grado di capire chi siete.\n" +
                      "L'identità è stata introdotta per permettere a delle persone di scrivere sotto uno pseudonimo fisso, se lo desiderano.\n\n" +
                      "Buon divertimento con questa funzione del nostro bot 😄!\n\n" +
                      "Se dovesse esserci qualsiasi problema, scriveteci alla pagina Facebook di PoliNetwork"
            }
        });
        await telegramBotAbstract.SendTextMessageAsync(e.Message.From.Id, text, ChatType.Private,
            e.Message.From.LanguageCode, ParseMode.Html, new ReplyMarkupObject(ReplyMarkupEnum.REMOVE),
            e.Message.From.Username);
    }

    private static async Task StartMessageAsync(TelegramBotAbstract sender, MessageEventArgs e)
    {
        var text = new Language(new Dictionary<string, string>
        {
            {
                "it", "Ciao! 👋\n\n" +
                      "Scrivi /help per la lista completa delle mie funzioni 👀\n\n" +
                      "Visita anche il nostro sito https://polinetwork.github.io"
            }
        });
        await sender.SendTextMessageAsync(e.Message.From.Id, text, ChatType.Private,
            e.Message.From.LanguageCode, ParseMode.Html, null, e.Message.From.Username);
    }
}