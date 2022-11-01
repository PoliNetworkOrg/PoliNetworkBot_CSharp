#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PoliNetworkBot_CSharp.Code.Bots.Anon;
using PoliNetworkBot_CSharp.Code.Config;
using PoliNetworkBot_CSharp.Code.Data;
using PoliNetworkBot_CSharp.Code.Data.Constants;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Enums.Action;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.Exceptions;
using PoliNetworkBot_CSharp.Code.Objects.InfoBot;
using PoliNetworkBot_CSharp.Code.Utils;
using PoliNetworkBot_CSharp.Code.Utils.CallbackUtils;
using PoliNetworkBot_CSharp.Code.Utils.Logger;
using PoliNetworkBot_CSharp.Test.IG;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using File = System.IO.File;
using ThreadAsync = PoliNetworkBot_CSharp.Code.Bots.Moderation.ThreadAsync;

#endregion

namespace PoliNetworkBot_CSharp.Code.MainProgram;

internal static class Program
{
    public static readonly BotConfigAll BotConfigAll = new();

    private static async Task Main(string[]? args)
    {
        var toExit = FirstThingsToDo();
        if (toExit == ToExit.EXIT)
        {
            Logger.WriteLine("Program will stop.");
            return;
        }

        throw new Exception("testing breaking change");
        while (true)
        {
            var (item1, item2) = MainGetMenuChoice2(args);

            switch (item1)
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
                    MainBot(item1, item2);
                    return;
                }

                case '4':
                {
                    ResetEverything(false);
                    return;
                }

                case '5':
                {
                    _ = await TestIg.MainIgAsync();
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
                    try
                    {
                        //SpamTest.Main2();
                        //Test_CheckLink.Test_CheckLink2();
                        await TestIg.MainIgAsync();
                        return;
                    }
                    catch
                    {
                        // ignored
                    }

                    break;
                }
            }
        }
    }

    private static Tuple<char, bool> MainGetMenuChoice2(IReadOnlyList<string>? args)
    {
        if (args == null || args.Count == 0) return new Tuple<char, bool>(MainGetMenuChoice(), false);

        var i = 0;
        foreach (var arg in args)
        {
            Console.WriteLine("Arg [" + i + "]:");
            Console.WriteLine(arg);
            i++;
        }

        return string.IsNullOrEmpty(args[0])
            ? new Tuple<char, bool>(MainGetMenuChoice(), false)
            : new Tuple<char, bool>(args[0][0], true);
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

        try
        {
            while (true)
                Console.ReadKey();
        }
        catch (Exception e)
        {
            while (true)
                Console.Read();
        }
    }

    private static ToExit FirstThingsToDo()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        if (File.Exists("psw_anon.txt"))
            ConfigAnon.Password = File.ReadAllText("psw_anon.txt");

        if (!Directory.Exists("../config"))
            Directory.CreateDirectory("../config");

        if (!Directory.Exists("../data"))
            Directory.CreateDirectory("../data");

        MessagesStore.InitializeMessageStore();
        CallbackUtils.InitializeCallbackDatas();
        DbConfig.InitializeDbConfig();

        var currentTimeZone = TimeZoneInfo.Local;
        Logger.WriteLine("Current TimeZone: " + currentTimeZone + " time: " + DateTime.Now);
        var allowedTextTimeZone = new List<string> { "roma", "rome", "europe" };
        return allowedTextTimeZone.Any(x => currentTimeZone.DisplayName.ToLower().Contains(x))
            ? ToExit.STAY
            : ToExit.EXIT;
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

            if (string.IsNullOrEmpty(reply)) continue;
            var first = reply[0];

            return first;
        }
    }

    private static ToExit LoadBotDisguisedAsUserBotConfig(bool alwaysYes)
    {
        try
        {
            BotConfigAll.BotDisguisedAsUserBotInfos =
                JsonConvert.DeserializeObject<BotConfig>(
                    File.ReadAllText(Paths.Info.ConfigBotDisguisedAsUserBotsInfo));
        }
        catch
        {
            // ignored
        }

        if (BotConfigAll.BotDisguisedAsUserBotInfos?.bots != null &&
            BotConfigAll.BotDisguisedAsUserBotInfos.bots.Count != 0)
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
                try
                {
                    BotConfigAll.BotDisguisedAsUserBotInfos =
                        JsonConvert.DeserializeObject<BotConfig>(
                            File.ReadAllText(Paths.Info.ConfigBotDisguisedAsUserBotsInfo));
                }
                catch
                {
                    // ignored
                }
            }
            else
            {
                Logger.WriteLine("Ok, bye!");
                return ToExit.EXIT;
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
        try
        {
            BotConfigAll.UserBotsInfos =
                JsonConvert.DeserializeObject<BotConfig>(File.ReadAllText(Paths.Info.ConfigUserBotsInfo));
        }
        catch
        {
            // ignored
        }

        if (BotConfigAll.UserBotsInfos is { bots: { } } && BotConfigAll.UserBotsInfos.bots.Count != 0)
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
                try
                {
                    BotConfigAll.UserBotsInfos =
                        JsonConvert.DeserializeObject<BotConfig>(File.ReadAllText(Paths.Info.ConfigUserBotsInfo));
                }
                catch
                {
                    // ignored
                }
            }
            else
            {
                Logger.WriteLine("Ok, bye!");
                return ToExit.EXIT;
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
        try
        {
            BotConfigAll.BotInfos =
                JsonConvert.DeserializeObject<BotConfig>(File.ReadAllText(Paths.Info.ConfigBotsInfo));
        }
        catch (Exception? ex)
        {
            Logger.WriteLine(ex);
        }

        if (BotConfigAll.BotInfos is { bots: { } } && BotConfigAll.BotInfos.bots.Count != 0)
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
                try
                {
                    BotConfigAll.BotInfos = JsonConvert.DeserializeObject<BotConfig>(
                        File.ReadAllText(Paths.Info.ConfigBotsInfo));
                }
                catch
                {
                    // ignored
                }
            }
            else
            {
                Logger.WriteLine("Ok, bye!");
                return ToExit.EXIT;
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

        GlobalVariables.Bots = new Dictionary<long, TelegramBotAbstract?>();
        if (BotConfigAll.BotInfos != null && advancedModeDebugDisguised == false && runOnlyUserBot == false)
            if (BotConfigAll.BotInfos.bots != null)
                foreach (var bot in BotConfigAll.BotInfos.bots)
                {
                    var token = bot.GetToken();
                    if (string.IsNullOrEmpty(token))
                        continue;

                    var botClient = new TelegramBotClient(token);
                    if (botClient.BotId == null) continue;

                    var x2 = bot.DbConfig;
                    DbConfigConnection? x1 = null;
                    if (x2 != null)
                        x1 = new DbConfigConnection(x2);
                    x1 ??= GlobalVariables.DbConfig;

                    GlobalVariables.Bots[botClient.BotId.Value] =
                        new TelegramBotAbstract(botClient, bot.GetWebsite(), bot.GetContactString(),
                            BotTypeApi.REAL_BOT, bot.GetOnMessage().S)
                        {
                            DbConfig = x1
                        };

                    var acceptMessages = bot.AcceptsMessages();
                    if (acceptMessages is null or false)
                        continue;

                    var onmessageMethod2 = bot.GetOnMessage();
                    if (onmessageMethod2.ActionMessageEvent == null)
                        continue;

                    BotClientWhole botClientWhole = new(botClient, bot, onmessageMethod2);
                    Thread t = new(() =>
                    {
                        try
                        {
                            PreStartupActionsAsync(GlobalVariables.Bots[botClient.BotId.Value], null, bot);
                            _ = StartBotsAsync2Async(botClientWhole);
                        }
                        catch (Exception? ex)
                        {
                            Logger.WriteLine(ex);
                        }
                    });
                    t.Start();

                    if (onmessageMethod2.S == BotStartMethods.Moderation.Item1)
                        moderationBots++;
                    else if (onmessageMethod2.S == BotStartMethods.Anon.Item1)
                        anonBots++;
                }

        if (BotConfigAll.UserBotsInfos != null && advancedModeDebugDisguised == false && runOnlyNormalBot == false)
            if (BotConfigAll.UserBotsInfos.bots != null)
                foreach (var userbot in BotConfigAll.UserBotsInfos.bots)
                {
                    var client = await UserbotConnect.ConnectAsync(userbot);
                    var userId = userbot.userId;
                    if (userId == null) continue;
                    TelegramBotAbstract? x2 = null;

                    try
                    {
                        x2 = client;
                    }
                    catch
                    {
                        // ignored
                    }

                    GlobalVariables.Bots[userId.Value] = x2;

                    var method = userbot.method;
                    if (method != null)
                        switch (method)
                        {
                            case "a":
                            case "A": //Administration
                            {
                                _ = Bots.Administration.Main.MainMethodAsync(GlobalVariables.Bots[userId.Value]);
                                break;
                            }
                        }
                }

        if (BotConfigAll.BotDisguisedAsUserBotInfos != null && advancedModeDebugDisguised && runOnlyUserBot == false &&
            runOnlyNormalBot == false)
            if (BotConfigAll.BotDisguisedAsUserBotInfos.bots != null)
                foreach (var userbot in BotConfigAll.BotDisguisedAsUserBotInfos.bots)
                {
                    var client = await UserbotConnect.ConnectAsync(userbot);
                    var userId = userbot.userId;
                    if (userId == null) continue;
                    GlobalVariables.Bots[userId.Value] = client;

                    _ = TestThingsDisguisedAsync(userId.Value);
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

    private static void PreStartupActionsAsync(TelegramBotAbstract? telegramBotAbstract,
        EventArgsContainer? messageEventArgs, BotInfoAbstract botInfoAbstract)
    {
        if (Logger.ContainsCriticalErrors(out var critics))
        {
            var toSend = "WARNING! \n";
            toSend += "Critical errors found in log while starting up! \n" + critics;
            NotifyUtil.NotifyOwners_AnError_AndLog3(toSend, telegramBotAbstract, messageEventArgs,
                FileTypeJsonEnum.SIMPLE_STRING, SendActionEnum.SEND_TEXT);
        }

        using var powershell = PowerShell.Create();
        foreach (var line in ScriptUtil.DoScript(powershell, "screen -ls", true)) Logger.WriteLine(line);

        if (botInfoAbstract.onMessages != BotStartMethods.Material.Item1)
            return;

        try
        {
            if (telegramBotAbstract != null)
                _ = Database.ExecuteSelect("SELECT * FROM FilePaths", telegramBotAbstract.DbConfig);
        }
        catch (Exception? ex)
        {
            Logger.WriteLine(ex);
            try
            {
                if (telegramBotAbstract != null)
                    Database.Execute("CREATE TABLE FilePaths (" +
                                     "file_and_git VARCHAR(250)," +
                                     "location VARCHAR(250)" +
                                     ") ", telegramBotAbstract.DbConfig);
                Logger.WriteLine("Created table FilePaths");
            }
            catch (Exception)
            {
                Logger.WriteLine("Execution cannot continue, database not reachable.", LogSeverityLevel.EMERGENCY);
                Environment.Exit(1);
            }
        }
    }

    private static Task StartBotsAsync2Async(BotClientWhole botClientWhole)
    {
        const int maxWait = 1000 * 10; //10 seconds
        var i = 0;
        int? offset = null;

        Logger.WriteLine("Starting on main loop for bot: " + botClientWhole.BotInfoAbstract.onMessages);

        while (true)
            try
            {
                List<Update>? updates = null;
                try
                {
                    if (botClientWhole.BotClient != null)
                        updates = botClientWhole.BotClient.GetUpdatesAsync(offset, timeout: 250).Result.ToList();
                }
                catch (Exception? ex)
                {
                    Logger.WriteLine("Critical exception in update application!", LogSeverityLevel.EMERGENCY);
                    Logger.WriteLine(ex, LogSeverityLevel.EMERGENCY);
                    continue;
                }

                if (updates != null)
                {
                    var duplicates = updates.GroupBy(s => s.Id).SelectMany(grp => grp.Skip(1)).ToList();

                    if (duplicates.Count > 0)
                    {
                        foreach (var duplicate in duplicates)
                        {
                            var msg = "I found a duplicated update";
                            msg += "\n";
                            msg += "----";
                            msg += "\n";
                            msg += "ID: " + duplicate.Id;
                            msg += "\n";
                            msg += "Message: " + duplicate.Message;
                            msg += "\n";
                            msg += "Type: " + duplicate.Type;
                            Logger.WriteLine(msg, LogSeverityLevel.ERROR);
                        }

                        updates = updates.Distinct().ToList();
                    }
                }

                if (updates is { Count: > 0 })
                {
                    i = 0;

                    var updates2 = updates.OrderBy(o => o.Id).ToList();

                    foreach (var update in updates2)
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

                i++;

                var wait = i * 200;
                Thread.Sleep(wait > maxWait ? maxWait : wait);
            }
            catch (Exception? e)
            {
                Logger.WriteLine("Critical exception in update application!", LogSeverityLevel.CRITICAL);
                Logger.WriteLine(e, LogSeverityLevel.CRITICAL);
            }
        // ReSharper disable once FunctionNeverReturns
    }

    private static void HandleUpdate(Update update, BotClientWhole botClientWhole)
    {
        switch (update.Type)
        {
            case UpdateType.Unknown:
                break;

            case UpdateType.Message:
            {
                if (update.Message != null && botClientWhole.UpdatesMessageLastId.ContainsKey(update.Message.Chat.Id))
                    if (botClientWhole.UpdatesMessageLastId[update.Message.Chat.Id] >= update.Message.MessageId)
                        return;

                if (update.Message != null)
                {
                    botClientWhole.UpdatesMessageLastId[update.Message.Chat.Id] = update.Message.MessageId;

                    botClientWhole.OnmessageMethod2.ActionMessageEvent?.GetAction()
                        ?.Invoke(botClientWhole.BotClient,
                            new MessageEventArgs(update.Message));
                }

                break;
            }
            case UpdateType.InlineQuery:
                break;

            case UpdateType.ChosenInlineResult:
                break;

            case UpdateType.CallbackQuery:
            {
                var callback = botClientWhole.BotInfoAbstract.GetCallbackEvent();
                if (update.CallbackQuery != null && callback != null)
                    callback(botClientWhole.BotClient, new CallbackQueryEventArgs(update.CallbackQuery));
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
        var bot = GlobalVariables.Bots?[userbotId];
        if (bot == null)
            return done;

        var replyMarkupObject = new ReplyMarkupObject(ReplyMarkupEnum.REMOVE);
        var text = new Language(new Dictionary<string, string?>
        {
            { "en", "ciao test" },
            { "it", "ciao test" }
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
        catch (Exception? e1)
        {
            Logger.WriteLine(e1);
        }

        return done;
    }

#pragma warning disable IDE0051 // Rimuovi i membri privati inutilizzati

    // ReSharper disable once UnusedMember.Local
    private static bool TestThings(long userId)
#pragma warning restore IDE0051 // Rimuovi i membri privati inutilizzati
    {
        /*
        _ = Data.GlobalVariables.Bots[user_id].SendMessageReactionAsync(chatId: 415600477, //test group
            emojiReaction: "😎", messageId: 8, Telegram.Bot.Types.Enums.ChatType.Group);
        */

        // ReSharper disable once ConvertToConstant.Local
        var done = true;
        _ = GlobalVariables.Bots?[userId];
        //Objects.TelegramMedia.GenericFile media = new Objects.TelegramMedia.Contact("+39 1234567890", "Mario", "Rossi", null);
        //done &= await bot.SendMedia(media, 107050697, ChatType.Private, "@EliaMaggioni", null, null);
        //done &= await CommandDispatcher.GetAllGroups(107050697, "@EliaMaggioni", bot, "it");
        return done;
    }
}