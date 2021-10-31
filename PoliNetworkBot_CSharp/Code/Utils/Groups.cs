#region

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
            await FixAllGroupsNames(telegramBotAbstract);
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

        public static async Task FixAllGroupsNames(TelegramBotAbstract telegramBotAbstract)
        {
            try
            {
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
                        newTitle = newTitleWithException?.Item1?.Title;
                        if (String.IsNullOrEmpty(oldTitle) && String.IsNullOrEmpty(newTitle))
                        {
                            throw new Exception("oldTitle and newTitle both null at line: " + i);
                        }
                        if (String.IsNullOrEmpty(newTitle))
                        {
                            if (newTitleWithException.Item2 != null)
                            {
                                Logger.WriteLine(" exception in migrated: \n\n" + newTitleWithException.Item2);
                            }
                            throw new Exception("newTitle is null where oldTitle: " + oldTitle + " migrated?");
                        }
                        if (String.IsNullOrEmpty(oldTitle))
                        {
                            Logger.WriteLine("oldTitle in group (newtitle = " + newTitle + ")");
                        }
                        if (oldTitle == newTitle) continue;
                        Logger.WriteLine("Changing name of group: " + oldTitle + " to: " + newTitle);
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
                        await NotifyUtil.NotifyOwners(e2, telegramBotAbstract);
                        await NotifyUtil.NotifyOwners("oldTitle: " + oldTitle + "\n\n NewTitle:" + newTitle, telegramBotAbstract);
                    }
                }
            }
            catch (Exception e)
            {
                await NotifyUtil.NotifyOwners(e, telegramBotAbstract);
            }
        }
    }
}