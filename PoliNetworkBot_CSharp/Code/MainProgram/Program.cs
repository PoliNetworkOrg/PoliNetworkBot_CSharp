#region

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Data;
using PoliNetworkBot_CSharp.Code.Data.Constants;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Utils;
using Telegram.Bot;

#endregion

namespace PoliNetworkBot_CSharp.Code.MainProgram
{
    internal class Program
    {
        public static List<BotInfo> botInfos;
        public static List<UserBotInfo> userBots;

        private static void Main()
        {
            Console.WriteLine("Hello World! Welcome to our bots system!\n" +
                              "If you want to reset everything, write 'n'. If not, write another character");
            var read_choice = Console.ReadLine();
            if (!string.IsNullOrEmpty(read_choice))
                if (read_choice.StartsWith("n"))
                {
                    NewConfig.NewConfigMethod(true, true);
                    Console.WriteLine("Reset done!");
                    return;
                }

            var to_exit = LoadBotConfig();
            if (to_exit == ToExit.EXIT)
                return;

            var to_exit2 = LoadUserBotConfig();
            if (to_exit2 == ToExit.EXIT)
                return;

            GlobalVariables.LoadToRam();

            Console.WriteLine("\nTo kill this process, you have to check the process list");

            _ = StartBotsAsync();

            while (true) Console.ReadKey();
        }

        private static ToExit LoadUserBotConfig()
        {
            userBots = FileSerialization.ReadFromBinaryFile<List<UserBotInfo>>(Paths.config_userbot);
            if (userBots == null || userBots.Count == 0)
            {
                Console.WriteLine(
                    "It seems that the userbot configuration isn't available. Do you want to reset it? (Y/N)");
                var read_choice2 = Console.ReadLine();
                if (!string.IsNullOrEmpty(read_choice2) && read_choice2.ToLower().StartsWith("y"))
                {
                    NewConfig.NewConfigMethod(false, true);

                    Console.WriteLine("Reset done! Do you wish to continue with the execution? (Y/N)");
                    var read_choice3 = Console.ReadLine();
                    if (!string.IsNullOrEmpty(read_choice3) && read_choice3.ToLower().StartsWith("y"))
                    {
                        //ok, keep going
                        userBots = FileSerialization.ReadFromBinaryFile<List<UserBotInfo>>(Paths.config_userbot);
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

        private static ToExit LoadBotConfig()
        {
            botInfos = FileSerialization.ReadFromBinaryFile<List<BotInfo>>(Paths.config_bot);
            if (botInfos == null || botInfos.Count == 0)
            {
                Console.WriteLine(
                    "It seems that the bot configuration isn't available. Do you want to reset it? (Y/N)");
                var read_choice2 = Console.ReadLine();
                if (!string.IsNullOrEmpty(read_choice2) && read_choice2.ToLower().StartsWith("y"))
                {
                    NewConfig.NewConfigMethod(true, false);

                    Console.WriteLine("Reset done! Do you wish to continue with the execution? (Y/N)");
                    var read_choice3 = Console.ReadLine();
                    if (!string.IsNullOrEmpty(read_choice3) && read_choice3.ToLower().StartsWith("y"))
                    {
                        //ok, keep going
                        botInfos = FileSerialization.ReadFromBinaryFile<List<BotInfo>>(Paths.config_bot);
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

        private static async Task StartBotsAsync()
        {
            GlobalVariables.Bots = new Dictionary<long, TelegramBotAbstract>();
            if (botInfos != null)
                foreach (var bot in botInfos)
                {
                    var botClient = new TelegramBotClient(bot.GetToken());
                    GlobalVariables.Bots[botClient.BotId] =
                        new TelegramBotAbstract(botClient, bot.GetWebsite(), bot.GetContactString());
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

            if (userBots != null)
                foreach (var userbot in userBots)
                {
                    var client = await UserbotConnect.ConnectAsync(userbot);
                    var user_id = userbot.GetUserId();
                    if (user_id != null)
                    {
                        GlobalVariables.Bots[user_id.Value] = new TelegramBotAbstract(client,
                            userbot.GetWebsite(), userbot.GetContactString(), user_id.Value);

                        _ = TestThingsAsync(user_id.Value);
                    }
                    else
                    {
                        try
                        {
                            client.Dispose();
                        }
                        catch
                        {
                            ;
                        }
                    }
                }
        }

#pragma warning disable CS1998 // Il metodo asincrono non contiene operatori 'await', pertanto verrà eseguito in modo sincrono

        private static async Task TestThingsAsync(long user_id)
#pragma warning restore CS1998 // Il metodo asincrono non contiene operatori 'await', pertanto verrà eseguito in modo sincrono
        {
            /*
            _ = Data.GlobalVariables.Bots[user_id].SendMessageReactionAsync(chatId: 415600477, //test group
                emojiReaction: "😎", messageId: 8, Telegram.Bot.Types.Enums.ChatType.Group);
            */
        }
    }
}