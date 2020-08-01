using PoliNetworkBot_CSharp.Objects;
using System.Collections.Generic;
using System.IO;

namespace PoliNetworkBot_CSharp.MainProgram
{
    internal class NewConfig
    {
        public static void NewConfigMethod()
        {
            var lines = File.ReadAllLines(Data.Constants.Paths.config_bots_info);
            List<BotInfo> botInfos = new List<BotInfo>();
            foreach (var line in lines)
            {
                if (!string.IsNullOrEmpty(line))
                {
                    var line_info = line.Split("_:::_");

                    var bot = new BotInfo();
                    bot.SetToken(line_info[0]);
                    bot.SetWebsite(line_info[1]);
                    bot.SetIsBot(true);
                    bot.SetAcceptMessages(true);
                    bot.SetOnMessages(line_info[2]);

                    botInfos.Add(bot);
                }
            }
            Utils.FileSerialization.WriteToBinaryFile(Data.Constants.Paths.config_bot, botInfos);

            DestroyDB_And_Redo_it();
        }

        private static void DestroyDB_And_Redo_it()
        {
            Utils.DirectoryUtils.CreateDirectory("data");

            string db_path = Data.Constants.Paths.db;
            db_path = db_path.Split('=')[1];
            File.WriteAllText(db_path, "");

            Redo_DB();
        }

        private static void Redo_DB()
        {
            string q1 = "CREATE TABLE Groups (" +
                "id INT(12) PRIMARY KEY, " +
                "bot_id INT(12)," +
                "valid CHAR(1)," +
                "link VARCHAR(250)," +
                "last_update_link DATETIME" +
                ") ";

            Utils.SQLite.Execute(q1);
        }
    }
}