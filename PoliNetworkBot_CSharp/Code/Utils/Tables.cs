#region

using System;
using System.Data;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils
{
    internal static class Tables
    {
        public static void FixIdTable(string tableName, string columnIdName, string uniqueColumn)
        {
            var r4 = GetMaxId(tableName, columnIdName);
            var q2 = "SELECT * FROM " + tableName + " WHERE " + columnIdName + " IS NULL";
            var r5 = SqLite.ExecuteSelect(q2);
            if (r5 == null)
                return;

            foreach (DataRow dr in r5.Rows)
            {
                r4++;

                var valueUnique = dr[uniqueColumn].ToString();
                var q3 = "UPDATE " + tableName + " SET " + columnIdName + "=" + r4 + " WHERE " + uniqueColumn +
                         "='" + valueUnique + "'";
                SqLite.Execute(q3);
            }
        }

        internal static int GetMaxId(string tableName, string columnIdName)
        {
            var q = "SELECT MAX(" + columnIdName + ") FROM " + tableName;
            var r = SqLite.ExecuteSelect(q);
            var r2 = SqLite.GetFirstValueFromDataTable(r);
            if (r2 == null) return 0;

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