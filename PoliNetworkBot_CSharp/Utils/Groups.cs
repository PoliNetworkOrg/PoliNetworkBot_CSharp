using System;
using System.Data;

namespace PoliNetworkBot_CSharp.Utils
{
    internal class Groups
    {
        internal static DataTable GetAllGroups()
        {
            var q1 = "SELECT * FROM Groups";
            return SQLite.ExecuteSelect(q1);
        }

        internal static async System.Threading.Tasks.Task<bool> CheckIfAdminAsync(int user_id, long chat_id, TelegramBotAbstract telegramBotAbstract)
        {
            if (Data.GlobalVariables.Creators.Contains(user_id))
                return true;

            return await telegramBotAbstract.IsAdminAsync(user_id, chat_id);
        }
    }
}