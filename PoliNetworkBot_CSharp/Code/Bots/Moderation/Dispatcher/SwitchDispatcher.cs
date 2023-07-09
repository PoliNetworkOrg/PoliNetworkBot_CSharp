using System.Collections.Generic;
using PoliNetworkBot_CSharp.Code.Config;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.CommandDispatcher;
using PoliNetworkBot_CSharp.Code.Utils;
using PoliNetworkBot_CSharp.Code.Utils.Backup;
using PoliNetworkBot_CSharp.Code.Utils.DatabaseUtils;
using PoliNetworkBot_CSharp.Code.Utils.Logger;
using PoliNetworkBot_CSharp.Code.Utils.Restore;
using PoliNetworkBot_CSharp.Code.Utils.Running;
using Telegram.Bot.Types.Enums;

namespace PoliNetworkBot_CSharp.Code.Bots.Moderation.Dispatcher;

public static class SwitchDispatcher
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
    public static readonly List<Command> Commands = new()
    {
        new Command("start", CommandDispatcher.HelpPrivate, new List<ChatType> { ChatType.Private },
            Permission.USER,
            new L("en", "Initialize bot", "it", "Inizializza il bot"), null, null),
        new Command("force_check_invite_links", CommandDispatcher.ForceCheckInviteLinksAsync,
            new List<ChatType> { ChatType.Private },
            Permission.CREATOR,
            new L("en", "Regenerates broken links for all groups", "it", "Rigenera tutti i link rotti dei gruppi"),
            null,
            null),
        new Command("contact", CommandDispatcher.ContactUs, new List<ChatType> { ChatType.Private },
            Permission.USER,
            new L("en", "Show PoliNetwork contact's information", "it",
                "Mostra le informazioni per contattare PoliNetwork"), null, null),
        new Command("help", CommandDispatcher.HelpPrivate, new List<ChatType> { ChatType.Private },
            Permission.USER,
            new L("en", "This Menu @args: (optional) specific command to describe", "it",
                "Questo menu @args: (optional) spiegazione per un comando"), null, null),
        new Command("help_all", CommandDispatcher.HelpExtended, new List<ChatType> { ChatType.Private },
            Permission.USER,
            new L("en", "Extended help menu, with all commands", "it", "Menu Help esteso con tutti i comandi"), null,
            null),
        new Command("mute_all", RestrictUser.MuteAllAsync, new List<ChatType> { ChatType.Private },
            Permission.ALLOWED_MUTE_ALL,
            new L("en",
                "Mute users from the network. @args: &lt;id to ban&gt; &lt;optional - until time&gt;. @condition: you need to reply to a message to explain the action",
                "it",
                "Mute un utente dal network. @args: &lt;id to ban&gt; &lt;optional - until time&gt; @condition: devi rispondere ad un messaggio di motivazione dell'azione"),
            null,
            e => e.Message.ReplyToMessage != null),
        new Command("unmute_all", RestrictUser.UnMuteAllAsync, new List<ChatType> { ChatType.Private },
            Permission.ALLOWED_MUTE_ALL,
            new L("en",
                "UNMute users from the network. @args: &lt;id to ban&gt; &lt;optional - until time&gt;. @condition: you need to reply to a message to explain the action",
                "it",
                "UNMute un utente dal network. @args: &lt;id to ban&gt; &lt;optional - until time&gt; @condition: devi rispondere ad un messaggio di motivazione dell'azione"),
            null,
            e => e.Message.ReplyToMessage != null),
        new Command("ban_all", RestrictUser.BanAllAsync, new List<ChatType> { ChatType.Private },
            Permission.ALLOWED_BAN_ALL,
            new L("en",
                "Ban users from the network. @args: &lt;id to ban&gt; &lt;optional - until time&gt;. @condition: you need to reply to a message to explain the action",
                "it",
                "Banna un utente dal network. @args: &lt;id to ban&gt; &lt;optional - until time&gt; @condition: devi rispondere ad un messaggio di motivazione dell'azione"),
            null,
            e => e.Message.ReplyToMessage != null),
        new Command("unban_all", RestrictUser.UnbanAllAsync, new List<ChatType> { ChatType.Private },
            Permission.ALLOWED_BAN_ALL,
            new L("en",
                "UNBan users from the network. @args: &lt;id to ban&gt; &lt;optional - until time&gt;. @condition: you need to reply to a message to explain the action",
                "it",
                "UNBanna un utente dal network. @args: &lt;id to ban&gt; &lt;optional - until time&gt; @condition: devi rispondere ad un messaggio di motivazione dell'azione"),
            null,
            e => e.Message.ReplyToMessage != null),
        new Command("ban_delete_all", RestrictUser.BanDeleteAllAsync, new List<ChatType> { ChatType.Private },
            Permission.ALLOWED_BAN_ALL,
            new L("en",
                "Ban users from the network and delete all its messages. @args: &lt;id to ban&gt; &lt;optional - until time&gt;. @condition: you need to reply to a message to explain the action",
                "it",
                "Banna un utente dal network e cancella tutti i suoi messaggi. @args: &lt;id to ban&gt; &lt;optional - until time&gt; @condition: devi rispondere ad un messaggio di motivazione dell'azione"),
            null,
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
        new Command("test_spam", CommandDispatcher.TestSpamAsync, new List<ChatType> { ChatType.Private },
            Permission.USER,
            new L("en", "Test a message for spam. @condition: you need to reply to the message to test", "it",
                "Testa un messaggio contro il filtro spam. @condition: devi rispondere al messaggio da testare"), null,
            e => e.Message.ReplyToMessage != null),
        new Command("groups", CommandDispatcher.SendRecommendedGroupsAsync,
            new List<ChatType> { ChatType.Private }, Permission.USER,
            new L("en", "Get suggested groups for you.", "it", "Ricevi i gruppi consigliati per te."), null, null),
        new Command("search", Groups.SendGroupsByTitle, new List<ChatType> { ChatType.Private }, Permission.USER,
            new L("en", "Search for a group by title.", "it",
                "Cerca gruppi per titolo. @condition: you need to reply to a message"), null, null),
        new Command("search", Groups.SendGroupsByTitle,
            new List<ChatType> { ChatType.Group, ChatType.Supergroup, ChatType.Channel }, Permission.USER,
            new L("en", "Search for a group by title. @condition: you need to reply to a message", "it",
                "Cerca gruppi per titolo. @condition: devi rispondere ad un messaggio"), null,
            e => e.Message.ReplyToMessage != null),
        new Command("reboot", RebootUtil.RebootWithLog, new List<ChatType> { ChatType.Private }, Permission.OWNER,
            new L("en", "Reboot the bot system", "it", "Riavvia il sistema di bot"), null, null,
            false), // do not turn this on, use /kill and let kubernetes handle reboots
        new Command("sendmessageinchannel", SendMessage.SendMessageInChannel2, new List<ChatType> { ChatType.Private },
            Permission.OWNER,
            new L("en", "Send message in channel", "it", "Invia messaggio in canale"),
            null, e => e.Message.ReplyToMessage != null),
        new Command("get_config", BotConfig.GetConfig2, new List<ChatType> { ChatType.Private },
            Permission.OWNER, new L("en", "Get bot config"), null, null),
        new Command("get_db_config", BotConfig.GetDbConfig, new List<ChatType> { ChatType.Private },
            Permission.OWNER, new L("en", "Get bot db config"), null, null),
        new Command("getgroups", Groups.GetGroups, new List<ChatType> { ChatType.Private },
            Permission.OWNER, new L("en", "Get bot groups"), null, null),

        new Command("qe", Database.QueryBotExec, new List<ChatType> { ChatType.Private },
            Permission.OWNER, new L("en", "Esegui una query execute"), null, null),
        new Command("qs", Database.QueryBotSelect, new List<ChatType> { ChatType.Private },
            Permission.OWNER, new L("en", "Esegui una query select"), null, null),

        new Command("allow_message", CommandDispatcher.AllowMessageAsync,
            new List<ChatType> { ChatType.Private }, Permission.HEAD_ADMIN,
            new L("en", "allow a message"), null, null),
        new Command("allow_message_owner", CommandDispatcher.AllowMessageOwnerAsync,
            new List<ChatType> { ChatType.Private },
            Permission.OWNER,
            new L("en", "allow a message owner"), null, null),
        new Command("allowed_messages", AllowedMessage.GetAllowedMessages, new List<ChatType> { ChatType.Private },
            Permission.OWNER,
            new L("en", "get allowed messages"), null, null),
        new Command("unallow_message", AllowedMessage.UnAllowMessage, new List<ChatType> { ChatType.Private },
            Permission.OWNER,
            new L("en", "unallow a message"), null, null),

        new Command("backup", BackupUtil.Backup, new List<ChatType> { ChatType.Private }, Permission.OWNER,
            new L("en", "backup"), null, null),

        new Command("updategroups", Groups.UpdateGroups, new List<ChatType> { ChatType.Private }, Permission.OWNER,
            new L("en", "Update Groups. Options:" +
                        "\n -dry Output to console" +
                        "\n -link-check Ask telegram if each link is working" +
                        "\n -fix-names Ask telegram if each db name is consistent with actual name"), null, null),
        new Command("update_links_from_json", InviteLinks.UpdateLinksFromJsonAsync2,
            new List<ChatType> { ChatType.Private }, Permission.OWNER,
            new L("en", "update links from json"), null, null),

        new Command("subscribe_log", Logger.SubscribeCommand, new List<ChatType> { ChatType.Private }, Permission.OWNER,
            new L("en", "subscribe log"), null, null),
        new Command("unsubscribe_log", Logger.UnsubscribeCommand, new List<ChatType> { ChatType.Private },
            Permission.OWNER,
            new L("en", "unsubscribe log"), null, null),
        new Command("getlog", Logger.GetLogCommand, new List<ChatType> { ChatType.Private }, Permission.OWNER,
            new L("en", "get log"), null, null),

        new Command("getrunningtime", TimeUtils.GetRunningTime, new List<ChatType> { ChatType.Private },
            Permission.OWNER,
            new L("en", "get running time"), null, null),
        new Command("testtime", TimeUtils.TestTime, new List<ChatType> { ChatType.Private }, Permission.USER,
            new L("en", "test time"), null, null),
        new Command("time", TimeUtils.GetTime, new List<ChatType> { ChatType.Private }, Permission.USER,
            new L("en", "get time"), null, null),

        new Command("rules", CommandDispatcher.GetRules, new List<ChatType> { ChatType.Private },
            Permission.USER,
            new L("en", "get rules"), null, null),
        new Command("rooms", CommandDispatcher.GetRooms, new List<ChatType> { ChatType.Private },
            Permission.USER,
            new L("en", "get rooms"), null, null),

        new Command("massivesend_polimi", MassiveSendUtil.MassiveGeneralSendAsyncCommand,
            new List<ChatType> { ChatType.Private }, Permission.OWNER,
            new L("en", "massive send polimi"), null, null),
        new Command("massivesend_polimi_test", MassiveSendUtil.MassiveGeneralSendAsyncTestCommand,
            new List<ChatType> { ChatType.Private }, Permission.OWNER,
            new L("en", "massive send polimi test"), null, null),

        new Command("getmessagesent", MessagesStore.GetMessagesSent, new List<ChatType> { ChatType.Private },
            Permission.OWNER,
            new L("en", "get messages sent"), null, e => e.Message.ReplyToMessage != null),

        new Command(new List<string> { "assoc_write", "assoc_send" }, AssocCommands.AssocWrite,
            new List<ChatType> { ChatType.Private }, Permission.USER,
            new L("en", "Insert a message in queue @condition: Reply to the message to send", "it",
                "Inserisci un messaggio associativo in coda @condition: Rispondi al messaggio da mandare"),
            new L("un",
                "- Inviare al bot \\@polinetwork3bot una foto con al di sotto del testo (nello stesso messaggio, come descrizione alla foto)\n " +
                "- Rispondere al messaggio inviato al punto precedente con il messaggio /assoc_write " +
                "(è importante rispondere al messaggio, che significa selezionare il messaggio e poi premere il tasto \"reply\" o \"rispondi\")\n" +
                "- Il bot chiede se lo si vuole \"mettere in coda\" o di scegliere una data\n" +
                "- Rispondere \"scegli la data \" \n" +
                "- Il formato della data è il seguente (non saranno accettati altri formati):\n" +
                "ANNO-MESE-GIORNO ORA:MINUTO\n" +
                "Esempio: 2020-12-31 23:59\n" +
                "Nota bene che c'è un solo spazio fra data e orario, e non ci sono spazi da altre parti. Siate molto precisi con il formato della data/ora"),
            e => e.Message.ReplyToMessage != null),
        new Command(new List<string> { "assoc_write_dry" }, AssocCommands.AssocWriteDry,
            new List<ChatType> { ChatType.Private }, Permission.OWNER,
            new L("en", "Insert a message in queue - DRY RUN @condition: Reply to the message to send - DRY RUN", "it",
                "Inserisci un messaggio associativo in coda @condition: Rispondi al messaggio da mandare"),
            null, e => e.Message.ReplyToMessage != null),
        new Command(new List<string> { "assoc_publish" }, AssocCommands.AssocPublish,
            new List<ChatType> { ChatType.Private }, Permission.OWNER,
            new L("en", "assoc publish"), null, null),
        new Command(new List<string> { "assoc_read" }, AssocCommands.AssocRead,
            new List<ChatType> { ChatType.Private }, Permission.USER,
            new L("en", "assoc read"), null, null),

        new Command(new List<string> { "assoc_read_all" }, AssocCommands.AssocReadAll,
            new List<ChatType> { ChatType.Private }, Permission.OWNER,
            new L("en", "assoc read all"), null, null),

        new Command(new List<string> { "assoc_delete", "assoc_remove" }, AssocCommands.AssocDelete,
            new List<ChatType> { ChatType.Private }, Permission.USER,
            new L("en", "assoc delete"), null, null),

        new Command("massive_send", MassiveSendUtil.MassiveSend,
            new List<ChatType> { ChatType.Private }, Permission.OWNER,
            new L("en", "massive send"), null,
            null, false),

        new Command("ban_all_history", CommandDispatcher.BanHistory,
            new List<ChatType> { ChatType.Private }, Permission.OWNER,
            new L("en", "ban all history"), null,
            null, false),

        new Command("send_message", CommandDispatcher.SendMessageInGroup,
            new List<ChatType> { ChatType.Private }, Permission.OWNER,
            new L("en",
                "Send message in a single group using the bot. @condition: Message to be sent. @args: Group ID"), null,
            e => e.Message.ReplyToMessage != null, false),


        new Command("restore_db", RestoreDbUtil.RestoreDbFromTelegram,
            new List<ChatType> { ChatType.Private }, Permission.OWNER,
            new L("en", "Restore db"), null,
            e => e.Message.ReplyToMessage != null),

        new Command("restore_db_ddl", RestoreDbUtil.RestoreDb_Ddl_FromTelegram,
            new List<ChatType> { ChatType.Private }, Permission.OWNER,
            new L("en", "Restore db ddl"), null,
            e => e.Message.ReplyToMessage != null),

        new Command("restore_db_full", RestoreDbUtil.RestoreDb_Full_FromTelegram,
            new List<ChatType> { ChatType.Private }, Permission.OWNER,
            new L("en", "Restore db full"), null,
            e => e.Message.ReplyToMessage != null),

        new Command("kill_yourself", RunningUtil.KillYourself,
            new List<ChatType> { ChatType.Private }, Permission.OWNER,
            new L("en", "Kill bot"), null,
            null)
    };
}