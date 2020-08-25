#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Objects;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils
{
    internal class Assoc
    {
        internal static async Task<int?> GetIDEntityFromPersonAsync(int id, Dictionary<string, string> language_list,
            TelegramBotAbstract sender, string lang)
        {
            var q =
                "SELECT Entities.id, Entities.name FROM (SELECT * FROM PeopleInEntities WHERE id_person = @idp) AS T1, Entities WHERE T1.id_entity = Entities.id";
            var r = SQLite.ExecuteSelect(q, new Dictionary<string, object> {{"@idp", id}});
            if (r == null || r.Rows.Count == 0) return null;

            if (r.Rows.Count == 1) return Convert.ToInt32(r.Rows[0].ItemArray[0]);

            var l = new Dictionary<string, int>();
            foreach (DataRow dr in r.Rows) l[dr.ItemArray[1].ToString()] = Convert.ToInt32(dr.ItemArray[0]);

            var r2 = await AskUser.AskBetweenRangeAsync(id, language_list, sender, lang,
                KeyboardMarkup.ArrayToMatrixString(l.Keys.ToList()));
            return l[r2];
        }
    }
}