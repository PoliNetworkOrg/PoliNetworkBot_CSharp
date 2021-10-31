using PoliNetworkBot_CSharp.Code.Data;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;

namespace PoliNetworkBot_CSharp.Code.Bots.Primo
{
    public class Main
    {
        internal static void MainMethod(object sender, MessageEventArgs e)
        {
            var t = new Thread(() => _ = MainMethod2(sender, e));
            t.Start();
        }

        private static async Task MainMethod2(object sender, MessageEventArgs e)
        {
            TelegramBotClient telegramBotClientBot = null;
            TelegramBotAbstract telegramBotClient = null;

            try
            {
                if (sender is TelegramBotClient tmp) telegramBotClientBot = tmp;

                if (telegramBotClientBot == null)
                    return;

                telegramBotClient = TelegramBotAbstract.GetFromRam(telegramBotClientBot);

                if (telegramBotClient == null)
                    return;

                await HandleMessageAsync(telegramBotClient, e);
            }
            catch (Exception exception)
            {
                Logger.WriteLine(exception.Message);

                await NotifyUtil.NotifyOwners(exception, telegramBotClient);
            }
        }

        private static async Task HandleMessageAsync(TelegramBotAbstract telegramBotClient, MessageEventArgs e)
        {
            if (e.Message.Chat.Id == 1001129635578 || e.Message.Chat.Id == -1001129635578)
                await HandleMessage2Async(telegramBotClient, e);
        }

        private static async Task HandleMessage2Async(TelegramBotAbstract telegramBotClient, MessageEventArgs e)
        {
            var t = e?.Message?.Text?.ToLower();

            if (string.IsNullOrEmpty(t))
                return;

            if (t == "/lista_primo@primopolibot" || t == "/lista_primo")
            {
                await HandleListAsync(telegramBotClient, e);
                return;
            }

            var valid = CheckIfValid(t);
            if (valid.Item1) await HandleMessage3Async(telegramBotClient, e, valid.Item2);
        }

        private static async Task HandleListAsync(TelegramBotAbstract telegramBotClient, MessageEventArgs e)
        {
            var taken = GetTaken();

            const string emojiTaken = "🚫";
            const string emojiFree = "✅️";

            var toSend = "";
            foreach (var word in GlobalVariables.wordToBeFirsts)
            {
                var isTaken = word.IsTaken(taken);
                if (isTaken)
                    toSend += emojiTaken;
                else
                    toSend += emojiFree;

                toSend += " " + word.GetWord();

                toSend += "\n";
            }

            var dict = new Dictionary<string, string>
            {
                {"en", toSend}
            };
            var text = new Language(dict);
            await SendMessage.SendMessageInAGroup(
                telegramBotClient, e.Message.From.LanguageCode,
                text,
                e.Message.Chat.Id, e.Message.Chat.Type,
                ParseMode.Html, e.Message.MessageId, true);
        }

        private static List<string> GetTaken()
        {
            var q = "SELECT * FROM Primo";
            var r = SqLite.ExecuteSelect(q);
            if (r == null || r.Rows.Count == 0)
                return new List<string>();

            var r2 = new List<string>();
            foreach (DataRow dr in r.Rows)
            {
                var dt = (DateTime)dr["when_king"];
                if (dt.Day == DateTime.Now.Day && dt.Month == DateTime.Now.Month && dt.Year == DateTime.Now.Year)
                    r2.Add(dr["title"].ToString());
            }

            return r2;
        }

        private static Tuple<bool, string> CheckIfValid(string t)
        {
            foreach (var x in GlobalVariables.wordToBeFirsts)
            {
                var x2 = x.Matches(t);
                if (x2.Item1)
                    return x2;
            }

            return new Tuple<bool, string>(false, null);
        }

        private static async Task HandleMessage3Async(TelegramBotAbstract telegramBotClient, MessageEventArgs e,
            string t)
        {
            if (string.IsNullOrEmpty(t))
                return;

            var q = "SELECT * FROM Primo WHERE title = @t";
            var r = SqLite.ExecuteSelect(q, new Dictionary<string, object> { { "@t", t } });
            if (r == null || r.Rows.Count == 0)
            {
                await MaybeKing(telegramBotClient, e, t, true);

                return;
            }

            var datetime = (DateTime)r.Rows[0]["when_king"];
            if (datetime.Day != DateTime.Now.Day || datetime.Month != DateTime.Now.Month ||
                datetime.Year != DateTime.Now.Year)
            {
                await MaybeKing(telegramBotClient, e, t, false);
                return;
            }

            var user = GenerateUserStringHtml(r.Rows[0]);
            var dict4 = new Dictionary<string, string>
            {
                {"it", "C'è già " + user + " come re " + t + "!"},
                {"en", "There is already " + user + " as the " + t + " king!"}
            };
            var text = new Language(dict4);
            var r4 = await SendMessage.SendMessageInAGroup(telegramBotClient, e.Message.From.LanguageCode, text,
                e.Message.Chat.Id, e.Message.Chat.Type, ParseMode.Html, e.Message.MessageId, true);
        }

        private static async Task MaybeKing(TelegramBotAbstract telegramBotClient, MessageEventArgs e, string t,
            bool toInsert)
        {
            var tooManyKingsForThisUser = CheckIfLimitOfMaxKingsHasBeenReached(telegramBotClient, e, t);
            if (tooManyKingsForThisUser.Item1 == false)
            {
                if (toInsert)
                {
                    var q2 = "INSERT INTO Primo (title, firstname, lastname, when_king, king_id) " +
                             " VALUES " +
                             " (@title, @fn, @ln, @wk, @ki)";

                    var r2 = SqLite.Execute(q2, new Dictionary<string, object>
                    {
                        {"@title", t},
                        {"@fn", e.Message.From.FirstName},
                        {"@ln", e.Message.From.LastName},
                        {"@wk", DateTime.Now},
                        {"@ki", e.Message.From.Id}
                    });
                }
                else
                {
                    var q3 =
                        "UPDATE Primo SET when_king = @wk, king_id = @ki, firstname = @fn, lastname = @ln WHERE title = @t";
                    var dict3 = new Dictionary<string, object>
                    {
                        {"@t", t},
                        {"@fn", e.Message.From.FirstName},
                        {"@ln", e.Message.From.LastName},
                        {"@wk", DateTime.Now},
                        {"@ki", e.Message.From.Id}
                    };
                    var r3 = SqLite.Execute(q3, dict3);
                }

                await SendMessageYouAreKingAsync(telegramBotClient, e, t);
                return;
            }

            var roles = GetRoles(tooManyKingsForThisUser.Item2);
            var dict4 = new Dictionary<string, string>
            {
                {"it", "Hai già troppi ruoli!" + roles},
                {"en", "You have already too many titles!" + roles}
            };
            var text = new Language(dict4);
            var r4 = await SendMessage.SendMessageInAGroup(telegramBotClient, e.Message.From.LanguageCode, text,
                e.Message.Chat.Id, e.Message.Chat.Type, ParseMode.Html, e.Message.MessageId, true);
        }

        private static string GetRoles(List<string> item2)
        {
            if (item2 == null || item2.Count == 0)
                return "";

            var r = "\n";

            foreach (var item3 in item2) r += item3 + ", ";

            r = r.Remove(r.Length - 1);
            r = r.Remove(r.Length - 1);

            return r;
        }

        private static Tuple<bool, List<string>> CheckIfLimitOfMaxKingsHasBeenReached(
            TelegramBotAbstract telegramBotClient, MessageEventArgs e, string t)
        {
            var q = "SELECT * FROM Primo";
            var r = SqLite.ExecuteSelect(q);
            if (r == null || r.Rows.Count == 0)
                return new Tuple<bool, List<string>>(false, null);

            var countOfUser = CountOfUserMethod(r, e);

            if (countOfUser == null)
                return new Tuple<bool, List<string>>(false, null);

            if (countOfUser.Count >= 2)
                return new Tuple<bool, List<string>>(true, countOfUser);

            return new Tuple<bool, List<string>>(false, null);
        }

        private static List<string> CountOfUserMethod(DataTable r, MessageEventArgs e)
        {
            if (r == null || e == null || r.Rows == null)
                return null;

            var r3 = new List<string>();
            foreach (DataRow dr in r.Rows)
            {
                if (dr == null)
                    continue;

                var id = (long)dr["king_id"];
                if (id == e.Message.From.Id)
                {
                    var dt = (DateTime)dr["when_king"];
                    if (DateTime.Now.Year == dt.Year && DateTime.Now.Month == dt.Month && DateTime.Now.Day == dt.Day)
                        r3.Add(dr["title"].ToString());
                }
            }

            return r3;
        }

        private static string GenerateUserStringHtml(DataRow dataRow)
        {
            var link = "tg://user?id=" + dataRow["king_id"];
            var r = "<a href='" + link + "'>";
            r += NameOfUser(dataRow);
            r += "</a>";
            return r;
        }

        private static string NameOfUser(DataRow dataRow)
        {
            var fn = dataRow["firstname"]?.ToString();
            var ln = dataRow["lastname"]?.ToString();

            if (string.IsNullOrEmpty(fn) == false && string.IsNullOrEmpty(ln) == false) return fn + " " + ln;

            if (string.IsNullOrEmpty(fn) && string.IsNullOrEmpty(ln))
                return "[NULL]";

            if (string.IsNullOrEmpty(fn))
                return ln;

            if (string.IsNullOrEmpty(ln))
                return fn;

            return "[EMPTY]";
        }

        private static async Task<MessageSentResult> SendMessageYouAreKingAsync(TelegramBotAbstract telegramBotClient,
            MessageEventArgs e, string t)
        {
            var dict = new Dictionary<string, string>
            {
                {"it", "Congratulazioni, sei il re " + t + "!"},
                {"en", "Congratulations, you are the " + t + " king!"}
            };
            var text = new Language(dict);
            var r = await SendMessage.SendMessageInAGroup(telegramBotClient, e.Message.From.LanguageCode, text,
                e.Message.Chat.Id, e.Message.Chat.Type, ParseMode.Html, e.Message.MessageId, true);
            return r;
        }
    }
}