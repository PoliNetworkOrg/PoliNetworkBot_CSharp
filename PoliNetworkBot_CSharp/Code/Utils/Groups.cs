#region

using PoliNetworkBot_CSharp.Code.Data;
using PoliNetworkBot_CSharp.Code.Objects;
using System;
using System.Data;
using System.Threading.Tasks;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils
{
    internal static class Groups
    {
        internal static DataTable GetAllGroups()
        {
            const string q1 = "SELECT * FROM Groups";
            return SqLite.ExecuteSelect(q1);
        }

        internal static async Task<Tuple<bool, Exception>> CheckIfAdminAsync(int userId, string username, long chatId,
            TelegramBotAbstract telegramBotAbstract)
        {
            if (GlobalVariables.Creators.Contains(username))
                return new Tuple<bool, Exception>(true, null);

            return await telegramBotAbstract.IsAdminAsync(userId, chatId);
        }
    }
}