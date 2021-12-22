#region

using JsonPolimi_Core_nf.Data;
using JsonPolimi_Core_nf.Tipi;
using PoliNetworkBot_CSharp.Code.Bots.Anon;
using PoliNetworkBot_CSharp.Code.Data;
using PoliNetworkBot_CSharp.Code.Objects;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils
{
    internal static class Groups
    {

        private static Dictionary<long, DateTime> _inhibitionPeriod = new Dictionary<long, DateTime>();

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
                        Logger.WriteLine(e3);
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

        internal static async Task CheckForGroupTitleUpdateAsync(TelegramBotAbstract telegramBotClient, MessageEventArgs e)
        {
            try
            {
                if (e.Message?.Chat?.Id == null || e.Message?.Chat?.Title == null)
                    return;
                //if(_inhibitionPeriod.TryGetValue(e.Message.Chat.Id, out var lastUpdate) && lastUpdate.AddHours(24) > DateTime.Now)
                //    return;
                //_inhibitionPeriod.Add(e.Message.Chat.Id, DateTime.Now);
                var q1 = "SELECT * FROM Groups WHERE id = " + e.Message.Chat.Id;
                var groups = SqLite.ExecuteSelect(q1);
                var indexTitle = groups.Columns.IndexOf("title");
                var indexId = groups.Columns.IndexOf("id");
                var indexLink = groups.Columns.IndexOf("link");
                var linkInTable = groups.Rows[0]?[indexLink].ToString();
                var indexIdInTable = (long)groups.Rows[0]?[indexId];
                var oldTitle = (string)groups.Rows[0]?[indexTitle];
                var newChat = e.Message.Chat;
                var newTitleWithException = new Tuple<Telegram.Bot.Types.Chat, Exception>(newChat, null);

                GroupCheckAndUpdate(telegramBotClient, indexIdInTable, oldTitle, newTitleWithException);

                if (Variabili.L == null) Variabili.L = new ListaGruppo();

                Variabili.L.HandleSerializedObject(groups);

                var linkFunzionante = Variabili.L.GetElem(0).CheckSeIlLinkVa(false);

                if (linkFunzionante != null && !linkFunzionante.Value)
                {
                    await InviteLinks.CreateInviteLinkAsync(indexIdInTable, telegramBotClient, e);
                }

            }
            catch (Exception ex)
            {
                Logger.WriteLine(ex);
                _ = NotifyUtil.NotifyOwners(ex, telegramBotClient);
            }
        }

        private static void GroupCheckAndUpdate(TelegramBotAbstract telegramBotAbstract,
            long indexIdInTable,
            string oldTitle,
            Tuple<Telegram.Bot.Types.Chat, Exception> newTitleWithException)
        {
            string newTitle = newTitleWithException?.Item1?.Title;
            if (String.IsNullOrEmpty(oldTitle) && String.IsNullOrEmpty(newTitle))
            {
                Logger.GroupsFixLog.OldNullNewNull(newTitleWithException?.Item1?.Id, indexIdInTable);
                Logger.GroupsFixLog.CountIgnored();
                //throw new Exception("oldTitle and newTitle both null at line: " + i);
                return;
            }
            if (String.IsNullOrEmpty(newTitle))
            {
                Logger.GroupsFixLog.NewNull(indexIdInTable, oldTitle, newTitleWithException?.Item2);
                Logger.GroupsFixLog.CountIgnored();
                return;
                //Logger.WriteLine(" exception in migrated: \n\n" + newTitleWithException.Item2);
                //throw new Exception("newTitle is null where oldTitle: " + oldTitle + " migrated?");
            }
            if (String.IsNullOrEmpty(oldTitle))
            {
                Logger.GroupsFixLog.OldNull(newTitle);
                oldTitle = "[previously null]";
                //Logger.WriteLine("oldTitle in group (newtitle = " + newTitle + ")");
            }
            if (oldTitle == newTitle)
            {
                Logger.GroupsFixLog.CountIgnored();
                return;
            }
            long? id = indexIdInTable;
            var q = "UPDATE Groups SET title = @title WHERE id = @id";
            if (id == null) return;
            var d = new Dictionary<string, object>
                        {
                            {"@title", newTitle},
                            {"@id", id.Value}
                        };
            SqLite.Execute(q, d);
            Logger.GroupsFixLog.NameChange(oldTitle, newTitle);
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