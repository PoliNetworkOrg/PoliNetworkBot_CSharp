using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace PoliNetworkBot_CSharp.Utils
{
    internal class Assoc
    {
        internal static async System.Threading.Tasks.Task<int?> GetIDEntityFromPersonAsync(int id, Dictionary<string, string> language_list, TelegramBotAbstract sender, string lang)
        {
            string q = "SELECT Entities.id, Entities.name FROM (SELECT * FROM PeopleInEntities WHERE id_person = @idp) AS T1, Entities WHERE T1.id_entity = Entities.id";
            var r = Utils.SQLite.ExecuteSelect(q, new System.Collections.Generic.Dictionary<string, object>() { { "@idp", id } });
            if (r == null || r.Rows.Count == 0)
            {
                return null;
            }

            if (r.Rows.Count == 1)
            {
                return Convert.ToInt32(r.Rows[0].ItemArray[0]);
            }

            Dictionary<string, int> l = new Dictionary<string, int>();
            foreach (DataRow dr in r.Rows)
            {
                l[dr.ItemArray[1].ToString()] = Convert.ToInt32(dr.ItemArray[0]);
            }

            string r2 = await AskUser.AskBetweenRangeAsync(id, language_list, sender, lang, Utils.KeyboardMarkup.ArrayToMatrixString(l.Keys.ToList()));
            return l[r2];
        }
    }
}