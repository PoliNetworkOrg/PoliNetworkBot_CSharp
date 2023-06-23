#region

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.TelegramBotAbstract;
using SampleNuGet.Objects;
using Telegram.Bot.Types.Enums;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils;

internal static class AskUser
{
    public static readonly DictionaryUserAnswer UserAnswers = new();

    internal static async Task<string?> AskAsync(long? idUser, Language question,
        TelegramBotAbstract? sender, string? lang, string? username, int? messageThreadId,
        bool sendMessageConfirmationChoice = false)
    {
        if (sender == null) return null;
        var botId = sender.GetId();

        UserAnswers.Reset(idUser, botId);

        await sender.SendTextMessageAsync(idUser, question, ChatType.Private, parseMode: ParseMode.Html,
            replyMarkupObject: new ReplyMarkupObject(ReplyMarkupEnum.FORCED), lang: lang, username: username,
            messageThreadId: messageThreadId);

        var result =
            await WaitForAnswer(idUser, sendMessageConfirmationChoice, sender, lang, username, messageThreadId);
        UserAnswers.Delete(idUser, botId);
        return result;
    }

    private static async Task<string?> WaitForAnswer(long? idUser, bool sendMessageConfirmationChoice,
        TelegramBotAbstract? telegramBotAbstract, string? lang, string? username, int? messageThreadId)
    {
        if (idUser == null)
            return null;

        try
        {
            if (telegramBotAbstract != null)
            {
                var botId = telegramBotAbstract.GetId();
                var tcs = UserAnswers.GetNewTcs(idUser.Value, botId);
                UserAnswers.SetAnswerProcessed(idUser.Value, botId, false);
                UserAnswers.AddWorkCompleted(idUser.Value, botId, sendMessageConfirmationChoice, telegramBotAbstract,
                    lang,
                    username, messageThreadId);

                if (tcs != null) return await tcs.Task;
            }
        }
        catch
        {
            // ignored
        }

        return null;
    }

    internal static async Task<string?> AskBetweenRangeAsync(long? idUser, Language? question,
        TelegramBotAbstract? sender, string? lang, IEnumerable<List<Language>>? options,
        string? username, int? messageThreadId,
        bool sendMessageConfirmationChoice = true, long? messageIdToReplyTo = 0)
    {
        if (sender == null) return null;
        var botId = sender.GetId();

        UserAnswers.Reset(idUser, botId);

        var list = KeyboardMarkup.OptionsStringToKeyboard(options, lang);
        if (list != null)
        {
            var replyMarkupObject = new ReplyMarkupObject(
                new ReplyMarkupOptions(
                    list
                )
            );

            var m1 = await sender.SendTextMessageAsync(idUser, question, ChatType.Private,
                parseMode: ParseMode.Html, replyMarkupObject: replyMarkupObject, lang: lang, username: username,
                replyToMessageId: messageIdToReplyTo, messageThreadId: messageThreadId);
        }

        var result =
            await WaitForAnswer(idUser, sendMessageConfirmationChoice, sender, lang, username, messageThreadId);
        UserAnswers.Delete(idUser, botId);
        return result;
    }

    internal static async Task<string?> GetSedeAsync(TelegramBotAbstract? sender, MessageEventArgs? e)
    {
        var options = new List<List<Language>>
        {
            new() { new Language(new Dictionary<string, string?> { { "en", "Milano Leonardo" } }) },
            new() { new Language(new Dictionary<string, string?> { { "en", "Milano Bovisa" } }) },
            new() { new Language(new Dictionary<string, string?> { { "en", "Como" } }) }
        };
        var question = new Language(new Dictionary<string, string?>
        {
            { "it", "In che sede?" },
            { "en", "In which territorial pole?" }
        });
        var eMessage = e?.Message;
        var eMessageFrom = eMessage?.From;
        var reply = await AskBetweenRangeAsync(eMessageFrom?.Id,
            sender: sender,
            lang: eMessageFrom?.LanguageCode,
            options: options,
            username: eMessageFrom?.Username,
            sendMessageConfirmationChoice: true,
            question: question, messageThreadId: eMessage?.MessageThreadId);

        if (string.IsNullOrEmpty(reply))
            return null;

        return reply switch
        {
            "Milano Leonardo" => "MIA",
            "Milano Bovisa" => "MIB",
            "Como" => "COE",
            _ => null
        };
    }

    internal static async Task<bool> AskYesNo(long? id, Language? question, bool defaultBool,
        TelegramBotAbstract? sender, string? lang, string? username, int? messageThreadId)
    {
        var l1 = new Language(new Dictionary<string, string?>
        {
            { "it", "Si" },
            { "en", "Yes" }
        });
        var l2 = new Language(new Dictionary<string, string?>
        {
            { "it", "No" },
            { "en", "No" }
        });

        var options = new List<List<Language>>
        {
            new()
            {
                l1, l2
            }
        };

        var r = await AskBetweenRangeAsync(id, question, sender, lang, options, username, messageThreadId);

        if (l1.Matches(r)) return true;

        return !l2.Matches(r) && defaultBool;
    }

    internal static async Task<DateTime?> AskHours(long? id, Language question, TelegramBotAbstract? sender,
        string? languageCode, string? username, int? messageThreadId)
    {
        var s = await AskAsync(id, question, sender, languageCode, username, messageThreadId);
        return DateTimeClass.GetHours(s);
    }

    internal static async Task<Tuple<DateTimeSchedule?, Exception?, string?>?> AskDateAsync(long? id, string text,
        string? lang,
        TelegramBotAbstract? sender,
        string? username, int? messageThreadId)
    {
        if (string.IsNullOrEmpty(text))
            return await AskDate2Async(id, lang, sender, username, messageThreadId);

        var s = text.Split(' ');
        if (s.Length == 1) return await AskDate2Async(id, lang, sender, username, messageThreadId);

        switch (s[1])
        {
            case "ora":
            case "now":
            {
                return new Tuple<DateTimeSchedule?, Exception?, string?>(new DateTimeSchedule(DateTime.Now, true),
                    null, s[1]);
            }
        }

        return await AskDate2Async(id, lang, sender, username, messageThreadId);
    }

    private static async Task<Tuple<DateTimeSchedule?, Exception?, string?>?> AskDate2Async(long? id, string? lang,
        TelegramBotAbstract? sender,
        string? username, int? messageThreadId)
    {
        var lang2 = new Language(new Dictionary<string, string?>
        {
            { "it", "Inserisci una data (puoi scrivere anche 'fra un'ora')" },
            { "en", "Insert a date (you can also write 'in an hour')" }
        });

        var reply = await AskAsync(id, lang2, sender, lang, username, messageThreadId);
        try
        {
            var tuple1 = SampleNuGet.Utils.DateTimeClass.GetDateTimeFromString(reply);
            if (tuple1 != null)
            {
                var dateTime = tuple1.Item1;
                var exception = tuple1.Item2;
                if (exception != null)
                    return new Tuple<DateTimeSchedule?, Exception?, string?>(null, exception, reply);

                return new Tuple<DateTimeSchedule?, Exception?, string?>(new DateTimeSchedule(dateTime, true),
                    null, reply);
            }
        }
        catch (Exception e1)
        {
            return new Tuple<DateTimeSchedule?, Exception?, string?>(null, e1, reply);
        }

        return null;
    }
}