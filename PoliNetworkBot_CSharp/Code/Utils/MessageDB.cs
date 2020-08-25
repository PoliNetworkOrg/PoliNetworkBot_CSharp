#region

using System;
using System.Collections.Generic;
using Telegram.Bot.Types.Enums;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils
{
    internal class MessageDB
    {
        internal static bool AddMessage(MessageType type, string message_text,
            int message_from_id_person, int? message_from_id_entity,
            int photo_id, long id_chat_sent_into, DateTime? sent_date)
        {
            var q = "INSERT INTO Messages " +
                    "(id, from_id_person, from_id_entity, type, " +
                    "id_photo, message_text, id_chat_sent_into, sent_date) " +
                    "VALUES " +
                    "(@id, @fip, @fie, @t, @idp, @mt, @icsi, @sent_date);";

            var type_i = GetMessageTypeByName(type);
            if (type_i == null) return false;

            var id = Tables.GetMaxId("Messages", "id");
            id++;

            SQLite.Execute(q, new Dictionary<string, object>
            {
                {"@id", id},
                {"@fip", message_from_id_person},
                {"@fie", message_from_id_entity},
                {"@t", type_i},
                {"@idp", photo_id},
                {"@mt", message_text},
                {"@icsi", id_chat_sent_into},
                {"@sent_date", sent_date}
            });

            return true;
        }

        private static int? GetMessageTypeByName(MessageType type, int times = 1)
        {
            if (times < 0)
                return null;

            var q1 = "SELECT id FROM MessageTypes WHERE name = @name";
            var keyValuePairs = new Dictionary<string, object> {{"@name", type.ToString()}};
            var r1 = SQLite.ExecuteSelect(q1, keyValuePairs);
            var r2 = SQLite.GetFirstValueFromDataTable(r1);
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
            var q = "INSERT INTO MessageTypes (name) VALUES (@name)";
            var keyValuePairs = new Dictionary<string, object> {{"@name", type.ToString()}};
            SQLite.Execute(q, keyValuePairs);
            Tables.FixIdTable("MessageTypes", "id", "name");
        }
    }
}