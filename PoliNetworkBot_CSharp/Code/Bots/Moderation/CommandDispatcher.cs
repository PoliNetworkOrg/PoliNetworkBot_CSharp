#region

using PoliNetworkBot_CSharp.Code.Data;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.TelegramMedia;
using PoliNetworkBot_CSharp.Code.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TeleSharp.TL;

#endregion

namespace PoliNetworkBot_CSharp.Code.Bots.Moderation
{
    internal static class CommandDispatcher
    {
        public static async Task CommandDispatcherMethod(TelegramBotAbstract sender, MessageEventArgs e)
        {
            var cmdLines = e.Message.Text.Split(' ');
            var cmd = cmdLines[0].Trim();
            if (cmd.Contains("@"))
            {
                string[] cmd2 = cmd.Split("@");
                string botUsername = await sender.GetBotUsernameAsync();
                if (cmd2[1].ToLower() != botUsername.ToLower())
                {
                    return;
                }
            }

            switch (cmd)
            {
                case "/start":
                    {
                        await Start(sender, e);
                        return;
                    }

                case "/force_check_invite_links":
                    {
                        if (GlobalVariables.Creators.Contains(e.Message?.Chat?.Username?.ToLower()))
                            _ = ForceCheckInviteLinksAsync(sender, e);
                        else
                            await DefaultCommand(sender, e);
                        return;
                    }

                case "/contact":
                    {
                        await ContactUs(sender, e);
                        return;
                    }

                case "/help":
                    {
                        await Help(sender, e);
                        return;
                    }

                case "/muteAll":
                    {
                        if (e.Message.Chat.Type != ChatType.Private)
                        {
                            await CommandNotSentInPrivateAsync(sender, e);
                            return;
                        }

                        if (GlobalVariables.AllowedMuteAll.Contains(e.Message.From?.Username?.ToLower()))
                            _ = MuteAllAsync(sender, e, cmdLines, e.Message.From.LanguageCode, e.Message.From.Username);
                        else
                            await DefaultCommand(sender, e);
                        return;
                    }

                case "/banAll":
                    {
                        if (e.Message.Chat.Type != ChatType.Private)
                        {
                            await CommandNotSentInPrivateAsync(sender, e);
                            return;
                        }

                        if (GlobalVariables.AllowedBanAll.Contains(e.Message.From?.Username?.ToLower()))
                            _ = BanAllAsync(sender, e, cmdLines, e.Message.From.LanguageCode, e.Message.From.Username);
                        else
                            await DefaultCommand(sender, e);
                        return;
                    }

                /*
            case "/massiveSend":
                {
                    if (e.Message.Chat.Type != ChatType.Private)
                    {
                        await CommandNotSentInPrivateAsync(sender, e);
                        return;
                    }

                    try
                    {
                        if (GlobalVariables.AllowedBanAll.Contains(e.Message.From?.Username?.ToLower()))
                            _ = MassiveSendAsync(sender, e, cmdLines, e.Message.From.LanguageCode, e.Message.From.Username);
                        else
                            await DefaultCommand(sender, e);
                    }
                    catch
                    {
                        ;
                    }

                    return;
                }
                */

                case "/ban":
                    {
                        _ = BanUserAsync(sender, e, cmdLines);
                        return;
                    }
                case "/banAllHistory":
                    {
                        // _ = BanUserAsync(sender, e, cmdLines);
                        _ = BanUserHistoryAsync(sender, e);
                        return;
                    }

                case "/unbanAll":
                    {
                        if (e.Message.Chat.Type != ChatType.Private)
                        {
                            await CommandNotSentInPrivateAsync(sender, e);
                            return;
                        }

                        if (GlobalVariables.AllowedBanAll.Contains(e.Message.From?.Username?.ToLower()))
                            _ = UnbanAllAsync(sender, e, cmdLines, e.Message.From.LanguageCode, e.Message.From.Username);
                        else
                            await DefaultCommand(sender, e);
                        return;
                    }

                case "/test_spam":
                    {
                        if (e.Message == null)
                            return;
                        if (e.Message.ReplyToMessage == null)
                            return;

                        await TestSpamAsync(e.Message.ReplyToMessage, sender, e);
                        return;
                    }

                case "/groups":
                    {
                        await SendRecommendedGroupsAsync(sender, e);
                        return;
                    }

                case "/getGroups":
                    {
                        if ((GlobalVariables.Creators.Contains(e.Message.From.Username) || Utils.Owners.CheckIfOwner(e.Message.From.Id))
                            && e.Message.Chat.Type == ChatType.Private)
                        {
                            string username = null;
                            if (!string.IsNullOrEmpty(e.Message.From.Username))
                                username = e.Message.From.Username;

                            _ = GetAllGroups(e.Message.From.Id, username, sender, e.Message.From.LanguageCode);
                            return;
                        }

                        await DefaultCommand(sender, e);

                        return;
                    }

                case "/testtime":
                    {
                        if (e.Message.Chat.Type == ChatType.Private)
                        {
                            await TestTime(sender, e);
                        }

                        return;
                    }

                case "/time":
                    {
                        var lang = new Language(new Dictionary<string, string>
                        {
                            {"", DateTimeClass.NowAsStringAmericanFormat()}
                        });
                        await SendMessage.SendMessageInPrivate(sender, userIdToSendTo: e.Message.From.Id,
                            usernameToSendTo: e.Message.From.Username, langCode: e.Message.From.LanguageCode,
                            text: lang, parseMode: ParseMode.Default, messageIdToReplyTo: null);
                        return;
                    }

                case "/assoc_write":
                case "/assoc_send":
                    {
                        _ = await Assoc.Assoc_SendAsync(sender, e);
                        return;
                    }

                case "/assoc_publish":
                    {
                        if (Utils.Owners.CheckIfOwner(e.Message.From.Id))
                            _ = await Assoc.Assoc_Publish(sender, e);
                        else
                            _ = await DefaultCommand(sender, e);
                        return;
                    }

                case "/assoc_read":
                    {
                        _ = await Assoc.Assoc_Read(sender, e, false);
                        return;
                    }

                case "/assoc_read_all":
                    {
                        if (Utils.Owners.CheckIfOwner(e.Message.From.Id))
                            _ = await Assoc.Assoc_ReadAll(sender, e);
                        else
                            _ = await DefaultCommand(sender, e);
                        return;
                    }

                case "/assoc_delete":
                case "/assoc_remove":
                    {
                        _ = await Assoc.Assoc_Delete(sender, e);
                        return;
                    }

                case "/rooms":
                    {
                        await Bots.Moderation.Rooms.RoomsMainAsync(sender, e);
                        return;
                    }

                case "/rules":
                    {
                        _ = await Rules(sender, e);
                        return;
                    }

                case "/qe":
                    {
                        _ = await QueryBot(execute_true_select_false: true, e, sender);
                        return;
                    }

                case "/qs":
                    {
                        _ = await QueryBot(execute_true_select_false: false, e, sender);
                        return;
                    }

                case "/update_links_from_json":
                    {
                        await InviteLinks.UpdateLinksFromJsonAsync(sender, e);
                        return;
                    }

                default:
                    {
                        await DefaultCommand(sender, e);
                        return;
                    }
            }
        }

        private static async Task TestSpamAsync(Message message, TelegramBotAbstract sender, MessageEventArgs e)
        {
            var r = Blacklist.IsSpam(message.Text, message.Chat.Id);
            var r2 = r.ToString();

            Dictionary<string, string> dict = new Dictionary<string, string>() {
                {"en", r2 }
            };
            Language text = new Language(dict);
            try
            {
                await sender.SendTextMessageAsync(e.Message.From.Id, text, ChatType.Private, "en", ParseMode.Default, null, null, null);
            }
            catch
            {
                ;
            }
        }

        private static async Task<object> MassiveSendAsync(TelegramBotAbstract sender, MessageEventArgs e, string[] cmdLines, string languageCode, string username)
        {
            System.Data.DataTable groups = Utils.SqLite.ExecuteSelect("Select id From Groups");

            if (groups == null || groups.Rows == null || groups.Rows.Count == 0)
            {
                Dictionary<string, string> dict = new Dictionary<string, string>() { { "en", "No groups!" } };
                await sender.SendTextMessageAsync(e.Message.From.Id, new Language(dict), ChatType.Private, e.Message.From.LanguageCode, ParseMode.Html, null, e.Message.From.Username, e.Message.MessageId, false);
            }
            int counter = 0;

            Dictionary<string, string> dict2 = new Dictionary<string, string>() {
                {
                    "en",
                    "Buonasera a tutti, vi ricordiamo che lunedì 24 fino al 27 verranno aperti i seggi online per le elezioni, fate sentire la vostra voce mi raccomando. <b>Votate!</b>\nPotete informarvi su modalità di voto e candidati al sito\npolinetworkelezioni.github.io/it" +
                    "\n\n\n" +
                    "Good evening everyone, we remind you that on Monday 24th to 27th the online polling stations will be open for the elections, please let your voice be heard. <b>Vote!</b>\nYou can find out about voting procedures and candidates in the website\npolinetworkelezioni.github.io/en"
                }
            };

            try
            {
                foreach (DataRow element in groups.Rows)
                {
                    try
                    {
                        long groupId = Convert.ToInt64(element.ItemArray[0]);

                        try
                        {
                            await SendMessage.SendMessageInAGroup(sender, "en", new Language(dict2), groupId, ChatType.Supergroup, ParseMode.Html, null, default, default);
                            counter++;
                        }
                        catch
                        {
                            try
                            {
                                await SendMessage.SendMessageInAGroup(sender, "en", new Language(dict2), groupId, ChatType.Group, ParseMode.Html, null, default, default);
                                counter++;
                            }
                            catch
                            {
                                ;
                            }
                        }
                    }
                    catch
                    {
                        ;
                    }

                    await Task.Delay(500);
                }
            }
            catch
            {
                ;
            }

            Language text = new Language(new Dictionary<string, string>() {
                {"en", "Sent in  " + counter + " groups"}
            });

            await Task.Delay(500);

            await sender.SendTextMessageAsync(e.Message.From.Id, text, ChatType.Private, e.Message.From.LanguageCode, ParseMode.Html, null, e.Message.From.Username, e.Message.MessageId, false);
            return true;
        }

        private static async Task<object> BanUserHistoryAsync(TelegramBotAbstract sender, MessageEventArgs e)
        {
            bool r = Utils.Owners.CheckIfOwner(e.Message.From.Id);
            if (!r)
            {
                return r;
            }

            string query = "SELECT DISTINCT T1.target FROM " +
                "(SELECT * FROM Banned WHERE banned_true_unbanned_false = 83 ORDER BY when_banned DESC) AS T1, " +
                "(SELECT * FROM Banned WHERE banned_true_unbanned_false != 83 ORDER BY when_banned DESC) AS T2 " +
                "WHERE (T1.target = T2.target AND T1.when_banned >= T2.when_banned AND T1.target IN (SELECT target FROM(SELECT target FROM Banned WHERE banned_true_unbanned_false != 83 ORDER BY when_banned DESC))) OR (T1.target NOT IN (SELECT target FROM (SELECT target FROM Banned WHERE banned_true_unbanned_false != 83 ORDER BY when_banned DESC)))";
            System.Data.DataTable x = Utils.SqLite.ExecuteSelect(query);

            if (x == null || x.Rows == null || x.Rows.Count == 0)
            {
                Language text3 = new Language(new Dictionary<string, string>() {
                    {"en", "There are no users to ban!"}
                });
                await sender.SendTextMessageAsync(e.Message.From.Id, text3, ChatType.Private, e.Message.From.LanguageCode, ParseMode.Html, null, e.Message.From.Username, e.Message.MessageId, false);
                return false;
            }

            System.Data.DataTable groups = Utils.SqLite.ExecuteSelect("Select id From Groups");
            /*
            if(e.Message.Text.Length !=10)
            {
                Language text2 = new Language(new Dictionary<string, string>() {
                    {"en", "Group not found (1)!"}
                });
                await sender.SendTextMessageAsync(e.Message.From.Id, text2, ChatType.Private, e.Message.From.LanguageCode, ParseMode.Html, null, e.Message.From.Username, e.Message.MessageId, false);
                return false;
            }
            */
            int counter = 0;
            string channel = Regex.Match(e.Message.Text, @"\d+").Value;
            if (groups.Select("id = " + "'" + channel + "'").Length != 1)
            {
                Language text2 = new Language(new Dictionary<string, string>() {
                    {"en", "Group not found! (2)"}
                });
                await sender.SendTextMessageAsync(e.Message.From.Id, text2, ChatType.Private, e.Message.From.LanguageCode, ParseMode.Html, null, e.Message.From.Username, e.Message.MessageId, false);
                return false;
            }

            foreach (DataRow element in x.Rows)
            {
                long userToBeBanned = Convert.ToInt64(element.ItemArray[0]);
                await RestrictUser.BanUserFromGroup(sender, e, userToBeBanned, Convert.ToInt64(channel), null);
                counter++;
            }

            Language text = new Language(new Dictionary<string, string>() {
                {"en", "Banned " + counter + " in group: " + groups.Select(channel)}
            });
            await sender.SendTextMessageAsync(e.Message.From.Id, text, ChatType.Private, e.Message.From.LanguageCode, ParseMode.Html, null, e.Message.From.Username, e.Message.MessageId, false);
            return true;
        }

        private static async Task<int?> QueryBot(bool execute_true_select_false, MessageEventArgs e, TelegramBotAbstract sender)
        {
            if (e.Message.ForwardFrom != null)
                return null;

            if (e.Message.From == null)
                return null;

            if (GlobalVariables.IsOwner(e.Message.From.Id))
            {
                return await QueryBot2(execute_true_select_false, e, sender);
            }

            return null;
        }

        private static async Task<int?> QueryBot2(bool execute_true_select_false, MessageEventArgs e, TelegramBotAbstract sender)
        {
            if (e.Message.ReplyToMessage == null || string.IsNullOrEmpty(e.Message.ReplyToMessage.Text))
            {
                Language text = new Language(new Dictionary<string, string>() {
                    {"en", "You have to reply to a message containing the query" }
                });
                await sender.SendTextMessageAsync(e.Message.From.Id, text, ChatType.Private, e.Message.From.LanguageCode, ParseMode.Html, null, e.Message.From.Username, e.Message.MessageId, false);
                return null;
            }

            string query = e.Message.ReplyToMessage.Text;
            if (execute_true_select_false)
            {
                var i = Utils.SqLite.Execute(query);

                Language text = new Language(new Dictionary<string, string>() {
                    {"en", "Query execution. Result: " + i.ToString() }
                });
                await sender.SendTextMessageAsync(e.Message.From.Id, text, ChatType.Private, e.Message.From.LanguageCode, ParseMode.Html, null, e.Message.From.Username, e.Message.MessageId, false);
                return i;
            }
            else
            {
                System.Data.DataTable x = Utils.SqLite.ExecuteSelect(query);
                MemoryStream x2 = Utils.StreamSerialization.SerializeToStream(x);
                TelegramFile documentInput = new TelegramFile(x2, "table.bin", "Query result", "application/octet-stream");
                TLAbsInputPeer peer2 = new TLInputPeerUser() { UserId = e.Message.From.Id };
                Tuple<TLAbsInputPeer, long> peer = new Tuple<TLAbsInputPeer, long>(peer2, e.Message.From.Id);
                Language text2 = new Language(new Dictionary<string, string>() {
                    {"en", "Query result" }
                });
                bool v = await sender.SendFileAsync(documentInput, peer, text2, TextAsCaption.AS_CAPTION, e.Message.From.Username, e.Message.From.LanguageCode, e.Message.MessageId, false);
                return v ? 1 : 0;
            }
        }

        private async static Task<MessageSentResult> TestTime(TelegramBotAbstract sender, MessageEventArgs e)
        {
            Tuple<DateTimeSchedule, Exception, string> sentDate = await DateTimeClass.AskDateAsync(e.Message.From.Id, e.Message.Text,
                    e.Message.From.LanguageCode, sender, e.Message.From.Username);

            if (sentDate.Item2 != null)
            {
                await Utils.NotifyUtil.NotifyOwners(new ExceptionNumbered(sentDate.Item2), sender, 0, sentDate.Item3);

                return null;
            }

            DateTime? sentDate2 = sentDate.Item1.GetDate();

            Dictionary<string, string> dict = new Dictionary<string, string>() {
                {"en", Utils.DateTimeClass.DateTimeToItalianFormat(sentDate2) }
            };
            Language text = new Language(dict);
            return await Utils.SendMessage.SendMessageInPrivate(sender, e.Message.From.Id,
                e.Message.From.LanguageCode, e.Message.From.Username,
                text, ParseMode.Default, e.Message.MessageId);
        }

        private static async Task<MessageSentResult> Rules(TelegramBotAbstract sender, MessageEventArgs e)
        {
            const string text = "Ecco le regole!\n" +
                "https://polinetwork.github.io/it/rules";

            const string textEng = "Here are the rules!\n" +
                "https://polinetwork.github.io/en/rules";

            var text2 = new Language(new Dictionary<string, string>
            {
                {"en", textEng},
                {"it", text}
            });

            return await SendMessage.SendMessageInPrivate(sender, e.Message.From.Id,
                e.Message.From.LanguageCode,
                e.Message.From.Username, text2, ParseMode.Html, null);
        }

        private static async Task SendRecommendedGroupsAsync(TelegramBotAbstract sender, MessageEventArgs e)
        {
            const string text = "<i>Lista di gruppi consigliati</i>:\n" +
                "\n👥 Gruppo di tutti gli studenti @PoliGruppo 👈\n" +
                "\n📖 Libri @PoliBook\n" +
                "\n🤪 Spotted & Memes @PolimiSpotted @PolimiMemes\n" +
                "\n🥳 Eventi @PoliEventi\n" +
                "\nRicordiamo che sul nostro sito vi sono tutti i link ai gruppi con tanto ricerca, facci un salto!\n" +
                "https://polinetwork.github.io/";

            const string textEng = "<i>List of recommended groups</i>:\n" +
                "\n👥 Group with all students @PoliGruppo 👈\n" +
                "\n📖 Books @PoliBook\n" +
                "\n🤪 Spotted & Memes @PolimiSpotted @PolimiMemes\n" +
                "\n🥳 Events @PoliEventi\n" +
                "\nWe remind you that on our website there are all link to the groups, and they are searchable, have a look!\n" +
                "https://polinetwork.github.io/";

            var text2 = new Language(new Dictionary<string, string>
            {
                {"en", textEng},
                {"it", text}
            });
            await SendMessage.SendMessageInPrivate(sender, e.Message.From.Id,
                e.Message.From.LanguageCode,
                e.Message.From.Username, text2, ParseMode.Html, null);
        }

        public static async Task<bool> GetAllGroups(long chatId, string username, TelegramBotAbstract sender,
            string lang)
        {
            System.Data.DataTable groups = Groups.GetAllGroups();
            Stream stream = new MemoryStream();
            FileSerialization.SerializeFile(groups, ref stream);
            TLAbsInputPeer peer2 = new TLInputPeerUser { UserId = (int)chatId };
            var peer = new Tuple<TLAbsInputPeer, long>(peer2, chatId);

            var text2 = new Language(new Dictionary<string, string>
            {
                {"en", "Here are all groups:"},
                {"it", "Ecco tutti i gruppi:"}
            });
            return await SendMessage.SendFileAsync(new TelegramFile(stream, "groups.bin",
                    null, "application/octet-stream"), peer,
                text2, TextAsCaption.BEFORE_FILE,
                sender, username, lang, null, true);
        }

        private static async Task<PoliNetworkBot_CSharp.Code.Objects.SuccessWithException> BanUserAsync(TelegramBotAbstract sender, MessageEventArgs e,
            string[] stringInfo)
        {
            PoliNetworkBot_CSharp.Code.Objects.SuccessWithException r = await Groups.CheckIfAdminAsync(e.Message.From.Id, e.Message.From.Username, e.Message.Chat.Id, sender);
            if (!r.IsSuccess())
            {
                return r;
            }

            if (e.Message.ReplyToMessage == null)
            {
                var userIdFound = await Info.GetTargetUserIdAsync(stringInfo[1], sender);
                if (userIdFound == null)
                {
                    Exception e2 = new Exception("Can't find userid (1)");
                    await Utils.NotifyUtil.NotifyOwners(new ExceptionNumbered(e2), sender);
                    return new PoliNetworkBot_CSharp.Code.Objects.SuccessWithException(false, e2);
                }
                else
                {
                    int? targetId = userIdFound.GetID();
                    if (targetId == null)
                    {
                        Exception e2 = new Exception("Can't find userid (2)");
                        await Utils.NotifyUtil.NotifyOwners(new ExceptionNumbered(e2), sender, 0);
                        return new PoliNetworkBot_CSharp.Code.Objects.SuccessWithException(false, e2);
                    }
                    else
                    {
                        return await RestrictUser.BanUserFromGroup(sender, e, targetId.Value, e.Message.Chat.Id, null);
                    }
                }
            }
            else
            {
                var targetInt = e.Message.ReplyToMessage.From.Id;
                return await RestrictUser.BanUserFromGroup(sender, e, targetInt, e.Message.Chat.Id, stringInfo);
            }
        }

        private static async Task<PoliNetworkBot_CSharp.Code.Objects.SuccessWithException> UnbanAllAsync(TelegramBotAbstract sender, MessageEventArgs e, string[] target, string lang, string username)
        {
            return await BanAllUnbanAllMethod1Async2Async(sender, e, target, lang, username, Enums.RestrictAction.UNBAN);
        }

        private static async Task<PoliNetworkBot_CSharp.Code.Objects.SuccessWithException> BanAllAsync(TelegramBotAbstract sender, MessageEventArgs e,
            string[] target, string lang, string username)
        {
            return await BanAllUnbanAllMethod1Async2Async(sender, e, target, lang, username, Enums.RestrictAction.BAN);
        }

        private static async Task<PoliNetworkBot_CSharp.Code.Objects.SuccessWithException> MuteAllAsync(TelegramBotAbstract sender, MessageEventArgs e, string[] target, string lang, string username)
        {
            return await BanAllUnbanAllMethod1Async2Async(sender, e, target, lang, username, Enums.RestrictAction.MUTE);
        }

        private static async Task<SuccessWithException> BanAllUnbanAllMethod1Async2Async(TelegramBotAbstract sender, MessageEventArgs e,
            string[] target, string lang, string username, RestrictAction bAN)
        {
            ValueWithException<DateTime?> d1 = GetDateTime(target);
            try
            {
                if (d1 == null)
                {
                    await BanAllUnbanAllMethod1Async(bAN, GetFinalTarget(e, target), e, sender, lang, username, until: null);
                    return new SuccessWithException(true);
                }

                DateTime? d2 = d1.GetValue();
                await BanAllUnbanAllMethod1Async(bAN, GetFinalTarget(e, target), e, sender, lang, username, d2);
            }
            catch (Exception ex)
            {
                List<Exception> ex2 = Concat(ex, d1);
                return new PoliNetworkBot_CSharp.Code.Objects.SuccessWithException(false, ex2);
            }

            return new PoliNetworkBot_CSharp.Code.Objects.SuccessWithException(true, d1.GetExceptions());
        }

        private static List<Exception> Concat(Exception ex, ValueWithException<DateTime?> d1)
        {
            List<Exception> r = new List<Exception>
            {
                ex
            };
            if (d1 != null && d1.ContainsExceptions())
            {
                r.AddRange(d1.GetExceptions());
            }

            return r;
        }

        private static ValueWithException<DateTime?> GetDateTime(string[] target)
        {
            if (target == null)
                return null;
            if (target.Length < 3)
                return null;

            string s = "";
            for (int i = 2; i < target.Length; i++)
            {
                s += target[i] + " ";
            }
            s = s.Trim();
            return Utils.DateTimeClass.GetFromString(s);
        }

        private static async Task BanAllUnbanAllMethod1Async(Code.Enums.RestrictAction restrictAction, string finalTarget,
            MessageEventArgs e, TelegramBotAbstract sender, string lang, string username, DateTime? until)
        {
            if (string.IsNullOrEmpty(finalTarget))
            {
                var lang2 = new Language(new Dictionary<string, string>
                    {
                        {"en", "We can't find the target."},
                        {"it", "Non riusciamo a trovare il bersaglio"}
                    });
                await sender.SendTextMessageAsync(e.Message.From.Id, lang2, ChatType.Private,
                    lang, ParseMode.Default, username: username,
                    replyMarkupObject: new ReplyMarkupObject(ReplyMarkupEnum.REMOVE));

                return;
            }

            Tuple<BanUnbanAllResult, List<ExceptionNumbered>, int> done = await RestrictUser.BanAllAsync(sender, e, finalTarget, restrictAction, until);
            var text2 = done.Item1.GetLanguage(restrictAction, finalTarget, done.Item3);

            await SendMessage.SendMessageInPrivate(sender, e.Message.From.Id,
                e.Message.From.LanguageCode,
                e.Message.From.Username, text2,
                parseMode: ParseMode.Default,
                e.Message.MessageId);

            await SendReportOfSuccessAndFailures(sender, e, done);
        }

        private static async Task SendReportOfSuccessAndFailures(TelegramBotAbstract sender, MessageEventArgs e,
            Tuple<BanUnbanAllResult, List<ExceptionNumbered>, int> done)
        {
            try
            {
                await SendReportOfSuccessAndFailures2(StreamSerialization.SerializeToStream(done.Item1.GetSuccess()), "success.bin", sender, e);
                await SendReportOfSuccessAndFailures2(StreamSerialization.SerializeToStream(done.Item1.GetFailed()), "failed.bin", sender, e);
            }
            catch
            {
                ;
            }
        }

        private static async Task SendReportOfSuccessAndFailures2(Stream stream, string filename, TelegramBotAbstract sender, MessageEventArgs e)
        {
            TelegramFile file = new TelegramFile(stream, filename, "", "application/octet-stream");
            Tuple<TLAbsInputPeer, long> peer = new Tuple<TLAbsInputPeer, long>(null, e.Message.From.Id);
            Language text = new Language(dict: new Dictionary<string, string>() {
                {"en", "" }
            });
            await Utils.SendMessage.SendFileAsync(file, peer, text, TextAsCaption.AS_CAPTION,
                sender, e.Message.From.Username, e.Message.From.LanguageCode, null, true);
        }

        private static string GetFinalTarget(MessageEventArgs e, IReadOnlyList<string> target)
        {
            return (e.Message.ReplyToMessage == null && target.Count >= 2) ? target[1] : e.Message.ReplyToMessage.From.Id.ToString();
        }

        private static async Task<bool> DefaultCommand(TelegramBotAbstract sender, MessageEventArgs e)
        {
            var text2 = new Language(new Dictionary<string, string>
            {
                {
                    "en",
                    "I'm sorry, but I don't know this command. Try to ask the administrators (/contact)"
                },
                {
                    "it",
                    "Mi dispiace, ma non conosco questo comando. Prova a contattare gli amministratori (/contact)"
                }
            });
            await SendMessage.SendMessageInPrivate(sender, e.Message.From.Id,
                e.Message.From.LanguageCode,
                e.Message.From.Username, text2,
                parseMode: ParseMode.Default,
                null);

            return true;
        }

        private static async Task Help(TelegramBotAbstract sender, MessageEventArgs e)
        {
            if (e.Message.Chat.Type == ChatType.Private)
            {
                await HelpPrivate(sender, e);
            }
            else
            {
                await CommandNotSentInPrivateAsync(sender, e);
            }
        }

        private static async Task CommandNotSentInPrivateAsync(TelegramBotAbstract sender, MessageEventArgs e)
        {
            var lang = new Language(new Dictionary<string, string>
                {
                    {"it", "Questo messaggio funziona solo in chat privata"},
                    {"en", "This command only works in private chat with me"}
                });
            await SendMessage.SendMessageInPrivateOrAGroup(sender,
                lang, e.Message.From.LanguageCode, e.Message.From.Username, e.Message.From.Id,
                e.Message.From.FirstName, e.Message.From.LastName, e.Message.Chat.Id, e.Message.Chat.Type);

            await sender.DeleteMessageAsync(e.Message.Chat.Id, e.Message.MessageId, e.Message.Chat.Type, null);
        }

        private static async Task HelpPrivate(TelegramBotAbstract sender, MessageEventArgs e)
        {
            const string text = "<i>Lista di funzioni</i>:\n" +
                                //"\n📑 Sistema di recensioni dei corsi (per maggiori info /help_review)\n" +
                                //"\n🔖 Link ai materiali nei gruppi (per maggiori info /help_material)\n" +
                                "\n🙋 <a href='https://polinetwork.github.io/it/faq/index.html'>" +
                                "FAQ (domande frequenti)</a>\n" +
                                "\n🏫 Ricerca aule libere /rooms\n" +
                                //"\n🕶️ Sistema di pubblicazione anonima (per maggiori info /help_anon)\n" +
                                //"\n🎙️ Registrazione delle lezioni (per maggiori info /help_record)\n" +
                                "\n👥 Gruppo consigliati e utili /groups\n" +
                                "\n⚠ Hai già letto le regole del network? /rules\n" +
                                "\n✍ Per contattarci /contact";

            const string textEng = "<i>List of features</i>:\n" +
                                   //"\n📑 Review system of courses (for more info /help_review)\n" +
                                   //"\n🔖 Link to notes (for more info /help_material)\n" +
                                   "\n🙋 <a href='https://polinetwork.github.io/it/faq/index.html'>" +
                                   "FAQ (frequently asked questions)</a>\n" +
                                   "\n🏫 Find free rooms /rooms\n" +
                                   //"\n🕶️ Anonymous posting system (for more info /help_anon)\n" +
                                   //"\n🎙️ Record of lessons (for more info /help_record)\n" +
                                   "\n👥 Recommended groups /groups\n" +
                                   "\n⚠ Have you already read our network rules? /rules\n" +
                                   "\n✍ To contact us /contact";

            var text2 = new Language(new Dictionary<string, string>
            {
                {"en", textEng},
                {"it", text}
            });
            await SendMessage.SendMessageInPrivate(sender, e.Message.From.Id,
                e.Message.From.LanguageCode,
                e.Message.From.Username, text2, ParseMode.Html, null);
        }

        private static async Task ContactUs(TelegramBotAbstract telegramBotClient, MessageEventArgs e)
        {
            await DeleteMessage.DeleteIfMessageIsNotInPrivate(telegramBotClient, e);
            var lang2 = new Language(new Dictionary<string, string>
            {
                {"it", telegramBotClient.GetContactString()},
                {"en", telegramBotClient.GetContactString()}
            });
            await telegramBotClient.SendTextMessageAsync(e.Message.Chat.Id,
                lang2, e.Message.Chat.Type, e.Message.From.LanguageCode,
                ParseMode.Default,
                new ReplyMarkupObject(ReplyMarkupEnum.REMOVE), e.Message.From.Username
            );
        }

        private static async Task ForceCheckInviteLinksAsync(TelegramBotAbstract sender, MessageEventArgs e)
        {
            int? n = null;
            try
            {
                n = await InviteLinks.FillMissingLinksIntoDB_Async(sender);
            }
            catch (Exception e2)
            {
                await Utils.NotifyUtil.NotifyOwners(new ExceptionNumbered(e2), sender, 0);
            }

            if (n == null)
                return;

            var text2 = new Language(new Dictionary<string, string>
            {
                {"en", "I have updated n=" + n + " links"},
                {"it", "Ho aggiornato n=" + n + " link"}
            });
            await SendMessage.SendMessageInPrivate(sender, e.Message.From.Id,
                e.Message.From.LanguageCode,
                e.Message.From.Username, text2,
                parseMode: ParseMode.Default,
                e.Message.MessageId);
        }

        private static async Task Start(TelegramBotAbstract telegramBotClient, MessageEventArgs e)
        {
            await DeleteMessage.DeleteIfMessageIsNotInPrivate(telegramBotClient, e);
            var lang2 = new Language(new Dictionary<string, string>
            {
                {
                    "it", "Ciao! 👋\n" +
                          "\nScrivi /help per la lista completa delle mie funzioni 👀\n" +
                          "\nVisita anche il nostro sito " + telegramBotClient.GetWebSite()
                },
                {
                    "en", "Hi! 👋\n" +
                          "\nWrite /help for the complete list of my functions👀\n" +
                          "\nAlso visit our site " + telegramBotClient.GetWebSite()
                }
            });
            await telegramBotClient.SendTextMessageAsync(e.Message.Chat.Id,
                lang2,
                e.Message.Chat.Type, replyMarkupObject: new ReplyMarkupObject(ReplyMarkupEnum.REMOVE),
                lang: e.Message.From.LanguageCode, username: e.Message.From.Username, parseMode: ParseMode.Default
            );
        }
    }
}