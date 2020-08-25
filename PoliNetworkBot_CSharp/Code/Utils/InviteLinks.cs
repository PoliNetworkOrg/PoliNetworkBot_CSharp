#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Objects;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils
{
    internal class InviteLinks
    {
        internal static async Task<int> FillMissingLinksIntoDB_Async(TelegramBotAbstract sender)
        {
            var q1 = "SELECT id FROM Groups WHERE link IS NULL OR link = ''";
            var dt = SQLite.ExecuteSelect(q1);

            var n = 0;
            if (dt == null || dt.Rows.Count == 0)
                return n;

            foreach (DataRow dr in dt.Rows)
            {
                var success = await CreateInviteLinkAsync((long) dr.ItemArray[0], sender);
                if (success)
                    n++;
            }

            return n;
        }

        internal static async Task<bool> CreateInviteLinkAsync(long chat_id, TelegramBotAbstract sender)
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

            var q1 = "UPDATE Groups SET link = @link, last_update_link = @lul WHERE id = @id";
            SQLite.Execute(q1, new Dictionary<string, object>
            {
                {"@link", r},
                {"@lul", DateTime.Now},
                {"@id", chat_id}
            });

            return true;
        }
    }
}