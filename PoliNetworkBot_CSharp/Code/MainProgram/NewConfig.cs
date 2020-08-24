using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PoliNetworkBot_CSharp.Objects;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace PoliNetworkBot_CSharp.MainProgram
{
    internal class NewConfig
    {
        public static void NewConfigMethod(bool reset_bot, bool reset_userbot)
        {
            if (reset_bot)
            {
                ResetBotMethod();
            }

            if (reset_userbot)
            {
                ResetUserbotMethod();
            }

            DestroyDB_And_Redo_it();
        }

        private static void ResetUserbotMethod()
        {
            var lines = File.ReadAllText(Data.Constants.Paths.config_user_bots_info).Split("| _:r:_ |");
            List<UserBotInfo> botInfos = new List<UserBotInfo>();
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                if (!string.IsNullOrEmpty(line))
                {
                    line = line.Trim();

                    if (!string.IsNullOrEmpty(line))
                    {
                        var line_info = line.Split("| _:c:_ |");

                        var bot = new UserBotInfo();
                        bot.SetApiId(line_info[0].Trim());
                        bot.SetApiHash(line_info[1].Trim());
                        bot.SetUserId(line_info[2].Trim());
                        bot.SetNumberCountry(line_info[3].Trim());
                        bot.SetNumberNumber(line_info[4].Trim());
                        bot.SetPasswordToAuthenticate(line_info[5].Trim());

                        botInfos.Add(bot);
                    }
                }
            }
            Utils.FileSerialization.WriteToBinaryFile(Data.Constants.Paths.config_userbot, botInfos);
        }

        private static void ResetBotMethod()
        {
            var lines = File.ReadAllText(Data.Constants.Paths.config_bots_info).Split("| _:r:_ |");
            List<BotInfo> botInfos = new List<BotInfo>();
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                if (!string.IsNullOrEmpty(line))
                {
                    line = line.Trim();

                    if (!string.IsNullOrEmpty(line))
                    {
                        var line_info = line.Split("| _:c:_ |");

                        var bot = new BotInfo();
                        bot.SetToken(line_info[0].Trim());
                        bot.SetWebsite(line_info[1].Trim());
                        bot.SetIsBot(true);
                        bot.SetAcceptMessages(true);
                        bot.SetOnMessages(line_info[2].Trim());
                        bot.SetContactString(line_info[3].Trim());

                        botInfos.Add(bot);
                    }
                }
            }
            Utils.FileSerialization.WriteToBinaryFile(Data.Constants.Paths.config_bot, botInfos);
        }

        private static void DestroyDB_And_Redo_it()
        {
            Utils.DirectoryUtils.CreateDirectory("data");

            string db_path = Data.Constants.Paths.db;
            db_path = db_path.Split('=')[1];
            try
            {
                File.WriteAllText(db_path, "");
            }
            catch
            {
                ;
            }

            CleanDB();

            Redo_DB();
        }

        private static void CleanDB()
        {
            string s = "SELECT name FROM sqlite_master WHERE type='table'";
            var r1 = Utils.SQLite.ExecuteSelect(s);
            if (r1 == null)
                return;

            foreach (DataRow dr in r1.Rows)
            {
                var name = dr.ItemArray[0].ToString();
                if (name.StartsWith("sqlite_"))
                {
                    ;
                }
                else
                {
                    string q = "DROP TABLE IF EXISTS " + name;
                    Utils.SQLite.Execute(q);
                }
            }
        }

        private static void Redo_DB()
        {
            Utils.SQLite.Execute("CREATE TABLE Groups (" +
                "id INT(12) PRIMARY KEY, " +
                "bot_id INT(12)," +
                "valid CHAR(1)," +
                "link VARCHAR(250)," +
                "last_update_link DATETIME," +
                "type VARCHAR(250)," +
                "title VARCHAR(250)" +
                ") ");

            Utils.SQLite.Execute("CREATE TABLE PeopleInEntities (" +
                "id_entity INT(12)," +
                "id_person INT(12)," +
                "CONSTRAINT PK_Person PRIMARY KEY (id_entity,id_person)" +
                ");");

            Utils.SQLite.Execute("CREATE TABLE Entities (" +
                "id INT(12) PRIMARY KEY," +
                "name VARCHAR(250)" +
                ");");

            FillAssoc();

            Utils.SQLite.Execute("CREATE TABLE Messages (" +
                "id INT(12) PRIMARY KEY," +
                "from_id_person INT(12)," +
                "from_id_entity INT(12)," +
                "type int INT(12)," +
                "id_photo INT(12)," +
                "id_video INT(12)," +
                "id_file INT(12)," +
                "id_voice INT(12)," +
                "id_audio INT(12)," +
                "message_text TEXT," +
                "sent_date DATETIME," +
                "has_been_sent BOOLEAN," +
                "message_id_tg INT(12)," +
                "id_chat_sent_into INT(12)" +
                ");");

            Utils.SQLite.Execute("CREATE TABLE MessageTypes (" +
                "id INT(12) PRIMARY KEY," +
                "name VARCHAR(250)" +
                ");");

            Utils.SQLite.Execute("CREATE TABLE Photos (" +
                "id_photo INT(12) PRIMARY KEY," +
                "file_id VARCHAR(250)," +
                "file_size INT(12)," +
                "height INT(12)," +
                "width INT(12)," +
                "unique_id VARCHAR(250)" +
                ");");
        }

        private static void FillAssoc()
        {
            //read assoc from polinetwork python config file and fill db
            string s = File.ReadAllText("../../../Old/config/assoc.json");
            Newtonsoft.Json.Linq.JObject r = JsonConvert.DeserializeObject<Newtonsoft.Json.Linq.JObject>(s);
            Newtonsoft.Json.Linq.JEnumerable<Newtonsoft.Json.Linq.JToken> r2 = r.Children();
            foreach (Newtonsoft.Json.Linq.JToken r3 in r2)
            {
                if (r3 is Newtonsoft.Json.Linq.JProperty r4)
                {
                    string name = r4.Name;
                    var r5 = r4.Value;
                    List<Int64> users = GetUsersFromAssocJson(r5);
                    AddAssocToDB(name, users);
                }
            }
        }

        private static bool AddAssocToDB(string name, List<Int64> users)
        {
            string q1 = "INSERT INTO Entities (Name) VALUES (@name)";
            int i = Utils.SQLite.Execute(q1, new System.Collections.Generic.Dictionary<string, object>() { { "@name", name } });

            Utils.Tables.FixIDTable(table_name: "Entities", column_id_name: "id", unique_column: "name");

            string q2 = "SELECT id FROM Entities WHERE Name = @name";
            var r2 = Utils.SQLite.ExecuteSelect(q2, new System.Collections.Generic.Dictionary<string, object>() { { "@name", name } });

            object r3 = Utils.SQLite.GetFirstValueFromDataTable(r2);
            int? r4 = null;
            try
            {
                r4 = Convert.ToInt32(r3);
            }
            catch
            {
                ;
            }

            if (r4 == null)
                return false;

            if (users == null)
                return true;

            if (users.Count == 0)
                return true;

            foreach (var u in users)
            {
                string q3 = "INSERT INTO PeopleInEntities (id_entity, id_person) VALUES (@ide, @idp)";
                int i2 = Utils.SQLite.Execute(q3, new System.Collections.Generic.Dictionary<string, object>() { { "@ide", r4.Value }, { "@idp", u } });
            }

            return true;
        }

        private static List<Int64> GetUsersFromAssocJson(JToken r1)
        {
            var r2 = r1.Children();
            foreach (JToken r3 in r2)
            {
                if (r3 is JProperty r4)
                {
                    if (r4.Name == "users")
                    {
                        JToken r5 = r4.Value;
                        if (r5 is JArray r6)
                        {
                            List<Int64> users = new List<Int64>();
                            foreach (JToken r7 in r6)
                            {
                                if (r7 is JValue r8)
                                {
                                    users.Add(Convert.ToInt64(r8.Value));
                                }
                            }
                            return users;
                        }
                    }
                }
            }

            return null;
        }
    }
}