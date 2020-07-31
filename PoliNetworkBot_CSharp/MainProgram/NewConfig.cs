using PoliNetworkBot_CSharp.Objects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PoliNetworkBot_CSharp.MainProgram
{
    class NewConfig
    {
        public static void NewConfigMethod()
        {
            var lines = File.ReadAllLines(Data.Constants.Paths.config_tokens);
            List<BotInfo> botInfos = new List<BotInfo>();
            foreach (var line in lines)
            {
                if (!string.IsNullOrEmpty(line))
                {
                    var bot = new BotInfo();
                    bot.SetToken(line);
                    bot.SetIsBot(true);
                    bot.SetAcceptMessages(true);
                    bot.SetOnMessages(Data.Constants.BotStartMethods.moderation);
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
                "valid CHAR(1)" +
                ") ";

            Utils.SQLite.Execute(q1);
        }
    }
}
