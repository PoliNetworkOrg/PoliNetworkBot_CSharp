#region

using System;
using System.Collections.Generic;
using Telegram.Bot.Types.Enums;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils
{
    internal static class MessageDb
    {
        internal static bool AddMessage(MessageType type, string messageText,
            int messageFromIdPerson, int? messageFromIdEntity,
            int photoId, long idChatSentInto, DateTime? sentDate)
        {
            const string q = "INSERT INTO Messages " +
                             "(id, from_id_person, from_id_entity, type, " +
                             "id_photo, message_text, id_chat_sent_into, sent_date) " +
                             "VALUES " +
                             "(@id, @fip, @fie, @t, @idp, @mt, @icsi, @sent_date);";

            var typeI = GetMessageTypeByName(type);
            if (typeI == null) return false;

            var id = Tables.GetMaxId("Messages", "id");
            id++;

            SqLite.Execute(q, new Dictionary<string, object>
            {
                {"@id", id},
                {"@fip", messageFromIdPerson},
                {"@fie", messageFromIdEntity},
                {"@t", typeI},
                {"@idp", photoId},
                {"@mt", messageText},
                {"@icsi", idChatSentInto},
                {"@sent_date", sentDate}
            });

            return true;
        }

        private static int? GetMessageTypeByName(MessageType type, int times = 1)
        {
            while (true)
            {
                if (times < 0) return null;

                const string q1 = "SELECT id FROM MessageTypes WHERE name = @name";
                var keyValuePairs = new Dictionary<string, object> {{"@name", type.ToString()}};
                var r1 = SqLite.ExecuteSelect(q1, keyValuePairs);
                var r2 = SqLite.GetFirstValueFromDataTable(r1);
                if (r1 == null || r1.Rows.Count == 0 || r2 == null)
                {
                    AddMessageType(type);
                    times--;
                    continue;
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
        }

        private static void AddMessageType(MessageType type)
        {
            const string q = "INSERT INTO MessageTypes (name) VALUES (@name)";
            var keyValuePairs = new Dictionary<string, object> {{"@name", type.ToString()}};
            SqLite.Execute(q, keyValuePairs);
            Tables.FixIdTable("MessageTypes", "id", "name");
        }
    }
}