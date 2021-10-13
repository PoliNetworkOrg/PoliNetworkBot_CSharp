﻿#region

using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TeleSharp.TL;

#endregion

namespace PoliNetworkBot_CSharp.Code.Bots.Moderation
{
    internal static class ModerationCheck
    {
        public static async Task<Tuple<ToExit, Telegram.Bot.Types.ChatMember[], List<int>, string>> CheckIfToExitAndUpdateGroupList(TelegramBotAbstract sender, MessageEventArgs e)
        {
            switch (e.Message.Chat.Type)
            {
                case ChatType.Private:
                    return new Tuple<ToExit, Telegram.Bot.Types.ChatMember[], List<int>, string>(ToExit.STAY, null, new List<int>() { 13 }, "private");

                case ChatType.Group:
                    break;

                case ChatType.Channel:
                    break;

                case ChatType.Supergroup:
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            //start | exclude groups, bot will not operate in them
            if (e.Message.Chat.Id == Code.Bots.Anon.ConfigAnon.ModAnonCheckGroup)
                return new Tuple<ToExit, ChatMember[], List<int>, string>(ToExit.STAY, null, new List<int> { 30 }, null);
            //end | exclude groups

            const string q1 = "SELECT id, valid FROM Groups WHERE id = @id";
            var dt = SqLite.ExecuteSelect(q1, new Dictionary<string, object> { { "@id", e.Message.Chat.Id } });
            if (dt != null && dt.Rows.Count > 0)
            {
                Tuple<ToExit, ChatMember[], List<int>, string> r1 = await CheckIfToExit(sender, e, dt.Rows[0].ItemArray[1]);
                r1.Item3.Insert(0, 11);
                return r1;
            }

            InsertGroup(sender, e);
            var r2 = await CheckIfToExit(sender, e, null);
            var list2 = r2.Item3;
            list2.Insert(0, 12);
            return new Tuple<ToExit, ChatMember[], List<int>, string>(r2.Item1, r2.Item2, list2, r2.Item4);
        }

        internal async static Task<List<long>> CheckIfNotAuthorizedBotHasBeenAdded(MessageEventArgs e, TelegramBotAbstract telegramBotClient)
        {
            if (e == null || telegramBotClient == null)
                return null;

            if (e.Message.NewChatMembers == null || e.Message.NewChatMembers.Length == 0)
                return null;

            List<long> not_authorized_bot = new List<long>();
            foreach (Telegram.Bot.Types.User new_member in e.Message.NewChatMembers)
            {
                if (new_member.IsBot)
                {
                    not_authorized_bot.Add(new_member.Id);
                }
            }

            if (not_authorized_bot.Count == 0)
            {
                return null;
            }

            int user_that_added_bots = e.Message.From.Id;
            PoliNetworkBot_CSharp.Code.Objects.SuccessWithException is_admin = await telegramBotClient.IsAdminAsync(user_that_added_bots, e.Message.Chat.Id);
            if (is_admin.IsSuccess())
            {
                return null;
            }

            return not_authorized_bot;
        }

        private static async Task<Tuple<ToExit, Telegram.Bot.Types.ChatMember[], List<int>, string>> CheckIfToExit(TelegramBotAbstract telegramBotClient, MessageEventArgs e,
            object v)
        {
            switch (v)
            {
                case null:
                case DBNull _:
                    {
                        var r1 = await CheckIfToExit_NullValueAndUpdateIt(telegramBotClient, e);
                        var list1 = r1.Item3;
                        list1.Insert(0, 1);
                        return new Tuple<ToExit, ChatMember[], List<int>, string>(r1.Item1, r1.Item2, list1, null);
                    }
                case char b:
                    {
                        return b != 'Y' ? new Tuple<ToExit, ChatMember[], List<int>, string>(ToExit.EXIT, null, new List<int> { 6 }, b.ToString()) :
                            new Tuple<ToExit, ChatMember[], List<int>, string>(ToExit.STAY, null, new List<int> { 7 }, b.ToString());
                    }
                case string s when string.IsNullOrEmpty(s):
                    {
                        var r3 = await CheckIfToExit_NullValueAndUpdateIt(telegramBotClient, e);
                        var list3 = r3.Item3;
                        list3.Insert(0, 14);
                        return new Tuple<ToExit, ChatMember[], List<int>, string>(r3.Item1, r3.Item2, list3, s);
                    }
                case int i2:
                    {
                        if (i2 != 1)
                        {
                            return new Tuple<ToExit, ChatMember[], List<int>, string>(ToExit.EXIT, null, new List<int> { 41 }, i2.ToString());
                        }
                        else
                        {
                            return new Tuple<ToExit, ChatMember[], List<int>, string>(ToExit.STAY, null, new List<int> { 42 }, i2.ToString());
                        }
                    }
                case string s:
                    {
                        s = s.Trim();

                        if (!(s == "Y" || s == "1"))
                        {
                            return new Tuple<ToExit, ChatMember[], List<int>, string>(ToExit.EXIT, null, new List<int> { 8 }, s);
                        }
                        else
                        {
                            return new Tuple<ToExit, ChatMember[], List<int>, string>(ToExit.STAY, null, new List<int> { 9 }, s);
                        }
                    }
                default:
                    {
                        var r2 = await CheckIfToExit_NullValueAndUpdateIt(telegramBotClient, e);
                        var list2 = r2.Item3;
                        list2.Insert(0, 10);
                        return new Tuple<ToExit, ChatMember[], List<int>, string>(r2.Item1, r2.Item2, list2, v?.ToString());
                    }
            }
        }

        private static async Task<Tuple<ToExit, Telegram.Bot.Types.ChatMember[], List<int>>> CheckIfToExit_NullValueAndUpdateIt(TelegramBotAbstract telegramBotClient,
            MessageEventArgs e)
        {
            var r2 = await CheckIfToExit_NullValue2Async(telegramBotClient, e);
            var r = r2.Item1;
            string valid = r == ToExit.STAY ? "Y" : "N";

            string q = "UPDATE Groups SET valid = @valid WHERE id = @id";
            Dictionary<string, object> d = new Dictionary<string, object>() {
                {"@valid", valid },
                {"@id", e.Message.Chat.Id }
            };
            Utils.SqLite.Execute(q, d);

            var list1 = r2.Item3;
            list1.Insert(0, 2);
            return new Tuple<ToExit, Telegram.Bot.Types.ChatMember[], List<int>>(r, r2.Item2, list1);
        }

        private static async Task<Tuple<ToExit, Telegram.Bot.Types.ChatMember[], List<int>>> CheckIfToExit_NullValue2Async(TelegramBotAbstract telegramBotClient, MessageEventArgs e)
        {
            Telegram.Bot.Types.ChatMember[] r = await telegramBotClient.GetChatAdministratorsAsync(e.Message.Chat.Id);
            if (r == null)
                return new Tuple<ToExit, Telegram.Bot.Types.ChatMember[], List<int>>(ToExit.STAY, r, new List<int>() { 3 });

            foreach (var chatMember in r)
            {
                bool? isCreator = Utils.Creators.CheckIfIsCreatorOrSubCreator(chatMember);
                if (isCreator != null && isCreator.Value)
                    return new Tuple<ToExit, Telegram.Bot.Types.ChatMember[], List<int>>(ToExit.STAY, r, new List<int>() { 4 });
            }

            return new Tuple<ToExit, Telegram.Bot.Types.ChatMember[], List<int>>(ToExit.EXIT, r, new List<int>() { 5 });
        }

        private static void InsertGroup(TelegramBotAbstract sender, MessageEventArgs e)
        {
            const string q1 = "INSERT INTO Groups (id, bot_id, type, title) VALUES (@id, @botid, @type, @title)";
            SqLite.Execute(q1, new Dictionary<string, object>
            {
                {"@id", e.Message.Chat.Id},
                {"@botid", sender.GetId()},
                {"@type", e.Message.Chat.Type.ToString()},
                {"@title", e.Message.Chat.Title}
            });
            _ = CreateInviteLinkAsync(sender, e);
        }

        private static async Task<NuovoLink> CreateInviteLinkAsync(TelegramBotAbstract sender, MessageEventArgs e)
        {
            return await InviteLinks.CreateInviteLinkAsync(e.Message.Chat.Id, sender, e);
        }

        private static List<UsernameAndNameCheckResult> CheckUsername(MessageEventArgs e)
        {
            if (Data.GlobalVariables.NoUsernameCheckInThisChats.Contains(e.Message.Chat.Id))
            {
                return null;
            }

            if (e.Message != null && e.Message.From != null && Data.GlobalVariables.AllowedNoUsernameFromThisUserId.Contains(e.Message.From.Id))
                return null;

            var r = new List<UsernameAndNameCheckResult>
            {
                CheckUsername2(e.Message.From.Username, e.Message.From.FirstName,
                    lastName: e.Message.From.LastName, language: e.Message.From.LanguageCode,
                    userId: e.Message.From.Id, messageId: e.Message.MessageId)
            };

            if (e.Message.NewChatMembers == null || e.Message.NewChatMembers.Length == 0)
                return r;

            r.AddRange(from user in e.Message.NewChatMembers
                       where user != null && user.Id != r[0].GetUserId()
                       select CheckUsername2(user.Username, user.FirstName, user.Id,
                           user.LastName, user.LanguageCode, messageId: e.Message.MessageId));

            return r;
        }

        private static UsernameAndNameCheckResult CheckUsername2(string fromUsername, string fromFirstName, int userId,
            string lastName, string language, int? messageId)
        {
            var username = false;
            var name = false;

            if (string.IsNullOrEmpty(fromUsername))
            {
                if (Data.GlobalVariables.AllowedNoUsernameFromThisUserId.Contains(userId))
                {
                    username = false;
                }
                else
                {
                    username = true;
                }
            }

            if (fromFirstName.Length < 2)
                name = true;

            return new UsernameAndNameCheckResult(username, name, language,
                fromUsername, userId, fromFirstName, lastName, messageId);
        }

        public static SpamType CheckSpam(MessageEventArgs e)
        {
            if (e.Message != null && e.Message.Chat != null && e.Message.Chat.Type == ChatType.Private)
                return SpamType.ALL_GOOD;

            if (e.Message != null && e.Message.From != null && e.Message.Chat != null && (e.Message.From.Id == 777000 || e.Message.From.Id == e.Message.Chat.Id))
            {
                return SpamType.ALL_GOOD;
            }

            if (Code.Data.GlobalVariables.AllowedSpam.Contains(e.Message?.From?.Username?.ToLower()))
                return SpamType.ALL_GOOD;

            if (string.IsNullOrEmpty(e.Message.Text))
                return Utils.SpamTypeUtil.Merge(Blacklist.IsSpam(e.Message.Caption, e.Message.Chat.Id), Blacklist.IsSpam(e.Message.Photo));

            if (e.Message.Text.StartsWith("/"))
                return SpamType.ALL_GOOD;

            var isForeign = DetectForeignLanguage(e);

            if (isForeign)
                return SpamType.FOREIGN;

            return Utils.SpamTypeUtil.Merge(Blacklist.IsSpam(e.Message.Text, e.Message.Chat.Id), Blacklist.IsSpam(e.Message.Photo));
        }

        public static List<long> whitelistForeignGroups = new List<long>()
        {
            -1001394018284 //japan group
        };

        private static bool DetectForeignLanguage(MessageEventArgs e)
        {
            if (whitelistForeignGroups.Contains(e.Message.Chat.Id))
                return false;

            var koreanCharactersCount = Regex.Matches(e.Message.Text, @"[\uac00-\ud7a3]").Count;
            var japaneseCharactersCount = Regex.Matches(e.Message.Text, @"[\u3040-\u30ff]").Count;
            var chineseCharactersCount = Regex.Matches(e.Message.Text, @"[\u4e00-\u9FFF]").Count;

            if (koreanCharactersCount + japaneseCharactersCount + chineseCharactersCount >= 3)
                return true;

            return false;
        }

        private static async Task SendUsernameWarning(TelegramBotAbstract telegramBotClient,
            bool username, bool name, string lang, string usernameOfUser,
            long chatId, int userId, int? messageId, ChatType messageChatType,
            string firstName, string lastName, User[] newChatMembers)
        {
            var s1I = "Imposta un username e un nome più lungo dalle impostazioni di telegram per poter scrivere in questo gruppo\n";
            if (username && !name)
                s1I = "Imposta un username dalle impostazioni di telegram per poter scrivere in questo gruppo\n";
            else if (!username && name)
                s1I = "Imposta un nome più lungo " +
                      "dalle impostazioni di telegram\n";

            var s1E = "Set an username and a longer first name from telegram settings to write in this group\n";
            if (username && !name)
                s1E = "Set an username from telegram settings to write in this group\n";
            else if (!username && name)
                s1E = "Set a longer first name from telegram settings to write in this group\n";

            var s2 = new Language(new Dictionary<string, string>
            {
                {"it", s1I},
                {"en", s1E}
            });

            MessageSentResult r1 = await SendMessage.SendMessageInPrivateOrAGroup(telegramBotClient, s2, lang,
                usernameOfUser, userId, firstName, lastName, chatId, messageChatType, parseMode: ParseMode.Html);

            const int MINUTES_WAIT = 2;

            if (r1.GetChatType() != ChatType.Private)
            {
                var r2 = r1.GetMessage();
                if (r2 != null)
                {
                    if (r2 is TLMessage r3)
                    {
                        lock (Code.Data.GlobalVariables.MessagesToDelete)
                        {
                            TimeSpan timeUntilDelete = TimeSpan.FromMinutes(MINUTES_WAIT);
                            DateTime TimeToDelete = DateTime.Now + timeUntilDelete;

                            var toDelete = new Code.Objects.MessageToDelete(r3, chatId, TimeToDelete, telegramBotClient.GetId(), r1.GetChatType(), null);
                            Code.Data.GlobalVariables.MessagesToDelete.Add(toDelete);

                            Utils.FileSerialization.WriteToBinaryFile(Data.Constants.Paths.Bin.MessagesToDelete, Code.Data.GlobalVariables.MessagesToDelete);
                        }
                    }
                    else if (r2 is Telegram.Bot.Types.Message r4)
                    {
                        lock (Code.Data.GlobalVariables.MessagesToDelete)
                        {
                            TimeSpan timeUntilDelete = TimeSpan.FromMinutes(MINUTES_WAIT);
                            DateTime TimeToDelete = DateTime.Now + timeUntilDelete;

                            var toDelete = new Code.Objects.MessageToDelete(r4, chatId, TimeToDelete, telegramBotClient.GetId(), r1.GetChatType(), null);
                            Code.Data.GlobalVariables.MessagesToDelete.Add(toDelete);

                            Utils.FileSerialization.WriteToBinaryFile(Data.Constants.Paths.Bin.MessagesToDelete, Code.Data.GlobalVariables.MessagesToDelete);
                        }
                    }
                    else
                    {
                        string e4 = "Attempted to add a message to be deleted in queue\n" + r2?.GetType() + " " + r2?.ToString();
                        Exception e3 = new Exception(e4);
                        await Utils.NotifyUtil.NotifyOwners(e3, telegramBotClient);
                    }
                }
            }

            if (newChatMembers == null || newChatMembers.Length == 0)
            {
                await RestrictUser.Mute(60 * 5, telegramBotClient, chatId, userId, messageChatType);
            }

            if (messageId != null)
                await telegramBotClient.DeleteMessageAsync(chatId, messageId.Value, messageChatType, null);
        }

        public static async Task AntiSpamMeasure(TelegramBotAbstract telegramBotClient, MessageEventArgs e,
            SpamType checkSpam)
        {
            if (checkSpam == SpamType.ALL_GOOD)
                return;

            await RestrictUser.Mute(60 * 5, telegramBotClient, e.Message.Chat.Id, e.Message.From.Id, e.Message.Chat.Type);

            switch (checkSpam)
            {
                case SpamType.SPAM_LINK:
                    {
                        var text2 = new Language(new Dictionary<string, string>
                    {
                        {"en", "You sent a message with spam, and you were muted for 5 minutes"},
                        {"it", "Hai inviato un messaggio con spam, e quindi il bot ti ha mutato per 5 minuti"}
                    });

                        await SendMessage.SendMessageInPrivate(telegramBotClient, e.Message.From.Id,
                            e.Message.From.LanguageCode,
                            e.Message.From.Username, text2, parseMode: ParseMode.Default, null);

                        break;
                    }
                case SpamType.NOT_ALLOWED_WORDS:
                    {
                        var text2 = new Language(new Dictionary<string, string>
                    {
                        {"en", "You sent a message with banned words, and you were muted for 5 minutes"},
                        {"it", "Hai inviato un messaggio con parole bandite, e quindi il bot ti ha mutato per 5 minuti"}
                    });

                        await SendMessage.SendMessageInPrivate(telegramBotClient, e.Message.From.Id,
                            e.Message.From.LanguageCode,
                            e.Message.From.Username, text2, parseMode: ParseMode.Default, null);

                        break;
                    }
                case SpamType.FORMAT_INCORRECT:
                {
                    var text2 = new Language(new Dictionary<string, string>
                    {
                        {"en", "You have sent a message that does not follow the group format"},
                        {"it", "Hai inviato un messaggio che non rispetta il format del gruppo"}
                    });

                    await SendMessage.SendMessageInPrivate(telegramBotClient, e.Message.From.Id,
                        e.Message.From.LanguageCode,
                        e.Message.From.Username, text2, parseMode: ParseMode.Default, null);

                    break;
                }

                case SpamType.FOREIGN:
                    {
                        var text2 = new Language(new Dictionary<string, string>
                        {
                            {"en", "You sent a message with banned characters, and you were muted for 5 minutes"},
                            {
                                "it",
                                "Hai inviato un messaggio con caratteri banditi, e quindi il bot ti ha mutato per 5 minuti"
                            }
                        });

                        await SendMessage.SendMessageInPrivate(telegramBotClient, e.Message.From.Id,
                            e.Message.From.LanguageCode,
                            e.Message.From.Username, text2,
                           parseMode: ParseMode.Default, null);
                        break;
                    }

                // ReSharper disable once UnreachableSwitchCaseDueToIntegerAnalysis
                case SpamType.ALL_GOOD:
                    return;

                default:
                    throw new ArgumentOutOfRangeException(nameof(checkSpam), checkSpam, null);
            }

            await telegramBotClient.DeleteMessageAsync(e.Message.Chat.Id, e.Message.MessageId, e.Message.Chat.Type, null);
        }

        public static async Task<bool> CheckUsernameAndName(MessageEventArgs e, TelegramBotAbstract telegramBotClient)
        {
            var usernameCheck = CheckUsername(e);
            if (usernameCheck == null)
            {
                return false;
            }

            bool donesomething = false;

            foreach (var usernameCheck2 in usernameCheck)
                if (usernameCheck2 != null)
                    if (usernameCheck2.Name || usernameCheck2.UsernameBool)
                    {
                        await SendUsernameWarning(telegramBotClient,
                            usernameCheck2.UsernameBool,
                            usernameCheck2.Name,
                            usernameCheck2.GetLanguage(),
                            usernameCheck2.GetUsername(),
                            e.Message.Chat.Id,
                            usernameCheck2.GetUserId(),
                            usernameCheck2.GetMessageId(),
                            e.Message.Chat.Type,
                            usernameCheck2.GetFirstName(),
                            usernameCheck2.GetLastName(),
                            e.Message.NewChatMembers);

                        donesomething = true;
                    }

            return donesomething;
        }
    }
}