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
using PoliNetworkBot_CSharp.Code.Bots.Moderation.Generic;
using PoliNetworkBot_CSharp.Code.Config;
using PoliNetworkBot_CSharp.Code.Data.Constants;
using PoliNetworkBot_CSharp.Code.Data.Variables;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.AbstractBot;
using PoliNetworkBot_CSharp.Code.Objects.Exceptions;
using PoliNetworkBot_CSharp.Code.Objects.InfoBot;
using PoliNetworkBot_CSharp.Code.Utils.DatabaseUtils;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using File = System.IO.File;

namespace PoliNetworkBot_CSharp.Code.Utils.Main;

public static class ProgramUtil
{
    public static readonly BotConfigAll BotConfigAll = new();

    internal static Tuple<char, bool> MainGetMenuChoice2(IReadOnlyList<string>? args)
    {
        if (args == null || args.Count == 0)
        {
            var mainGetMenuChoice = MainGetMenuChoice();
            return new Tuple<char, bool>(mainGetMenuChoice, false);
        }

        var i = 0;
        foreach (var arg in args)
        {
            Console.WriteLine("Arg [" + i + "]:");
            Console.WriteLine(arg);
            i++;
        }

        var value = args[0];
        if (!string.IsNullOrEmpty(value))
            return new Tuple<char, bool>(value[0], true);

        var mainGetMenuChoice2 = MainGetMenuChoice();
        return new Tuple<char, bool>(mainGetMenuChoice2, false);
    }

    internal static void MainBot(char readChoice, bool alwaysYes)
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

        Logger.Logger.WriteLine("\nTo kill this process, you have to check the process list");

        DbConfig.InitializeDbConfig();


        _ = StartBotsAsync(readChoice == '3', readChoice == '8', readChoice == '9');

        try
        {
            while (true)
            {
                Thread.Sleep(1000);
                Console.ReadKey();
            }
        }
        catch (Exception)
        {
            while (true)
            {
                Thread.Sleep(1000);
                Console.Read();
            }
        }
    }

    internal static ToExit FirstThingsToDo()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        if (File.Exists("psw_anon.txt"))
            ConfigAnon.Password = File.ReadAllText("psw_anon.txt");

        if (!Directory.Exists("../config"))
            Directory.CreateDirectory("../config");

        if (!Directory.Exists("./data"))
            Directory.CreateDirectory("./data");

        //MessagesStore.InitializeMessageStore();
        //CallbackUtils.CallbackUtils.InitializeCallbackDatas();
        //DbConfig.InitializeDbConfig();
        //todo

        var currentTimeZone = TimeZoneInfo.Local;
        Logger.Logger.WriteLine("Current TimeZone: " + currentTimeZone + " time: " + DateTime.Now);
        var allowedTextTimeZone = new List<string> { "roma", "rome", "europe" };
        return allowedTextTimeZone.Any(x => currentTimeZone.DisplayName.ToLower().Contains(x))
            ? ToExit.STAY
            : ToExit.EXIT;
    }

    internal static void ResetEverything(bool alsoFillTablesFromJson)
    {
        NewConfig.NewConfigMethod(true, true, true, true, alsoFillTablesFromJson);
        Logger.Logger.WriteLine("Reset done!");
    }

    private static char MainGetMenuChoice()
    {
        while (true)
        {
            Logger.Logger.WriteLine("Welcome to our bots system!\n" +
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
                                    "r) Reset db and do 2\n" +
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

        Logger.Logger.WriteLine(
            "It seems that the bot disguised as userbot configuration isn't available. Do you want to reset it? (Y/N)");
        var readChoice2 = alwaysYes ? "y" : Console.ReadLine();
        if (!string.IsNullOrEmpty(readChoice2) && readChoice2.ToLower().StartsWith("y"))
        {
            NewConfig.NewConfigMethod(false, false, true, false, false);

            Logger.Logger.WriteLine("Reset done! Do you wish to continue with the execution? (Y/N)");
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
                Logger.Logger.WriteLine("Ok, bye!");
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

        if (BotConfigAll.UserBotsInfos is { bots: not null } && BotConfigAll.UserBotsInfos.bots.Count != 0)
            return ToExit.STAY;

        Logger.Logger.WriteLine(
            "It seems that the userbot configuration isn't available. Do you want to reset it? (Y/N)");
        var readChoice2 = alwaysYes ? "y" : Console.ReadLine();
        if (!string.IsNullOrEmpty(readChoice2) && readChoice2.ToLower().StartsWith("y"))
        {
            NewConfig.NewConfigMethod(false, true, false, false, false);

            Logger.Logger.WriteLine("Reset done! Do you wish to continue with the execution? (Y/N)");
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
                Logger.Logger.WriteLine("Ok, bye!");
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
            Logger.Logger.WriteLine(ex);
        }

        if (BotConfigAll.BotInfos is { bots: not null } && BotConfigAll.BotInfos.bots.Count != 0)
            return ToExit.STAY;

        Logger.Logger.WriteLine(
            "It seems that the bot configuration isn't available. Do you want to reset it? (Y/N)");
        var readChoice2 = alwaysYes ? "y" : Console.ReadLine();
        if (!string.IsNullOrEmpty(readChoice2) && readChoice2.ToLower().StartsWith("y"))
        {
            NewConfig.NewConfigMethod(true, false, false, false, false);

            Logger.Logger.WriteLine("Reset done! Do you wish to continue with the execution? (Y/N)");
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
                Logger.Logger.WriteLine("Ok, bye!");
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

                    await PrintBotInfo(botClient);

                    var x2 = bot.DbConfig;
                    DbConfigConnection? x1 = null;
                    if (x2 != null)
                        x1 = new DbConfigConnection(x2);
                    x1 ??= GlobalVariables.DbConfig;


                    var onMessageMethodObject = bot.GetOnMessage();
                    var website = bot.GetWebsite();
                    var contactString = bot.GetContactString();
                    var botGithubToken = bot.GithubToken;
                    var mode = onMessageMethodObject.S;
                    var telegramBotAbstract = new TelegramBotAbstract(
                        botClient,
                        website,
                        contactString,
                        BotTypeApi.REAL_BOT,
                        mode,
                        botGithubToken,
                        bot
                    )
                    {
                        DbConfig = x1
                    };


                    GlobalVariables.Bots[botClient.BotId.Value] =
                        telegramBotAbstract;

                    var acceptMessages = bot.AcceptsMessages();
                    if (acceptMessages is null or false)
                        continue;

                    if (onMessageMethodObject.ActionMessageEvent == null)
                        continue;

                    BotClientWhole botClientWhole = new(botClient, bot, onMessageMethodObject);
                    Thread t = new(() =>
                    {
                        try
                        {
                            PreStartupActionsAsync(GlobalVariables.Bots[botClient.BotId.Value], null, bot);
                            _ = StartBotsAsync2Async(botClientWhole);
                        }
                        catch (Exception? ex)
                        {
                            Logger.Logger.WriteLine(ex);
                        }
                    });
                    t.Start();

                    if (onMessageMethodObject.S == BotStartMethods.Moderation.Item1)
                        moderationBots++;
                    else if (onMessageMethodObject.S == BotStartMethods.Anon.Item1)
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
            var t = new Thread(ThreadAsyncModeration.DoThingsAsyncBot);
            t.Start();
        }

        if (GlobalVariables.Bots.Keys.Count > 0 && anonBots > 0)
        {
            var t = new Thread(ThreadAsyncAnon.DoThingsAsyncBot_Anon_First_Async);
            t.Start();
        }
    }

    private static async Task PrintBotInfo(ITelegramBotClient botClient)
    {
        try
        {
            Console.WriteLine("Started #####START#####");
            Console.WriteLine("Started id " + botClient.BotId);
            Console.WriteLine("Started username " + (await botClient.GetMeAsync()).Username);
            Console.WriteLine("Started #####END#####");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    private static void PreStartupActionsAsync(TelegramBotAbstract? telegramBotAbstract,
        EventArgsContainer? messageEventArgs, BotInfoAbstract botInfoAbstract)
    {
        Logger.Logger.EnableSelfManagedLogger = botInfoAbstract.EnableSelfManagedLogger ?? false;

        using var powershell = PowerShell.Create();
        foreach (var line in ScriptUtil.DoScript(powershell, "screen -ls", true)) Logger.Logger.WriteLine(line);

        if (botInfoAbstract.onMessages != BotStartMethods.Material.Item1)
            return;

        try
        {
            if (telegramBotAbstract != null)
                _ = Database.ExecuteSelect("SELECT * FROM FilePaths", telegramBotAbstract.DbConfig);
        }
        catch (Exception? ex)
        {
            Logger.Logger.WriteLine(ex);
            try
            {
                if (telegramBotAbstract != null)
                    Database.Execute("CREATE TABLE FilePaths (" +
                                     "file_and_git VARCHAR(250)," +
                                     "location VARCHAR(250)" +
                                     ") ", telegramBotAbstract.DbConfig);
                Logger.Logger.WriteLine("Created table FilePaths");
            }
            catch (Exception)
            {
                Logger.Logger.WriteLine("Execution cannot continue, database not reachable.",
                    LogSeverityLevel.EMERGENCY);
                Environment.Exit(1);
            }
        }
    }

    private static Task StartBotsAsync2Async(BotClientWhole botClientWhole)
    {
        Logger.Logger.WriteLine("Starting on main loop for bot: " + botClientWhole.BotInfoAbstract.onMessages);
        int? offset = null;
        while (true)
            try
            {
                List<Update>? updates = null;
                try
                {
                    Thread.Sleep(200);
                    if (botClientWhole.BotClient != null)
                        updates = botClientWhole.BotClient.GetUpdatesAsync(offset, 20, 250)
                            .Result.ToList();
                    Logger.Logger.WriteLine("Received " + updates?.Count + " Updates. Offset: " + offset,
                        LogSeverityLevel.DEBUG);
                }
                catch (Exception e) when
                    (e is ApiRequestException or AggregateException) // Overlap in cluster to verify healthy application
                {
                    Logger.Logger.WriteLine(e, LogSeverityLevel.ALERT);
                    Logger.Logger.WriteLine("Probably other container is still active, waiting 10 seconds");
                    Thread.Sleep(10 * 1000);
                }
                catch (Exception? ex)
                {
                    Logger.Logger.WriteLine("Critical exception in update application!", LogSeverityLevel.EMERGENCY);
                    Logger.Logger.WriteLine(ex, LogSeverityLevel.EMERGENCY);
                    continue;
                }

                if (updates == null || updates.Count == 0) continue;


                var enumerable = updates.Select(update =>
                {
                    return new Thread(ThreadStart);

                    void ThreadStart()
                    {
                        try
                        {
                            HandleUpdate(update, botClientWhole);
                        }
                        catch (Exception e)
                        {
                            Logger.Logger.WriteLine(e, LogSeverityLevel.ALERT);
                        }
                    }
                });

                foreach (var thread in enumerable)
                    try
                    {
                        thread.Start();
                    }
                    catch (Exception e)
                    {
                        Logger.Logger.WriteLine(e, LogSeverityLevel.ALERT);
                    }

                offset ??= 0;
                offset = updates.Last().Id + 1;
            }
            catch (Exception? e)
            {
                Logger.Logger.WriteLine("Critical exception in update application!", LogSeverityLevel.CRITICAL);
                Logger.Logger.WriteLine(e, LogSeverityLevel.CRITICAL);
            }
        // ReSharper disable once FunctionNeverReturns
    }

    private static void HandleUpdate(Update update, BotClientWhole botClientWhole)
    {
        switch (update.Type)
        {
            case UpdateType.Unknown:
                break;

            case UpdateType.EditedMessage:
            case UpdateType.Message:
            {
                var updateMessage = update.Message ?? update.EditedMessage;
                if (updateMessage != null &&
                    botClientWhole.UpdatesMessageLastId.TryGetValue(updateMessage.Chat.Id, out var value))
                    if (value >= updateMessage.MessageId)
                        return;

                if (updateMessage != null)
                {
                    botClientWhole.UpdatesMessageLastId[updateMessage.Chat.Id] = updateMessage.MessageId;

                    var edit = update.Type == UpdateType.EditedMessage;
                    botClientWhole.OnmessageMethod2.ActionMessageEvent?.GetAction()
                        ?.Invoke(botClientWhole.BotClient,
                            new MessageEventArgs(updateMessage, edit));
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
            {
                //todo: eventualmente gestire le richieste di ingresso ai gruppi
                break;
            }
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


        var messageOptions = new MessageOptions

        {
            ChatId = 768169879,
            Text = text,
            ChatType = ChatType.Private,
            ReplyMarkupObject = replyMarkupObject,
            Username = "@polinetwork3bot"
        };
        await bot.SendTextMessageAsync(messageOptions);

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
            Logger.Logger.WriteLine(e1);
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