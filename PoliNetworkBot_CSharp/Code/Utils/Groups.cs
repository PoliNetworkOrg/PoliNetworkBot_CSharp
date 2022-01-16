#region

using JsonPolimi_Core_nf.Tipi;
using PoliNetworkBot_CSharp.Code.Bots.Anon;
using PoliNetworkBot_CSharp.Code.Data;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils
{
    internal static class Groups
    {
        private static readonly Dictionary<long, InfoChat> GroupsInRam = new();

        public static async Task<DataTable> GetGroupsAndFixNames(TelegramBotAbstract telegramBotAbstract)
        {
            await FixAllGroupsName(telegramBotAbstract);
            return GetAllGroups();
        }

        internal static DataTable GetAllGroups()
        {
            const string q1 = "SELECT * FROM Groups";
            return SqLite.ExecuteSelect(q1);
        }

        internal static DataTable GetGroupsByTitle(string query, int limit)
        {
            string q1 = "SELECT id,title,link " +
                "FROM Groups " +
                "WHERE title LIKE @title " +
                "AND ( valid = 'Y' or valid = 1 ) COLLATE NOCASE LIMIT " + limit.ToString();
            var seo = query.Split(" ");
            var query2 = seo.Aggregate("", (current, word) => current + ('%' + word));
            query2 += "%";
            return SqLite.ExecuteSelect(q1, new Dictionary<string, object> { { "@title", query2 } });
        }

        internal static async Task<SuccessWithException> CheckIfAdminAsync(long userId, string username, long chatId,
            TelegramBotAbstract telegramBotAbstract)
        {
            if (GlobalVariables.Creators.Contains(username))
                return new SuccessWithException(true);

            return await telegramBotAbstract.IsAdminAsync(userId, chatId);
        }

        public static async Task FixAllGroupsName(TelegramBotAbstract telegramBotAbstract)
        {
            try
            {
                Logger.WriteLine("Starting fix of groups name");
                ;
                const string q1 = "SELECT * FROM Groups";
                var groups = SqLite.ExecuteSelect(q1);
                var indexTitle = groups.Columns.IndexOf("title");
                var indexId = groups.Columns.IndexOf("id");
                for (var i = 0; i < groups.Rows.Count; i++)
                {
                    long indexIdInTable = (long)groups.Rows[i]?[indexId];
                    var oldTitle = (string)groups.Rows[i]?[indexTitle];
                    string newTitle = "";
                    try
                    {
                        await Task.Delay(300);
                        Tuple<Telegram.Bot.Types.Chat, Exception> newTitleWithException = null;
                        int e = 0;
                        while ((newTitleWithException == null || newTitleWithException.Item2 is Telegram.Bot.Exceptions.ApiRequestException)
                            && e < 3)
                        {
                            newTitleWithException = await telegramBotAbstract.GetChat(indexIdInTable);
                            if (newTitleWithException.Item2 is Telegram.Bot.Exceptions.ApiRequestException)
                            {
                                await Task.Delay(1000 * 60 * 5);
                            }
                            e++;
                        }
                        newTitle = newTitleWithException?.Item1?.Title;
                        GroupCheckAndUpdate(telegramBotAbstract, indexIdInTable, oldTitle, newTitleWithException);
                    }
                    catch (Exception e2)
                    {
                        var e3 = new Exception("Unexpected exception in FixAllGroupsName \n\noldTitle: " + oldTitle +
                            "\n NewTitle: " + newTitle + "\n\n" + e2);
                        await NotifyUtil.NotifyOwners(e3, telegramBotAbstract);
                    }
                }
                Logger.GroupsFixLog.SendLog(telegramBotAbstract);
            }
            catch (Exception e)
            {
                await NotifyUtil.NotifyOwners(e, telegramBotAbstract);
            }
        }

        internal static void CheckForGroupUpdate(TelegramBotAbstract telegramBotClient, MessageEventArgs e)
        {
            try
            {
                if (e.Message?.Chat?.Id == null || e.Message?.Chat?.Title == null)
                    return;

                GroupsFixLogUpdatedEnum groupsFixLogUpdatedEnum = CheckForGroupUpdateAsync2(e.Message.Chat);

                if (groupsFixLogUpdatedEnum == GroupsFixLogUpdatedEnum.NEW_NAME)
                {
                    Logger.GroupsFixLog.SendLog(telegramBotClient, GroupsFixLogUpdatedEnum.NEW_NAME);
                }

                _ = CheckIfInviteIsWorking(e, telegramBotClient);
            }
            catch (Exception ex)
            {
                _ = NotifyUtil.NotifyOwners(ex, telegramBotClient);
            }
        }

        private static async Task CheckIfInviteIsWorking(MessageEventArgs e, TelegramBotAbstract telegramBotClient)
        {
            try
            {
                if (e.Message?.Chat?.Id == null || e.Message?.Chat?.Title == null)
                    return;
                InfoChat infoChat;
                bool? getDone;

                lock (GroupsInRam)
                {
                    getDone = GroupsInRam.TryGetValue(e.Message.Chat.Id, out infoChat);
                }

                if (infoChat != null && getDone.Value)
                {
                    if (infoChat.IsInhibited())
                    {
                        return;
                    }
                }
                else
                {
                    infoChat = new InfoChat(e.Message.Chat, DateTime.Now);
                    lock (GroupsInRam)
                    {
                        GroupsInRam[e.Message.Chat.Id] = infoChat;
                    }
                }
                const string q1 = "SELECT * FROM Groups WHERE id = @id";
                var groups = SqLite.ExecuteSelect(q1, new Dictionary<string, object> { { "@id", e.Message.Chat.Id } });
                if (groups.Rows.Count == 0)
                {
                    throw new Exception("No group found with id: " + e.Message.Chat.Id + " while running CheckForGroupTitleUpdateAsync");
                }
                var indexId = groups.Columns.IndexOf("id");
                var indexIdInTable = (long)groups.Rows[0][indexId];

                var l = new ListaGruppo();

                var g = l.CreaGruppo(groups.Rows[0]);

                var linkFunzionante = g.CheckSeIlLinkVa(false, null);

                if (linkFunzionante != null && !linkFunzionante.Value)
                {
                    var nuovoLink = await InviteLinks.CreateInviteLinkAsync(indexIdInTable, telegramBotClient);
                    if (nuovoLink.isNuovo != SuccessoGenerazioneLink.ERRORE)
                    {
                        await NotifyUtil.NotifyOwners("Fixed link for group " + e.Message.Chat.Title + " id: " + e.Message.Chat.Id, telegramBotClient);
                    }
                }

                infoChat.UpdateTimeOfLastLinkCheck();
            }
            catch (Exception ex)
            {
                _ = NotifyUtil.NotifyOwners(ex, telegramBotClient);
            }
        }

        private static GroupsFixLogUpdatedEnum CheckForGroupUpdateAsync2(Chat group)
        {
            InfoChat telegramGroup;
            bool? groupInRamGetDone;
            lock (GroupsInRam)
            {
                groupInRamGetDone = GroupsInRam.TryGetValue(group.Id, out telegramGroup);
            }

            if (telegramGroup != null && groupInRamGetDone.Value)
            {
                if (group.Title == telegramGroup._Chat.Title) return GroupsFixLogUpdatedEnum.DID_NOTHING;
                lock (GroupsInRam)
                {
                    GroupsInRam.Remove(@group.Id);
                    GroupsInRam.Add(@group.Id, new InfoChat(@group, DateTime.Now));
                }
                return GroupCheckAndUpdate2(@group.Id, @group.Title, telegramGroup._Chat.Title);
            }

            const string q1 = "SELECT * FROM Groups WHERE id = @id";
            var groups = SqLite.ExecuteSelect(q1, new Dictionary<string, object> { { "@id", group.Id } });
            if (groups.Rows.Count == 0)
            {
                throw new Exception("No group found with id: " + group.Id + " while running CheckForGroupTitleUpdateAsync");
            }

            var row = groups.Rows[0];

            if (row == null)
                return GroupsFixLogUpdatedEnum.UNKNOWN;

            lock (GroupsInRam)
            {
                if (!GroupsInRam.ContainsKey(group.Id))
                    GroupsInRam[group.Id] = new InfoChat(group, DateTime.Now);
                else
                {
                    GroupsInRam[group.Id]._Chat = group;
                }
            }

            var indexTitle = groups.Columns.IndexOf("title");
            var oldTitle = (string)row[indexTitle];

            return oldTitle != @group.Title ? GroupCheckAndUpdate2(@group.Id, @group.Title, oldTitle) : GroupsFixLogUpdatedEnum.DID_NOTHING;
        }

        private static GroupsFixLogUpdatedEnum GroupCheckAndUpdate(TelegramBotAbstract telegramBotAbstract,
            long indexIdInTable,
            string oldTitle,
            Tuple<Telegram.Bot.Types.Chat, Exception> newTitleWithException)
        {
            string newTitle = newTitleWithException?.Item1?.Title;
            if (String.IsNullOrEmpty(oldTitle) && String.IsNullOrEmpty(newTitle))
            {
                Logger.GroupsFixLog.OldNullNewNull(newTitleWithException?.Item1?.Id, indexIdInTable);
                Logger.GroupsFixLog.CountIgnored();
                return GroupsFixLogUpdatedEnum.DID_NOTHING;
            }
            if (String.IsNullOrEmpty(newTitle))
            {
                Logger.GroupsFixLog.NewNull(indexIdInTable, oldTitle, newTitleWithException?.Item2);
                Logger.GroupsFixLog.CountIgnored();
                return GroupsFixLogUpdatedEnum.DID_NOTHING;
            }
            if (String.IsNullOrEmpty(oldTitle))
            {
                Logger.GroupsFixLog.OldNull(newTitle);
                oldTitle = "[previously null]";
            }
            if (oldTitle == newTitle)
            {
                Logger.GroupsFixLog.CountIgnored();
                return GroupsFixLogUpdatedEnum.DID_NOTHING;
            }

            return GroupCheckAndUpdate2(indexIdInTable, newTitle, oldTitle);
        }

        private static GroupsFixLogUpdatedEnum GroupCheckAndUpdate2(long id, string newTitle, string oldTitle)
        {
            var q = "UPDATE Groups SET title = @title WHERE id = @id";

            var d = new Dictionary<string, object>
                        {
                            {"@title", newTitle},
                            {"@id", id}
                        };
            SqLite.Execute(q, d);
            Logger.GroupsFixLog.NameChange(oldTitle, newTitle);
            return GroupsFixLogUpdatedEnum.NEW_NAME;
        }

        internal static async Task SendMessageExitingAndThenExit(TelegramBotAbstract telegramBotClient, MessageEventArgs e)
        {
            try
            {
                switch (e.Message.Chat.Type)
                {
                    case Telegram.Bot.Types.Enums.ChatType.Group:
                    case Telegram.Bot.Types.Enums.ChatType.Supergroup:
                        {
                            try
                            {
                                Dictionary<string, string> dict = new()
                                {
                                    {
                                        "it",
                                        "Il bot non è autorizzato in questo gruppo. Contattare gli amministratori di PoliNetwork."
                                    },
                                    {
                                        "en",
                                        "The bot is not authorized in this group. Contact the PoliNetwork administrators."
                                    }
                                };
                                Language lang = new(dict);

                                await Utils.SendMessage.SendMessageInAGroup(
                                        telegramBotClient, e.Message.From.LanguageCode, lang,
                                        e.Message.Chat.Id, e.Message.Chat.Type,
                                        Telegram.Bot.Types.Enums.ParseMode.Html, null, true
                                    );
                            }
                            catch
                            {
                                ;
                            }

                            await telegramBotClient.ExitGroupAsync(e);
                        }
                        break;

                    default:
                        break;
                }
            }
            catch
            {
                ;
            }
        }
    }
}