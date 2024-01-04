#region

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects.AbstractBot;
using Telegram.Bot.Types.Enums;

#endregion

namespace PoliNetworkBot_CSharp.Code.Objects;

public class DictionaryUserAnswer
{
    private readonly Dictionary<long, AnswerWithTask> _d;

    public DictionaryUserAnswer()
    {
        _d = new Dictionary<long, AnswerWithTask>();
    }

    internal void Reset(long? idUser, long? botId)
    {
        if (idUser == null)
            return;

        if (!_d.ContainsKey(idUser.Value))
            _d[idUser.Value] = new AnswerWithTask();

        if (botId == null)
            return;

        _d[idUser.Value].InitializeIfKeyNotPresent(botId.Value);


        _d[idUser.Value].ResetItem1(botId.Value);
        var answerTelegram = _d[idUser.Value].GetItem1(botId.Value);
        answerTelegram?.Reset();
    }

    internal void Delete(long? idUser, long? botId)
    {
        if (botId == null || idUser == null) return;
        _d[idUser.Value].Delete(botId.Value);
    }

    internal void SetAnswerProcessed(long idUser, long? botId, bool v)
    {
        if (botId == null) return;
        var answerTelegram = _d[idUser].GetItem1(botId.Value);
        answerTelegram?.SetAnswerProcessed(v);
    }

    internal void AddWorkCompleted(long idUser, long? botId, bool sendMessageConfirmationChoice,
        AbstractBot.TelegramBotAbstract? telegramBotAbstract, string? lang, string? username)
    {
        if (botId == null) return;

        var answerTelegram = _d[idUser].GetItem1(botId.Value);
        if (answerTelegram != null)
            // ReSharper disable once AsyncVoidLambda
            answerTelegram.WorkCompleted += async result =>
                await OnAnswerTelegramWorkCompleted(answerTelegram, sendMessageConfirmationChoice,
                    telegramBotAbstract, result, idUser, lang, username, botId);
    }


    private async Task OnAnswerTelegramWorkCompleted(AnswerTelegram? answerTelegram, bool sendMessageConfirmationChoice,
        AbstractBot.TelegramBotAbstract? telegramBotAbstract, object? result, long idUser,
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
                    {
                        var messageOptions =
                            new MessageOptions

                            {
                                ChatId = idUser,
                                Text = languageReply,
                                ChatType = ChatType.Private,
                                Lang = lang,
                                ReplyMarkupObject = replyMarkup,
                                Username = username
                            };
                        await telegramBotAbstract.SendTextMessageAsync(messageOptions);
                    }
                }

                answerTelegram.SetAnswerProcessed(true);

                if (result != null)
                {
                    var resultstring = result.ToString();
                    crashed = false;
                    if (botId != null)
                    {
                        var taskCompletionSource = _d[idUser].GetItem2(botId.Value);
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
                _d[idUser].GetItem2(botId.Value)?.TrySetResult("");
    }

    internal TaskCompletionSource<string?>? GetNewTcs(long idUser, long? botId)
    {
        if (botId == null) return null;
        _d[idUser].ResetItem2(botId.Value);
        return _d[idUser].GetItem2(botId.Value);
    }

    internal bool ContainsUser(long? userId, long? botId)
    {
        if (botId == null || userId == null) return false;

        return _d.ContainsKey(userId.Value) && _d[userId.Value].ContainsKey(botId.Value);
    }

    internal AnswerTelegram.State? GetState(long? userId, long? botId)
    {
        if (botId == null || userId == null) return null;
        if (_d[userId.Value].GetItem1(botId.Value) == null) return null;
        var answerTelegram = _d[userId.Value].GetItem1(botId.Value);
        return answerTelegram?.GetState();
    }

    internal void RecordAnswer(long? userId, long? botId, string? text)
    {
        if (botId == null || userId == null) return;
        var answerTelegram = _d[userId.Value].GetItem1(botId.Value);
        answerTelegram?.RecordAnswer(text);
    }
}