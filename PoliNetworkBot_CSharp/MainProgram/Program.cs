using PoliNetworkBot_CSharp.Objects;
using System;
using System.Collections.Generic;
using Telegram.Bot;

namespace PoliNetworkBot_CSharp
{
    internal class Program
    {
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

            List<BotInfo> botInfos = Utils.FileSerialization.ReadFromBinaryFile<List<BotInfo>>(Data.Constants.Paths.config_bot);
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
                        return;
                    }
                }
                else
                {
                    return;
                }
            }

            Data.GlobalVariables.LoadToRam();

            Data.GlobalVariables.Bots = new Dictionary<long, TelegramBotAbstract>();
            foreach (var bot in botInfos)
            {
                if (bot.IsBot())
                {
                    TelegramBotClient botClient = new TelegramBotClient(bot.GetToken());
                    Data.GlobalVariables.Bots[botClient.BotId] = new TelegramBotAbstract(botClient, bot.GetWebsite());
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
                else
                {
                    //todo: userbots
                }
            }

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }
    }
}