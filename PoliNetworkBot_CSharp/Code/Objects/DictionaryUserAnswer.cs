using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot.Types.Enums;

namespace PoliNetworkBot_CSharp.Code.Utils
{
    public class DictionaryUserAnswer
    {
        public Dictionary<long, Couple<AnswerTelegram, TaskCompletionSource<string>>> d;

        public DictionaryUserAnswer()
        {
            this.d = new Dictionary<long, Couple<AnswerTelegram, TaskCompletionSource<string>>>();
        }

        internal void Reset(long idUser)
        {
            if (!d.ContainsKey(idUser))
            {
                d[idUser] = new Couple<AnswerTelegram, TaskCompletionSource<string>>();
            }
            else if (d[idUser] == null)
            {
                d[idUser] = new Couple<AnswerTelegram, TaskCompletionSource<string>>();
            }

            d[idUser].Item1 = null;
            d[idUser].Item1 = new AnswerTelegram();
            d[idUser].Item1.Reset();
        }

        internal void Delete(long idUser)
        {
            d[idUser].Item1 = null;
            d[idUser].Item2 = null;
        }

        internal void SetAnswerProcessed(long idUser, bool v)
        {
            this.d[idUser].Item1.SetAnswerProcessed(v);
        }

        internal void AddWorkCompleted(long idUser, bool sendMessageConfirmationChoice, TelegramBotAbstract telegramBotAbstract, string lang, string username)
        {
            this.d[idUser].Item1.WorkCompleted += async result =>
            {
                bool crashed = true;
                try
                {
                    if (this.d[idUser].Item1.GetState() == AnswerTelegram.State.ANSWERED && this.d[idUser].Item1.GetAlreadyProcessedAnswer() == false)
                    {
                        if (sendMessageConfirmationChoice)
                        {
                            var replyMarkup = new ReplyMarkupObject(ReplyMarkupEnum.REMOVE);
                            var languageReply = new Language(new Dictionary<string, string>
                            {
                                {"en", "You choose [" + result + "]"},
                                {"it", "Hai scelto [" + result + "]"}
                            });
                            await telegramBotAbstract.SendTextMessageAsync(idUser,
                                languageReply,
                                ChatType.Private, lang, default, replyMarkup, username);
                        }

                        ;

                        this.d[idUser].Item1.SetAnswerProcessed(true);

                        var resultstring = result.ToString();
                        crashed = false;
                        var done = this.d[idUser].Item2.TrySetResult(resultstring);

                        ;
                    }
                }
                catch
                {
                    ;
                }

                if (crashed)
                {
                    this.d[idUser].Item2.TrySetResult("");
                }
            };
        }

        internal TaskCompletionSource<string> GetNewTCS(long idUser)
        {
            this.d[idUser].Item2 = new TaskCompletionSource<string>();
            return this.d[idUser].Item2;
        }

        internal bool ContainsUser(int userId)
        {
            return this.d.ContainsKey(userId);
        }

        internal AnswerTelegram.State? GetState(int id)
        {
            if (this.d[id] != null)
            {
                if (this.d[id].Item1 != null)
                    return this.d[id].Item1.GetState();
            }

            return null;
        }

        internal void RecordAnswer(int id, string text)
        {
            this.d[id].Item1.RecordAnswer(text);
        }
    }
}