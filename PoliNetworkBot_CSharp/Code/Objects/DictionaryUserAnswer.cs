#region

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Enums;
using Telegram.Bot.Types.Enums;

#endregion

namespace PoliNetworkBot_CSharp.Code.Objects;

public class DictionaryUserAnswer
{
    private readonly Dictionary<long, Dictionary<long, Couple<AnswerTelegram, TaskCompletionSource<string?>?>>> _d;

    public DictionaryUserAnswer()
    {
        _d = new Dictionary<long, Dictionary<long, Couple<AnswerTelegram, TaskCompletionSource<string?>?>>>();
    }

    internal void Reset(long? idUser, long? botId)
    {
        if (idUser == null)
            return;

        if (!_d.ContainsKey(idUser.Value))
            _d[idUser.Value] = new Dictionary<long, Couple<AnswerTelegram, TaskCompletionSource<string?>?>>();

        _d[idUser.Value] ??= new Dictionary<long, Couple<AnswerTelegram, TaskCompletionSource<string?>?>>();
        if (botId == null) return;
        if (!_d[idUser.Value].ContainsKey(botId.Value))
            _d[idUser.Value][botId.Value] = new Couple<AnswerTelegram, TaskCompletionSource<string?>?>();

        _d[idUser.Value][botId.Value] ??= new Couple<AnswerTelegram, TaskCompletionSource<string?>?>();

        _d[idUser.Value][botId.Value].Item1 = null;
        _d[idUser.Value][botId.Value].Item1 = new AnswerTelegram();
        var answerTelegram = _d[idUser.Value][botId.Value].Item1;
        answerTelegram?.Reset();
    }

    internal void Delete(long? idUser, long? botId)
    {
        if (botId == null || idUser == null) return;
        _d[idUser.Value][botId.Value].Item1 = null;
        _d[idUser.Value][botId.Value].Item2 = null;
    }

    internal void SetAnswerProcessed(long idUser, long? botId, bool v)
    {
        if (botId == null) return;
        var answerTelegram = _d[idUser][botId.Value].Item1;
        answerTelegram?.SetAnswerProcessed(v);
    }

    internal void AddWorkCompleted(long idUser, long? botId, bool sendMessageConfirmationChoice,
        TelegramBotAbstract? telegramBotAbstract, string? lang, string? username)
    {
        if (botId == null) return;

        var answerTelegram = _d[idUser][botId.Value].Item1;
        if (answerTelegram != null)
            // ReSharper disable once AsyncVoidLambda
            answerTelegram.WorkCompleted += async result =>
                await OnAnswerTelegramWorkCompleted(answerTelegram, sendMessageConfirmationChoice,
                    telegramBotAbstract, result, idUser, lang, username, botId);
    }


    private async Task OnAnswerTelegramWorkCompleted(AnswerTelegram? answerTelegram, bool sendMessageConfirmationChoice,
        TelegramBotAbstract? telegramBotAbstract, object? result, long idUser,
        string? lang, string? username, long? botId)
    {
        var crashed = true;
        try
        {
            if (answerTelegram != null && answerTelegram.GetState() == AnswerTelegram.State.ANSWERED &&
                answerTelegram.GetAlreadyProcessedAnswer() == false)
            {
                if (sendMessageConfirmationChoice)
                {
                    var replyMarkup = new ReplyMarkupObject(ReplyMarkupEnum.REMOVE);
                    var languageReply = new Language(new Dictionary<string, string?>
                        { { "en", "You chose [" + result + "]" }, { "it", "Hai scelto [" + result + "]" } });
                    if (telegramBotAbstract != null)
                        await telegramBotAbstract.SendTextMessageAsync(idUser, languageReply, ChatType.Private, lang,
                            ParseMode.Html, replyMarkup, username);
                }

                answerTelegram.SetAnswerProcessed(true);

                if (result != null)
                {
                    var resultstring = result.ToString();
                    crashed = false;
                    if (botId != null)
                    {
                        var taskCompletionSource = _d[idUser][botId.Value].Item2;
                        var done = taskCompletionSource != null && taskCompletionSource.TrySetResult(resultstring);
                        Console.WriteLine("Task" + idUser + " " + botId.Value + " " + done);
                    }
                }
            }
        }
        catch
        {
            // ignored
        }

        if (crashed)
            if (botId != null)
                _d[idUser][botId.Value].Item2?.TrySetResult("");
    }

    internal TaskCompletionSource<string?>? GetNewTcs(long idUser, long? botId)
    {
        if (botId == null) return null;
        _d[idUser][botId.Value].Item2 = new TaskCompletionSource<string?>();
        return _d[idUser][botId.Value].Item2;
    }

    internal bool ContainsUser(long? userId, long? botId)
    {
        if (botId == null || userId == null) return false;

        return _d.ContainsKey(userId.Value) && _d[userId.Value].ContainsKey(botId.Value);
    }

    internal AnswerTelegram.State? GetState(long? userId, long? botId)
    {
        if (botId == null || userId == null) return null;
        if (_d[userId.Value][botId.Value].Item1 == null) return null;
        var answerTelegram = _d[userId.Value][botId.Value].Item1;
        return answerTelegram?.GetState();
    }

    internal void RecordAnswer(long? userId, long? botId, string? text)
    {
        if (botId == null || userId == null) return;
        var answerTelegram = _d[userId.Value][botId.Value].Item1;
        answerTelegram?.RecordAnswer(text);
    }
}