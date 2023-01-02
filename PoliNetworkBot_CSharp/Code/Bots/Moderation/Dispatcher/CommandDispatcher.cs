#region

using System;
using System.Collections.Generic;
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
using PoliNetworkBot_CSharp.Code.Config;
using PoliNetworkBot_CSharp.Code.Data.Constants;
using PoliNetworkBot_CSharp.Code.Data.Variables;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Enums.Action;
using PoliNetworkBot_CSharp.Code.Objects;
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
    public static async Task<CommandExecutionState> SendMessageInGroup(MessageEventArgs? e,
        TelegramBotAbstract? sender, string[]? args)
    {
        if (e?.Message.ReplyToMessage == null || sender == null || args == null || args.Length == 0)
            return CommandExecutionState.UNMET_CONDITIONS;

        await SendMessage.ForwardMessage(sender, e, e.Message.Chat.Id, args[0], e.Message.MessageId, false, null,
            CancellationToken.None);
        return CommandExecutionState.SUCCESSFUL;
    }

    public static Task<CommandExecutionState> BanHistory(MessageEventArgs? e, TelegramBotAbstract? sender)
    {
        throw new NotImplementedException();
        //todo: complete
        //_ = BanUserHistoryAsync(sender, e, false);
        //return Task.CompletedTask;
    }

    public static async Task<CommandExecutionState> GetRooms(MessageEventArgs? e, TelegramBotAbstract? sender)
    {
        await Rooms.RoomsMainAsync(sender, e);
        return CommandExecutionState.SUCCESSFUL;
    }

    public static async Task<CommandExecutionState> GetRules(MessageEventArgs? e, TelegramBotAbstract? sender)
    {
        _ = await Rules(sender, e);
        return CommandExecutionState.SUCCESSFUL;
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
                switch (command.TryTrigger(e, sender, cmd, args))
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


    public static async Task<CommandExecutionState> AllowMessageOwnerAsync(MessageEventArgs? e,
        TelegramBotAbstract? sender)
    {
        if (e == null) return CommandExecutionState.UNMET_CONDITIONS;
        if (e.Message.ReplyToMessage == null || (string.IsNullOrEmpty(e.Message.ReplyToMessage.Text) &&
                                                 string.IsNullOrEmpty(e.Message.ReplyToMessage.Caption)))
        {
            var text = new Language(new Dictionary<string, string?>
            {
                { "en", "You have to reply to a message containing the message" },
                { "it", "You have to reply to a message containing the message" }
            });

            if (sender != null)
                await sender.SendTextMessageAsync(e.Message.From?.Id, text, ChatType.Private,
                    e.Message.From?.LanguageCode, ParseMode.Html, null, e.Message.From?.Username,
                    e.Message.MessageId);
            return CommandExecutionState.UNMET_CONDITIONS;
        }

        MessagesStore.AllowMessageOwner(e.Message.ReplyToMessage.Text);
        MessagesStore.AllowMessageOwner(e.Message.ReplyToMessage.Caption);
        Logger.WriteLine(
            e.Message.ReplyToMessage.Text ?? e.Message.ReplyToMessage.Caption ??
            "Error in allowmessage, both caption and text are null");
        return CommandExecutionState.SUCCESSFUL;
    }

    public static async Task<CommandExecutionState> AllowMessageAsync(MessageEventArgs? e, TelegramBotAbstract? sender)
    {
        var fourHours = new TimeSpan(4, 0, 0);
        await Assoc.AllowMessage(e, sender, fourHours);
        return CommandExecutionState.SUCCESSFUL;
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
            await SendMessage.SendMessageInPrivateOrAGroup(sender,
                lang, e.Message.From?.LanguageCode, e.Message.From?.Username, e.Message.From?.Id,
                e.Message.From?.FirstName, e.Message.From?.LastName, e.Message.Chat.Id, e.Message.Chat.Type);
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

        using var powershell = PowerShell.Create();
        const string cd = Paths.Data.PoliNetworkWebsiteData;
        ScriptUtil.DoScript(powershell, "cd " + cd, debug);
        ScriptUtil.DoScript(powershell, "git fetch org", debug);
        ScriptUtil.DoScript(powershell, "git pull --force", debug);
        ScriptUtil.DoScript(powershell, "git add . --ignore-errors", debug);

        var commit = @"git commit -m ""[Automatic Commit] Updated Group List""" +
                     @" --author=""" + GitHubConfig.GetUser() + "<" + GitHubConfig.GetEmail() +
                     @">""";
        ScriptUtil.DoScript(powershell, commit, debug);

        var push = @"git push https://" + GitHubConfig.GetUser() + ":" +
                   GitHubConfig.GetPassword() + "@" +
                   GitHubConfig.GetRepo() + @" --all -f";
        ScriptUtil.DoScript(powershell, push, debug);

        const string hubPr =
            @"hub pull-request -m ""[AutoCommit] Groups Update"" -b PoliNetworkOrg:main -h PoliNetworkDev:main -l bot -f";

        var result = ScriptUtil.DoScript(powershell, hubPr, debug);

        powershell.Stop();

        var toBeSent = result.Aggregate("", (current, s) => current + s + "\n");

        var text = result.Count > 0
            ? new Dictionary<string, string?>
            {
                { "it", "Done \n" + toBeSent },
                { "en", "Done \n" + toBeSent }
            }
            : new Dictionary<string, string?>
            {
                { "it", "Error in execution" },
                { "en", "Error in execution" }
            };

        _ = NotifyUtil.NotifyOwners_AnError_AndLog3(
            "UpdateGroup result: \n" + (string.IsNullOrEmpty(toBeSent) ? "No PR created" : toBeSent), sender, null,
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
        using var powershell = PowerShell.Create();
        ScriptUtil.DoScript(powershell, "cd ./data/", true);
        ScriptUtil.DoScript(powershell, "git clone https://" + GitHubConfig.GetRepo(), true);
        ScriptUtil.DoScript(powershell, "cd ./polinetworkWebsiteData", true);
        ScriptUtil.DoScript(powershell, "git remote add org https://" + GitHubConfig.GetRemote(), true);
    }

    public static async Task<CommandExecutionState> TestSpamAsync(MessageEventArgs? e, TelegramBotAbstract? sender)
    {
        var message = e?.Message.ReplyToMessage;
        if (message == null)
            return CommandExecutionState.UNMET_CONDITIONS;
        if (e?.Message == null)
            return CommandExecutionState.SUCCESSFUL;

        var r2 = MessagesStore.StoreAndCheck(e.Message.ReplyToMessage);

        if (r2 is not (SpamType.SPAM_PERMITTED or SpamType.SPAM_LINK))
        {
            var e2 = new MessageEventArgs(e.Message.ReplyToMessage ?? e.Message);
            r2 = await CheckSpam.CheckSpamAsync(e2, sender, false);
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
                if (e.Message.From != null)
                    if (sender != null)
                        await sender.SendTextMessageAsync(e.Message.From.Id, text, ChatType.Private, "en",
                            ParseMode.Html,
                            null, null);
            }
            catch
            {
                // ignored
            }
        }
        catch
        {
            return CommandExecutionState.ERROR_DEFAULT;
        }

        return CommandExecutionState.SUCCESSFUL;
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
        if (e.Message.ReplyToMessage == null || string.IsNullOrEmpty(e.Message.ReplyToMessage.Text))
        {
            var text = new Language(new Dictionary<string, string?>
            {
                { "en", "You have to reply to a message containing the query" }
            });
            if (e.Message.From == null) return null;
            if (sender != null)
                await sender.SendTextMessageAsync(e.Message.From.Id, text, ChatType.Private,
                    e.Message.From.LanguageCode, ParseMode.Html, null, e.Message.From.Username,
                    e.Message.MessageId);
            return null;
        }

        var query = e.Message.ReplyToMessage.Text;
        if (executeTrueSelectFalse)
            if (sender != null)
            {
                var i = Database.Execute(query, sender.DbConfig);

                var text = new Language(new Dictionary<string, string?>
                {
                    { "en", "Query execution. Result: " + i }
                });
                if (e.Message.From != null)
                    await sender.SendTextMessageAsync(e.Message.From.Id, text, ChatType.Private,
                        e.Message.From.LanguageCode, ParseMode.Html, null, e.Message.From.Username,
                        e.Message.MessageId);
                return i;
            }


        if (sender == null) return null;
        var x = Database.ExecuteSelect(query, sender.DbConfig);
        var x2 = StreamSerialization.SerializeToStream(x);
        var documentInput =
            new TelegramFile(x2, "table.bin", new L("Query result"), "application/octet-stream",
                TextAsCaption.AS_CAPTION);

        if (e.Message.From == null)
            return -1;

        PeerAbstract peer = new(e.Message.From.Id, e.Message.Chat.Type);
        var v = sender.SendFileAsync(documentInput, peer, e.Message.From.Username,
            e.Message.From.LanguageCode, e.Message.MessageId, false);
        return v ? 1 : 0;
    }

    public static async Task<MessageSentResult?> TestTime(TelegramBotAbstract? sender, MessageEventArgs? e)
    {
        if (e?.Message.From == null)
            return null;

        if (e.Message.Text == null) return null;
        var tuple1 = await AskUser.AskDateAsync(e.Message.From.Id,
            e.Message.Text,
            e.Message.From.LanguageCode, sender, e.Message.From.Username);

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
        return await SendMessage.SendMessageInPrivate(sender, e.Message.From.Id,
            e.Message.From.LanguageCode, e.Message.From.Username,
            text, ParseMode.Html, e.Message.MessageId, InlineKeyboardMarkup.Empty(), EventArgsContainer.Get(e));
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

        return await SendMessage.SendMessageInPrivate(sender, e?.Message.From?.Id,
            e?.Message.From?.LanguageCode,
            e?.Message.From?.Username, text2, ParseMode.Html, null, InlineKeyboardMarkup.Empty(),
            EventArgsContainer.Get(e));
    }

    public static async Task<CommandExecutionState> SendRecommendedGroupsAsync(MessageEventArgs? e,
        TelegramBotAbstract? sender)
    {
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
        await SendMessage.SendMessageInPrivate(sender, e?.Message.From?.Id,
            e?.Message.From?.LanguageCode,
            e?.Message.From?.Username, text2, ParseMode.Html, null, InlineKeyboardMarkup.Empty(),
            EventArgsContainer.Get(e));
        return CommandExecutionState.SUCCESSFUL;
    }

    public static Task<bool> GetAllGroups(long? chatId, string? username, TelegramBotAbstract? sender,
        string? lang, ChatType? chatType)
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
        return Task.FromResult(SendMessage.SendFileAsync(new TelegramFile(stream, "groups.bin",
                text2, "application/octet-stream", TextAsCaption.BEFORE_FILE), peer,
            sender, username, lang, null, true));
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
        await SendMessage.SendMessageInPrivate(sender, e?.Message.From?.Id,
            e?.Message.From?.LanguageCode,
            e?.Message.From?.Username, text2,
            ParseMode.Html,
            null, InlineKeyboardMarkup.Empty(), EventArgsContainer.Get(e));

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
            await SendMessage.SendMessageInPrivateOrAGroup(sender,
                lang, e.Message.From?.LanguageCode, e.Message.From?.Username, e.Message.From?.Id,
                e.Message.From?.FirstName, e.Message.From?.LastName, e.Message.Chat.Id, e.Message.Chat.Type);

            if (sender != null)
                await sender.DeleteMessageAsync(e.Message.Chat.Id, e.Message.MessageId, null);
        }
    }

    public static async Task<CommandExecutionState> HelpExtended(MessageEventArgs? e, TelegramBotAbstract? sender)
    {
        await Help.HelpExtendedSlave(e, sender);
        return CommandExecutionState.SUCCESSFUL;
    }

    public static async Task<CommandExecutionState> HelpPrivate(MessageEventArgs? e, TelegramBotAbstract? sender,
        string[]? args)
    {
        if (args == null || args.Length == 0)
            await Help.HelpPrivateSlave(e, sender);
        else
            await Help.HelpSpecific(e, sender, args);
        return CommandExecutionState.SUCCESSFUL;
    }

    public static async Task<CommandExecutionState> ContactUs(MessageEventArgs? e,
        TelegramBotAbstract? telegramBotClient)
    {
        await DeleteMessage.DeleteIfMessageIsNotInPrivate(telegramBotClient, e?.Message);
        if (telegramBotClient == null)
            return CommandExecutionState.ERROR_DEFAULT;

        var lang2 = new Language(new Dictionary<string, string?>
        {
            { "it", telegramBotClient.GetContactString() },
            { "en", telegramBotClient.GetContactString() }
        });
        await telegramBotClient.SendTextMessageAsync(e?.Message.Chat.Id,
            lang2, e?.Message.Chat.Type, e?.Message.From?.LanguageCode,
            ParseMode.Html,
            new ReplyMarkupObject(ReplyMarkupEnum.REMOVE), e?.Message.From?.Username
        );
        return CommandExecutionState.SUCCESSFUL;
    }

    public static async Task<CommandExecutionState> ForceCheckInviteLinksAsync(MessageEventArgs? e,
        TelegramBotAbstract? sender)
    {
        long? n = null;
        try
        {
            n = await InviteLinks.FillMissingLinksIntoDB_Async(sender, e);
        }
        catch (Exception? e2)
        {
            NotifyUtil.NotifyOwnersClassic(new ExceptionNumbered(e2), sender, EventArgsContainer.Get(e));
        }

        if (n == null)
            return CommandExecutionState.ERROR_DEFAULT;

        var text2 = new Language(new Dictionary<string, string?>
        {
            { "en", "I have updated n=" + n + " links" },
            { "it", "Ho aggiornato n=" + n + " link" }
        });
        if (e != null)
            await SendMessage.SendMessageInPrivate(sender, e.Message.From?.Id,
                e.Message.From?.LanguageCode,
                e.Message.From?.Username, text2,
                ParseMode.Html,
                e.Message.MessageId, InlineKeyboardMarkup.Empty(), EventArgsContainer.Get(e));
        return CommandExecutionState.SUCCESSFUL;
    }

    private static async Task Start(MessageEventArgs? e, TelegramBotAbstract? telegramBotClient, string[]? args)
    {
        await DeleteMessage.DeleteIfMessageIsNotInPrivate(telegramBotClient, e?.Message);
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
            await telegramBotClient.SendTextMessageAsync(e?.Message.Chat.Id,
                lang2,
                e?.Message.Chat.Type, replyMarkupObject: new ReplyMarkupObject(ReplyMarkupEnum.REMOVE),
                lang: e?.Message.From?.LanguageCode, username: e?.Message.From?.Username, parseMode: ParseMode.Html
            );
    }

    public static async Task<bool> BanMessageActions(TelegramBotAbstract? telegramBotClient, MessageEventArgs? e)
    {
        return await NotifyUtil.NotifyOwnersBanAction(telegramBotClient, EventArgsContainer.Get(e),
            e?.Message.LeftChatMember?.Id,
            e?.Message.LeftChatMember?.Username);
    }
}