using PoliNetworkBot_CSharp.Objects;
using System.Collections.Generic;
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
                "last_update_link DATETIME," +
                "type VARCHAR(250)," +
                "title VARCHAR(250)" +
                ") ";

            Utils.SQLite.Execute(q1);
        }
    }
}