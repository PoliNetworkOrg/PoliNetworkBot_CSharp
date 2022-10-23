﻿#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using JsonPolimi_Core_nf.Data;
using JsonPolimi_Core_nf.Tipi;
using JsonPolimi_Core_nf.Utils;
using PoliNetworkBot_CSharp.Code.Config;
using PoliNetworkBot_CSharp.Code.Data;
using PoliNetworkBot_CSharp.Code.Data.Constants;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Enums.Action;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.CommandDispatcher;
using PoliNetworkBot_CSharp.Code.Objects.TelegramMedia;
using PoliNetworkBot_CSharp.Code.Utils;
using PoliNetworkBot_CSharp.Code.Utils.Logger;
using Telegram.Bot.Types.Enums;

#endregion

namespace PoliNetworkBot_CSharp.Code.Bots.Moderation;

internal static class CommandDispatcher
{
    /// <summary>
    ///     List of commands actively listened to from the bot.
    ///     triggers are case insensitive!
    ///     in the helpMessage section insert the descriptions of the arguments if required.
    ///     It's delegated to the callee to verify presence and type of the arguments and if not present call
    ///     NotEnoughArgumentsException() inserting the text to be sent back to the user.
    ///     It's delegated to the caller to catch the exception and provide the feedback to the user
    ///     If optionalConditions are specified, they need to be documented in the helpMessage after the @condition tag, in
    ///     order to provide feedback to the user (and try to keep them standard)
    ///     optionalConditions are checked by the caller
    /// </summary>
    /// TODO porting of all commands in the new format
    /// done TODO make a iterative method to check all the commands and trigger the (first) selected
    /// half done TODO make the help method to read all the commands specified here and provide useful information in a nice clean way
    /// TODO verify restrict actions are still working (they probably aren't, we changed the target from IReadOnlyList
    /// <string
    /// ?>
    /// ? target to string[] containing all the targets to ban)
    private static readonly List<Command> Commands = new()
    {
        new Command("start", Start, new List<ChatType> { ChatType.Private }, Permission.USER,
            new L("en", "Initialize bot", "it", "Inizializza il bot"), null, null),
        new Command("force_check_invite_links", ForceCheckInviteLinksAsync, new List<ChatType> { ChatType.Private },
            Permission.CREATOR,
            new L("en", "Regenerates broken links for all groups", "it", "Rigenera tutti i link rotti dei gruppi"), null,
            null),
        new Command("contact", ContactUs, new List<ChatType> { ChatType.Private }, Permission.USER,
            new L("en", "Show PoliNetwork contact's information", "it",
                "Mostra le informazioni per contattare PoliNetwork"), null, null),
        new Command("help", HelpPrivate, new List<ChatType> { ChatType.Private }, Permission.USER,
            new L("en", "This Menu", "it", "Questo menu"), null, null),
        new Command("mute_all", RestrictUser.MuteAllAsync, new List<ChatType> { ChatType.Private },
            Permission.ALLOWED_MUTE_ALL,
            new L("en",
                "Mute users from the network. @args: list of ids. @condition: you need to reply to a message to explain the action",
                "it",
                "Mute un utente dal network. @args: lista di id. @condition: devi rispondere ad un messaggio di motivazione dell'azione"), null,
            e => e.Message.ReplyToMessage != null),
        new Command("unmute_all", RestrictUser.UnMuteAllAsync, new List<ChatType> { ChatType.Private },
            Permission.ALLOWED_MUTE_ALL,
            new L("en",
                "UNMute users from the network. @args: list of ids. @condition: you need to reply to a message to explain the action",
                "it",
                "UNMute un utente dal network. @args: lista di id. @condition: devi rispondere ad un messaggio di motivazione dell'azione"), null,
            e => e.Message.ReplyToMessage != null),
        new Command("ban_all", RestrictUser.BanAllAsync, new List<ChatType> { ChatType.Private },
            Permission.ALLOWED_BAN_ALL,
            new L("en",
                "Ban users from the network. @args: list of ids. @condition: you need to reply to a message to explain the action",
                "it",
                "Banna un utente dal network. @args: lista di id. @condition: devi rispondere ad un messaggio di motivazione dell'azione"), null,
            e => e.Message.ReplyToMessage != null),
        new Command("unban_all", RestrictUser.UnbanAllAsync, new List<ChatType> { ChatType.Private },
            Permission.ALLOWED_BAN_ALL,
            new L("en",
                "UNBan users from the network. @args: list of ids. @condition: you need to reply to a message to explain the action",
                "it",
                "UNBanna un utente dal network. @args: lista di id. @condition: devi rispondere ad un messaggio di motivazione dell'azione"), null,
            e => e.Message.ReplyToMessage != null),
        new Command("ban_delete_all", RestrictUser.BanDeleteAllAsync, new List<ChatType> { ChatType.Private },
            Permission.ALLOWED_BAN_ALL,
            new L("en",
                "Ban users from the network and delete all its messages. @args: list of ids. @condition: you need to reply to a message to explain the action",
                "it",
                "Banna un utente dal network e cancella tutti i suoi messaggi. @args: lista di id. @condition: devi rispondere ad un messaggio di motivazione dell'azione"), null,
            e => e.Message.ReplyToMessage != null),
        new Command("del", RestrictUser.DeleteMessageFromUser,
            new List<ChatType> { ChatType.Group, ChatType.Supergroup, ChatType.Channel }, Permission.ALLOWED_BAN_ALL,
            new L("en", "Delete a message in chat. @condition: you need to reply to the message to delete", "it",
                "Cancella un messaggio in chat. @condition: devi rispondere al messaggio da cancellare"), null,
            e => e.Message.ReplyToMessage != null),
        new Command("ban", RestrictUser.BanUserAsync,
            new List<ChatType> { ChatType.Group, ChatType.Supergroup, ChatType.Channel }, Permission.USER,
            new L("en", "Delete a message in chat. @condition: you need to reply to the message to delete", "it",
                "Cancella un messaggio in chat. @condition: devi rispondere al messaggio da cancellare"), null,
            e => e.Message.ReplyToMessage != null),
        new Command("test_spam", TestSpamAsync, new List<ChatType> { ChatType.Private }, Permission.USER,
            new L("en", "Test a message for spam. @condition: you need to reply to the message to test", "it",
                "Testa un messaggio contro il filtro spam. @condition: devi rispondere al messaggio da testare"), null,
            e => e.Message.ReplyToMessage != null),
        new Command("groups", SendRecommendedGroupsAsync, new List<ChatType> { ChatType.Private }, Permission.USER,
            new L("en", "Get suggested groups for you.", "it", "Ricevi i gruppi consigliati per te."), null, null),
        new Command("search", Groups.SendGroupsByTitle, new List<ChatType> { ChatType.Private }, Permission.USER,
            new L("en", "Search for a group by title.", "it", "Cerca gruppi per titolo."), null, null),
        new Command("search", Groups.SendGroupsByTitle,
            new List<ChatType> { ChatType.Group, ChatType.Supergroup, ChatType.Channel }, Permission.USER,
            new L("en", "Search for a group by title. @condition: you need to reply to a message", "it",
                "Cerca gruppi per titolo. @condition: devi rispondere ad un messaggio"), null,
            e => e.Message.ReplyToMessage != null),
        new Command("reboot", RebootUtil.RebootWithLog, new List<ChatType> { ChatType.Private }, Permission.OWNER,
            new L("en", "Reboot the bot system", "it", "Riavvia il sistema di bot"), null, null),

    };

    public static async Task<bool> CommandDispatcherMethod(TelegramBotAbstract? sender, MessageEventArgs e)
    {
        if (string.IsNullOrEmpty(e.Message.Text))
        {
            return false;
        }

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

        foreach (Command command in Commands)
        {   
            if(sender != null)
                command.TryTrigger(e, sender, cmd, args);
            if(command.hasBeenTriggered())
                return true;
        }

        ///// following to be cancelled!

        switch (cmd)
        {
            /*
        case "/massiveSend":
            {
                if (e.Message.Chat.Type != ChatType.Private)
                {
                    await CommandNotSentInPrivateAsync(sender, e);
                    return;
                }

                try
                {
                    if (GlobalVariables.AllowedBanAll.Contains(e.Message.From?.Username?.ToLower()))
                        _ = MassiveSendAsync(sender, e, cmdLines, e.Message.From.LanguageCode, e.Message.From.Username);
                    else
                        await DefaultCommand(sender, e);
                }
                catch
                {
                    ;
                }

                return;
            }
            */

            /*case "/banAllHistory":
                {
                    // _ = BanUserAsync(sender, e, cmdLines);
                    _ = BanUserHistoryAsync(sender, e, false);
                    return;
                }*/


            case "/sendmessageinchannel":
            {
                var message = e.Message;
                if (Owners.CheckIfOwner(e.Message.From?.Id) &&
                    message.Chat.Type == ChatType.Private)
                {
                    if (cmdLines != null && (e.Message.ReplyToMessage == null || cmdLines.Length != 2))
                        return false;
                    var text = new Language(new Dictionary<string, string?>
                    {
                        { "it", e.Message.ReplyToMessage?.Text ?? e.Message.ReplyToMessage?.Caption }
                    });
                    var c2 = cmdLines?[1];
                    if (cmdLines == null)
                        return false;

                    if (c2 != null)
                        _ = await SendMessage.SendMessageInAGroup(sender, e.Message.From?.LanguageCode,
                            text, e,
                            long.Parse(c2),
                            ChatType.Channel, ParseMode.Html, null, false);

                    return false;
                }

                await DefaultCommand(sender, e);

                return false;
            }

            case "/get_config":
            {
                return !OwnerInPrivate(e)
                    ? await DefaultCommand(sender, e)
                    : await ConfigUtil.GetConfig(e.Message.From?.Id, e.Message.From?.Username, sender,
                        e.Message.From?.LanguageCode,
                        e.Message.Chat.Type);
            }

            case "/getGroups":
            {
                return !OwnerInPrivate(e)
                    ? await DefaultCommand(sender, e)
                    : await GetAllGroups(e.Message.From?.Id, e.Message.From?.Username, sender,
                        e.Message.From?.LanguageCode,
                        e.Message.Chat.Type);
            }

            case "/allowmessage":
            {
                var message = e.Message;
                if (Permissions.CheckPermissions(Permission.HEAD_ADMIN, e.Message.From) &&
                    message.Chat.Type == ChatType.Private)
                {
                    await AllowMessageAsync(e, sender);
                    return false;
                }

                await DefaultCommand(sender, e);

                return false;
            }

            case "/allowmessageowner":
            {
                var message = e.Message;
                if (Owners.CheckIfOwner(e.Message.From?.Id) &&
                    message.Chat.Type == ChatType.Private)
                {
                    await AllowMessageOwnerAsync(e, sender);
                    return false;
                }

                await DefaultCommand(sender, e);

                return false;
            }

            case "/allowedmessages":
            {
                var o = e.Message;
                if (Owners.CheckIfOwner(e.Message.From?.Id) && o.Chat.Type == ChatType.Private)
                {
                    var text = new Language(new Dictionary<string, string?>
                    {
                        { "it", "List of messages: " },
                        { "en", "List of messages: " }
                    });
                    if (sender == null)
                        return false;


                    var message1 = e.Message;

                    await sender.SendTextMessageAsync(e.Message.From?.Id, text,
                        ChatType.Private,
                        e.Message.From?.LanguageCode, ParseMode.Html, null,
                        e.Message.From?.Username,
                        message1.MessageId);
                    var messages = MessagesStore.GetAllMessages(x =>
                        x != null && x.AllowedStatus.GetStatus() == MessageAllowedStatusEnum.ALLOWED);
                    if (messages == null)
                        return false;

                    foreach (var m2 in messages.Select(message => message?.message)
                                 .Where(m2 => m2 != null))
                    {
                        text = new Language(new Dictionary<string, string?>
                        {
                            { "uni", m2 }
                        });
                        await sender.SendTextMessageAsync(e?.Message?.From?.Id, text,
                            ChatType.Private,
                            "uni", ParseMode.Html, null, e?.Message?.From?.Username);
                    }


                    return false;
                }

                await DefaultCommand(sender, e);

                return false;
            }

            case "/unallowmessage":
            {
                var message = e.Message;
                if (Owners.CheckIfOwner(e.Message.From?.Id) &&
                    message.Chat.Type == ChatType.Private)
                {
                    if (e.Message.ReplyToMessage == null || string.IsNullOrEmpty(e.Message.ReplyToMessage.Text))
                    {
                        var text = new Language(new Dictionary<string, string?>
                        {
                            { "en", "You have to reply to a message containing the message" }
                        });
                        if (sender == null)
                            return false;
                        var o = e.Message;
                        await sender.SendTextMessageAsync(e.Message.From?.Id, text,
                            ChatType.Private,
                            e.Message.From?.LanguageCode, ParseMode.Html, null,
                            e.Message.From?.Username,
                            o.MessageId);

                        return false;
                    }

                    MessagesStore.RemoveMessage(e.Message.ReplyToMessage.Text);
                    return false;
                }

                await DefaultCommand(sender, e);

                return false;
            }
            case "/updategroups_dry":
            {
                if (e is { Message: { } })
                    if (Owners.CheckIfOwner(e.Message.From?.Id) &&
                        e.Message.Chat.Type == ChatType.Private)
                    {
                        var text = await UpdateGroups(sender, true, true, false, e);

                        await SendMessage.SendMessageInPrivate(sender, e.Message.From?.Id,
                            e.Message.From?.LanguageCode, e.Message.From?.Username, text.Language,
                            ParseMode.Html, null);
                        return false;
                    }

                await DefaultCommand(sender, e);

                return false;
            }
            case "/updategroups":
            {
                if (e is { Message: { } })
                    if (Owners.CheckIfOwner(e.Message.From?.Id) &&
                        e.Message.Chat.Type == ChatType.Private)
                    {
                        var text = await UpdateGroups(sender, false, true, false, e);

                        await SendMessage.SendMessageInPrivate(sender, e.Message.From?.Id,
                            e.Message.From?.LanguageCode, e.Message.From?.Username, text.Language,
                            ParseMode.Html, null);

                        return false;
                    }

                await DefaultCommand(sender, e);

                return false;
            }
            case "/updategroupsandfixnames":
            {
                if (e is { Message: { } })
                    if (Owners.CheckIfOwner(e.Message.From?.Id) &&
                        e.Message.Chat.Type == ChatType.Private)
                    {
                        var text = await UpdateGroups(sender, false, true, true, e);

                        await SendMessage.SendMessageInPrivate(sender, e.Message.From?.Id,
                            e.Message.From?.LanguageCode, e.Message.From?.Username, text.Language,
                            ParseMode.Html, null);

                        return false;
                    }

                await DefaultCommand(sender, e);

                return false;
            }
            case "/updategroupsandfixnames_dry":
            {
                if (e is { Message: { } })
                    if (Owners.CheckIfOwner(e.Message.From?.Id) &&
                        e.Message.Chat.Type == ChatType.Private)
                    {
                        var text = await UpdateGroups(sender, true, true, true, e);

                        await SendMessage.SendMessageInPrivate(sender, e.Message.From?.Id,
                            e.Message.From?.LanguageCode, e.Message.From?.Username, text.Language,
                            ParseMode.Html, null);

                        return false;
                    }

                await DefaultCommand(sender, e);

                return false;
            }
            case "/backup":
            {
                if (e is { Message: { } })
                    if (Owners.CheckIfOwner(e.Message.From?.Id) &&
                        e.Message.Chat.Type == ChatType.Private)
                    {
                        if (e.Message.From != null)
                            await BackupHandler(e.Message.From.Id, sender, e.Message.From.Username,
                                e.Message.Chat.Type);

                        return false;
                    }

                await DefaultCommand(sender, e);

                return false;
            }
            case "/getrunningtime":
            {
                if (e is { Message: { } })
                    if (Owners.CheckIfOwner(e.Message.From?.Id) &&
                        e.Message.Chat.Type == ChatType.Private)
                    {
                        try
                        {
                            var lang = new Language(new Dictionary<string, string?>
                            {
                                { "", await GetRunningTime() }
                            });
                            await SendMessage.SendMessageInPrivate(sender, e.Message.From?.Id,
                                e.Message.From?.LanguageCode,
                                e.Message.From?.Username, lang, ParseMode.Html,
                                null);
                            return false;
                        }
                        catch (Exception? ex)
                        {
                            _ = NotifyUtil.NotifyOwnerWithLog2(ex, sender, e);
                        }

                        return false;
                    }

                await DefaultCommand(sender, e);

                return false;
            }
            case "/massivesend_polimi":
            {
                if (e is { Message: { } } && sender != null)
                    if (Owners.CheckIfOwner(e.Message.From?.Id) &&
                        e.Message.Chat.Type == ChatType.Private)
                        return await MassiveSendUtil.MassiveGeneralSendAsync(e, sender, false);

                await DefaultCommand(sender, e);

                return false;
            }
            case "/massivesend_polimi_test":
            {
                if (e is { Message: { } } && sender != null)
                    if (Owners.CheckIfOwner(e.Message.From?.Id) &&
                        e.Message.Chat.Type == ChatType.Private)
                        return await MassiveSendUtil.MassiveGeneralSendAsync(e, sender, true);

                await DefaultCommand(sender, e);

                return false;
            }
            case "/subscribe_log":
            {
                if (e is { Message: { } })
                    if (Owners.CheckIfOwner(e.Message.From?.Id) &&
                        e.Message.Chat.Type == ChatType.Private)
                    {
                        await Logger.Subscribe(e.Message.From?.Id, sender, e);

                        return false;
                    }

                await DefaultCommand(sender, e);

                return false;
            }
            case "/unsubscribe_log":
            {
                if (e is { Message: { } })
                    if (Owners.CheckIfOwner(e.Message.From?.Id) &&
                        e.Message.Chat.Type == ChatType.Private)
                    {
                        Logger.Unsubscribe(e.Message.From?.Id);

                        return false;
                    }

                await DefaultCommand(sender, e);

                return false;
            }
            case "/getlog":
            {
                var message = e.Message;
                if (Owners.CheckIfOwner(e.Message.From?.Id) &&
                    message.Chat.Type == ChatType.Private)
                {
                    Logger.GetLog(sender, e);
                    return false;
                }

                await DefaultCommand(sender, e);

                return false;
            }
            case "/getmessagesent":
            {
                if (e is { Message: { } })
                    if (Owners.CheckIfOwner(e.Message.From?.Id)
                        && e.Message.Chat.Type == ChatType.Private && e.Message.ReplyToMessage != null)
                    {
                        await MessagesStore.SendMessageDetailsAsync(sender, e);

                        return false;
                    }

                await DefaultCommand(sender, e);

                return false;
            }
            case "/testtime":
            {
                if (e.Message.Chat.Type != ChatType.Private)
                    return false;

                var time = await TestTime(sender, e);
                Console.WriteLine(time);

                return false;
            }

            case "/time":
            {
                var lang = new Language(new Dictionary<string, string?>
                {
                    { "", DateTimeClass.NowAsStringAmericanFormat() }
                });
                await SendMessage.SendMessageInPrivate(sender, e.Message.From?.Id,
                    e.Message.From?.LanguageCode,
                    e.Message.From?.Username, lang, ParseMode.Html,
                    null);

                return false;
            }

            case "/assoc_write":
            case "/assoc_send":
            {
                _ = await Assoc.Assoc_SendAsync(sender, e);
                return false;
            }

            case "/assoc_publish":
            {
                if (Owners.CheckIfOwner(e.Message.From?.Id))
                    _ = await Assoc.Assoc_Publish(sender, e);
                else
                    _ = await DefaultCommand(sender, e);
                return false;
            }

            case "/assoc_read":
            {
                _ = await Assoc.Assoc_Read(sender, e, false);
                return false;
            }

            case "/assoc_read_all":
            {
                if (Owners.CheckIfOwner(e.Message.From?.Id))
                    _ = await Assoc.Assoc_ReadAll(sender, e);
                else
                    _ = await DefaultCommand(sender, e);
                return false;
            }

            case "/assoc_delete":
            case "/assoc_remove":
            {
                _ = await Assoc.Assoc_Delete(sender, e);
                return false;
            }

            case "/rooms":
            {
                await Rooms.RoomsMainAsync(sender, e);
                return false;
            }

            case "/rules":
            {
                _ = await Rules(sender, e);
                return false;
            }

            case "/qe":
            {
                _ = await QueryBot(true, e, sender);
                return false;
            }

            case "/qs":
            {
                _ = await QueryBot(false, e, sender);
                return false;
            }

            case "/update_links_from_json":
            {
                await InviteLinks.UpdateLinksFromJsonAsync(sender, e);
                return false;
            }

            default:
            {
                await DefaultCommand(sender, e);
                return false;
            }
        }
    }

    private static bool OwnerInPrivate(MessageEventArgs e)
    {
        var message = e.Message;
        return GlobalVariables.Creators != null &&
               (GlobalVariables.Creators.ToList().Any(x => x.Matches(e.Message.From)) ||
                Owners.CheckIfOwner(e.Message.From?.Id)) && message.Chat.Type == ChatType.Private;
    }


    private static async Task AllowMessageOwnerAsync(MessageEventArgs? e, TelegramBotAbstract? sender)
    {
        if (e != null)
        {
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
            }
            else
            {
                MessagesStore.AllowMessageOwner(e.Message.ReplyToMessage.Text);
                MessagesStore.AllowMessageOwner(e.Message.ReplyToMessage.Caption);
                Logger.WriteLine(
                    e.Message.ReplyToMessage.Text ?? e.Message.ReplyToMessage.Caption ??
                    "Error in allowmessage, both caption and text are null");
            }
        }
    }

    private static async Task AllowMessageAsync(MessageEventArgs? e, TelegramBotAbstract? sender)
    {
        await Assoc.AllowMessage(e, sender);
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
        ScriptUtil.DoScript(powershell, "cd ../data/", true);
        ScriptUtil.DoScript(powershell, "git clone https://" + GitHubConfig.GetRepo(), true);
        ScriptUtil.DoScript(powershell, "cd ./polinetworkWebsiteData", true);
        ScriptUtil.DoScript(powershell, "git remote add org https://" + GitHubConfig.GetRemote(), true);
    }


    public static async Task BackupHandler(long sendTo, TelegramBotAbstract? botAbstract, string? username,
        ChatType chatType)
    {
        try
        {
            var jsonDb = BackupUtil.GetDB_AsJson(botAbstract);

            if (string.IsNullOrEmpty(jsonDb)) return;

            var bytes = Encoding.UTF8.GetBytes(jsonDb);
            var stream = new MemoryStream(bytes);

            var text2 = new Language(new Dictionary<string, string?>
            {
                { "it", "Backup:" }
            });

            var peer = new PeerAbstract(sendTo, chatType);

            await SendMessage.SendFileAsync(new TelegramFile(stream, "db.json",
                    null, "application/json"), peer,
                text2, TextAsCaption.BEFORE_FILE,
                botAbstract, username, "it", null, true);
        }
        catch (Exception? ex)
        {
            await NotifyUtil.NotifyOwnerWithLog2(ex, botAbstract, null);
        }
    }

    private static async Task TestSpamAsync(MessageEventArgs? e, TelegramBotAbstract? sender)
    {
        var message = e?.Message.ReplyToMessage;
        if (message == null)
            return;
        if (e?.Message != null)
        {
            var r2 = MessagesStore.StoreAndCheck(e.Message.ReplyToMessage);

            if (r2 is not (SpamType.SPAM_PERMITTED or SpamType.SPAM_LINK))
                r2 = await Blacklist.IsSpam(message.Text, message.Chat.Id, sender, true, e);

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
    }
#pragma warning disable IDE0051 // Rimuovi i membri privati inutilizzati

    /*
     private static async Task<bool> MassiveSendAsync(TelegramBotAbstract sender, MessageEventArgs e,
        string textToSend)
    {
        

        textToSend =        "Buonasera a tutti, vi ricordiamo che lunedì 24 fino al 27 verranno aperti i seggi online per le elezioni, fate sentire la vostra voce mi raccomando. <b>Votate!</b>\nPotete informarvi su modalità di voto e candidati al sito\npolinetworkelezioni.github.io/it" +
                "\n\n\n" +
                "Good evening everyone, we remind you that on Monday 24th to 27th the online polling stations will be open for the elections, please let your voice be heard. <b>Vote!</b>\nYou can find out about voting procedures and candidates in the website\npolinetworkelezioni.github.io/en"

         

        var groups = Database.ExecuteSelect("Select id FROM GroupsTelegram", sender?.DbConfig);

        return await MassiveSendSlaveAsync(sender, e, groups);
    }
    */


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

    private static async Task<long?> QueryBot(bool executeTrueSelectFalse, MessageEventArgs? e,
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
            new TelegramFile(x2, "table.bin", "Query result", "application/octet-stream");

        var text2 = new Language(new Dictionary<string, string?>
        {
            { "en", "Query result" }
        });
        if (e.Message.From == null)
            return -1;

        PeerAbstract peer = new(e.Message.From.Id, e.Message.Chat.Type);
        var v = await sender.SendFileAsync(documentInput, peer, text2, TextAsCaption.AS_CAPTION,
            e.Message.From.Username, e.Message.From.LanguageCode, e.Message.MessageId, false);
        return v ? 1 : 0;
    }

    private static async Task<MessageSentResult?> TestTime(TelegramBotAbstract? sender, MessageEventArgs? e)
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
            await NotifyUtil.NotifyOwnersClassic(new ExceptionNumbered(exception), sender, e, s);

            return null;
        }

        var dateTimeSchedule = tuple1?.Item1;
        if (dateTimeSchedule == null) return null;
        var sentDate2 = dateTimeSchedule.GetDate();

        var dict = new Dictionary<string, string?>
        {
            { "en", DateTimeClass.DateTimeToItalianFormat(sentDate2) }
        };
        var text = new Language(dict);
        return await SendMessage.SendMessageInPrivate(sender, e.Message.From.Id,
            e.Message.From.LanguageCode, e.Message.From.Username,
            text, ParseMode.Html, e.Message.MessageId);
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
            e?.Message.From?.Username, text2, ParseMode.Html, null);
    }

    private static async Task SendRecommendedGroupsAsync(MessageEventArgs? e, TelegramBotAbstract? sender)
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
            e?.Message.From?.Username, text2, ParseMode.Html, null);
    }

    private static async Task<bool> GetAllGroups(long? chatId, string? username, TelegramBotAbstract? sender,
        string? lang, ChatType? chatType)
    {
        var groups = Groups.GetAllGroups(sender);
        Stream? stream = new MemoryStream();
        FileSerialization.SerializeFile(groups, ref stream);

        if (chatType == null)
            return false;

        var peer = new PeerAbstract(chatId, chatType.Value);

        var text2 = new Language(new Dictionary<string, string?>
        {
            { "en", "Here are all groups:" },
            { "it", "Ecco tutti i gruppi:" }
        });
        return await SendMessage.SendFileAsync(new TelegramFile(stream, "groups.bin",
                null, "application/octet-stream"), peer,
            text2, TextAsCaption.BEFORE_FILE,
            sender, username, lang, null, true);
    }


    private static async Task<bool> DefaultCommand(TelegramBotAbstract? sender, MessageEventArgs? e)
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
            null);

        return true;
    }

    private static async Task Help(MessageEventArgs? e, TelegramBotAbstract? sender)
    {
        if (e?.Message != null && e.Message.Chat.Type == ChatType.Private)
            await HelpPrivate(e, sender);
        else
            await CommandNotSentInPrivateAsync(sender, e);
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

    private static async Task HelpPrivate(MessageEventArgs? e, TelegramBotAbstract? sender)
    {
        const string text = "<i>Lista di funzioni</i>:\n" +
                            //"\n📑 Sistema di recensioni dei corsi (per maggiori info /help_review)\n" +
                            //"\n🔖 Link ai materiali nei gruppi (per maggiori info /help_material)\n" +
                            "\n🙋 <a href='https://polinetwork.org/it/faq/index.html'>" +
                            "FAQ (domande frequenti)</a>\n" +
                            "\n🏫 Ricerca aule libere /rooms\n" +
                            //"\n🕶️ Sistema di pubblicazione anonima (per maggiori info /help_anon)\n" +
                            //"\n🎙️ Registrazione delle lezioni (per maggiori info /help_record)\n" +
                            "\n👥 Gruppo consigliati e utili /groups\n" +
                            "\n⚠ Hai già letto le regole del network? /rules\n" +
                            "\n✍ Per contattarci /contact";

        const string textEng = "<i>List of features</i>:\n" +
                               //"\n📑 Review system of courses (for more info /help_review)\n" +
                               //"\n🔖 Link to notes (for more info /help_material)\n" +
                               "\n🙋 <a href='https://polinetwork.org/it/faq/index.html'>" +
                               "FAQ (frequently asked questions)</a>\n" +
                               "\n🏫 Find free rooms /rooms\n" +
                               //"\n🕶️ Anonymous posting system (for more info /help_anon)\n" +
                               //"\n🎙️ Record of lessons (for more info /help_record)\n" +
                               "\n👥 Recommended groups /groups\n" +
                               "\n⚠ Have you already read our network rules? /rules\n" +
                               "\n✍ To contact us /contact";



        var text2 = new Language(new Dictionary<string, string?>
        {
            { "en", textEng + "\nCommands available:\n" + String.Join("\n\n", Commands.Select(x => x.HelpMessage().Select("en"))) },
            { "it", text + "\nCommands available:\n" + String.Join("\n\n", Commands.Select(x => x.HelpMessage().Select("it"))) }
        });
        await SendMessage.SendMessageInPrivate(sender, e?.Message.From?.Id,
            e?.Message.From?.LanguageCode,
            e?.Message.From?.Username, text2, ParseMode.Html, null);
    }

    private static async Task ContactUs(MessageEventArgs? e, TelegramBotAbstract? telegramBotClient)
    {
        await DeleteMessage.DeleteIfMessageIsNotInPrivate(telegramBotClient, e?.Message);
        if (telegramBotClient != null)
        {
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
        }
    }

    private static async Task ForceCheckInviteLinksAsync(MessageEventArgs? e, TelegramBotAbstract? sender)
    {
        long? n = null;
        try
        {
            n = await InviteLinks.FillMissingLinksIntoDB_Async(sender, e);
        }
        catch (Exception? e2)
        {
            await NotifyUtil.NotifyOwnersClassic(new ExceptionNumbered(e2), sender, e);
        }

        if (n == null)
            return;

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
                e.Message.MessageId);
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
        return await NotifyUtil.NotifyOwnersBanAction(telegramBotClient, e, e?.Message.LeftChatMember?.Id,
            e?.Message.LeftChatMember?.Username);
    }
}