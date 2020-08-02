using PoliNetworkBot_CSharp.Objects;
using System;
using System.Collections.Generic;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace PoliNetworkBot_CSharp
{
    internal class Program
    {
        public static List<BotInfo> botInfos = null;
        public static List<UserBotInfo> userBots = null;

        private static void Main()
        {
            Console.WriteLine("Hello World! Welcome to our bots system!\n" +
                "If you want to reset everything, write 'n'. If not, write another character");
            var read_choice = Console.ReadLine();
            if (!string.IsNullOrEmpty(read_choice))
            {
                if (read_choice.StartsWith("n"))
                {
                    MainProgram.NewConfig.NewConfigMethod();
                    Console.WriteLine("Reset done!");
                    return;
                }
            }

            var to_exit = LoadBotConfig();
            if (to_exit == ToExit.EXIT)
                return;

            LoadUserBotConfig();

            Data.GlobalVariables.LoadToRam();

            StartBotsAsync();

            Console.WriteLine("\nTo kill this process, you have to check the process list");
            while (true)
            {
                Console.ReadKey();
            }
        }

        private static void LoadUserBotConfig()
        {
            throw new NotImplementedException();
        }

        private static ToExit LoadBotConfig()
        {
            botInfos = Utils.FileSerialization.ReadFromBinaryFile<List<BotInfo>>(Data.Constants.Paths.config_bot);
            if (botInfos == null || botInfos.Count == 0)
            {
                Console.WriteLine("It seems that the configuration isn't available. Do you want to reset it? (Y/N)");
                var read_choice2 = Console.ReadLine();
                if (!string.IsNullOrEmpty(read_choice2) && read_choice2.ToLower().StartsWith("y"))
                {
                    MainProgram.NewConfig.NewConfigMethod();

                    Console.WriteLine("Reset done! Do you wish to continue with the execution? (Y/N)");
                    var read_choice3 = Console.ReadLine();
                    if (!string.IsNullOrEmpty(read_choice3) && read_choice3.ToLower().StartsWith("y"))
                    {
                        //ok, keep going
                        botInfos = Utils.FileSerialization.ReadFromBinaryFile<List<BotInfo>>(Data.Constants.Paths.config_bot);
                    }
                    else
                    {
                        Console.WriteLine("Ok, bye!");
                        return ToExit.EXIT;
                    }
                }
                else
                {
                    return ToExit.EXIT;
                }
            }

            return ToExit.STAY;
        }

        private static async System.Threading.Tasks.Task StartBotsAsync()
        {
            Data.GlobalVariables.Bots = new Dictionary<long, TelegramBotAbstract>();
            if (botInfos != null)
            {
                foreach (var bot in botInfos)
                {
                    TelegramBotClient botClient = new TelegramBotClient(bot.GetToken());
                    Data.GlobalVariables.Bots[botClient.BotId] = new TelegramBotAbstract(botClient, bot.GetWebsite(), bot.GetContactString());
                    var me = botClient.GetMeAsync().Result;
                    Console.WriteLine(me.Id);
                    if (bot.AcceptsMessages())
                    {
                        var onmessage_method = bot.GetOnMessage();
                        if (onmessage_method != null)
                        {
                            botClient.OnMessage += onmessage_method;
                            botClient.StartReceiving();
                        }
                    }
                }
            }

            if (userBots != null)
            {
                foreach (var userbot in userBots)
                {
                    TLSharp.Core.TelegramClient client = await Utils.UserbotConnect.ConnectAsync(userbot);
                    Data.GlobalVariables.Bots[userbot.GetUserId()] = new TelegramBotAbstract(client, userbot.GetWebsite(), userbot.GetContactString());
                }
            }
        }
    }
}