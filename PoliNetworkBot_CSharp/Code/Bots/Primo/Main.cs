#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Data;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Utils;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

#endregion

namespace PoliNetworkBot_CSharp.Code.Bots.Primo;

public static class Main
{
    internal static void MainMethod(object? sender, MessageEventArgs? e)
    {
        var t = new Thread(() =>
        {
            if (sender != null)
                _ = MainMethod2(sender, e);
        });
        t.Start();
    }

    private static async Task<MessageSentResult?> MainMethod2(object sender, MessageEventArgs? e)
    {
        TelegramBotClient? telegramBotClientBot = null;
        TelegramBotAbstract? telegramBotClient = null;

        try
        {
            if (sender is TelegramBotClient tmp) telegramBotClientBot = tmp;


            telegramBotClient = TelegramBotAbstract.GetFromRam(telegramBotClientBot);

            if (telegramBotClient == null)
                return null;

            return await HandleMessageAsync(telegramBotClient, e);
        }
        catch (Exception? exception)
        {
            await NotifyUtil.NotifyOwners(exception, telegramBotClient, e);
        }

        return null;
    }

    private static async Task<MessageSentResult?> HandleMessageAsync(TelegramBotAbstract? telegramBotClient,
        MessageEventArgs? e)
    {
        return e?.Message?.Chat.Id is 1001129635578 or -1001129635578
            ? await HandleMessage2Async(telegramBotClient, e)
            : null;
    }

    private static async Task<MessageSentResult?> HandleMessage2Async(TelegramBotAbstract? telegramBotClient,
        MessageEventArgs? e)
    {
        var t = e?.Message?.Text?.ToLower();

        if (string.IsNullOrEmpty(t))
            return null;

        if (t is "/lista_primo@primopolibot" or "/lista_primo") return await HandleListAsync(telegramBotClient, e);

        var (b, s) = CheckIfValid(t);
        var b1 = b ?? false;
        if (!b1)
            return null;

        return string.IsNullOrEmpty(s) ? null : await HandleMessage3Async(telegramBotClient, e, s);
    }

    private static async Task<MessageSentResult?> HandleListAsync(TelegramBotAbstract? telegramBotClient,
        MessageEventArgs? e)
    {
        var taken = GetTaken(telegramBotClient);

        const string emojiTaken = "🚫";
        const string emojiFree = "✅️";

        var toSend = "";
        if (GlobalVariables.WordToBeFirsts != null)
            foreach (var word in GlobalVariables.WordToBeFirsts)
            {
                var isTaken = word.IsTaken(taken);
                if (isTaken)
                    toSend += emojiTaken;
                else
                    toSend += emojiFree;

                toSend += " " + word.GetWord();

                toSend += "\n";
            }

        var dict = new Dictionary<string, string?>
        {
            { "en", toSend }
        };
        var text = new Language(dict);
        var message = e?.Message;
        if (message != null)
            return await SendMessage.SendMessageInAGroup(
                telegramBotClient, e?.Message?.From?.LanguageCode,
                text, e,
                message.Chat.Id, message.Chat.Type,
                ParseMode.Html, message.MessageId, true);
        return null;
    }

    private static List<string?> GetTaken(TelegramBotAbstract? telegramBotAbstract)
    {
        const string? q = "SELECT * FROM Primo";
        var r = Database.ExecuteSelect(q, telegramBotAbstract?.DbConfig);
        if (r == null || r.Rows.Count == 0)
            return new List<string?>();

        return (from DataRow dr in r.Rows
            let dt = (DateTime)dr["when_king"]
            where dt.Day == DateTime.Now.Day && dt.Month == DateTime.Now.Month && dt.Year == DateTime.Now.Year
            select dr["title"].ToString()).ToList();
    }

    private static Tuple<bool?, string?> CheckIfValid(string t)
    {
        if (GlobalVariables.WordToBeFirsts == null) return new Tuple<bool?, string?>(false, null);
        foreach (var x2 in GlobalVariables.WordToBeFirsts.Select(x => x.Matches(t)).Where(x2 => x2.Item1))
            return new Tuple<bool?, string?>(x2.Item1, x2.Item2);

        return new Tuple<bool?, string?>(false, null);
    }

    private static async Task<MessageSentResult?> HandleMessage3Async(TelegramBotAbstract? telegramBotClient,
        MessageEventArgs? e,
        string? t)
    {
        if (string.IsNullOrEmpty(t))
            return null;

        const string? q = "SELECT * FROM Primo WHERE title = @t";
        if (telegramBotClient == null)
            return null;

        var r = Database.ExecuteSelect(q, telegramBotClient.DbConfig,
            new Dictionary<string, object?> { { "@t", t } });
        if (r == null || r.Rows.Count == 0) return await MaybeKing(telegramBotClient, e, t, true);

        var datetime = (DateTime)r.Rows[0]["when_king"];
        if (datetime.Day != DateTime.Now.Day || datetime.Month != DateTime.Now.Month ||
            datetime.Year != DateTime.Now.Year)
            return await MaybeKing(telegramBotClient, e, t, false);

        var user = GenerateUserStringHtml(r.Rows[0]);
        var dict4 = new Dictionary<string, string?>
        {
            { "it", "C'è già " + user + " come re " + t + "!" },
            { "en", "There is already " + user + " as the " + t + " king!" }
        };
        var text = new Language(dict4);
        var r5 = e?.Message;
        if (r5 != null)
            return await SendMessage.SendMessageInAGroup(telegramBotClient, e?.Message?.From?.LanguageCode,
                text,
                e,
                r5.Chat.Id, r5.Chat.Type, ParseMode.Html, r5.MessageId, true);

        return null;
    }

    private static async Task<MessageSentResult?> MaybeKing(TelegramBotAbstract? telegramBotClient, MessageEventArgs? e,
        string? t,
        bool toInsert)
    {
        if (telegramBotClient == null)
            return null;

        var (b, list) = CheckIfLimitOfMaxKingsHasBeenReached(e, telegramBotClient);
        if (b == false)
        {
            if (toInsert)
            {
                const string? q2 = "INSERT INTO Primo (title, firstname, lastname, when_king, king_id) " +
                                   " VALUES " +
                                   " (@title, @fn, @ln, @wk, @ki)";


                var m1 = e?.Message;
                if (m1 != null)
                {
                    var r2 = Database.Execute(q2, telegramBotClient.DbConfig, new Dictionary<string, object?>
                    {
                        { "@title", t },
                        { "@fn", m1.From?.FirstName },
                        { "@ln", m1.From?.LastName },
                        { "@wk", DateTime.Now },
                        { "@ki", m1.From?.Id }
                    });
                }
            }
            else
            {
                const string q3 =
                    "UPDATE Primo SET when_king = @wk, king_id = @ki, firstname = @fn, lastname = @ln WHERE title = @t";
                var m1 = e?.Message;
                if (m1 == null)
                    return await SendMessageYouAreKingAsync(telegramBotClient, e, t);

                var dict3 = new Dictionary<string, object?>
                {
                    { "@t", t },
                    { "@fn", e?.Message?.From?.FirstName },
                    { "@ln", e?.Message?.From?.LastName },
                    { "@wk", DateTime.Now },
                    { "@ki", m1.From?.Id }
                };
                var r3 = Database.Execute(q3, telegramBotClient.DbConfig, dict3);
            }

            return await SendMessageYouAreKingAsync(telegramBotClient, e, t);
        }

        var roles = GetRoles(list);
        var dict4 = new Dictionary<string, string?>
        {
            { "it", "Hai già troppi ruoli!" + roles },
            { "en", "You have already too many titles!" + roles }
        };
        var text = new Language(dict4);
        var r5 = e?.Message;
        if (r5 != null)
            return await SendMessage.SendMessageInAGroup(telegramBotClient, r5.From?.LanguageCode, text, e,
                r5.Chat.Id, r5.Chat.Type, ParseMode.Html, r5.MessageId, true);

        return null;
    }

    private static string GetRoles(IReadOnlyCollection<string?>? item2)
    {
        if (item2 == null || item2.Count == 0)
            return "";

        var r = item2.Aggregate("\n", (current, item3) => current + item3 + ", ");

        r = r.Remove(r.Length - 1);
        r = r.Remove(r.Length - 1);

        return r;
    }

    private static Tuple<bool, List<string?>?> CheckIfLimitOfMaxKingsHasBeenReached(MessageEventArgs? e,
        TelegramBotAbstract? telegramBotAbstract)
    {
        const string q = "SELECT * FROM Primo";
        var r = Database.ExecuteSelect(q, telegramBotAbstract?.DbConfig);
        if (r == null || r.Rows.Count == 0)
            return new Tuple<bool, List<string?>?>(false, null);

        var countOfUser = CountOfUserMethod(r, e);

        if (countOfUser == null)
            return new Tuple<bool, List<string?>?>(false, null);

        return countOfUser.Count >= 2
            ? new Tuple<bool, List<string?>?>(true, countOfUser)
            : new Tuple<bool, List<string?>?>(false, null);
    }

    private static List<string?>? CountOfUserMethod(DataTable? r, MessageEventArgs? e)
    {
        if (r == null || e == null)
            return null;

        var message = e.Message;
        return (from DataRow dr in r.Rows
            where dr != null
            let id = (long)dr["king_id"]
            where message is { From: { } } && id == message.From.Id
            let dt = (DateTime)dr["when_king"]
            where DateTime.Now.Year == dt.Year && DateTime.Now.Month == dt.Month && DateTime.Now.Day == dt.Day
            select dr["title"].ToString()).ToList();
    }

    private static string GenerateUserStringHtml(DataRow dataRow)
    {
        var link = "tg://user?id=" + dataRow["king_id"];
        var r = "<a href='" + link + "'>";
        r += NameOfUser(dataRow);
        r += "</a>";
        return r;
    }

    private static string? NameOfUser(DataRow dataRow)
    {
        var fn = dataRow["firstname"].ToString();
        var ln = dataRow["lastname"].ToString();

        if (string.IsNullOrEmpty(fn) == false && string.IsNullOrEmpty(ln) == false) return fn + " " + ln;

        if (string.IsNullOrEmpty(fn) && string.IsNullOrEmpty(ln))
            return "[NULL]";

        if (string.IsNullOrEmpty(fn))
            return ln;

        return string.IsNullOrEmpty(ln) ? fn : "[EMPTY]";
    }

    private static async Task<MessageSentResult?> SendMessageYouAreKingAsync(TelegramBotAbstract? telegramBotClient,
        MessageEventArgs? e, string? t)
    {
        var dict = new Dictionary<string, string?>
        {
            { "it", "Congratulazioni, sei il re " + t + "!" },
            { "en", "Congratulations, you are the " + t + " king!" }
        };
        var text = new Language(dict);
        var m1 = e?.Message;
        if (e == null) return null;
        if (m1 == null) return null;
        var r = await SendMessage.SendMessageInAGroup(telegramBotClient, m1.From?.LanguageCode, text, e,
            m1.Chat.Id, m1.Chat.Type, ParseMode.Html, m1.MessageId, true);
        return r;
    }
}