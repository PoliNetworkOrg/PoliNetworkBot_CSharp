#region

using System;
using System.Collections.Generic;
using System.Threading;
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
    internal static class Program
    {
        private static List<BotInfo> _botInfos;
        private static List<UserBotInfo> _userBotsInfos;
        private static List<BotDisguisedAsUserBotInfo> _botDisguisedAsUserBotInfos;

        private static void Main()
        {
            Console.WriteLine("Hello World! Welcome to our bots system!\n" +
                              "If you want to reset everything, write 'n'. If not, write another character");
            var readChoice = Console.ReadLine();
            if (!string.IsNullOrEmpty(readChoice))
                if (readChoice.StartsWith("n"))
                {
                    NewConfig.NewConfigMethod(true, true, true);
                    Console.WriteLine("Reset done!");
                    return;
                }

            var toExit = LoadBotConfig();
            if (toExit == ToExit.EXIT)
                return;

            var toExit2 = LoadUserBotConfig();
            if (toExit2 == ToExit.EXIT)
                return;

            GlobalVariables.LoadToRam();

            Console.WriteLine("\nTo kill this process, you have to check the process list");

            _ = StartBotsAsync();

            while (true) Console.ReadKey();
        }

        private static ToExit LoadUserBotConfig()
        {
            _userBotsInfos = FileSerialization.ReadFromBinaryFile<List<UserBotInfo>>(Paths.ConfigUserbot);
            if (_userBotsInfos != null && _userBotsInfos.Count != 0)
                return ToExit.STAY;

            Console.WriteLine(
                "It seems that the userbot configuration isn't available. Do you want to reset it? (Y/N)");
            var readChoice2 = Console.ReadLine();
            if (!string.IsNullOrEmpty(readChoice2) && readChoice2.ToLower().StartsWith("y"))
            {
                NewConfig.NewConfigMethod(false, true);

                Console.WriteLine("Reset done! Do you wish to continue with the execution? (Y/N)");
                var readChoice3 = Console.ReadLine();
                if (!string.IsNullOrEmpty(readChoice3) && readChoice3.ToLower().StartsWith("y"))
                {
                    //ok, keep going
                    _userBotsInfos = FileSerialization.ReadFromBinaryFile<List<UserBotInfo>>(Paths.ConfigUserbot);
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

            return ToExit.STAY;
        }

        private static ToExit LoadBotConfig()
        {
            _botInfos = FileSerialization.ReadFromBinaryFile<List<BotInfo>>(Paths.ConfigBot);
            if (_botInfos != null && _botInfos.Count != 0)
                return ToExit.STAY;

            Console.WriteLine(
                "It seems that the bot configuration isn't available. Do you want to reset it? (Y/N)");
            var readChoice2 = Console.ReadLine();
            if (!string.IsNullOrEmpty(readChoice2) && readChoice2.ToLower().StartsWith("y"))
            {
                NewConfig.NewConfigMethod(true, false);

                Console.WriteLine("Reset done! Do you wish to continue with the execution? (Y/N)");
                var readChoice3 = Console.ReadLine();
                if (!string.IsNullOrEmpty(readChoice3) && readChoice3.ToLower().StartsWith("y"))
                {
                    //ok, keep going
                    _botInfos = FileSerialization.ReadFromBinaryFile<List<BotInfo>>(Paths.ConfigBot);
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

            return ToExit.STAY;
        }

        private static async Task StartBotsAsync()
        {
            GlobalVariables.Bots = new Dictionary<long, TelegramBotAbstract>();
            if (_botInfos != null)
                foreach (var bot in _botInfos)
                {
                    var botClient = new TelegramBotClient(bot.GetToken());
                    GlobalVariables.Bots[botClient.BotId] =
                        new TelegramBotAbstract(botClient, bot.GetWebsite(), bot.GetContactString());
                    if (!bot.AcceptsMessages())
                        continue;

                    var onmessageMethod = bot.GetOnMessage();
                    if (onmessageMethod == null)
                        continue;

                    botClient.OnMessage += onmessageMethod;
                    botClient.StartReceiving();
                }

            if (_userBotsInfos != null)
                foreach (var userbot in _userBotsInfos)
                {
                    var client = await UserbotConnect.ConnectAsync(userbot);
                    var userId = userbot.GetUserId();
                    if (userId != null)
                    {
                        GlobalVariables.Bots[userId.Value] = new TelegramBotAbstract(client,
                            userbot.GetWebsite(), userbot.GetContactString(), userId.Value);

                        _ = TestThingsAsync(userId.Value);
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

            if (GlobalVariables.Bots.Keys.Count > 0)
            {
                var t = new Thread(CheckMessagesToSend);
                t.Start();
            }
        }

        private static async void CheckMessagesToSend()
        {
            while (true)
            {
                await MessageDb.CheckMessagesToSend();
                Thread.Sleep(10 * 1000); //10 sec
            }

            // ReSharper disable once FunctionNeverReturns
        }

        private static async Task TestThingsAsync(long userId)
        {
            /*
            _ = Data.GlobalVariables.Bots[user_id].SendMessageReactionAsync(chatId: 415600477, //test group
                emojiReaction: "😎", messageId: 8, Telegram.Bot.Types.Enums.ChatType.Group);
            */
        }
    }
}