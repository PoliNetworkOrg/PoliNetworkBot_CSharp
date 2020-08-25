#region

using System.Data;
using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Data;
using PoliNetworkBot_CSharp.Code.Objects;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils
{
    internal class Groups
    {
        internal static DataTable GetAllGroups()
        {
            var q1 = "SELECT * FROM Groups";
            return SQLite.ExecuteSelect(q1);
        }

        internal static async Task<bool> CheckIfAdminAsync(int user_id, long chat_id,
            TelegramBotAbstract telegramBotAbstract)
        {
            if (GlobalVariables.Creators.Contains(user_id))
                return true;

            return await telegramBotAbstract.IsAdminAsync(user_id, chat_id);
        }
    }
}