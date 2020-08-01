using System.Data;

namespace PoliNetworkBot_CSharp.Utils
{
    internal class Groups
    {
        internal static DataTable GetAllGroups(TelegramBotAbstract sender)
        {
            var q1 = "SELECT * FROM Groups";
            return SQLite.ExecuteSelect(q1);
        }
    }
}