#region

using PoliNetworkBot_CSharp.Code.Data;
using PoliNetworkBot_CSharp.Code.Objects;
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

        internal static async Task<SuccessWithException> CheckIfAdminAsync(long userId, string username, long chatId,
            TelegramBotAbstract telegramBotAbstract)
        {
            if (GlobalVariables.Creators.Contains(username))
                return new SuccessWithException(true);

            return await telegramBotAbstract.IsAdminAsync(userId, chatId);
        }

        public static async Task FixAllGroupsNames()
        {
            const string q1 = "SELECT * FROM Groups";
            var groups =  SqLite.ExecuteSelect(q1);
            groups.Rows[0].ToString();
        }
    }
}