#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JsonPolimi_Core_nf.Data;
using JsonPolimi_Core_nf.Tipi;
using JsonPolimi_Core_nf.Utils;
using PoliNetworkBot_CSharp.Code.Bots.Moderation.SpamCheck;
using PoliNetworkBot_CSharp.Code.Data.Constants;
using PoliNetworkBot_CSharp.Code.Data.Variables;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Enums.Action;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.Action;
using PoliNetworkBot_CSharp.Code.Objects.Exceptions;
using PoliNetworkBot_CSharp.Code.Objects.TelegramBotAbstract;
using PoliNetworkBot_CSharp.Code.Objects.TelegramMedia;
using PoliNetworkBot_CSharp.Code.Utils;
using PoliNetworkBot_CSharp.Code.Utils.DatabaseUtils;
using PoliNetworkBot_CSharp.Code.Utils.Logger;
using PoliNetworkBot_CSharp.Code.Utils.Notify;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

#endregion

namespace PoliNetworkBot_CSharp.Code.Bots.Moderation.Dispatcher;

internal static class CommandDispatcher
{
    public static void SendMessageInGroup(ActionFuncGenericParams actionFuncGenericParams)
    {
        var eMessage1 = actionFuncGenericParams.MessageEventArgs?.Message;
        if (eMessage1?.ReplyToMessage == null || actionFuncGenericParams.TelegramBotAbstract == null ||
            actionFuncGenericParams.Strings == null || actionFuncGenericParams.Strings.Length == 0)
        {
            actionFuncGenericParams.CommandExecutionState = CommandExecutionState.UNMET_CONDITIONS;
            return;
        }

        var forwardMessage = SendMessage.ForwardMessage(
            actionFuncGenericParams.TelegramBotAbstract,
            actionFuncGenericParams.MessageEventArgs, eMessage1.Chat.Id,
            actionFuncGenericParams.Strings[0], eMessage1.MessageId,
            eMessage1.MessageThreadId, false, null,
            CancellationToken.None);
        forwardMessage.Wait();
        actionFuncGenericParams.CommandExecutionState = CommandExecutionState.SUCCESSFUL;
    }

    public static void BanHistory(ActionFuncGenericParams actionFuncGenericParams)
    {
        throw new NotImplementedException();
        //todo: complete
        //_ = BanUserHistoryAsync(sender, e, false);
        //return Task.CompletedTask;
    }

    public static void GetRooms(ActionFuncGenericParams actionFuncGenericParams)
    {
        var roomsMainAsync = Rooms.RoomsMainAsync(actionFuncGenericParams.TelegramBotAbstract, actionFuncGenericParams.MessageEventArgs);
        roomsMainAsync.Wait();
        actionFuncGenericParams.CommandExecutionState = CommandExecutionState.SUCCESSFUL;
    }

    public static void GetRules(ActionFuncGenericParams actionFuncGenericParams)
    {
        var rules = Rules(actionFuncGenericParams.TelegramBotAbstract, actionFuncGenericParams.MessageEventArgs);
        rules.Wait();
        actionFuncGenericParams.CommandExecutionState = CommandExecutionState.SUCCESSFUL;
    }

    public static async Task<bool> CommandDispatcherMethod(TelegramBotAbstract? sender, MessageEventArgs? e)
    {
        if (e != null && string.IsNullOrEmpty(e.Message.Text)) return false;

        if (e?.Message.Text == null)
            return await DefaultCommand(sender, e);

        var cmdLines = e.Message.Text.Split(' ');
        var cmd = cmdLines[0].Trim();
        var args = cmdLines.Skip(1).ToArray();

        if (string.IsNullOrEmpty(cmd))
        {
            await DefaultCommand(sender, e);
            return false;
        }

        if (cmd.Contains('@'))
        {
            var cmd2 = cmd.Split("@");
            if (sender != null)
            {
                var botUsername = await sender.GetBotUsernameAsync();
                if (!string.Equals(cmd2[1], botUsername, StringComparison.CurrentCultureIgnoreCase))
                    return false;
            }
        }

        if (sender == null)
            return await DefaultCommand(sender, e);

        foreach (var command in SwitchDispatcher.Commands)
            try
            {
                var commandExecutionState = command.TryTrigger(e, sender, cmd, args);
                switch (commandExecutionState)
                {
                    case CommandExecutionState.SUCCESSFUL:
                        return true;
                    case CommandExecutionState.UNMET_CONDITIONS:
                        if (e.Message.Chat.Type == ChatType.Private)
                            await NotifyUserCommandError(new L(
                                    "it",
                                    "Formattazione del messaggio errata. \n" +
                                    "Per informazioni aggiuntive scrivi<b>\n" +
                                    "/help " + string.Join("</b> \n<b>/help ", command.GetTriggers().ToArray()) +
                                    "</b>",
                                    "en",
                                    "The message is wrongly formatted. \n" +
                                    "For additional info type <b>\n" +
                                    "/help " + string.Join("</b> \n<b>/help ", command.GetTriggers().ToArray()) +
                                    "</b>"),
                                sender, e);
                        else
                            await sender.DeleteMessageAsync(e.Message.Chat.Id, e.Message.MessageId, null);
                        return false;
                    case CommandExecutionState.NOT_TRIGGERED:
                    case CommandExecutionState.INSUFFICIENT_PERMISSIONS:
                    case CommandExecutionState.ERROR_NOT_ENABLED:
                    case CommandExecutionState.ERROR_DEFAULT:
                    case null:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (Exception ex)
            {
                NotifyUtil.NotifyOwnersClassic(new ExceptionNumbered(ex), sender, EventArgsContainer.Get(e));
                return false;
            }

        return await DefaultCommand(sender, e);
    }

    private static async Task<MessageSentResult?> NotifyUserCommandError(Language message, TelegramBotAbstract sender,
        MessageEventArgs? e)
    {
        if (e != null)
            return await sender.SendTextMessageAsync(e.Message.From?.Id, message, ChatType.Private,
                e.Message.From?.LanguageCode, ParseMode.Html, null, e.Message.From?.Username,
                e.Message.MessageId);
        return null;
    }


    public static void AllowMessageOwnerAsync(ActionFuncGenericParams actionFuncGenericParams)
    {
        if (actionFuncGenericParams.MessageEventArgs == null)
        {
            actionFuncGenericParams.CommandExecutionState = CommandExecutionState.UNMET_CONDITIONS;
            return;
        }

        var e = actionFuncGenericParams.MessageEventArgs;
        var sender = actionFuncGenericParams.TelegramBotAbstract;
        if (e.Message.ReplyToMessage == null || (string.IsNullOrEmpty(e.Message.ReplyToMessage.Text) &&
                                                 string.IsNullOrEmpty(e.Message.ReplyToMessage.Caption)))
        {
            var text = new Language(new Dictionary<string, string?>
            {
                { "en", "You have to reply to a message containing the message" },
                { "it", "You have to reply to a message containing the message" }
            });

            if (sender != null)
            {
                var sendTextMessageAsync = sender.SendTextMessageAsync(e.Message.From?.Id, text, ChatType.Private,
                    e.Message.From?.LanguageCode, ParseMode.Html, null, e.Message.From?.Username,
                    e.Message.MessageId);
                sendTextMessageAsync.Wait();
                return;
            }

            actionFuncGenericParams.CommandExecutionState = CommandExecutionState.UNMET_CONDITIONS;
            return;
        }

        MessagesStore.AllowMessageOwner(e.Message.ReplyToMessage.Text);
        MessagesStore.AllowMessageOwner(e.Message.ReplyToMessage.Caption);
        Logger.WriteLine(
            e.Message.ReplyToMessage.Text ?? e.Message.ReplyToMessage.Caption ??
            "Error in allowmessage, both caption and text are null");
        actionFuncGenericParams.CommandExecutionState = CommandExecutionState.SUCCESSFUL;
    }

    public static void AllowMessageAsync(ActionFuncGenericParams actionFuncGenericParams)
    {
        var fourHours = new TimeSpan(4, 0, 0);
        var allowMessage = Assoc.AllowMessage(actionFuncGenericParams.MessageEventArgs,
            actionFuncGenericParams.TelegramBotAbstract, fourHours);
        allowMessage.Wait();
        actionFuncGenericParams.CommandExecutionState = CommandExecutionState.SUCCESSFUL;
    }

    public static async Task<string?> GetRunningTime()
    {
        try
        {
            using var powershell = PowerShell.Create();
            const string path = "./static/build-date.txt";
            return await File.ReadAllTextAsync(path);
        }
        catch
        {
            // ignored
        }

        return null;
    }

    private static async Task CommandNeedsAReplyToMessage(TelegramBotAbstract? sender, MessageEventArgs? e)
    {
        var lang = new Language(new Dictionary<string, string?>
        {
            { "it", "E' necessario rispondere ad un messaggio per usare questo comando" },
            { "en", "This command only works with a reply to message" }
        });
        if (e != null)
        {
            var eMessage = e.Message;
            var eMessageFrom = eMessage.From;
            await SendMessage.SendMessageInPrivateOrAGroup(sender,
                lang, eMessageFrom?.LanguageCode, eMessageFrom?.Username, eMessageFrom?.Id,
                eMessageFrom?.FirstName, eMessageFrom?.LastName, eMessage.Chat.Id,
                eMessage.Chat.Type, eMessage.MessageThreadId);
        }
    }

    public static async Task<UpdateGroupsResult> UpdateGroups(TelegramBotAbstract? sender, bool dry,
        bool debug,
        bool updateDb, MessageEventArgs? messageEventArgs)
    {
        Logger.WriteLine(
            "UpdateGroups started (dry: " + dry + ", debug: " + debug + ", updateDB: " + updateDb + ")",
            LogSeverityLevel.ALERT);
        List<ResultFixGroupsName>? x1 = null;
        if (updateDb) x1 = await Groups.FixAllGroupsName(sender, messageEventArgs);

        var groups = Groups.GetAllGroups(sender, true);

        Variabili.L = new ListaGruppo();

        Variabili.L.HandleSerializedObject(groups);

        CheckSeILinkVanno2(5, true, 10);

        var json =
            JsonBuilder.GetJson(new CheckGruppo(CheckGruppo.E.RICERCA_SITO_V3),
                false);

        if (!Directory.Exists(Paths.Data.PoliNetworkWebsiteData))
        {
            Directory.CreateDirectory(Paths.Data.PoliNetworkWebsiteData);
            InitGithubRepo();
        }

        const string path = Paths.Data.PoliNetworkWebsiteData + "/groupsGenerated.json";
        await File.WriteAllTextAsync(path, json, Encoding.UTF8);
        if (dry)
        {
            Logger.WriteLine(await File.ReadAllTextAsync(path));
            var l = new Language(new Dictionary<string, string?>
            {
                { "it", "Dry run completata" },
                { "en", "Dry run completed" }
            });
            return new UpdateGroupsResult(l, x1);
        }

        var output = ExecuteBashCommand("./static/github_pusher.sh");

        Logger.WriteLine(output);

        var text = output.Length > 0
            ? new Dictionary<string, string?>
            {
                { "it", "Done \n" },
                { "en", "Done \n" }
            }
            : new Dictionary<string, string?>
            {
                { "it", "Error in execution" },
                { "en", "Error in execution" }
            };

        _ = NotifyUtil.NotifyOwners_AnError_AndLog3(
            "UpdateGroup result: \n" + (string.IsNullOrEmpty(output) ? "No PR created" : "Command succesfuly executed"),
            sender, null,
            FileTypeJsonEnum.SIMPLE_STRING, SendActionEnum.SEND_FILE);

        var l1 = new Language(text);
        return new UpdateGroupsResult(l1, x1);
    }

    private static void CheckSeILinkVanno2(int volteCheCiRiprova, bool laPrimaVoltaControllaDaCapo,
        int waitOgniVoltaCheCiRiprova)
    {
        ParametriFunzione parametriFunzione = new();
        parametriFunzione.AddParam(volteCheCiRiprova, "volteCheCiRiprova");
        parametriFunzione.AddParam(laPrimaVoltaControllaDaCapo, "laPrimaVoltaControllaDaCapo");
        parametriFunzione.AddParam(waitOgniVoltaCheCiRiprova, "waitOgniVoltaCheCiRiprova");
        RunEventoLogged(Variabili.L.CheckSeILinkVanno, parametriFunzione);
    }

    private static void RunEventoLogged(Func<ParametriFunzione, EventoConLog> funcEvent,
        ParametriFunzione parametriFunzione)
    {
        var eventoLog = funcEvent.Invoke(parametriFunzione);
        eventoLog.RunAction();
        Logger.Log(eventoLog);
    }

    private static void InitGithubRepo()
    {
        Logger.WriteLine("Init websitedata repository");
        // using var powershell = PowerShell.Create();
        // ScriptUtil.DoScript(powershell, "cd ./data/", true);
        // ScriptUtil.DoScript(powershell, "/bin/bash -c \"ssh-add /git/ssh-key && git clone " + GitHubConfig.GetRepo() + "\"", true); //todo: add /git/ssh-key to GitHubConfig
        // ScriptUtil.DoScript(powershell, "cd ./polinetworkWebsiteData", true);
        // ScriptUtil.DoScript(powershell, "git remote add org " + GitHubConfig.GetRemote(), true);
        var output = ExecuteBashCommand("./static/github_cloner.sh");

        Logger.WriteLine(output);
    }

    private static string ExecuteBashCommand(string command)
    {
        // according to: https://stackoverflow.com/a/15262019/637142
        // thans to this we will pass everything as one command
        command = command.Replace("\"", "\"\"");

        var proc = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = "-c \"" + command + "\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            }
        };

        proc.Start();
        proc.WaitForExit();

        return proc.StandardOutput.ReadToEnd();
    }

    public static void TestSpamAsync(ActionFuncGenericParams actionFuncGenericParams)
    {
        var e = actionFuncGenericParams.MessageEventArgs;
        var sender = actionFuncGenericParams.TelegramBotAbstract;
        var message = e?.Message.ReplyToMessage;
        if (message == null)
        {
            actionFuncGenericParams.CommandExecutionState = CommandExecutionState.UNMET_CONDITIONS;
            return;
        }

        if (e?.Message == null)
        {
            actionFuncGenericParams.CommandExecutionState = CommandExecutionState.SUCCESSFUL;
            return;
        }

        var eMessage = e.Message;
        var r2 = MessagesStore.StoreAndCheck(eMessage.ReplyToMessage);

        if (r2 is not (SpamType.SPAM_PERMITTED or SpamType.SPAM_LINK))
        {
            var e2 = new MessageEventArgs(eMessage.ReplyToMessage ?? eMessage);
            var checkSpamAsync = CheckSpam.CheckSpamAsync(e2, sender, false);
            r2 = checkSpamAsync.Result;
        }


        try
        {
            var dict = new Dictionary<string, string?>
            {
                { "en", r2.ToString() }
            };
            var text = new Language(dict);
            try
            {
                if (eMessage.From != null)
                    if (sender != null)
                    {
                        var sendTextMessageAsync = sender.SendTextMessageAsync(
                            eMessage.From.Id, text, ChatType.Private, "en",
                            ParseMode.Html,
                            null, null,
                            eMessage.MessageThreadId);
                        sendTextMessageAsync.Wait();
                    }
            }
            catch
            {
                // ignored
            }
        }
        catch
        {
            actionFuncGenericParams.CommandExecutionState = CommandExecutionState.ERROR_DEFAULT;
            return;
        }

        actionFuncGenericParams.CommandExecutionState = CommandExecutionState.SUCCESSFUL;
    }
#pragma warning disable IDE0051 // Rimuovi i membri privati inutilizzati


    public static async Task<bool> MassiveSendAsync(TelegramBotAbstract sender, MessageEventArgs e,
        string textToSend)
    {
        var groups = Database.ExecuteSelect("Select id FROM GroupsTelegram", sender.DbConfig);

        return await MassiveSendUtil.MassiveSendSlaveAsync(sender, e, groups, textToSend, false);
    }


#pragma warning disable CS1998 // Il metodo asincrono non contiene operatori 'await', pertanto verrà eseguito in modo sincrono
#pragma warning disable IDE0051 // Rimuovi i membri privati inutilizzati

    private static async Task<List<long>> BanUserHistoryAsync(TelegramBotAbstract sender, long idGroup)
#pragma warning restore IDE0051 // Rimuovi i membri privati inutilizzati
#pragma warning restore CS1998 // Il metodo asincrono non contiene operatori 'await', pertanto verrà eseguito in modo sincrono
    {
        const string? queryForBannedUsers =
            "SELECT * from Banned as B1 WHERE when_banned >= (SELECT MAX(B2.when_banned) from Banned as B2 where B1.target = B2.target) and banned_true_unbanned_false = 83";
        var bannedUsers = Database.ExecuteSelect(queryForBannedUsers, sender.DbConfig);
        if (bannedUsers == null)
            return new List<long>();
        var bannedUsersId = bannedUsers.Rows[bannedUsers.Columns.IndexOf("target")].ItemArray;
        var bannedUsersIdArray = bannedUsersId.Select(user =>
        {
            var r1 = user?.ToString();
            return r1 != null ? long.Parse(r1) : 0;
        }).ToList();

        return bannedUsersIdArray;
    }

    /*
#pragma warning disable IDE0051 // Rimuovi i membri privati inutilizzati
    private static async Task<object> BanUserHistoryAsync(TelegramBotAbstract sender, MessageEventArgs e,
#pragma warning restore IDE0051 // Rimuovi i membri privati inutilizzati
        bool? revokeMessage)
    {
        var r = Owners.CheckIfOwner(e.Message.From.Id);
        if (!r) return r;

        var query = "SELECT DISTINCT T1.target FROM " +
                    "(SELECT * FROM Banned WHERE banned_true_unbanned_false = 83 ORDER BY when_banned DESC) AS T1, " +
                    "(SELECT * FROM Banned WHERE banned_true_unbanned_false != 83 ORDER BY when_banned DESC) AS T2 " +
                    "WHERE (T1.target = T2.target AND T1.when_banned >= T2.when_banned AND T1.target IN (SELECT target FROM(SELECT target FROM Banned WHERE banned_true_unbanned_false != 83 ORDER BY when_banned DESC))) OR (T1.target NOT IN (SELECT target FROM (SELECT target FROM Banned WHERE banned_true_unbanned_false != 83 ORDER BY when_banned DESC)))";
        var x = SqLite.ExecuteSelect(query);

        if (x == null || x.Rows == null || x.Rows.Count == 0)
        {
            var text3 = new Language(new Dictionary<string, string?>
            {
                {"en", "There are no users to ban!"}
            });
            await sender.SendTextMessageAsync(e.Message.From.Id, text3, ChatType.Private,
                e.Message.From.LanguageCode, ParseMode.Html, null, e.Message.From.Username, e.Message.MessageId);
            return false;
        }

        var groups = SqLite.ExecuteSelect("Select id FROM GroupsTelegram");
        /*
        if(e.Message.Text.Length !=10)
        {
            Language text2 = new Language(new Dictionary<string, string?>() {
                {"en", "Group not found (1)!"}
            });
            await sender.SendTextMessageAsync(e.Message.From.Id, text2, ChatType.Private, e.Message.From.LanguageCode, ParseMode.Html, null, e.Message.From.Username, e.Message.MessageId, false);
            return false;
        }

        var counter = 0;
        var channel = Regex.Match(e.Message.Text, @"\d+").Value;
        if (groups.Select("id = " + "'" + channel + "'").Length != 1)
        {
            var text2 = new Language(new Dictionary<string, string?>
            {
                {"en", "Group not found! (2)"}
            });
            await sender.SendTextMessageAsync(e.Message.From.Id, text2, ChatType.Private,
                e.Message.From.LanguageCode, ParseMode.Html, null, e.Message.From.Username, e.Message.MessageId);
            return false;
        }

        foreach (DataRow element in x.Rows)
        {
            var userToBeBanned = Convert.ToInt64(element.ItemArray[0]);
            await RestrictUser.BanUserFromGroup(sender, userToBeBanned, Convert.ToInt64(channel), null,
                revokeMessage);
            counter++;
        }

        var text = new Language(new Dictionary<string, string?>
        {
            {"en", "Banned " + counter + " in group: " + groups.Select(channel)}
        });
        await sender.SendTextMessageAsync(e.Message.From.Id, text, ChatType.Private, e.Message.From.LanguageCode,
            ParseMode.Html, null, e.Message.From.Username, e.Message.MessageId);
        return true;
    }
    */

    public static async Task<long?> QueryBot(bool executeTrueSelectFalse, MessageEventArgs? e,
        TelegramBotAbstract? sender)
    {
        if (e?.Message.ForwardFrom != null)
            return null;

        if (e?.Message.From == null)
            return null;

        if (GlobalVariables.IsOwner(e.Message.From.Id))
            return await QueryBot2(executeTrueSelectFalse, e, sender);

        return null;
    }

    private static async Task<long?> QueryBot2(bool executeTrueSelectFalse, MessageEventArgs? e,
        TelegramBotAbstract? sender)
    {
        if (e == null) return null;
        var eMessage = e.Message;
        var eMessageReplyToMessage = eMessage.ReplyToMessage;
        var value = eMessageReplyToMessage?.Text;
        var eMessageFrom = eMessage.From;
        if (eMessageReplyToMessage == null || string.IsNullOrEmpty(value))
        {
            var text = new Language(new Dictionary<string, string?>
            {
                { "en", "You have to reply to a message containing the query" }
            });
            if (eMessageFrom == null) return null;
            if (sender != null)
                await sender.SendTextMessageAsync(eMessageFrom.Id, text, ChatType.Private,
                    eMessageFrom.LanguageCode, ParseMode.Html, null, eMessageFrom.Username,
                    eMessage.MessageId);
            return null;
        }

        if (executeTrueSelectFalse)
            if (sender != null)
            {
                var i = Database.Execute(value, sender.DbConfig);

                var text = new Language(new Dictionary<string, string?>
                {
                    { "en", "Query execution. Result: " + i }
                });
                if (eMessageFrom != null)
                    await sender.SendTextMessageAsync(eMessageFrom.Id, text, ChatType.Private,
                        eMessageFrom.LanguageCode, ParseMode.Html, null, eMessageFrom.Username,
                        eMessage.MessageId);
                return i;
            }


        if (sender == null) return null;
        var x = Database.ExecuteSelect(value, sender.DbConfig);
        var x2 = StreamSerialization.SerializeToStream(x);
        var documentInput =
            new TelegramFile(x2, "table.bin", new L("Query result"), "application/octet-stream",
                TextAsCaption.AS_CAPTION);

        if (eMessageFrom == null)
            return -1;

        PeerAbstract peer = new(eMessageFrom.Id, eMessage.Chat.Type);
        var v = sender.SendFileAsync(documentInput, peer, eMessageFrom.Username,
            eMessageFrom.LanguageCode, eMessage.MessageId, false, eMessage.MessageThreadId);
        return v ? 1 : 0;
    }

    public static async Task<MessageSentResult?> TestTime(TelegramBotAbstract? sender, MessageEventArgs? e)
    {
        if (e?.Message.From == null)
            return null;

        var eMessage = e.Message;
        if (eMessage.Text == null) return null;
        var eMessageFrom = eMessage.From;
        var tuple1 = await AskUser.AskDateAsync(eMessageFrom.Id,
            eMessage.Text,
            eMessageFrom.LanguageCode, sender, eMessageFrom.Username, eMessage.MessageThreadId);

        var exception = tuple1?.Item2;
        if (exception != null)
        {
            var s = tuple1?.Item3;
            var extraInfo = new ExtraInfo
            {
                GenericInfo = s
            };
            var eventArgsContainer = new EventArgsContainer { MessageEventArgs = e };
            NotifyUtil.NotifyOwnersClassic(new ExceptionNumbered(exception), sender, eventArgsContainer, extraInfo);

            return null;
        }

        var dateTimeSchedule = tuple1?.Item1;
        if (dateTimeSchedule == null) return null;
        var sentDate2 = dateTimeSchedule.GetDate();

        var dict = new Dictionary<string, string?>
        {
            { "en", DateTimeClass.DateTimeToAmericanFormat(sentDate2) }
        };
        var text = new Language(dict);
        var argsContainer = EventArgsContainer.Get(e);
        return await SendMessage.SendMessageInPrivate(sender, eMessageFrom.Id,
            eMessageFrom.LanguageCode, eMessageFrom.Username,
            text, ParseMode.Html, eMessage.MessageId, InlineKeyboardMarkup.Empty(), argsContainer,
            eMessage.MessageThreadId);
    }

    private static async Task<MessageSentResult?> Rules(TelegramBotAbstract? sender, MessageEventArgs? e)
    {
        const string text = "Ecco le regole!\n" +
                            "https://polinetwork.org/it/rules";

        const string textEng = "Here are the rules!\n" +
                               "https://polinetwork.org/en/rules";

        var text2 = new Language(new Dictionary<string, string?>
        {
            { "en", textEng },
            { "it", text }
        });

        var eventArgsContainer = EventArgsContainer.Get(e);
        var eMessage = e?.Message;
        var eMessageFrom = eMessage?.From;
        return await SendMessage.SendMessageInPrivate(sender, eMessageFrom?.Id,
            eMessageFrom?.LanguageCode,
            eMessageFrom?.Username, text2, ParseMode.Html,
            null, InlineKeyboardMarkup.Empty(),
            eventArgsContainer, eMessage?.MessageThreadId);
    }

    public static void SendRecommendedGroupsAsync(ActionFuncGenericParams actionFuncGenericParams)
    {
        var e = actionFuncGenericParams.MessageEventArgs;
        var sender = actionFuncGenericParams.TelegramBotAbstract;
        
        const string text = "<i>Lista di gruppi consigliati</i>:\n" +
                            "\n👥 Gruppo di tutti gli studenti @PoliGruppo 👈\n" +
                            "\n📖 Libri @PoliBook\n" +
                            "\n🤪 Spotted & Memes @PolimiSpotted @PolimiMemes\n" +
                            "\n🥳 Eventi @PoliEventi\n" +
                            "\nRicordiamo che sul nostro sito vi sono tutti i link ai gruppi con tanto ricerca, facci un salto!\n" +
                            "https://polinetwork.github.io/";

        const string textEng = "<i>List of recommended groups</i>:\n" +
                               "\n👥 Group with all students @PoliGruppo 👈\n" +
                               "\n📖 Books @PoliBook\n" +
                               "\n🤪 Spotted & Memes @PolimiSpotted @PolimiMemes\n" +
                               "\n🥳 Events @PoliEventi\n" +
                               "\nWe remind you that on our website there are all link to the groups, and they are searchable, have a look!\n" +
                               "https://polinetwork.github.io/";

        var text2 = new Language(new Dictionary<string, string?>
        {
            { "en", textEng },
            { "it", text }
        });
        var eventArgsContainer = EventArgsContainer.Get(e);
        var eMessage = e?.Message;
        var eMessageFrom = eMessage?.From;
        var sendMessageInPrivate = SendMessage.SendMessageInPrivate(sender, eMessageFrom?.Id,
            eMessageFrom?.LanguageCode,
            eMessageFrom?.Username, text2, ParseMode.Html, null, InlineKeyboardMarkup.Empty(),
            eventArgsContainer, eMessage?.MessageThreadId);
        sendMessageInPrivate.Wait();
        actionFuncGenericParams.CommandExecutionState= CommandExecutionState.SUCCESSFUL;
    }

    public static Task<bool> GetAllGroups(long? chatId, string? username, TelegramBotAbstract? sender,
        string? lang, ChatType? chatType, int? messageThreadId)
    {
        var groups = Groups.GetAllGroups(sender);
        Stream? stream = new MemoryStream();
        FileSerialization.SerializeFile(groups, ref stream);

        if (chatType == null)
            return Task.FromResult(false);

        var peer = new PeerAbstract(chatId, chatType.Value);

        var text2 = new Language(new Dictionary<string, string?>
        {
            { "en", "Here are all groups:" },
            { "it", "Ecco tutti i gruppi:" }
        });
        var sendFileAsync = SendMessage.SendFileAsync(new TelegramFile(stream, "groups.bin",
                text2, "application/octet-stream", TextAsCaption.BEFORE_FILE), peer,
            sender, username, messageThreadId, lang, null, true);
        return Task.FromResult(sendFileAsync);
    }


    public static async Task<bool> DefaultCommand(TelegramBotAbstract? sender, MessageEventArgs? e)
    {
        var text2 = new Language(new Dictionary<string, string?>
        {
            {
                "en",
                "I'm sorry, but I don't know this command. Try to ask the administrators (/contact)"
            },
            {
                "it",
                "Mi dispiace, ma non conosco questo comando. Prova a contattare gli amministratori (/contact)"
            }
        });
        var eventArgsContainer = EventArgsContainer.Get(e);
        var eMessage = e?.Message;
        var eMessageFrom = eMessage?.From;
        await SendMessage.SendMessageInPrivate(sender, eMessageFrom?.Id,
            eMessageFrom?.LanguageCode,
            eMessageFrom?.Username, text2,
            ParseMode.Html,
            null, InlineKeyboardMarkup.Empty(), eventArgsContainer, eMessage?.MessageThreadId);

        return true;
    }

    private static async Task CommandNotSentInPrivateAsync(TelegramBotAbstract? sender, MessageEventArgs? e)
    {
        var lang = new Language(new Dictionary<string, string?>
        {
            { "it", "Questo messaggio funziona solo in chat privata" },
            { "en", "This command only works in private chat with me" }
        });
        if (e != null)
        {
            var eMessage = e.Message;
            var eMessageFrom = eMessage.From;
            var eMessageChat = eMessage.Chat;
            await SendMessage.SendMessageInPrivateOrAGroup(sender,
                lang, eMessageFrom?.LanguageCode, eMessageFrom?.Username, eMessageFrom?.Id,
                eMessageFrom?.FirstName, eMessageFrom?.LastName,
                eMessageChat.Id, eMessageChat.Type, eMessage.MessageThreadId);

            if (sender != null)
                await sender.DeleteMessageAsync(eMessageChat.Id, eMessage.MessageId, null);
        }
    }

    public static void HelpExtended(ActionFuncGenericParams actionFuncGenericParams)
    {
        var helpExtendedSlave = Help.HelpExtendedSlave(actionFuncGenericParams.MessageEventArgs, actionFuncGenericParams.TelegramBotAbstract);
        helpExtendedSlave.Wait();
        actionFuncGenericParams.CommandExecutionState =  CommandExecutionState.SUCCESSFUL;
    }

    public static void HelpPrivate(ActionFuncGenericParams actionFuncGenericParams)
    {
        var args = actionFuncGenericParams.Strings;
        var e = actionFuncGenericParams.MessageEventArgs;
        var sender = actionFuncGenericParams.TelegramBotAbstract;
        if (args == null || args.Length == 0)
        {
            var helpPrivateSlave = Help.HelpPrivateSlave(e, sender);
            helpPrivateSlave.Wait();
        }
        else
        {
            var helpSpecific = Help.HelpSpecific(e, sender, args);
            helpSpecific.Wait();
        }

        actionFuncGenericParams.CommandExecutionState = CommandExecutionState.SUCCESSFUL;
    }

    public static void ContactUs(ActionFuncGenericParams actionFuncGenericParams)
    {
        var e = actionFuncGenericParams.MessageEventArgs;
        var sender = actionFuncGenericParams.TelegramBotAbstract;
        var telegramBotClient = actionFuncGenericParams.TelegramBotAbstract;
        var eMessage = e?.Message;
        var deleteIfMessageIsNotInPrivate = DeleteMessage.DeleteIfMessageIsNotInPrivate(telegramBotClient, eMessage);
        deleteIfMessageIsNotInPrivate.Wait();
        if (telegramBotClient == null)
        {
            actionFuncGenericParams.CommandExecutionState = CommandExecutionState.ERROR_DEFAULT;
            return;
        }

        var lang2 = new Language(new Dictionary<string, string?>
        {
            { "it", telegramBotClient.GetContactString() },
            { "en", telegramBotClient.GetContactString() }
        });
        var eMessageChat = eMessage?.Chat;
        var eMessageFrom = eMessage?.From;
        var sendTextMessageAsync = telegramBotClient.SendTextMessageAsync(eMessageChat?.Id,
            lang2, eMessageChat?.Type, eMessageFrom?.LanguageCode,
            ParseMode.Html,
            new ReplyMarkupObject(ReplyMarkupEnum.REMOVE), eMessageFrom?.Username, eMessage?.MessageThreadId
        );
        sendTextMessageAsync.Wait();
        actionFuncGenericParams.CommandExecutionState =  CommandExecutionState.SUCCESSFUL;
    }

    public static void ForceCheckInviteLinksAsync(ActionFuncGenericParams actionFuncGenericParams)
    {
        if (actionFuncGenericParams.MessageEventArgs == null)
        {
            actionFuncGenericParams.CommandExecutionState = CommandExecutionState.UNMET_CONDITIONS;
            return;
        }

        long? n = null;
        var eventArgsContainer = EventArgsContainer.Get(actionFuncGenericParams.MessageEventArgs);
        try
        {
            var fillMissingLinksIntoDbAsync = 
                InviteLinks.FillMissingLinksIntoDB_Async(
                    actionFuncGenericParams.TelegramBotAbstract, actionFuncGenericParams.MessageEventArgs);
            n = fillMissingLinksIntoDbAsync.Result;
        }
        catch (Exception? e2)
        {
            NotifyUtil.NotifyOwnersClassic(new ExceptionNumbered(e2), actionFuncGenericParams.TelegramBotAbstract, eventArgsContainer);
        }

        if (n == null)
        {
            actionFuncGenericParams.CommandExecutionState = CommandExecutionState.ERROR_DEFAULT;
            return;
        }
 

        var text2 = new Language(new Dictionary<string, string?>
        {
            { "en", "I have updated n=" + n + " links" },
            { "it", "Ho aggiornato n=" + n + " link" }
        });

        var eMessage = actionFuncGenericParams.MessageEventArgs.Message;
        var eMessageFrom = eMessage.From;
        var sendMessageInPrivate = SendMessage.SendMessageInPrivate(actionFuncGenericParams.TelegramBotAbstract, eMessageFrom?.Id,
            eMessageFrom?.LanguageCode,
            eMessageFrom?.Username, text2,
            ParseMode.Html,
            eMessage.MessageId, InlineKeyboardMarkup.Empty(),
            eventArgsContainer, eMessage.MessageThreadId);
        sendMessageInPrivate.Wait();

        actionFuncGenericParams.CommandExecutionState = CommandExecutionState.SUCCESSFUL;
    }

    private static async Task Start(MessageEventArgs? e, TelegramBotAbstract? telegramBotClient, string[]? args)
    {
        var eMessage = e?.Message;
        await DeleteMessage.DeleteIfMessageIsNotInPrivate(telegramBotClient, eMessage);
        var lang2 = new Language(new Dictionary<string, string?>
        {
            {
                "it", "Ciao! 👋\n" +
                      "\nScrivi /help per la lista completa delle mie funzioni 👀\n" +
                      "\nVisita anche il nostro sito " + telegramBotClient?.GetWebSite()
            },
            {
                "en", "Hi! 👋\n" +
                      "\nWrite /help for the complete list of my functions👀\n" +
                      "\nAlso visit our site " + telegramBotClient?.GetWebSite()
            }
        });
        if (telegramBotClient != null)
        {
            var eMessageChat = eMessage?.Chat;
            var eMessageFrom = eMessage?.From;
            await telegramBotClient.SendTextMessageAsync(eMessageChat?.Id,
                lang2,
                eMessageChat?.Type, replyMarkupObject: new ReplyMarkupObject(ReplyMarkupEnum.REMOVE),
                lang: eMessageFrom?.LanguageCode, username: eMessageFrom?.Username, parseMode: ParseMode.Html,
                messageThreadId: eMessage?.MessageThreadId
            );
        }
    }

    public static async Task<bool> BanMessageActions(TelegramBotAbstract? telegramBotClient, MessageEventArgs? e)
    {
        return await NotifyUtil.NotifyOwnersBanAction(telegramBotClient, EventArgsContainer.Get(e),
            e?.Message.LeftChatMember?.Id,
            e?.Message.LeftChatMember?.Username);
    }
}