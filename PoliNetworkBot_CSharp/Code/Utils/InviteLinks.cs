using PoliNetworkBot_CSharp.Code.Objects;
using System;
using System.Data;

namespace PoliNetworkBot_CSharp.Code.Utils
{
    internal class InviteLinks
    {
        internal static async System.Threading.Tasks.Task<int> FillMissingLinksIntoDB_Async(TelegramBotAbstract sender)
        {
            string q1 = "SELECT id FROM Groups WHERE link IS NULL OR link = ''";
            DataTable dt = Utils.SQLite.ExecuteSelect(q1);

            int n = 0;
            if (dt == null || dt.Rows.Count == 0)
                return n;

            foreach (DataRow dr in dt.Rows)
            {
                bool success = await CreateInviteLinkAsync((long)dr.ItemArray[0], sender);
                if (success)
                    n++;
            }
            return n;
        }

        internal static async System.Threading.Tasks.Task<bool> CreateInviteLinkAsync(long chat_id, TelegramBotAbstract sender)
        {
            string r = null;
            try
            {
                r = await sender.ExportChatInviteLinkAsync(chat_id);
            }
            catch
            {
                ;
            }

            if (string.IsNullOrEmpty(r))
                return false;

            string q1 = "UPDATE Groups SET link = @link, last_update_link = @lul WHERE id = @id";
            Utils.SQLite.Execute(q1, new System.Collections.Generic.Dictionary<string, object>()
            {
                { "@link", r},
                {"@lul" , DateTime.Now },
                {"@id", chat_id }
            });

            return true;
        }
    }
}