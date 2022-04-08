#region

using PoliNetworkBot_CSharp.Code.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot.Types.Enums;

#endregion

namespace PoliNetworkBot_CSharp.Code.Objects;

public class DictionaryUserAnswer
{
    private readonly Dictionary<long, Dictionary<long, Couple<AnswerTelegram, TaskCompletionSource<string>>>> d;

    public DictionaryUserAnswer()
    {
        d = new Dictionary<long, Dictionary<long, Couple<AnswerTelegram, TaskCompletionSource<string>>>>();
    }

    internal void Reset(long? idUser, long? botId)
    {
        if (idUser == null)
            return;

        if (!d.ContainsKey(idUser.Value))
            d[idUser.Value] = new Dictionary<long, Couple<AnswerTelegram, TaskCompletionSource<string>>>();

        d[idUser.Value] ??= new Dictionary<long, Couple<AnswerTelegram, TaskCompletionSource<string>>>();
        if (botId == null) return;
        if (!d[idUser.Value].ContainsKey(botId.Value))
            d[idUser.Value][botId.Value] = new Couple<AnswerTelegram, TaskCompletionSource<string>>();

        d[idUser.Value][botId.Value] ??= new Couple<AnswerTelegram, TaskCompletionSource<string>>();

        d[idUser.Value][botId.Value].Item1 = null;
        d[idUser.Value][botId.Value].Item1 = new AnswerTelegram();
        d[idUser.Value][botId.Value].Item1.Reset();
    }

    internal void Delete(long? idUser, long? botId)
    {
        if (botId == null || idUser == null) return;
        d[idUser.Value][botId.Value].Item1 = null;
        d[idUser.Value][botId.Value].Item2 = null;
    }

    internal void SetAnswerProcessed(long idUser, long? botId, bool v)
    {
        if (botId != null) d[idUser][botId.Value].Item1.SetAnswerProcessed(v);
    }

    internal void AddWorkCompleted(long idUser, long? botId, bool sendMessageConfirmationChoice,
        TelegramBotAbstract telegramBotAbstract, string lang, string username)
    {
        if (botId == null) return;

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
                            { "en", "You chose [" + result + "]" },
                            { "it", "Hai scelto [" + result + "]" }
                        });
                        await telegramBotAbstract.SendTextMessageAsync(idUser,
                            languageReply,
                            ChatType.Private, lang, ParseMode.Html, replyMarkup, username);
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

    internal TaskCompletionSource<string> GetNewTcs(long idUser, long? botId)
    {
        if (botId == null) return null;
        d[idUser][botId.Value].Item2 = new TaskCompletionSource<string>();
        return d[idUser][botId.Value].Item2;
    }

    internal bool ContainsUser(long? userId, long? botId)
    {
        if (botId == null || userId == null) return false;

        return d.ContainsKey(userId.Value) && d[userId.Value].ContainsKey(botId.Value);
    }

    internal AnswerTelegram.State? GetState(long? userId, long? botId)
    {
        if (botId == null || userId == null) return null;
        if (d[userId.Value][botId.Value] == null) return null;
        if (d[userId.Value][botId.Value].Item1 != null)
            return d[userId.Value][botId.Value].Item1.GetState();
        return null;
    }

    internal void RecordAnswer(long? userId, long? botId, string text)
    {
        if (botId == null || userId == null) return;
        d[userId.Value][botId.Value].Item1.RecordAnswer(text);
    }
}