using System;
using System.Data;

namespace PoliNetworkBot_CSharp.Code.Utils
{
    internal class Tables
    {
        public static void FixIDTable(string table_name, string column_id_name, string unique_column)
        {
            int r4 = Tables.GetMaxID(table_name, column_id_name);
            string q2 = "SELECT * FROM " + table_name + " WHERE " + column_id_name + " IS NULL";
            var r5 = Utils.SQLite.ExecuteSelect(q2);
            if (r5 == null)
                return;

            foreach (DataRow dr in r5.Rows)
            {
                r4++;

                string value_unique = dr[unique_column].ToString();
                string q3 = "UPDATE " + table_name + " SET " + column_id_name + "=" + r4.ToString() + " WHERE " + unique_column + "='" + value_unique + "'";
                Utils.SQLite.Execute(q3);
            }
        }

        internal static int GetMaxID(string table_name, string column_id_name)
        {
            string q = "SELECT MAX(" + column_id_name + ") FROM " + table_name;
            var r = Utils.SQLite.ExecuteSelect(q);
            var r2 = Utils.SQLite.GetFirstValueFromDataTable(r);
            if (r2 == null)
            {
                return 0;
            }

            try
            {
                return Convert.ToInt32(r2);
            }
            catch
            {
                return 0;
            }
        }
    }
}