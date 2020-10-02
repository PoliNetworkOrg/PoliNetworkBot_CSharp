#region

using PoliNetworkBot_CSharp.Code.Config;
using PoliNetworkBot_CSharp.Code.Data;
using PoliNetworkBot_CSharp.Code.Data.Constants;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.InfoBot;
using PoliNetworkBot_CSharp.Code.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

#endregion

namespace PoliNetworkBot_CSharp.Code.MainProgram
{
    internal static class Program
    {
        private static List<BotInfo> _botInfos;
        private static List<UserBotInfo> _userBotsInfos;
        private static List<BotDisguisedAsUserBotInfo> _botDisguisedAsUserBotInfos;

        private static void Main(string[] args)
        {
            FirstThingsToDo();

            var readChoice = MainGetMenuChoice();

            switch (readChoice)
            {
                case '1': //reset everything
                    {
                        ResetEverything(alsoFillTablesFromJson: true);

                        return;
                    }

                case '2': //normal mode
                case '3': //disguised bot test
                    {
                        var toExit = LoadBotConfig();
                        if (toExit == ToExit.EXIT)
                            return;

                        var toExit2 = LoadUserBotConfig();
                        if (toExit2 == ToExit.EXIT)
                            return;

                        var toExit3 = LoadBotDisguisedAsUserBotConfig();
                        if (toExit3 == ToExit.EXIT)
                            return;

                        GlobalVariables.LoadToRam();

                        Console.WriteLine("\nTo kill this process, you have to check the process list");

                        _ = StartBotsAsync(readChoice == '3');

                        while (true) Console.ReadKey();
                        return;
                    }

                case '4':
                    {
                        ResetEverything(alsoFillTablesFromJson: false);
                        return;
                    }
            }
        }

        private static void FirstThingsToDo()
        {
            Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
        }

        private static void ResetEverything(bool alsoFillTablesFromJson)
        {
            NewConfig.NewConfigMethod(true, true, true, true, alsoFillTablesFromJson);
            Console.WriteLine("Reset done!");
        }

        private static char MainGetMenuChoice()
        {
            while (true)
            {
                Console.WriteLine("Welcome to our bots system!\n" +
                                  "What do you want to do?\n" +
                                  "1) Reset everything\n" +
                                  "2) Normal mode (no disguised)\n" +
                                  "3) Only Disguised bot\n" +
                                  "4) Reset everything but don't fill tables\n" +
                                  "\n");

                var reply = Console.ReadLine();

                if (!string.IsNullOrEmpty(reply))
                {
                    var first = reply[0];

                    switch (first)
                    {
                        case '1':
                        case '2':
                        case '3':
                        case '4':
                            return first;
                    }
                }
            }
        }

        private static ToExit LoadBotDisguisedAsUserBotConfig()
        {
            _botDisguisedAsUserBotInfos =
                FileSerialization.ReadFromBinaryFile<List<BotDisguisedAsUserBotInfo>>(Paths.Bin
                    .ConfigBotDisguisedAsUserbot);
            if (_botDisguisedAsUserBotInfos != null && _botDisguisedAsUserBotInfos.Count != 0)
                return ToExit.STAY;

            Console.WriteLine(
                "It seems that the bot disguised as userbot configuration isn't available. Do you want to reset it? (Y/N)");
            var readChoice2 = Console.ReadLine();
            if (!string.IsNullOrEmpty(readChoice2) && readChoice2.ToLower().StartsWith("y"))
            {
                NewConfig.NewConfigMethod(false, false, true, false, false);

                Console.WriteLine("Reset done! Do you wish to continue with the execution? (Y/N)");
                var readChoice3 = Console.ReadLine();
                if (!string.IsNullOrEmpty(readChoice3) && readChoice3.ToLower().StartsWith("y"))
                {
                    //ok, keep going
                    _botDisguisedAsUserBotInfos =
                        FileSerialization.ReadFromBinaryFile<List<BotDisguisedAsUserBotInfo>>(Paths.Bin
                            .ConfigBotDisguisedAsUserbot);
                }
                else
                {
                    Console.WriteLine("Ok, bye!");
                    return ToExit.SKIP;
                }
            }
            else
            {
                return ToExit.SKIP;
            }

            return ToExit.STAY;
        }

        private static ToExit LoadUserBotConfig()
        {
            _userBotsInfos = FileSerialization.ReadFromBinaryFile<List<UserBotInfo>>(Paths.Bin.ConfigUserbot);
            if (_userBotsInfos != null && _userBotsInfos.Count != 0)
                return ToExit.STAY;

            Console.WriteLine(
                "It seems that the userbot configuration isn't available. Do you want to reset it? (Y/N)");
            var readChoice2 = Console.ReadLine();
            if (!string.IsNullOrEmpty(readChoice2) && readChoice2.ToLower().StartsWith("y"))
            {
                NewConfig.NewConfigMethod(false, true, false, false, false);

                Console.WriteLine("Reset done! Do you wish to continue with the execution? (Y/N)");
                var readChoice3 = Console.ReadLine();
                if (!string.IsNullOrEmpty(readChoice3) && readChoice3.ToLower().StartsWith("y"))
                {
                    //ok, keep going
                    _userBotsInfos = FileSerialization.ReadFromBinaryFile<List<UserBotInfo>>(Paths.Bin.ConfigUserbot);
                }
                else
                {
                    Console.WriteLine("Ok, bye!");
                    return ToExit.SKIP;
                }
            }
            else
            {
                return ToExit.SKIP;
            }

            return ToExit.STAY;
        }

        private static ToExit LoadBotConfig()
        {
            _botInfos = FileSerialization.ReadFromBinaryFile<List<BotInfo>>(Paths.Bin.ConfigBot);
            if (_botInfos != null && _botInfos.Count != 0)
                return ToExit.STAY;

            Console.WriteLine(
                "It seems that the bot configuration isn't available. Do you want to reset it? (Y/N)");
            var readChoice2 = Console.ReadLine();
            if (!string.IsNullOrEmpty(readChoice2) && readChoice2.ToLower().StartsWith("y"))
            {
                NewConfig.NewConfigMethod(true, false, false, false, false);

                Console.WriteLine("Reset done! Do you wish to continue with the execution? (Y/N)");
                var readChoice3 = Console.ReadLine();
                if (!string.IsNullOrEmpty(readChoice3) && readChoice3.ToLower().StartsWith("y"))
                {
                    //ok, keep going
                    _botInfos = FileSerialization.ReadFromBinaryFile<List<BotInfo>>(Paths.Bin.ConfigBot);
                }
                else
                {
                    Console.WriteLine("Ok, bye!");
                    return ToExit.SKIP;
                }
            }
            else
            {
                return ToExit.SKIP;
            }

            return ToExit.STAY;
        }

        private static async Task StartBotsAsync(bool advancedModeDebugDisguised)
        {
            GlobalVariables.Bots = new Dictionary<long, TelegramBotAbstract>();
            if (_botInfos != null && advancedModeDebugDisguised == false)
                foreach (var bot in _botInfos)
                {
                    var botClient = new TelegramBotClient(bot.GetToken());
                    GlobalVariables.Bots[botClient.BotId] =
                        new TelegramBotAbstract(botClient, bot.GetWebsite(), bot.GetContactString(),
                            BotTypeApi.REAL_BOT);
                    if (!bot.AcceptsMessages())
                        continue;

                    var onmessageMethod = bot.GetOnMessage();
                    if (onmessageMethod == null)
                        continue;

                    botClient.OnMessage += onmessageMethod;
                    botClient.StartReceiving();
                }

            if (_userBotsInfos != null && advancedModeDebugDisguised == false)
                foreach (var userbot in _userBotsInfos)
                {
                    var client = await UserbotConnect.ConnectAsync(userbot);
                    var userId = userbot.GetUserId();
                    if (userId != null)
                    {
                        GlobalVariables.Bots[userId.Value] = new TelegramBotAbstract(client,
                            userbot.GetWebsite(), userbot.GetContactString(), userId.Value, BotTypeApi.USER_BOT);

                        char? method = userbot.GetMethod();
                        if (method != null)
                        {
                            switch (method)
                            {
                                case 'A': //Administration
                                    {
                                        _ = Code.Bots.Administration.Main.MainMethodAsync(GlobalVariables.Bots[userId.Value]);
                                        break;
                                    }
                            }
                        }
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

            if (_botDisguisedAsUserBotInfos != null && advancedModeDebugDisguised)
                foreach (var userbot in _botDisguisedAsUserBotInfos)
                {
                    var client = await UserbotConnect.ConnectAsync(userbot);
                    var userId = userbot.GetUserId();
                    if (userId != null)
                    {
                        GlobalVariables.Bots[userId.Value] = new TelegramBotAbstract(client,
                            userbot.GetWebsite(), userbot.GetContactString(), userId.Value,
                            BotTypeApi.DISGUISED_BOT);

                        _ = TestThingsDisguisedAsync(userId.Value);
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

        private static async Task<bool> TestThingsDisguisedAsync(long userbotId)
        {
            var done = true;
            var bot = GlobalVariables.Bots[userbotId];
            var replyMarkupObject = new ReplyMarkupObject(ReplyMarkupEnum.REMOVE);
            var text = new Language(new Dictionary<string, string>
            {
                {"en", "ciao test"},
                {"it", "ciao test"}
            });
            await bot.SendTextMessageAsync(5651789, text, ChatType.Private,
                "", default, replyMarkupObject, "@ArmeF97");
            done &= await bot.CreateGroup("Gruppo test by bot",
                null, new List<long> { 5651789 });

            return done;
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

        private static async Task<bool> TestThingsAsync(long userId)
        {
            /*
            _ = Data.GlobalVariables.Bots[user_id].SendMessageReactionAsync(chatId: 415600477, //test group
                emojiReaction: "😎", messageId: 8, Telegram.Bot.Types.Enums.ChatType.Group);
            */

            var done = true;
            _ = GlobalVariables.Bots[userId];
            //Objects.TelegramMedia.GenericFile media = new Objects.TelegramMedia.Contact("+39 1234567890", "Mario", "Rossi", null);
            //done &= await bot.SendMedia(media, 5651789, ChatType.Private, "@ArmeF97", null, null);
            //done &= await CommandDispatcher.GetAllGroups(5651789, "@ArmeF97", bot, "it");
            return done;
        }
    }
}