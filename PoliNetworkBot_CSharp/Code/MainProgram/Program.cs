#region

using PoliNetworkBot_CSharp.Code.Bots.Anon;
using PoliNetworkBot_CSharp.Code.Config;
using PoliNetworkBot_CSharp.Code.Data;
using PoliNetworkBot_CSharp.Code.Data.Constants;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.InfoBot;
using PoliNetworkBot_CSharp.Code.Utils;
using PoliNetworkBot_CSharp.Test.IG;
using PoliNetworkBot_CSharp.Test.Spam;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using ThreadAsync = PoliNetworkBot_CSharp.Code.Bots.Moderation.ThreadAsync;

#endregion

namespace PoliNetworkBot_CSharp.Code.MainProgram
{
    internal static class Program
    {
        private static List<BotInfo> _botInfos;
        private static List<UserBotInfo> _userBotsInfos;
        private static List<BotDisguisedAsUserBotInfo> _botDisguisedAsUserBotInfos;

        private static async Task Main(string[] args)
        {
            FirstThingsToDo();

            while (true)
            {
                var readChoice = MainGetMenuChoice();

                switch (readChoice)
                {
                    case '1': //reset everything
                        {
                            ResetEverything(true);

                            return;
                        }

                    case '2': //normal mode
                    case '3': //disguised bot test
                    case '8':
                    case '9':
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

                            _ = StartBotsAsync(readChoice == '3', readChoice == '8', readChoice == '9');

                            while (true) Console.ReadKey();
                            return;
                        }

                    case '4':
                        {
                            ResetEverything(false);
                            return;
                        }

                    case '5':
                        {
                            _ = await Test_IG.MainIGAsync();
                            return;
                        }

                    case '6':
                        {
                            NewConfig.NewConfigMethod(true, false, false, false, false);
                            return;
                        }

                    case '7':
                        {
                            NewConfig.NewConfigMethod(false, false, true, false, false);
                            return;
                        }
                    case 't':
                        {
                            SpamTest.Main2();
                            return;
                        }
                }
            }
        }

        private static void FirstThingsToDo()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            if (File.Exists("psw_anon.txt")) ConfigAnon.password = File.ReadAllText("psw_anon.txt");
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
                                  "5) Test IG\n" +
                                  "6) Reset only bot config\n" +
                                  "7) Reset only disguised bot config\n" +
                                  "8) Run only userbots\n" +
                                  "9) Run only normal bots\n" +
                                  "t) Test\n" +
                                  "\n");

                var reply = Console.ReadLine();

                if (!string.IsNullOrEmpty(reply))
                {
                    var first = reply[0];

                    return first;
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

        public static async Task StartBotsAsync(bool advancedModeDebugDisguised, bool runOnlyUserBot,
            bool runOnlyNormalBot)
        {
            var moderationBots = 0;
            var anonBots = 0;

            GlobalVariables.Bots = new Dictionary<long, TelegramBotAbstract>();
            if (_botInfos != null && advancedModeDebugDisguised == false && runOnlyUserBot == false)
                foreach (var bot in _botInfos)
                {
                    var botClient = new TelegramBotClient(bot.GetToken());
                    GlobalVariables.Bots[botClient.BotId] =
                        new TelegramBotAbstract(botClient, bot.GetWebsite(), bot.GetContactString(),
                            BotTypeApi.REAL_BOT, bot.GetOnMessage().Item2);
                    if (!bot.AcceptsMessages())
                        continue;

                    var onmessageMethod2 = bot.GetOnMessage();
                    if (onmessageMethod2 == null || onmessageMethod2.Item1 == null)
                        continue;

                    botClient.OnMessage += onmessageMethod2.Item1;
                    botClient.StartReceiving(bot.GetAllowedUpdates());

                    if (bot.Callback()) botClient.OnCallbackQuery += bot.GetCallbackEvent();

                    if (onmessageMethod2.Item2 == BotStartMethods.Moderation) moderationBots++;

                    if (onmessageMethod2.Item2 == BotStartMethods.Anon) anonBots++;
                }

            if (_userBotsInfos != null && advancedModeDebugDisguised == false && runOnlyNormalBot == false)
                foreach (var userbot in _userBotsInfos)
                {
                    var client = await UserbotConnect.ConnectAsync(userbot);
                    var userId = userbot.GetUserId();
                    if (userId != null)
                    {
                        TelegramBotAbstract x2 = null;

                        try
                        {
                            x2 = new TelegramBotAbstract(client,
                                userbot.GetWebsite(), userbot.GetContactString(), userId.Value, BotTypeApi.USER_BOT,
                                userbot.GetOnMessage().Item2);
                        }
                        catch
                        {
                            ;
                        }

                        GlobalVariables.Bots[userId.Value] = x2;

                        var method = userbot.GetMethod();
                        if (method != null)
                            switch (method)
                            {
                                case 'a':
                                case 'A': //Administration
                                    {
                                        _ = Bots.Administration.Main.MainMethodAsync(GlobalVariables.Bots[userId.Value]);
                                        break;
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

            if (_botDisguisedAsUserBotInfos != null && advancedModeDebugDisguised && runOnlyUserBot == false &&
                runOnlyNormalBot == false)
                foreach (var userbot in _botDisguisedAsUserBotInfos)
                {
                    var client = await UserbotConnect.ConnectAsync(userbot);
                    var userId = userbot.GetUserId();
                    if (userId != null)
                    {
                        GlobalVariables.Bots[userId.Value] = new TelegramBotAbstract(client,
                            userbot.GetWebsite(), userbot.GetContactString(), userId.Value,
                            BotTypeApi.DISGUISED_BOT, userbot.GetOnMessage().Item2);

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

            if (GlobalVariables.Bots.Keys.Count > 0 && moderationBots > 0)
            {
                var t = new Thread(ThreadAsync.DoThingsAsyncBot);
                t.Start();
            }

            if (GlobalVariables.Bots.Keys.Count > 0 && anonBots > 0)
            {
                var t = new Thread(Bots.Anon.ThreadAsync.DoThingsAsyncBotAsync);
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

            /*
            done &= await bot.CreateGroup("Gruppo test by bot",
                null, new List<long> { 5651789 });
            */

            try
            {
                done &= await bot.UpdateUsername("PoliAssociazioni", "PoliAssociazioni2");
            }
            catch (Exception e1)
            {
                ;
            }

            ;

            return done;
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