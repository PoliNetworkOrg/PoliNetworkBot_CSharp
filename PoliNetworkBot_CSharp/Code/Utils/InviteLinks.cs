#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Objects;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils
{
    internal static class InviteLinks
    {
        internal static async Task<int> FillMissingLinksIntoDB_Async(TelegramBotAbstract sender)
        {
            const string q1 = "SELECT id FROM Groups WHERE link IS NULL OR link = ''";
            var dt = SqLite.ExecuteSelect(q1);

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

        internal static async Task<bool> CreateInviteLinkAsync(long chatId, TelegramBotAbstract sender)
        {
            string r = null;
            try
            {
                r = await sender.ExportChatInviteLinkAsync(chatId);
            }
            catch
            {
                // ignored
            }

            if (string.IsNullOrEmpty(r))
                return false;

            const string q1 = "UPDATE Groups SET link = @link, last_update_link = @lul WHERE id = @id";
            SqLite.Execute(q1, new Dictionary<string, object>
            {
                {"@link", r},
                {"@lul", DateTime.Now},
                {"@id", chatId}
            });

            return true;
        }
    }
}