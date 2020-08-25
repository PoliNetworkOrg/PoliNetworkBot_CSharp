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
    internal static class Assoc
    {
        internal static async Task<int?> GetIdEntityFromPersonAsync(int id, Dictionary<string, string> languageList,
            TelegramBotAbstract sender, string lang)
        {
            const string q = "SELECT Entities.id, Entities.name FROM (SELECT * FROM PeopleInEntities WHERE id_person = @idp) AS T1, Entities WHERE T1.id_entity = Entities.id";
            var r = SqLite.ExecuteSelect(q, new Dictionary<string, object> {{"@idp", id}});
            if (r == null || r.Rows.Count == 0) return null;

            if (r.Rows.Count == 1) return Convert.ToInt32(r.Rows[0].ItemArray[0]);

            var l = new Dictionary<string, int>();
            foreach (DataRow dr in r.Rows)
            {
                var s = dr.ItemArray[1].ToString();
                if (!string.IsNullOrEmpty(s))
                    l[s] = Convert.ToInt32(dr.ItemArray[0]);
            }

            var r2 = await AskUser.AskBetweenRangeAsync(id, languageList, sender, lang,
                KeyboardMarkup.ArrayToMatrixString(l.Keys.ToList()));
            return l[r2];
        }
    }
}