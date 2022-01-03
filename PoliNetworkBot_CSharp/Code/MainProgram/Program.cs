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
                Tuple<char, bool> readChoice = MainGetMenuChoice2(args);

                switch (readChoice.Item1)
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
                            MainBot(readChoice.Item1, readChoice.Item2);
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

        private static Tuple<char, bool> MainGetMenuChoice2(string[] args)
        {
            if (args == null || args.Length == 0)
            {
                return new Tuple<char, bool>(MainGetMenuChoice(), false);
            }

            int i = 0;
            foreach (var arg in args)
            {
                Console.WriteLine("Arg [" + i + "]:");
                Console.WriteLine(arg);
            }

            if (string.IsNullOrEmpty(args[0]))
            {
                return new Tuple<char, bool>(MainGetMenuChoice(), false);
            }

            return new Tuple<char, bool>(args[0][0], true);
        }

        private static void MainBot(char readChoice, bool alwaysYes)
        {
            var toExit = LoadBotConfig(alwaysYes);
            if (toExit == ToExit.EXIT)
                return;

            var toExit2 = LoadUserBotConfig(alwaysYes);
            if (toExit2 == ToExit.EXIT)
                return;

            var toExit3 = LoadBotDisguisedAsUserBotConfig(alwaysYes);
            if (toExit3 == ToExit.EXIT)
                return;

            GlobalVariables.LoadToRam();

            Logger.WriteLine("\nTo kill this process, you have to check the process list");

            _ = StartBotsAsync(readChoice == '3', readChoice == '8', readChoice == '9');

            while (true)
                Console.ReadKey();
        }

        private static void FirstThingsToDo()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            if (File.Exists("psw_anon.txt")) ConfigAnon.password = File.ReadAllText("psw_anon.txt");
        }

        private static void ResetEverything(bool alsoFillTablesFromJson)
        {
            NewConfig.NewConfigMethod(true, true, true, true, alsoFillTablesFromJson);
            Logger.WriteLine("Reset done!");
        }

        private static char MainGetMenuChoice()
        {
            while (true)
            {
                Logger.WriteLine("Welcome to our bots system!\n" +
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

        private static ToExit LoadBotDisguisedAsUserBotConfig(bool alwaysYes)
        {
            _botDisguisedAsUserBotInfos =
                FileSerialization.ReadFromBinaryFile<List<BotDisguisedAsUserBotInfo>>(Paths.Bin
                    .ConfigBotDisguisedAsUserbot);
            if (_botDisguisedAsUserBotInfos != null && _botDisguisedAsUserBotInfos.Count != 0)
                return ToExit.STAY;

            Logger.WriteLine(
                "It seems that the bot disguised as userbot configuration isn't available. Do you want to reset it? (Y/N)");
            var readChoice2 = alwaysYes ? "y" : Console.ReadLine();
            if (!string.IsNullOrEmpty(readChoice2) && readChoice2.ToLower().StartsWith("y"))
            {
                NewConfig.NewConfigMethod(false, false, true, false, false);

                Logger.WriteLine("Reset done! Do you wish to continue with the execution? (Y/N)");
                var readChoice3 = alwaysYes ? "y" : Console.ReadLine();
                if (!string.IsNullOrEmpty(readChoice3) && readChoice3.ToLower().StartsWith("y"))
                {
                    //ok, keep going
                    _botDisguisedAsUserBotInfos =
                        FileSerialization.ReadFromBinaryFile<List<BotDisguisedAsUserBotInfo>>(Paths.Bin
                            .ConfigBotDisguisedAsUserbot);
                }
                else
                {
                    Logger.WriteLine("Ok, bye!");
                    return ToExit.SKIP;
                }
            }
            else
            {
                return ToExit.SKIP;
            }

            return ToExit.STAY;
        }

        private static ToExit LoadUserBotConfig(bool alwaysYes)
        {
            _userBotsInfos = FileSerialization.ReadFromBinaryFile<List<UserBotInfo>>(Paths.Bin.ConfigUserbot);
            if (_userBotsInfos != null && _userBotsInfos.Count != 0)
                return ToExit.STAY;

            Logger.WriteLine(
                "It seems that the userbot configuration isn't available. Do you want to reset it? (Y/N)");
            var readChoice2 = alwaysYes ? "y" : Console.ReadLine();
            if (!string.IsNullOrEmpty(readChoice2) && readChoice2.ToLower().StartsWith("y"))
            {
                NewConfig.NewConfigMethod(false, true, false, false, false);

                Logger.WriteLine("Reset done! Do you wish to continue with the execution? (Y/N)");
                var readChoice3 = alwaysYes ? "y" : Console.ReadLine();
                if (!string.IsNullOrEmpty(readChoice3) && readChoice3.ToLower().StartsWith("y"))
                {
                    //ok, keep going
                    _userBotsInfos = FileSerialization.ReadFromBinaryFile<List<UserBotInfo>>(Paths.Bin.ConfigUserbot);
                }
                else
                {
                    Logger.WriteLine("Ok, bye!");
                    return ToExit.SKIP;
                }
            }
            else
            {
                return ToExit.SKIP;
            }

            return ToExit.STAY;
        }

        private static ToExit LoadBotConfig(bool alwaysYes)
        {
            _botInfos = FileSerialization.ReadFromBinaryFile<List<BotInfo>>(Paths.Bin.ConfigBot);
            if (_botInfos != null && _botInfos.Count != 0)
                return ToExit.STAY;

            Logger.WriteLine(
                "It seems that the bot configuration isn't available. Do you want to reset it? (Y/N)");
            var readChoice2 = alwaysYes ? "y" : Console.ReadLine();
            if (!string.IsNullOrEmpty(readChoice2) && readChoice2.ToLower().StartsWith("y"))
            {
                NewConfig.NewConfigMethod(true, false, false, false, false);

                Logger.WriteLine("Reset done! Do you wish to continue with the execution? (Y/N)");
                var readChoice3 = alwaysYes ? "y" : Console.ReadLine();
                if (!string.IsNullOrEmpty(readChoice3) && readChoice3.ToLower().StartsWith("y"))
                {
                    //ok, keep going
                    _botInfos = FileSerialization.ReadFromBinaryFile<List<BotInfo>>(Paths.Bin.ConfigBot);
                }
                else
                {
                    Logger.WriteLine("Ok, bye!");
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
                    if (botClient.BotId != null)
                    {
                        GlobalVariables.Bots[botClient.BotId.Value] =
                            new TelegramBotAbstract(botClient, bot.GetWebsite(), bot.GetContactString(),
                                BotTypeApi.REAL_BOT, bot.GetOnMessage().Item2);
                        if (!bot.AcceptsMessages())
                            continue;

                        var onmessageMethod2 = bot.GetOnMessage();
                        if (onmessageMethod2 == null || onmessageMethod2.Item1 == null)
                            continue;

                        BotClientWhole botClientWhole = new(botClient, bot, onmessageMethod2);
                        Thread t = new(start: () =>
                        {
                            try
                            {
                                _ = StartBotsAsync2Async(botClientWhole);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex);
                            }
                        });
                        t.Start();

                        if (onmessageMethod2.Item2 == BotStartMethods.Moderation) moderationBots++;

                        if (onmessageMethod2.Item2 == BotStartMethods.Anon) anonBots++;
                    }
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

        private static async Task StartBotsAsync2Async(BotClientWhole botClientWhole)
        {
            const int MAX_WAIT = 1000 * 60 * 5; //5 minutes
            int i = 0;
            int? offset = null;

            while (true)
            {
                Telegram.Bot.Types.Update[] updates = null;
                try
                {
                    updates = await botClientWhole.botClient.GetUpdatesAsync(offset);
                }
#pragma warning disable CS0168 // La variabile è dichiarata, ma non viene mai usata
                catch (Exception ex)
#pragma warning restore CS0168 // La variabile è dichiarata, ma non viene mai usata
                {
                    //Console.WriteLine(ex);
                    //Console.WriteLine("\n");
                }

                if (updates != null && updates.Length > 0)
                {
                    i = 0;
                    foreach (Telegram.Bot.Types.Update update in updates)
                    {
                        if (update != null)
                        {
                            try
                            {
                                HandleUpdate(update, botClientWhole);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                            }

                            offset = update.Id + 1;
                        }
                    }
                }

                i++;

                int wait = i * 500;
                Thread.Sleep(wait > MAX_WAIT ? MAX_WAIT : wait);
            }
        }

        private static void HandleUpdate(Telegram.Bot.Types.Update update, BotClientWhole botClientWhole)
        {
            switch (update.Type)
            {
                case UpdateType.Unknown:
                    break;

                case UpdateType.Message:
                    {
                        botClientWhole.onmessageMethod2.Item1(botClientWhole.botClient, new MessageEventArgs(update.Message));
                        break;
                    }
                case UpdateType.InlineQuery:
                    break;

                case UpdateType.ChosenInlineResult:
                    break;

                case UpdateType.CallbackQuery:
                    {
                        var callback = botClientWhole.bot.GetCallbackEvent();
                        callback(botClientWhole.botClient, new CallbackQueryEventArgs(update.CallbackQuery));
                        break;
                    }
                case UpdateType.EditedMessage:
                    break;

                case UpdateType.ChannelPost:
                    break;

                case UpdateType.EditedChannelPost:
                    break;

                case UpdateType.ShippingQuery:
                    break;

                case UpdateType.PreCheckoutQuery:
                    break;

                case UpdateType.Poll:
                    break;

                case UpdateType.PollAnswer:
                    break;

                case UpdateType.MyChatMember:
                    break;

                case UpdateType.ChatMember:
                    break;

                case UpdateType.ChatJoinRequest:
                    break;
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
            await bot.SendTextMessageAsync(768169879, text, ChatType.Private,
                "", default, replyMarkupObject, "@polinetwork3bot");

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
                Logger.WriteLine(e1);
            }

            ;

            return done;
        }

#pragma warning disable IDE0051 // Rimuovi i membri privati inutilizzati

        private static bool TestThings(long userId)
#pragma warning restore IDE0051 // Rimuovi i membri privati inutilizzati
        {
            /*
            _ = Data.GlobalVariables.Bots[user_id].SendMessageReactionAsync(chatId: 415600477, //test group
                emojiReaction: "😎", messageId: 8, Telegram.Bot.Types.Enums.ChatType.Group);
            */

            var done = true;
            _ = GlobalVariables.Bots[userId];
            //Objects.TelegramMedia.GenericFile media = new Objects.TelegramMedia.Contact("+39 1234567890", "Mario", "Rossi", null);
            //done &= await bot.SendMedia(media, 107050697, ChatType.Private, "@EliaMaggioni", null, null);
            //done &= await CommandDispatcher.GetAllGroups(107050697, "@EliaMaggioni", bot, "it");
            return done;
        }
    }
}