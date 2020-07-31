using PoliNetworkBot_CSharp.Objects;
using System;
using System.Collections.Generic;
using Telegram.Bot;

namespace PoliNetworkBot_CSharp
{
    class Program
    {

        static void Main()
        {
            Console.WriteLine("Hello World! Welcome to our bots system!\n" +
                "If you want to reset everything, write 'n'. If not, write another character");
            var read_choice = Console.ReadLine();
            if (!string.IsNullOrEmpty(read_choice))
            {
                if (read_choice.StartsWith("n"))
                {
                    MainProgram.NewConfig.NewConfigMethod();
                    return;
                }
            }

            List<BotInfo> botInfos = Utils.FileSerialization.ReadFromBinaryFile<List<BotInfo>>(Data.Constants.Paths.config_bot);
            if (botInfos == null || botInfos.Count == 0)
            {
                return;
            }

            Data.GlobalVariables.Bots = new List<TelegramBotAbstract>();
            foreach (var bot in botInfos)
            {
                if (bot.IsBot())
                {
                    TelegramBotClient botClient = new TelegramBotClient(bot.GetToken());
                    Data.GlobalVariables.Bots.Add(new TelegramBotAbstract(botClient));
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
