using System;
using System.Collections.Generic;
using Telegram.Bot.Types.Enums;

namespace PoliNetworkBot_CSharp.Utils
{
    internal class MessageDB
    {
        internal static bool AddMessage(MessageType type, string message_text,
            int message_from_id_person, int? message_from_id_entity,
            int photo_id, long id_chat_sent_into, DateTime? sent_date)
        {
            string q = "INSERT INTO Messages " +
                "(id, from_id_person, from_id_entity, type, " +
                "id_photo, message_text, id_chat_sent_into, sent_date) " +
                "VALUES " +
                "(@id, @fip, @fie, @t, @idp, @mt, @icsi, @sent_date);";

            int? type_i = MessageDB.GetMessageTypeByName(type);
            if (type_i == null)
            {
                return false;
            }

            int id = Utils.Tables.GetMaxID(table_name: "Messages", column_id_name: "id");
            id++;

            Utils.SQLite.Execute(q, new System.Collections.Generic.Dictionary<string, object>() {
                {"@id", id },
                {"@fip", message_from_id_person},
                {"@fie" , message_from_id_entity },
                {"@t", type_i },
                {"@idp", photo_id },
                {"@mt", message_text },
                {"@icsi", id_chat_sent_into },
                {"@sent_date", sent_date }
            });

            return true;
        }

        private static int? GetMessageTypeByName(MessageType type, int times = 1)
        {
            if (times < 0)
                return null;

            string q1 = "SELECT id FROM MessageTypes WHERE name = @name";
            Dictionary<string, object> keyValuePairs = new Dictionary<string, object>() { { "@name", type.ToString() } };
            var r1 = Utils.SQLite.ExecuteSelect(q1, keyValuePairs);
            var r2 = Utils.SQLite.GetFirstValueFromDataTable(r1);
            if (r1 == null || r1.Rows.Count == 0 || r2 == null)
            {
                AddMessageType(type);
                return GetMessageTypeByName(type, times - 1);
            }

            try
            {
                return Convert.ToInt32(r2);
            }
            catch
            {
                return null;
            }
        }

        private static void AddMessageType(MessageType type)
        {
            string q = "INSERT INTO MessageTypes (name) VALUES (@name)";
            Dictionary<string, object> keyValuePairs = new Dictionary<string, object>() { { "@name", type.ToString() } };
            Utils.SQLite.Execute(q, keyValuePairs);
            Utils.Tables.FixIDTable("MessageTypes", "id", "name");
        }
    }
}