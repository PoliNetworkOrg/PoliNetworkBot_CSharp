using System;
using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Args;

namespace PoliNetworkBot_CSharp.Bots.Moderation
{
    internal class Main
    {
        internal static void MainMethod(object sender, MessageEventArgs e)
        {
            TelegramBotClient telegramBotClient = null;
            if (sender is TelegramBotClient tmp)
            {
                telegramBotClient = tmp;
            }

            if (telegramBotClient == null)
                return;

            bool to_exit = CheckIfToExitAndUpdateGroupList(telegramBotClient, e);
            if (to_exit)
            {
                ExitFromChat(telegramBotClient, e);
                return;
            }

            Tuple<bool, bool> check_username = CheckUsername(e);
            if (check_username.Item1 || check_username.Item2)
            {
                SendUsernameWarning(telegramBotClient, e, check_username.Item1, check_username.Item2);
                return;
            }

            SpamType check_spam = CheckSpam(e);
            if (check_spam != SpamType.ALL_GOOD)
            {
                AntiSpamMeasure(telegramBotClient, e, check_spam);
                return;
            }

            if (!string.IsNullOrEmpty(e.Message.Text))
            {
                if (e.Message.Text.StartsWith("/"))
                {
                    CommandDispatcher(telegramBotClient, e);
                }
            }
        }

        private static void AntiSpamMeasure(TelegramBotClient telegramBotClient, MessageEventArgs e, SpamType check_spam)
        {
            if (check_spam == SpamType.ALL_GOOD)
                return;

            Utils.RestrictUser.Mute(60 * 5, telegramBotClient, e);
            string language = e.Message.From.LanguageCode.ToLower();
            switch (check_spam)
            {
                case SpamType.SPAM_LINK:
                    {
                        string text = language switch
                        {
                            "en" => "You sent a message with spam, and you were muted for 5 minutes",
                            _ => "Hai inviato un messaggio con spam, e quindi il bot ti ha mutato per 5 minuti",
                        };
                        Utils.SendMessage.SendMessageInPrivate(telegramBotClient, e, text);
                        break;
                    }
                case SpamType.NOT_ALLOWED_WORDS:
                    {
                        string text = language switch
                        {
                            "en" => "You sent a message with banned words, and you were muted for 5 minutes",
                            _ => "Hai inviato un messaggio con parole bandite, e quindi il bot ti ha mutato per 5 minuti",
                        };
                        Utils.SendMessage.SendMessageInPrivate(telegramBotClient, e, text);
                        break;
                    }

                case SpamType.ALL_GOOD:
                    {
                        return;
                    }

                case SpamType.FOREIGN:
                    {
                        string text = language switch
                        {
                            "en" => "You sent a message with banned characters, and you were muted for 5 minutes",
                            _ => "Hai inviato un messaggio con caratteri banditi, e quindi il bot ti ha mutato per 5 minuti",
                        };
                        Utils.SendMessage.SendMessageInPrivate(telegramBotClient, e, text);
                        break;
                    }
            }

            telegramBotClient.DeleteMessageAsync(e.Message.Chat.Id, e.Message.MessageId);
        }

        private static SpamType CheckSpam(MessageEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Message.Text))
            {
                //todo
                return SpamType.ALL_GOOD;
            }

            if (e.Message.Text.StartsWith("/"))
                return SpamType.ALL_GOOD;

            bool is_foreign = DetectForeignLanguage(e);
            if (is_foreign)
                return SpamType.FOREIGN;

            return Blacklist.IsSpam(e.Message.Text);
        }

        private static bool DetectForeignLanguage(MessageEventArgs e)
        {
            if (e.Message.Chat.Id == -1001394018284)
                return false;

            if (
                Regex.Match(e.Message.Text, "[\uac00-\ud7a3]").Success ||
                Regex.Match(e.Message.Text, "[\u3040-\u30ff]").Success ||
                Regex.Match(e.Message.Text, "[\u4e00-\u9FFF]").Success
                )
            {
                return true;
            }

            return false;
        }

        private static void SendUsernameWarning(TelegramBotClient telegramBotClient, MessageEventArgs e, bool username, bool name)
        {
            string s1 = "Imposta un username e un nome più lungo dalle impostazioni di telegram\n\n" +
                          "Set an username and a longer first name from telegram settings";
            if (username && !name)
            {
                s1 = "Imposta un username dalle impostazioni di telegram\n\n" +
                          "Set an username from telegram settings";
            }
            else if (!username && name)
            {
                s1 = "Imposta un nome più lungo " +
                          "dalle impostazioni di telegram\n\n" +
                          "Set a longer first name from telegram settings";
            }

            Utils.SendMessage.SendMessageInPrivateOrAGroup(telegramBotClient, e, s1);
            Utils.RestrictUser.Mute(time: 60 * 5, telegramBotClient, e);
            telegramBotClient.DeleteMessageAsync(e.Message.Chat.Id, e.Message.MessageId);
        }

        private static Tuple<bool, bool> CheckUsername(MessageEventArgs e)
        {
            bool username = false;
            bool name = false;

            if (string.IsNullOrEmpty(e.Message.From.Username))
            {
                username = true;
            }

            if (e.Message.From.FirstName.Length < 2)
                name = true;

            return new Tuple<bool, bool>(username, name);
        }

        private static void ExitFromChat(TelegramBotClient sender, MessageEventArgs e)
        {
            sender.LeaveChatAsync(e.Message.Chat.Id);
        }

        private static bool CheckIfToExitAndUpdateGroupList(TelegramBotClient sender, MessageEventArgs e)
        {
            switch (e.Message.Chat.Type)
            {
                case Telegram.Bot.Types.Enums.ChatType.Private:
                    return false;
            }

            string q1 = "SELECT id, valid FROM Groups WHERE id = @id";
            var dt = Utils.SQLite.ExecuteSelect(q1, new System.Collections.Generic.Dictionary<string, object>() { { "@id", e.Message.Chat.Id } });
            if (dt != null && dt.Rows.Count > 0)
            {
                return CheckIfToExit(sender, e, dt.Rows[0].ItemArray[1]);
            }
            else
            {
                InsertGroup(sender, e);
                return CheckIfToExit(sender, e, null);
            }
        }

        private static void InsertGroup(TelegramBotClient sender, MessageEventArgs e)
        {
            string q1 = "INSERT INTO Groups (id, bot_id) VALUES (@id, @botid)";
            Utils.SQLite.Execute(q1, new System.Collections.Generic.Dictionary<string, object>() { { "@id", e.Message.Chat.Id }, { "@botid", sender.BotId } });
            _ = CreateInviteLinkAsync(sender, e);
        }

        private static async System.Threading.Tasks.Task<bool> CreateInviteLinkAsync(TelegramBotClient sender, MessageEventArgs e)
        {
            return await Utils.InviteLinks.CreateInviteLinkAsync(e.Message.Chat.Id, sender);
        }

        private static bool CheckIfToExit(TelegramBotClient telegramBotClient, MessageEventArgs e, object v)
        {
            if (v == null || v is System.DBNull)
            {
                return CheckIfToExit_NullValue(telegramBotClient, e);
            }

            if (v is char b)
            {
                return b != 'Y';
            }

            if (v is string s)
            {
                if (string.IsNullOrEmpty(s))
                {
                    return CheckIfToExit_NullValue(telegramBotClient, e);
                }

                return s != "Y";
            }

            return CheckIfToExit_NullValue(telegramBotClient, e);
        }

        private static bool CheckIfToExit_NullValue(TelegramBotClient telegramBotClient, MessageEventArgs e)
        {
            //todo: check if admins are allowed and set valid column
            return false;
        }

        private static void CommandDispatcher(TelegramBotClient sender, MessageEventArgs e)
        {
            switch (e.Message.Text)
            {
                case "/start":
                    {
                        Start(sender, e);
                        return;
                    }

                case "/force_check_invite_links":
                    {
                        if (Data.GlobalVariables.Creators.Contains(e.Message.Chat.Id))
                        {
                            _ = ForceCheckInviteLinksAsync(sender, e);
                        }
                        return;
                    }
            }
        }

        private static async System.Threading.Tasks.Task ForceCheckInviteLinksAsync(TelegramBotClient sender, MessageEventArgs e)
        {
            int n = await Utils.InviteLinks.FillMissingLinksIntoDB_Async(sender);
            Utils.SendMessage.SendMessageInPrivate(sender, e, "I have updated n=" + n.ToString() + " links");
        }

        private static void Start(TelegramBotClient telegramBotClient, MessageEventArgs e)
        {
            if (e.Message.Chat.Type != Telegram.Bot.Types.Enums.ChatType.Private)
            {
                try
                {
                    telegramBotClient.DeleteMessageAsync(e.Message.Chat.Id, e.Message.MessageId);
                }
                catch
                {
                    ;
                }
            }
            telegramBotClient.SendTextMessageAsync(e.Message.Chat.Id, "Ciao!");
        }
    }
}