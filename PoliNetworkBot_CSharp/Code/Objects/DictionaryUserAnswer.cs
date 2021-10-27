using System.Collections.Generic;
using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects;
using Telegram.Bot.Types.Enums;

namespace PoliNetworkBot_CSharp.Code.Utils
{
    public class DictionaryUserAnswer
    {
        public Dictionary<long, Dictionary<long, Couple<AnswerTelegram, TaskCompletionSource<string>>>> d;

        public DictionaryUserAnswer()
        {
            d = new Dictionary<long, Dictionary<long, Couple<AnswerTelegram, TaskCompletionSource<string>>>>();
        }

        internal void Reset(long idUser, long? botId)
        {
            if (!d.ContainsKey(idUser))
                d[idUser] = new Dictionary<long, Couple<AnswerTelegram, TaskCompletionSource<string>>>();

            if (d[idUser] == null)
                d[idUser] = new Dictionary<long, Couple<AnswerTelegram, TaskCompletionSource<string>>>();
            if (botId != null)
            {
                if (!d[idUser].ContainsKey(botId.Value))
                    d[idUser][botId.Value] = new Couple<AnswerTelegram, TaskCompletionSource<string>>();

                if (d[idUser][botId.Value] == null)
                    d[idUser][botId.Value] = new Couple<AnswerTelegram, TaskCompletionSource<string>>();

                d[idUser][botId.Value].Item1 = null;
                d[idUser][botId.Value].Item1 = new AnswerTelegram();
                d[idUser][botId.Value].Item1.Reset();
            }
        }

        internal void Delete(long idUser, long? botId)
        {
            if (botId != null)
            {
                d[idUser][botId.Value].Item1 = null;
                d[idUser][botId.Value].Item2 = null;
            }
        }

        internal void SetAnswerProcessed(long idUser, long? botId, bool v)
        {
            if (botId != null) d[idUser][botId.Value].Item1.SetAnswerProcessed(v);
        }

        internal void AddWorkCompleted(long idUser, long? botId, bool sendMessageConfirmationChoice,
            TelegramBotAbstract telegramBotAbstract, string lang, string username)
        {
            if (botId != null)
                d[idUser][botId.Value].Item1.WorkCompleted += async result =>
                {
                    var crashed = true;
                    try
                    {
                        if (d[idUser][botId.Value].Item1.GetState() == AnswerTelegram.State.ANSWERED &&
                            d[idUser][botId.Value].Item1.GetAlreadyProcessedAnswer() == false)
                        {
                            if (sendMessageConfirmationChoice)
                            {
                                var replyMarkup = new ReplyMarkupObject(ReplyMarkupEnum.REMOVE);
                                var languageReply = new Language(new Dictionary<string, string>
                                {
                                    {"en", "You chose [" + result + "]"},
                                    {"it", "Hai scelto [" + result + "]"}
                                });
                                await telegramBotAbstract.SendTextMessageAsync(idUser,
                                    languageReply,
                                    ChatType.Private, lang, default, replyMarkup, username);
                            }

                            ;

                            d[idUser][botId.Value].Item1.SetAnswerProcessed(true);

                            var resultstring = result.ToString();
                            crashed = false;
                            var done = d[idUser][botId.Value].Item2.TrySetResult(resultstring);

                            ;
                        }
                    }
                    catch
                    {
                        ;
                    }

                    if (crashed) d[idUser][botId.Value].Item2.TrySetResult("");
                };
        }

        internal TaskCompletionSource<string> GetNewTCS(long idUser, long? botId)
        {
            if (botId != null)
            {
                d[idUser][botId.Value].Item2 = new TaskCompletionSource<string>();
                return d[idUser][botId.Value].Item2;
            }

            return null;
        }

        internal bool ContainsUser(long userId, long? botId)
        {
            if (botId == null) return false;

            return d.ContainsKey(userId) && d[userId].ContainsKey(botId.Value);
        }

        internal AnswerTelegram.State? GetState(long userId, long? botId)
        {
            if (botId != null)
                if (d[userId][botId.Value] != null)
                    if (d[userId][botId.Value].Item1 != null)
                        return d[userId][botId.Value].Item1.GetState();
            return null;
        }

        internal void RecordAnswer(long userId, long? botId, string text)
        {
            if (botId == null) return;
            d[userId][botId.Value].Item1.RecordAnswer(text);
        }
    }
}