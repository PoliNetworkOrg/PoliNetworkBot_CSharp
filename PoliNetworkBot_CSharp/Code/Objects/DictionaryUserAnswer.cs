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

        internal void Reset(long idUser, long botId)
        {
            if (!d.ContainsKey(idUser))
                d[idUser] = new Dictionary<long, Couple<AnswerTelegram, TaskCompletionSource<string>>>();

            if (d[idUser] == null)
                d[idUser] = new Dictionary<long, Couple<AnswerTelegram, TaskCompletionSource<string>>>();

            if (!d[idUser].ContainsKey(botId))
                d[idUser][botId] = new Couple<AnswerTelegram, TaskCompletionSource<string>>();

            if (d[idUser][botId] == null) d[idUser][botId] = new Couple<AnswerTelegram, TaskCompletionSource<string>>();

            d[idUser][botId].Item1 = null;
            d[idUser][botId].Item1 = new AnswerTelegram();
            d[idUser][botId].Item1.Reset();
        }

        internal void Delete(long idUser, long botId)
        {
            d[idUser][botId].Item1 = null;
            d[idUser][botId].Item2 = null;
        }

        internal void SetAnswerProcessed(long idUser, long botId, bool v)
        {
            d[idUser][botId].Item1.SetAnswerProcessed(v);
        }

        internal void AddWorkCompleted(long idUser, long botId, bool sendMessageConfirmationChoice,
            TelegramBotAbstract telegramBotAbstract, string lang, string username)
        {
            d[idUser][botId].Item1.WorkCompleted += async result =>
            {
                var crashed = true;
                try
                {
                    if (d[idUser][botId].Item1.GetState() == AnswerTelegram.State.ANSWERED &&
                        d[idUser][botId].Item1.GetAlreadyProcessedAnswer() == false)
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

                        d[idUser][botId].Item1.SetAnswerProcessed(true);

                        var resultstring = result.ToString();
                        crashed = false;
                        var done = d[idUser][botId].Item2.TrySetResult(resultstring);

                        ;
                    }
                }
                catch
                {
                    ;
                }

                if (crashed) d[idUser][botId].Item2.TrySetResult("");
            };
        }

        internal TaskCompletionSource<string> GetNewTCS(long idUser, long botId)
        {
            d[idUser][botId].Item2 = new TaskCompletionSource<string>();
            return d[idUser][botId].Item2;
        }

        internal bool ContainsUser(int userId, long botId)
        {
            return d.ContainsKey(userId) ? d[userId].ContainsKey(botId) : false;
        }

        internal AnswerTelegram.State? GetState(int userId, long botId)
        {
            if (d[userId][botId] != null)
                if (d[userId][botId].Item1 != null)
                    return d[userId][botId].Item1.GetState();

            return null;
        }

        internal void RecordAnswer(int userId, long botId, string text)
        {
            d[userId][botId].Item1.RecordAnswer(text);
        }
    }
}