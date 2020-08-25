#region

using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using PoliNetworkBot_CSharp.Code.Data.Constants;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils
{
    public class SQLite
    {
        public static int Execute(string query, Dictionary<string, object> args = null)
        {
            int numberOfRowsAffected;

            //setup the connection to the database
            using var con = new SQLiteConnection(Paths.db);
            con.Open();

            //open a new command
            using (var cmd = new SQLiteCommand(query, con))
            {
                //set the arguments given in the query
                if (args != null)
                    foreach (var pair in args)
                        cmd.Parameters.AddWithValue(pair.Key, pair.Value);

                //execute the query and get the number of row affected
                numberOfRowsAffected = cmd.ExecuteNonQuery();
            }

            return numberOfRowsAffected;
        }

        public static DataTable ExecuteSelect(string query, Dictionary<string, object> args = null)
        {
            if (string.IsNullOrEmpty(query.Trim()))
                return null;

            using var con = new SQLiteConnection(Paths.db);
            con.Open();
            using var cmd = new SQLiteCommand(query, con);
            if (args != null)
                foreach (var entry in args)
                    cmd.Parameters.AddWithValue(entry.Key, entry.Value);

            var da = new SQLiteDataAdapter(cmd);

            var dt = new DataTable();
            da.Fill(dt);

            da.Dispose();
            return dt;
        }

        internal static object GetFirstValueFromDataTable(DataTable dt)
        {
            try
            {
                return dt.Rows[0].ItemArray[0];
            }
            catch
            {
                return null;
            }
        }
    }
}