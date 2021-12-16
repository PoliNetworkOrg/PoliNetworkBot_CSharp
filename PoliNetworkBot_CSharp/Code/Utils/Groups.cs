#region

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
                    string oldTitle = "";
                    string newTitle = "";
                    try
                    {
                        oldTitle = (string)groups.Rows[i][indexTitle];

                        var newTitleWithException = (await telegramBotAbstract.GetChat((long)groups.Rows[i][indexId]));

                        await Task.Delay(100);

                        newTitle = newTitleWithException?.Item1?.Title;
                        if (String.IsNullOrEmpty(oldTitle) && String.IsNullOrEmpty(newTitle))
                        {
                            Logger.GroupsFixLog.OldNullNewNull(newTitleWithException?.Item1?.Id, (long)groups.Rows[i][indexId]);
                            Logger.GroupsFixLog.CountIgnored();
                            //throw new Exception("oldTitle and newTitle both null at line: " + i);
                            continue;
                        }
                        if (String.IsNullOrEmpty(newTitle))
                        {
                            Logger.GroupsFixLog.NewNull((long)groups.Rows[i][indexId], oldTitle, newTitleWithException?.Item2);
                            Logger.GroupsFixLog.CountIgnored();
                            continue;
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
                            continue;
                        }
                        Logger.GroupsFixLog.NameChange(oldTitle, newTitle);
                        var id = groups.Rows[i][indexId] as long?;
                        var q = "UPDATE Groups SET title = @title WHERE id = @id";
                        if (id == null) continue;
                        var d = new Dictionary<string, object>
                        {
                            {"@title", newTitle},
                            {"@id", id.Value}
                        };
                        SqLite.Execute(q, d);
                    }
                    catch (Exception e2)
                    {
                        var e3 = new Exception("Unexpected exception in FixAllGroupsName \n\noldTitle: " + oldTitle +
                            "\n NewTitle: " + newTitle + "\n\n" + e2.Message);
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