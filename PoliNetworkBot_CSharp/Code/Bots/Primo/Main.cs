﻿using PoliNetworkBot_CSharp.Code.Objects;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;

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
                Console.WriteLine(exception.Message);

                await Utils.NotifyUtil.NotifyOwners(exception, telegramBotClient);
            }
        }

        private static async Task HandleMessageAsync(TelegramBotAbstract telegramBotClient, MessageEventArgs e)
        {
            if (e.Message.Chat.Id == 1001129635578 || e.Message.Chat.Id == -1001129635578)
            {
                await HandleMessage2Async(telegramBotClient, e);
            }
        }

        private static async Task HandleMessage2Async(TelegramBotAbstract telegramBotClient, MessageEventArgs e)
        {
            string t = e?.Message?.Text?.ToLower();

            if (string.IsNullOrEmpty(t))
                return;

            bool valid = CheckIfValid(t);
            if (valid)
            {
                await HandleMessage3Async(telegramBotClient, e, t);
                return;
            }
        }

        private static bool CheckIfValid(string t)
        {
            foreach (var x in Code.Data.GlobalVariables.wordToBeFirsts)
            {
                if (x.Matches(t))
                    return true;
            }

            return false;
        }

        private static async Task HandleMessage3Async(TelegramBotAbstract telegramBotClient, MessageEventArgs e, string t)
        {
            string q = "SELECT * FROM Primo WHERE title = @t";
            var r = Utils.SqLite.ExecuteSelect(q, new Dictionary<string, object>() { { "@t", t } });
            if (r == null || r.Rows.Count == 0)
            {
                string q2 = "INSERT INTO Primo (title, firstname, lastname, when_king, king_id) " +
                    " VALUES " +
                    " (@title, @fn, @ln, @wk, @ki)";

                var r2 = Utils.SqLite.Execute(q2, new Dictionary<string, object>() {
                    { "@title", t},
                    { "@fn", e.Message.From.FirstName },
                    {"@ln", e.Message.From.LastName },
                    { "@wk", DateTime.Now },
                    {"@ki", e.Message.From.Id }
                });

                await SendMessageYouAreKingAsync(telegramBotClient, e, t);
                return;
            }

            var datetime = (DateTime)r.Rows[0]["when_king"];
            if (datetime.Day != DateTime.Now.Day || datetime.Month != DateTime.Now.Month || datetime.Year != DateTime.Now.Year)
            {
                string q3 = "UPDATE Primo SET when_king = @wk, king_id = @ki, firstname = @fn, lastname = @ln";
                Dictionary<string, object> dict3 = new Dictionary<string, object>() {
                    { "@fn", e.Message.From.FirstName },
                    {"@ln", e.Message.From.LastName },
                    { "@wk", DateTime.Now },
                    {"@ki", e.Message.From.Id }
                };
                var r3 = Utils.SqLite.Execute(q3, dict3);

                await SendMessageYouAreKingAsync(telegramBotClient, e, t);
                return;
            }


            string user = GenerateUserStringHtml(r.Rows[0]);
            Dictionary<string, string> dict4 = new Dictionary<string, string>() {
                {"it", "C'è già "+user+" come re " + t + "!" },
                {"en", "There is already "+user+" as the " + t + " king!" }
            };
            Language text = new Language(dict: dict4);
            var r4 = await Utils.SendMessage.SendMessageInAGroup(telegramBotClient, 0, e.Message.From.LanguageCode, e.Message.From.Username, text, e.Message.From.FirstName,
                e.Message.From.LastName, e.Message.Chat.Id, e.Message.Chat.Type, Telegram.Bot.Types.Enums.ParseMode.Html, e.Message.MessageId, true);
            return;

        }

        private static string GenerateUserStringHtml(System.Data.DataRow dataRow)
        {
            string link = "tg://user?id=" + dataRow["king_id"].ToString();
            string r = "<a href='"+link+"'>";
            r += NameOfUser(dataRow);
            r += "</a>";
            return r;
        }

        private static string NameOfUser(System.Data.DataRow dataRow)
        {
            string fn = dataRow["firstname"]?.ToString();
            string ln = dataRow["lastname"]?.ToString();

            if (string.IsNullOrEmpty(fn) == false && string.IsNullOrEmpty(ln) == false)
            {
                return (fn + " " + ln);
            }

            if (string.IsNullOrEmpty(fn) && string.IsNullOrEmpty(ln))
                return "[NULL]";

            if (string.IsNullOrEmpty(fn))
                return ln;

            if (string.IsNullOrEmpty(ln))
                return fn;

            return "[EMPTY]";
        }

        private static async Task<Tuple<bool,object>> SendMessageYouAreKingAsync(TelegramBotAbstract telegramBotClient, MessageEventArgs e, string t)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>() {
                {"it", "Congratulazioni, sei il re " + t + "!" },
                {"en", "Contratulations, you are the " + t + " king!" }
            };
            Language text = new Language(dict: dict);
            var r = await Utils.SendMessage.SendMessageInAGroup(telegramBotClient, 0, e.Message.From.LanguageCode, e.Message.From.Username, text, e.Message.From.FirstName,
                e.Message.From.LastName, e.Message.Chat.Id, e.Message.Chat.Type, Telegram.Bot.Types.Enums.ParseMode.Html, e.Message.MessageId, true);
            return r;
        }
    }
}
