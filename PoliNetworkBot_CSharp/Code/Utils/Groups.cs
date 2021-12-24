#region

using JsonPolimi_Core_nf.Data;
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
using JsonPolimi_Core_nf.Utils;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils
{
    internal static class Groups
    {
        private static readonly Dictionary<long, DateTime> _inhibitionPeriod = new();

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

            return SqLite.ExecuteSelect(q1, new Dictionary<string, object> { { "@title", '%' + query + '%' } });
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

        internal static async Task CheckForGroupUpdateAsync(TelegramBotAbstract telegramBotClient, MessageEventArgs e)
        {
            try
            {
                if (e.Message?.Chat?.Id == null || e.Message?.Chat?.Title == null)
                    return;
                if (_inhibitionPeriod.TryGetValue(e.Message.Chat.Id, out var lastUpdate) && lastUpdate.AddHours(24) > DateTime.Now)
                    return;
                _inhibitionPeriod.Remove(e.Message.Chat.Id);
                _inhibitionPeriod.TryAdd(e.Message.Chat.Id, DateTime.Now);
                const string q1 = "SELECT * FROM Groups WHERE id = @id";
                var groups = SqLite.ExecuteSelect(q1, new Dictionary<string, object> { { "@id", e.Message.Chat.Id } });
                if (groups.Rows.Count == 0)
                {
                    throw new Exception("No group found with id: " + e.Message.Chat.Id + " while running CheckForGroupTitleUpdateAsync");
                }
                var indexTitle = groups.Columns.IndexOf("title");
                var indexId = groups.Columns.IndexOf("id");
                var indexLink = groups.Columns.IndexOf("link");
                var indexIdInTable = (long)groups.Rows[0][indexId];
                var oldTitle = (string)groups.Rows[0][indexTitle];
                var newChat = e.Message.Chat;
                var newTitleWithException = new Tuple<Telegram.Bot.Types.Chat, Exception>(newChat, null);

                var updatedOrNot = GroupCheckAndUpdate(telegramBotClient, indexIdInTable, oldTitle, newTitleWithException);

                if (updatedOrNot == GroupsFixLogUpdatedEnum.NEW_NAME)
                {
                    Logger.GroupsFixLog.SendLog(telegramBotClient, GroupsFixLogUpdatedEnum.NEW_NAME);
                    groups = SqLite.ExecuteSelect(q1, new Dictionary<string, object> {{"@id", e.Message.Chat.Id}});
                }

                var l = new ListaGruppo();
                
                var g = l.CreaGruppo(groups.Rows[0]);
                
                var linkFunzionante = g.CheckSeIlLinkVa(false);
                
                
                if (linkFunzionante != null && !linkFunzionante.Value)
                {
                    var nuovoLink = await InviteLinks.CreateInviteLinkAsync(indexIdInTable, telegramBotClient);
                    if (nuovoLink.isNuovo != SuccessoGenerazioneLink.ERRORE)
                    {
                        await NotifyUtil.NotifyOwners("Fixed link for group " + e.Message.Chat.Title + " id: " + e.Message.Chat.Id, telegramBotClient);
                    }
                }
            }
            catch (Exception ex)
            {
                _ = NotifyUtil.NotifyOwners(ex, telegramBotClient);
            }
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
            long? id = indexIdInTable;
            var q = "UPDATE Groups SET title = @title WHERE id = @id";
            if (id == null) return GroupsFixLogUpdatedEnum.DID_NOTHING;
            var d = new Dictionary<string, object>
                        {
                            {"@title", newTitle},
                            {"@id", id.Value}
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