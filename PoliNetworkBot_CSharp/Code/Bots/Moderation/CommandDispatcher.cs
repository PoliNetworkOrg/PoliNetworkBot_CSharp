﻿#region

using JsonPolimi_Core_nf.Data;
using JsonPolimi_Core_nf.Tipi;
using JsonPolimi_Core_nf.Utils;
using PoliNetworkBot_CSharp.Code.Bots.Anon;
using PoliNetworkBot_CSharp.Code.Config;
using PoliNetworkBot_CSharp.Code.Data;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.TelegramMedia;
using PoliNetworkBot_CSharp.Code.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TeleSharp.TL;
using File = System.IO.File;

#endregion

namespace PoliNetworkBot_CSharp.Code.Bots.Moderation
{
    internal static class CommandDispatcher
    {
        public static async Task CommandDispatcherMethod(TelegramBotAbstract sender, MessageEventArgs e)
        {
            var cmdLines = e.Message.Text.Split(' ');
            var cmd = cmdLines[0].Trim();
            if (cmd.Contains("@"))
            {
                var cmd2 = cmd.Split("@");
                var botUsername = await sender.GetBotUsernameAsync();
                if (cmd2[1].ToLower() != botUsername.ToLower()) return;
            }

            switch (cmd)
            {
                case "/start":
                    {
                        await Start(sender, e);
                        return;
                    }

                case "/force_check_invite_links":
                    {
                        if (GlobalVariables.Creators.Contains(e.Message?.Chat?.Username?.ToLower()))
                            _ = ForceCheckInviteLinksAsync(sender, e);
                        else
                            await DefaultCommand(sender, e);
                        return;
                    }

                case "/contact":
                    {
                        await ContactUs(sender, e);
                        return;
                    }

                case "/help":
                    {
                        await Help(sender, e);
                        return;
                    }

                case "/muteAll":
                    {
                        if (e.Message.Chat.Type != ChatType.Private)
                        {
                            await CommandNotSentInPrivateAsync(sender, e);
                            return;
                        }

                        if (e.Message.ReplyToMessage == null)
                        {
                            await CommandNeedsAReplyToMessage(sender, e);
                            return;
                        }

                        if (GlobalVariables.AllowedMuteAll.Contains(e.Message.From?.Username?.ToLower()))
                            _ = MuteAllAsync(sender, e, cmdLines, e.Message.From?.LanguageCode, e.Message.From?.Username,
                                false);
                        else
                            await DefaultCommand(sender, e);
                        return;
                    }
                case "/unmuteAll":
                    {
                        if (e.Message.Chat.Type != ChatType.Private)
                        {
                            await CommandNotSentInPrivateAsync(sender, e);
                            return;
                        }

                        if (e.Message.ReplyToMessage == null)
                        {
                            await CommandNeedsAReplyToMessage(sender, e);
                            return;
                        }

                        if (GlobalVariables.AllowedMuteAll.Contains(e.Message.From?.Username?.ToLower()))
                            _ = UnMuteAllAsync(sender, e, cmdLines, e.Message.From?.LanguageCode, e.Message.From?.Username,
                                false);
                        else
                            await DefaultCommand(sender, e);
                        return;
                    }

                case "/banAll":
                    {
                        if (e.Message.Chat.Type != ChatType.Private)
                        {
                            await CommandNotSentInPrivateAsync(sender, e);
                            return;
                        }

                        if (e.Message.ReplyToMessage == null)
                        {
                            await CommandNeedsAReplyToMessage(sender, e);
                            return;
                        }

                        if (GlobalVariables.AllowedBanAll.Contains(e.Message.From?.Username?.ToLower()))
                            _ = BanAllAsync(sender, e, cmdLines, e.Message.From?.LanguageCode, e.Message.From?.Username,
                                false);
                        else
                            await DefaultCommand(sender, e);
                        return;
                    }

                case "/banDeleteAll":
                    {
                        if (e.Message.Chat.Type != ChatType.Private)
                        {
                            await CommandNotSentInPrivateAsync(sender, e);
                            return;
                        }

                        if (e.Message.ReplyToMessage == null)
                        {
                            await CommandNeedsAReplyToMessage(sender, e);
                            return;
                        }

                        if (GlobalVariables.AllowedBanAll.Contains(e.Message.From?.Username?.ToLower()))
                            _ = BanAllAsync(sender, e, cmdLines, e.Message.From?.LanguageCode, e.Message.From?.Username,
                                true);
                        else
                            await DefaultCommand(sender, e);
                        return;
                    }

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

                case "/ban":
                    {
                        _ = BanUserAsync(sender, e, cmdLines, false);
                        return;
                    }
                /*case "/banAllHistory":
                    {
                        // _ = BanUserAsync(sender, e, cmdLines);
                        _ = BanUserHistoryAsync(sender, e, false);
                        return;
                    }*/

                case "/unbanAll":
                    {
                        if (e.Message.Chat.Type != ChatType.Private)
                        {
                            await CommandNotSentInPrivateAsync(sender, e);
                            return;
                        }

                        if (e.Message.ReplyToMessage == null)
                        {
                            await CommandNeedsAReplyToMessage(sender, e);
                            return;
                        }

                        if (GlobalVariables.AllowedBanAll.Contains(e.Message.From?.Username?.ToLower()))
                            _ = UnbanAllAsync(sender, e, cmdLines, e.Message.From.LanguageCode, e.Message.From.Username,
                                false);
                        else
                            await DefaultCommand(sender, e);
                        return;
                    }

                case "/test_spam":
                    {
                        if (e.Message == null)
                            return;
                        if (e.Message.ReplyToMessage == null)
                            return;

                        await TestSpamAsync(e.Message.ReplyToMessage, sender, e);
                        return;
                    }

                case "/groups":
                    {
                        await SendRecommendedGroupsAsync(sender, e);
                        return;
                    }

                case "/search":
                    {
                        var query = "";
                        for (var i = 1; i < cmdLines.Length; i++)
                        {
                            query += cmdLines[i] + " ";
                        }

                        if (!string.IsNullOrEmpty(query))
                            query = query[0..^1];

                        _ = SendGroupsByTitle(query, sender, e, 6);

                        return;
                    }

                case "/reboot":
                    {
                        if (Owners.CheckIfOwner(e.Message.From.Id)
                            && e.Message.Chat.Type == ChatType.Private)
                        {
                            Reboot();
                            return;
                        }

                        await DefaultCommand(sender, e);

                        return;
                    }

                case "/getGroups":
                    {
                        if ((GlobalVariables.Creators.Contains(e.Message.From.Username) ||
                             Owners.CheckIfOwner(e.Message.From.Id))
                            && e.Message.Chat.Type == ChatType.Private)
                        {
                            string username = null;
                            if (!string.IsNullOrEmpty(e.Message.From.Username))
                                username = e.Message.From.Username;

                            _ = GetAllGroups(e.Message.From.Id, username, sender, e.Message.From.LanguageCode);
                            return;
                        }

                        await DefaultCommand(sender, e);

                        return;
                    }

                case "/allowmessage":
                    {
                        if (Owners.CheckIfOwner(e.Message.From.Id)
                            && e.Message.Chat.Type == ChatType.Private)
                        {
                            if (e.Message.ReplyToMessage == null || string.IsNullOrEmpty(e.Message.ReplyToMessage.Text))
                            {
                                var text = new Language(new Dictionary<string, string>
                            {
                                {"en", "You have to reply to a message containing the message"},
                                {"it", "You have to reply to a message containing the message"}
                            });
                                await sender.SendTextMessageAsync(e.Message.From.Id, text, ChatType.Private,
                                    e.Message.From.LanguageCode, ParseMode.Html, null, e.Message.From.Username,
                                    e.Message.MessageId);
                                return;
                            }

                            AllowedMessages.AddMessage(e.Message.ReplyToMessage.Text);
                            return;
                        }

                        await DefaultCommand(sender, e);

                        return;
                    }

                case "/allowedmessages":
                    {
                        if (Owners.CheckIfOwner(e.Message.From.Id)
                            && e.Message.Chat.Type == ChatType.Private)
                        {
                            var text = new Language(new Dictionary<string, string>
                        {
                            {"it", "List of messages: "},
                            {"en", "List of messages: "}
                        });
                            await sender.SendTextMessageAsync(e.Message.From.Id, text, ChatType.Private,
                                e.Message.From.LanguageCode, ParseMode.Html, null, e.Message.From.Username,
                                e.Message.MessageId);
                            List<string> messages = AllowedMessages.GetAllMessages();
                            foreach (var message in messages)
                            {
                                text = new Language(new Dictionary<string, string>
                            {
                                {"uni", message},
                            });
                                await sender.SendTextMessageAsync(e.Message.From.Id, text, ChatType.Private,
                                    "uni", ParseMode.Html, null, e.Message.From.Username);
                            }

                            return;
                        }

                        await DefaultCommand(sender, e);

                        return;
                    }

                case "/unallowmessage":
                    {
                        if (Owners.CheckIfOwner(e.Message.From.Id)
                            && e.Message.Chat.Type == ChatType.Private)
                        {
                            if (e.Message.ReplyToMessage == null || string.IsNullOrEmpty(e.Message.ReplyToMessage.Text))
                            {
                                var text = new Language(new Dictionary<string, string>
                            {
                                {"en", "You have to reply to a message containing the message"}
                            });
                                await sender.SendTextMessageAsync(e.Message.From.Id, text, ChatType.Private,
                                    e.Message.From.LanguageCode, ParseMode.Html, null, e.Message.From.Username,
                                    e.Message.MessageId);
                                return;
                            }

                            AllowedMessages.RemoveMessage(e.Message.ReplyToMessage.Text);
                            return;
                        }

                        await DefaultCommand(sender, e);

                        return;
                    }
                case "/updategroups_dry":
                    {
                        if (Owners.CheckIfOwner(e.Message.From.Id)
                            && e.Message.Chat.Type == ChatType.Private)
                        {
                            var text = await UpdateGroups(sender, true, true, false);

                            await SendMessage.SendMessageInPrivate(sender, e.Message.From.Id,
                                e.Message.From.LanguageCode, e.Message.From.Username, new Language(text),
                                ParseMode.Html, null);

                            return;
                        }

                        await DefaultCommand(sender, e);

                        return;
                    }
                case "/updategroups":
                    {
                        if (Owners.CheckIfOwner(e.Message.From.Id)
                            && e.Message.Chat.Type == ChatType.Private)
                        {
                            var text = await UpdateGroups(sender, false, true, false);

                            await SendMessage.SendMessageInPrivate(sender, e.Message.From.Id,
                                e.Message.From.LanguageCode, e.Message.From.Username, new Language(text),
                                ParseMode.Html, null);

                            return;
                        }

                        await DefaultCommand(sender, e);

                        return;
                    }
                case "/updategroupsandfixnames":
                    {
                        if (Owners.CheckIfOwner(e.Message.From.Id)
                            && e.Message.Chat.Type == ChatType.Private)
                        {
                            var text = await UpdateGroups(sender, false, true, true);

                            await SendMessage.SendMessageInPrivate(sender, e.Message.From.Id,
                                e.Message.From.LanguageCode, e.Message.From.Username, new Language(text),
                                ParseMode.Html, null);

                            return;
                        }

                        await DefaultCommand(sender, e);

                        return;
                    }
                case "/updategroupsandfixnames_dry":
                    {
                        if (Owners.CheckIfOwner(e.Message.From.Id)
                            && e.Message.Chat.Type == ChatType.Private)
                        {
                            var text = await UpdateGroups(sender, true, true, true);

                            await SendMessage.SendMessageInPrivate(sender, e.Message.From.Id,
                                e.Message.From.LanguageCode, e.Message.From.Username, new Language(text),
                                ParseMode.Html, null);

                            return;
                        }

                        await DefaultCommand(sender, e);

                        return;
                    }
                case "/backup":
                    {
                        if (Owners.CheckIfOwner(e.Message.From.Id)
                            && e.Message.Chat.Type == ChatType.Private)
                        {
                            await BackupHandler(e.Message.From.Id, sender, e.Message.From.Username);

                            return;
                        }

                        await DefaultCommand(sender, e);

                        return;
                    }
                case "/getrunningtime":
                    {
                        if (Owners.CheckIfOwner(e.Message.From.Id)
                            && e.Message.Chat.Type == ChatType.Private)
                        {
                            try
                            {
                                var lang = new Language(new Dictionary<string, string>
                            {
                                {"", await GetRunnigTime()}
                            });
                                await SendMessage.SendMessageInPrivate(sender, e.Message.From.Id,
                                    langCode: e.Message.From.LanguageCode,
                                    usernameToSendTo: e.Message.From.Username, text: lang, parseMode: ParseMode.Html,
                                    messageIdToReplyTo: null);
                                return;
                            }
                            catch (Exception ex)
                            {
                                _ = NotifyUtil.NotifyOwners(ex, sender);
                            }

                            return;
                        }

                        await DefaultCommand(sender, e);

                        return;
                    }
                case "/subscribe_log":
                    {
                        if (Owners.CheckIfOwner(e.Message.From.Id)
                            && e.Message.Chat.Type == ChatType.Private)
                        {
                            await Logger.Subscribe(e.Message.From.Id, sender);

                            return;
                        }

                        await DefaultCommand(sender, e);

                        return;
                    }
                case "/unsubscribe_log":
                    {
                        if (Owners.CheckIfOwner(e.Message.From.Id)
                            && e.Message.Chat.Type == ChatType.Private)
                        {
                            Logger.Unsubscribe(e.Message.From.Id);

                            return;
                        }

                        await DefaultCommand(sender, e);

                        return;
                    }
                case "/getlog":
                    {
                        if (Owners.CheckIfOwner(e.Message.From.Id)
                            && e.Message.Chat.Type == ChatType.Private)
                        {
                            Logger.PrintLog(sender,  new List<long>() { e.Message.From.Id, NotifyUtil.group_exception });


                            return;
                        }

                        await DefaultCommand(sender, e);

                        return;
                    }
                case "/testtime":
                    {
                        if (e.Message.Chat.Type == ChatType.Private) await TestTime(sender, e);

                        return;
                    }

                case "/time":
                    {
                        var lang = new Language(new Dictionary<string, string>
                    {
                        {"", DateTimeClass.NowAsStringAmericanFormat()}
                    });
                        await SendMessage.SendMessageInPrivate(sender, e.Message.From.Id,
                            langCode: e.Message.From.LanguageCode,
                            usernameToSendTo: e.Message.From.Username, text: lang, parseMode: ParseMode.Html,
                            messageIdToReplyTo: null);
                        return;
                    }

                case "/assoc_write":
                case "/assoc_send":
                    {
                        _ = await Assoc.Assoc_SendAsync(sender, e);
                        return;
                    }

                case "/assoc_publish":
                    {
                        if (Owners.CheckIfOwner(e.Message.From.Id))
                            _ = await Assoc.Assoc_Publish(sender, e);
                        else
                            _ = await DefaultCommand(sender, e);
                        return;
                    }

                case "/assoc_read":
                    {
                        _ = await Assoc.Assoc_Read(sender, e, false);
                        return;
                    }

                case "/assoc_read_all":
                    {
                        if (Owners.CheckIfOwner(e.Message.From.Id))
                            _ = await Assoc.Assoc_ReadAll(sender, e);
                        else
                            _ = await DefaultCommand(sender, e);
                        return;
                    }

                case "/assoc_delete":
                case "/assoc_remove":
                    {
                        _ = await Assoc.Assoc_Delete(sender, e);
                        return;
                    }

                case "/rooms":
                    {
                        await Rooms.RoomsMainAsync(sender, e);
                        return;
                    }

                case "/rules":
                    {
                        _ = await Rules(sender, e);
                        return;
                    }

                case "/qe":
                    {
                        _ = await QueryBot(true, e, sender);
                        return;
                    }

                case "/qs":
                    {
                        _ = await QueryBot(false, e, sender);
                        return;
                    }

                case "/update_links_from_json":
                    {
                        await InviteLinks.UpdateLinksFromJsonAsync(sender, e);
                        return;
                    }

                default:
                    {
                        await DefaultCommand(sender, e);
                        return;
                    }
            }
        }

        public static async Task<string> GetRunnigTime()
        {
            using var powershell = PowerShell.Create();
            const string path = "../../../build-date.txt";
            return await File.ReadAllTextAsync(path);
        }

        private static async Task CommandNeedsAReplyToMessage(TelegramBotAbstract sender, MessageEventArgs e)
        {
            var lang = new Language(new Dictionary<string, string>
            {
                {"it", "E' necessario rispondere ad un messaggio per usare questo comando"},
                {"en", "This command only works with a reply to message"}
            });
            await SendMessage.SendMessageInPrivateOrAGroup(sender,
                lang, e.Message?.From?.LanguageCode, e.Message?.From?.Username, e.Message?.From?.Id,
                e.Message?.From?.FirstName, e.Message?.From?.LastName, e.Message.Chat.Id, e.Message.Chat.Type);
        }

        private static void Reboot()
        {
            using var powershell = PowerShell.Create();
            if (DoScript(powershell, "screen -ls", true).Aggregate("", (current, a) => current + a).Contains("rebooter"))
            {
                return;
            }
            DoScript(powershell, "screen -d -m -S rebooter ../../../rebooter.sh", true);
        }

        private static async Task<object> SendGroupsByTitle(string query, TelegramBotAbstract sender,
            MessageEventArgs e, int limit)
        {
            try
            {
                if (string.IsNullOrEmpty(query))
                    return null;

                var groups = Groups.GetGroupsByTitle(query, limit);

                var indexTitle = groups.Columns.IndexOf("title");
                var indexLink = groups.Columns.IndexOf("link");
                var buttons = new List<InlineKeyboardButton>();
                foreach (DataRow row in groups.Rows)
                {
                    if (string.IsNullOrEmpty(row?[indexLink]?.ToString()) ||
                        string.IsNullOrEmpty(row?[indexTitle]?.ToString()))
                        continue;

                    var urlButton = new InlineKeyboardButton(row[indexTitle].ToString() ?? "Error!")
                    {
                        Url = row[indexLink].ToString()
                    };

                    buttons.Add(urlButton);
                    //groupsMessage += row[indexTitle] + " [->] " + row[indexLink];
                    //groupsMessage += "\n";
                }

                var buttonsMatrix = (buttons != null && buttons.Count > 0) ? Utils.KeyboardMarkup.ArrayToMatrixString(buttons) : null;

                var text2 = GetTextSearchResult(limit, buttonsMatrix);

                var inline = (buttonsMatrix == null) ? null : new InlineKeyboardMarkup(buttonsMatrix);

                return e.Message.Chat.Type switch
                {
                    ChatType.Sender or ChatType.Private => await SendMessage.SendMessageInPrivate(sender,
                                                                                    e.Message.From.Id, e.Message?.From?.LanguageCode,
                                                                                    "", text2, ParseMode.Html, e.Message?.ReplyToMessage?.MessageId, inline),
                    ChatType.Group or ChatType.Channel or ChatType.Supergroup => await SendMessage.SendMessageInAGroup(sender,
                                                                                    e.Message?.From?.LanguageCode, text2,
                                                                                    e.Message.Chat.Id, e.Message.Chat.Type,
                                                                                    ParseMode.Html, e.Message?.ReplyToMessage?.MessageId, true, 0, inline),
                    _ => throw new ArgumentOutOfRangeException(),
                };
            }
            catch (Exception exception)
            {
                _ = NotifyUtil.NotifyOwners(exception, sender);
                return null;
            }
        }

        private static Language GetTextSearchResult(int limit, List<List<InlineKeyboardButton>> buttonsMatrix)
        {
            return buttonsMatrix switch
            {
                null => new Language(new Dictionary<string, string>()
                    {
                        {"en", "<b>No results</b>."},
                        {"it", "<b>Nessun risultato</b>."}
                    }
                ),
                _ => limit switch
                {
                    <= 0 => new Language(new Dictionary<string, string>()
                        {
                            {"en", "<b>Here are the groups </b>:"},
                            {"it", "<b>Ecco i gruppi</b>:"}
                        }
                    ),
                    _ => new Language(new Dictionary<string, string>
                        {
                            {"en", "<b>Here are the groups </b> (max "+limit+"):"},
                            {"it", "<b>Ecco i gruppi</b> (max "+limit+"):"}
                        }
                    ),
                },
            };
        }

        public static async Task<Dictionary<string, string>> UpdateGroups(TelegramBotAbstract sender, bool dry, bool debug,
            bool updateDb)
        {
            Logger.WriteLine("UpdateGroups started (dry: " + dry + ", debug: " + debug + ", updateDB: " + updateDb + ")", LogSeverityLevel.ALERT);

            if (updateDb)
            {
                await Groups.FixAllGroupsName(sender);
            }

            var groups = Groups.GetAllGroups();

            Variabili.L = new ListaGruppo();

            Variabili.L.HandleSerializedObject(groups);

            CheckSeILinkVanno2(5, true, 10);

            var json =
                JsonBuilder.GetJson(new CheckGruppo(CheckGruppo.E.RICERCA_SITO_V3),
                    false);

            if (!Directory.Exists(GitHubConfig.GetPath()))
            {
                Directory.CreateDirectory("./data/");
                InitGithubRepo();
            }

            var path = GitHubConfig.GetPath() + "groupsGenerated.json";
            await File.WriteAllTextAsync(path, json, Encoding.UTF8);
            if (dry)
            {
                Logger.WriteLine(await File.ReadAllTextAsync(path));
                return new Dictionary<string, string>()
                {
                    {"it", "Dry run completata"},
                    {"en", "Dry run completed"}
                };
            }

            using var powershell = PowerShell.Create();
            var cd = GitHubConfig.GetPath();
            DoScript(powershell, "cd " + cd, debug);
            DoScript(powershell, "git fetch org", debug);
            DoScript(powershell, "git pull --force", debug);
            DoScript(powershell, "git add . --ignore-errors", debug);

            var commit = @"git commit -m ""[Automatic Commit] Updated Group List""" +
                         @" --author=""" + GitHubConfig.GetUser() + "<" + GitHubConfig.GetEmail() +
                         @">""";
            DoScript(powershell, commit, debug);

            var push = @"git push https://" + GitHubConfig.GetUser() + ":" +
                       GitHubConfig.GetPassword() + "@" +
                       GitHubConfig.GetRepo() + @" --all -f";
            DoScript(powershell, push, debug);

            var hub_pr =
                @"hub pull-request -m ""[AutoCommit] Groups Update"" -b PoliNetworkOrg:main -h PoliNetworkDev:main -l bot -f";

            var result = DoScript(powershell, hub_pr, debug);

            powershell.Stop();

            var toBeSent = result.Aggregate("", (current, s) => current + (s + "\n"));

            var text = result.Count > 0
                ? new Dictionary<string, string>()
                {
                    {"it", "Done \n" + toBeSent},
                    {"en", "Done \n" + toBeSent}
                }
                : new Dictionary<string, string>()
                {
                    {"it", "Error in execution"},
                    {"en", "Error in execution"},
                };

            _ = NotifyUtil.NotifyOwners("UpdateGroup result: \n" + (string.IsNullOrEmpty(toBeSent) ? "No PR created" : toBeSent), sender);

            return text;
        }

        public static void CheckSeILinkVanno2(int volteCheCiRiprova, bool laPrimaVoltaControllaDaCapo, int waitOgniVoltaCheCiRiprova)
        {
            ParametriFunzione parametriFunzione = new();
            parametriFunzione.AddParam(volteCheCiRiprova, "volteCheCiRiprova");
            parametriFunzione.AddParam(laPrimaVoltaControllaDaCapo, "laPrimaVoltaControllaDaCapo");
            parametriFunzione.AddParam(waitOgniVoltaCheCiRiprova, "waitOgniVoltaCheCiRiprova");
            RunEventoLogged(Variabili.L.CheckSeILinkVanno, parametriFunzione);
        }

        private static void RunEventoLogged(Func<ParametriFunzione, EventoConLog> func_event, ParametriFunzione parametriFunzione)
        {
            var eventoLog = func_event.Invoke(parametriFunzione);
            eventoLog.RunAction();
            Logger.Log(eventoLog);
        }

        private static void InitGithubRepo()
        {
            using var powershell = PowerShell.Create();
            DoScript(powershell, "cd ./data/", true);
            DoScript(powershell, "git clone https://" + GitHubConfig.GetRepo(), true);
            DoScript(powershell, "cd ./polinetworkWebsiteData", true);
            DoScript(powershell, "git remote add org https://" + GitHubConfig.GetRemote(), true);
        }

        public static List<string> DoScript(PowerShell powershell, string script, bool debug)
        {
            powershell.AddScript(script);
            Logger.WriteLine("Executing command: " + script);
            var results = powershell.Invoke().ToList();
            List<String> listString = new();
            if (debug)
                foreach (var t in results)
                {
                    Logger.WriteLine(t.ToString());
                    listString.Add(t.ToString());
                }

            powershell.Commands.Clear();
            return listString;
        }

        public static async Task BackupHandler(long sendTo, TelegramBotAbstract botAbstract, string username)
        {
            try
            {
                var db = await File.ReadAllBytesAsync("./data/db.db");

                var stream = new MemoryStream(db);

                var text2 = new Language(new Dictionary<string, string>
                {
                    {"it", "Backup:"}
                });

                TLAbsInputPeer peer2 = new TLInputPeerUser { UserId = (int)sendTo };
                var peer = new Tuple<TLAbsInputPeer, long>(peer2, sendTo);

                await SendMessage.SendFileAsync(new TelegramFile(stream, "db.db",
                        null, "application/octet-stream"), peer,
                    text2, TextAsCaption.BEFORE_FILE,
                    botAbstract, username, "it", null, true);
            }
            catch (Exception ex)
            {
                await NotifyUtil.NotifyOwners(ex, botAbstract);
            }
        }

        private static async Task TestSpamAsync(Message message, TelegramBotAbstract sender, MessageEventArgs e)
        {
            var r = Blacklist.IsSpam(message.Text, message.Chat.Id);
            var r2 = r.ToString();

            var dict = new Dictionary<string, string>
            {
                {"en", r2}
            };
            var text = new Language(dict);
            try
            {
                await sender.SendTextMessageAsync(e.Message.From.Id, text, ChatType.Private, "en", ParseMode.Html,
                    null, null);
            }
            catch
            {
                ;
            }
        }

#pragma warning disable IDE0051 // Rimuovi i membri privati inutilizzati

        private static async Task<object> MassiveSendAsync(TelegramBotAbstract sender, MessageEventArgs e,
#pragma warning restore IDE0051 // Rimuovi i membri privati inutilizzati
            string textToSend)
        {
            /*

            textToSend =        "Buonasera a tutti, vi ricordiamo che lunedì 24 fino al 27 verranno aperti i seggi online per le elezioni, fate sentire la vostra voce mi raccomando. <b>Votate!</b>\nPotete informarvi su modalità di voto e candidati al sito\npolinetworkelezioni.github.io/it" +
                    "\n\n\n" +
                    "Good evening everyone, we remind you that on Monday 24th to 27th the online polling stations will be open for the elections, please let your voice be heard. <b>Vote!</b>\nYou can find out about voting procedures and candidates in the website\npolinetworkelezioni.github.io/en"

             */

            var groups = SqLite.ExecuteSelect("Select id From Groups");

            if (groups == null || groups.Rows == null || groups.Rows.Count == 0)
            {
                var dict = new Dictionary<string, string> { { "en", "No groups!" } };
                await sender.SendTextMessageAsync(e.Message.From.Id, new Language(dict), ChatType.Private,
                    e.Message.From.LanguageCode, ParseMode.Html, null, e.Message.From.Username, e.Message.MessageId);
            }

            var counter = 0;

            var dict2 = new Dictionary<string, string>
            {
                {
                    "en",
                    textToSend
                }
            };

            try
            {
                foreach (DataRow element in groups.Rows)
                {
                    try
                    {
                        var groupId = Convert.ToInt64(element.ItemArray[0]);

                        try
                        {
                            await SendMessage.SendMessageInAGroup(sender, "en", new Language(dict2), groupId,
                                ChatType.Supergroup, ParseMode.Html, null, default);
                            counter++;
                        }
                        catch
                        {
                            try
                            {
                                await SendMessage.SendMessageInAGroup(sender, "en", new Language(dict2), groupId,
                                    ChatType.Group, ParseMode.Html, null, default);
                                counter++;
                            }
                            catch
                            {
                                ;
                            }
                        }
                    }
                    catch
                    {
                        ;
                    }

                    await Task.Delay(500);
                }
            }
            catch
            {
                ;
            }

            var text = new Language(new Dictionary<string, string>
            {
                {"en", "Sent in  " + counter + " groups"}
            });

            await Task.Delay(500);

            await sender.SendTextMessageAsync(e.Message.From.Id, text, ChatType.Private, e.Message.From.LanguageCode,
                ParseMode.Html, null, e.Message.From.Username, e.Message.MessageId);
            return true;
        }

#pragma warning disable CS1998 // Il metodo asincrono non contiene operatori 'await', pertanto verrà eseguito in modo sincrono
#pragma warning disable IDE0051 // Rimuovi i membri privati inutilizzati

        private static async Task<object> BanUserHistoryAsync(TelegramBotAbstract sender, long idGroup)
#pragma warning restore IDE0051 // Rimuovi i membri privati inutilizzati
#pragma warning restore CS1998 // Il metodo asincrono non contiene operatori 'await', pertanto verrà eseguito in modo sincrono
        {
            var queryForBannedUsers =
                "SELECT * from Banned as B1 WHERE when_banned >= (SELECT MAX(B2.when_banned) from Banned as B2 where B1.target == B2.target) and banned_true_unbanned_false == 83";
            var bannedUsers = SqLite.ExecuteSelect(queryForBannedUsers);
            List<long> bannedUsersIdArray = new();
            var bannedUsersId = bannedUsers.Rows[bannedUsers.Columns.IndexOf("target")].ItemArray;
            foreach (var user in bannedUsersId)
            {
                bannedUsersIdArray.Add(Int64.Parse(user.ToString()));
            }

            return true;
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
                var text3 = new Language(new Dictionary<string, string>
                {
                    {"en", "There are no users to ban!"}
                });
                await sender.SendTextMessageAsync(e.Message.From.Id, text3, ChatType.Private,
                    e.Message.From.LanguageCode, ParseMode.Html, null, e.Message.From.Username, e.Message.MessageId);
                return false;
            }

            var groups = SqLite.ExecuteSelect("Select id From Groups");
            /*
            if(e.Message.Text.Length !=10)
            {
                Language text2 = new Language(new Dictionary<string, string>() {
                    {"en", "Group not found (1)!"}
                });
                await sender.SendTextMessageAsync(e.Message.From.Id, text2, ChatType.Private, e.Message.From.LanguageCode, ParseMode.Html, null, e.Message.From.Username, e.Message.MessageId, false);
                return false;
            }

            var counter = 0;
            var channel = Regex.Match(e.Message.Text, @"\d+").Value;
            if (groups.Select("id = " + "'" + channel + "'").Length != 1)
            {
                var text2 = new Language(new Dictionary<string, string>
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

            var text = new Language(new Dictionary<string, string>
            {
                {"en", "Banned " + counter + " in group: " + groups.Select(channel)}
            });
            await sender.SendTextMessageAsync(e.Message.From.Id, text, ChatType.Private, e.Message.From.LanguageCode,
                ParseMode.Html, null, e.Message.From.Username, e.Message.MessageId);
            return true;
        }
        */

        private static async Task<long?> QueryBot(bool execute_true_select_false, MessageEventArgs e,
            TelegramBotAbstract sender)
        {
            if (e.Message.ForwardFrom != null)
                return null;

            if (e.Message.From == null)
                return null;

            if (GlobalVariables.IsOwner(e.Message.From.Id))
                return await QueryBot2(execute_true_select_false, e, sender);

            return null;
        }

        private static async Task<long?> QueryBot2(bool execute_true_select_false, MessageEventArgs e,
            TelegramBotAbstract sender)
        {
            if (e.Message.ReplyToMessage == null || string.IsNullOrEmpty(e.Message.ReplyToMessage.Text))
            {
                var text = new Language(new Dictionary<string, string>
                {
                    {"en", "You have to reply to a message containing the query"}
                });
                await sender.SendTextMessageAsync(e.Message.From.Id, text, ChatType.Private,
                    e.Message.From.LanguageCode, ParseMode.Html, null, e.Message.From.Username, e.Message.MessageId);
                return null;
            }

            var query = e.Message.ReplyToMessage.Text;
            if (execute_true_select_false)
            {
                var i = SqLite.Execute(query);

                var text = new Language(new Dictionary<string, string>
                {
                    {"en", "Query execution. Result: " + i}
                });
                await sender.SendTextMessageAsync(e.Message.From.Id, text, ChatType.Private,
                    e.Message.From.LanguageCode, ParseMode.Html, null, e.Message.From.Username, e.Message.MessageId);
                return i;
            }

            var x = SqLite.ExecuteSelect(query);
            var x2 = StreamSerialization.SerializeToStream(x);
            var documentInput =
                new TelegramFile(x2, "table.bin", "Query result", "application/octet-stream");
            TLAbsInputPeer peer2 = new TLInputPeerUser { UserId = (int)e.Message.From.Id };
            var peer = new Tuple<TLAbsInputPeer, long>(peer2, e.Message.From.Id);
            var text2 = new Language(new Dictionary<string, string>
            {
                {"en", "Query result"}
            });
            var v = await sender.SendFileAsync(documentInput, peer, text2, TextAsCaption.AS_CAPTION,
                e.Message.From.Username, e.Message.From.LanguageCode, e.Message.MessageId, false);
            return v ? 1 : 0;
        }

        private static async Task<MessageSentResult> TestTime(TelegramBotAbstract sender, MessageEventArgs e)
        {
            var sentDate = await DateTimeClass.AskDateAsync(e.Message.From.Id,
                e.Message.Text,
                e.Message.From.LanguageCode, sender, e.Message.From.Username);

            if (sentDate.Item2 != null)
            {
                await NotifyUtil.NotifyOwners(new ExceptionNumbered(sentDate.Item2), sender, 0, sentDate.Item3);

                return null;
            }

            var sentDate2 = sentDate.Item1.GetDate();

            var dict = new Dictionary<string, string>
            {
                {"en", DateTimeClass.DateTimeToItalianFormat(sentDate2)}
            };
            var text = new Language(dict);
            return await SendMessage.SendMessageInPrivate(sender, e.Message.From.Id,
                e.Message.From.LanguageCode, e.Message.From.Username,
                text, ParseMode.Html, e.Message.MessageId);
        }

        private static async Task<MessageSentResult> Rules(TelegramBotAbstract sender, MessageEventArgs e)
        {
            const string text = "Ecco le regole!\n" +
                                "https://polinetwork.github.io/it/rules";

            const string textEng = "Here are the rules!\n" +
                                   "https://polinetwork.github.io/en/rules";

            var text2 = new Language(new Dictionary<string, string>
            {
                {"en", textEng},
                {"it", text}
            });

            return await SendMessage.SendMessageInPrivate(sender, e.Message.From.Id,
                e.Message.From.LanguageCode,
                e.Message.From.Username, text2, ParseMode.Html, null);
        }

        private static async Task SendRecommendedGroupsAsync(TelegramBotAbstract sender, MessageEventArgs e)
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

            var text2 = new Language(new Dictionary<string, string>
            {
                {"en", textEng},
                {"it", text}
            });
            await SendMessage.SendMessageInPrivate(sender, e.Message.From.Id,
                e.Message.From.LanguageCode,
                e.Message.From.Username, text2, ParseMode.Html, null);
        }

        public static async Task<bool> GetAllGroups(long chatId, string username, TelegramBotAbstract sender,
            string lang)
        {
            var groups = Groups.GetAllGroups();
            Stream stream = new MemoryStream();
            FileSerialization.SerializeFile(groups, ref stream);
            TLAbsInputPeer peer2 = new TLInputPeerUser { UserId = (int)chatId };
            var peer = new Tuple<TLAbsInputPeer, long>(peer2, chatId);

            var text2 = new Language(new Dictionary<string, string>
            {
                {"en", "Here are all groups:"},
                {"it", "Ecco tutti i gruppi:"}
            });
            return await SendMessage.SendFileAsync(new TelegramFile(stream, "groups.bin",
                    null, "application/octet-stream"), peer,
                text2, TextAsCaption.BEFORE_FILE,
                sender, username, lang, null, true);
        }

        private static async Task<SuccessWithException> BanUserAsync(
            TelegramBotAbstract sender, MessageEventArgs e,
            string[] stringInfo, bool? revokeMessage)
        {
            var r =
                await Groups.CheckIfAdminAsync(e.Message.From.Id, e.Message.From.Username, e.Message.Chat.Id, sender);
            if (!r.IsSuccess()) return r;

            if (e.Message.ReplyToMessage == null)
            {
                var userIdFound = await Info.GetTargetUserIdAsync(stringInfo[1], sender);
                if (userIdFound == null)
                {
                    var e2 = new Exception("Can't find userid (1)");
                    await NotifyUtil.NotifyOwners(new ExceptionNumbered(e2), sender);
                    return new SuccessWithException(false, e2);
                }

                var targetId = userIdFound.GetID();
                if (targetId == null)
                {
                    var e2 = new Exception("Can't find userid (2)");
                    await NotifyUtil.NotifyOwners(new ExceptionNumbered(e2), sender);
                    return new SuccessWithException(false, e2);
                }

                return await RestrictUser.BanUserFromGroup(sender, targetId.Value, e.Message.Chat.Id, null,
                    revokeMessage);
            }

            var targetInt = e.Message.ReplyToMessage.From.Id;

            NotifyUtil.NotifyOwnersBanAction(sender, e, targetInt, e.Message.ReplyToMessage.From.Username);

            return await RestrictUser.BanUserFromGroup(sender, targetInt, e.Message.Chat.Id, stringInfo,
                revokeMessage);
        }

        private static async Task<SuccessWithException> UnbanAllAsync(
            TelegramBotAbstract sender, MessageEventArgs e, string[] target, string lang, string username,
            bool? revokeMessage)
        {
            return await BanAllUnbanAllMethod1Async2Async(sender, e, target, lang, username,
                RestrictAction.UNBAN, revokeMessage);
        }

        private static async Task<SuccessWithException> BanAllAsync(
            TelegramBotAbstract sender, MessageEventArgs e,
            string[] target, string lang, string username, bool? revokeMessage)
        {
            return await BanAllUnbanAllMethod1Async2Async(sender, e, target, lang, username, RestrictAction.BAN,
                revokeMessage);
        }

        private static async Task<SuccessWithException> MuteAllAsync(
            TelegramBotAbstract sender, MessageEventArgs e, string[] target, string lang, string username,
            bool? revokeMessage)
        {
            return await BanAllUnbanAllMethod1Async2Async(sender, e, target, lang, username, RestrictAction.MUTE,
                revokeMessage);
        }

        private static async Task<SuccessWithException> UnMuteAllAsync(
            TelegramBotAbstract sender, MessageEventArgs e, string[] target, string lang, string username,
            bool? revokeMessage)
        {
            return await BanAllUnbanAllMethod1Async2Async(sender, e, target, lang, username, RestrictAction.UNMUTE,
                revokeMessage);
        }

        private static async Task<SuccessWithException> BanAllUnbanAllMethod1Async2Async(TelegramBotAbstract sender,
            MessageEventArgs e,
            string[] target, string lang, string username, RestrictAction bAN,
            bool? revokeMessage)
        {
            var d1 = GetDateTime(target);
            try
            {
                await BanAllUnbanAllMethod1Async(bAN, GetFinalTargetForRestrictAll(e, target), e, sender, lang, username,
                    d1?.GetValue(), revokeMessage);
                return new SuccessWithException(true, d1?.GetExceptions());
            }
            catch (Exception ex)
            {
                var ex2 = Concat(ex, d1);
                return new SuccessWithException(false, ex2);
            }
        }

        private static List<Exception> Concat(Exception ex, ValueWithException<DateTime?> d1)
        {
            var r = new List<Exception>
            {
                ex
            };
            if (d1 != null && d1.ContainsExceptions()) r.AddRange(d1.GetExceptions());

            return r;
        }

        private static ValueWithException<DateTime?> GetDateTime(string[] target)
        {
            if (target == null)
                return null;
            if (target.Length < 3)
                return null;

            var s = "";
            for (var i = 2; i < target.Length; i++) s += target[i] + " ";

            s = s.Trim();
            return DateTimeClass.GetFromString(s);
        }

        private static async Task BanAllUnbanAllMethod1Async(RestrictAction restrictAction,
            string finalTarget,
            MessageEventArgs e, TelegramBotAbstract sender, string lang, string username, DateTime? until,
            bool? revokeMessage)
        {
            if (string.IsNullOrEmpty(finalTarget))
            {
                var lang2 = new Language(new Dictionary<string, string>
                {
                    {"en", "We can't find the target."},
                    {"it", "Non riusciamo a trovare il bersaglio"}
                });
                await sender.SendTextMessageAsync(e.Message.From.Id, lang2, ChatType.Private,
                    lang, ParseMode.Html, username: username,
                    replyMarkupObject: new ReplyMarkupObject(ReplyMarkupEnum.REMOVE));

                return;
            }

            if (string.IsNullOrEmpty(e.Message.ReplyToMessage?.Text))
            {
                var lang2 = new Language(new Dictionary<string, string>
                {
                    {"en", "The replied message cannot be empty!"},
                    {"it", "Il messaggio a cui rispondi non può essere vuoto"}
                });
                await sender.SendTextMessageAsync(e.Message.From.Id, lang2, ChatType.Private,
                    lang, ParseMode.Html, username: username,
                    replyMarkupObject: new ReplyMarkupObject(ReplyMarkupEnum.REMOVE));

                return;
            }

            var done =
                await RestrictUser.BanAllAsync(sender, e, finalTarget, restrictAction, until, revokeMessage);
            var text2 = done.Item1.GetLanguage(restrictAction, finalTarget, done.Item3);

            NotifyUtil.NotifyOwnersBanAction(sender, e, restrictAction, done, finalTarget, reason: e.Message.ReplyToMessage.Text);

            await SendMessage.SendMessageInPrivate(sender, e.Message.From.Id,
                e.Message.From.LanguageCode,
                e.Message.From.Username, text2,
                ParseMode.Html,
                e.Message.MessageId);

            await SendReportOfSuccessAndFailures(sender, e, done);
        }

        private static async Task SendReportOfSuccessAndFailures(TelegramBotAbstract sender, MessageEventArgs e,
            Tuple<BanUnbanAllResult, List<ExceptionNumbered>, long> done)
        {
            try
            {
                await SendReportOfSuccessAndFailures2(StreamSerialization.SerializeToStream(done.Item1.GetSuccess()),
                    "success.bin", sender, e);
                await SendReportOfSuccessAndFailures2(StreamSerialization.SerializeToStream(done.Item1.GetFailed()),
                    "failed.bin", sender, e);
            }
            catch
            {
                ;
            }
        }

        private static async Task SendReportOfSuccessAndFailures2(Stream stream, string filename,
            TelegramBotAbstract sender, MessageEventArgs e)
        {
            var file = new TelegramFile(stream, filename, "", "application/octet-stream");
            var peer = new Tuple<TLAbsInputPeer, long>(null, e.Message.From.Id);
            var text = new Language(new Dictionary<string, string>
            {
                {"en", ""}
            });
            await SendMessage.SendFileAsync(file, peer, text, TextAsCaption.AS_CAPTION,
                sender, e.Message.From.Username, e.Message.From.LanguageCode, null, true);
        }

        private static string GetFinalTargetForRestrictAll(MessageEventArgs e, IReadOnlyList<string> target)
        {
            return target[1];
        }

        private static string GetFinalTarget(MessageEventArgs e, IReadOnlyList<string> target)
        {
            return e.Message.ReplyToMessage == null && target.Count >= 2
                ? target[1]
                : e.Message.ReplyToMessage.From.Id.ToString();
        }

        private static async Task<bool> DefaultCommand(TelegramBotAbstract sender, MessageEventArgs e)
        {
            var text2 = new Language(new Dictionary<string, string>
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
            await SendMessage.SendMessageInPrivate(sender, e.Message.From.Id,
                e.Message.From.LanguageCode,
                e.Message.From.Username, text2,
                ParseMode.Html,
                null);

            return true;
        }

        private static async Task Help(TelegramBotAbstract sender, MessageEventArgs e)
        {
            if (e.Message.Chat.Type == ChatType.Private)
                await HelpPrivate(sender, e);
            else
                await CommandNotSentInPrivateAsync(sender, e);
        }

        private static async Task CommandNotSentInPrivateAsync(TelegramBotAbstract sender, MessageEventArgs e)
        {
            var lang = new Language(new Dictionary<string, string>
            {
                {"it", "Questo messaggio funziona solo in chat privata"},
                {"en", "This command only works in private chat with me"}
            });
            await SendMessage.SendMessageInPrivateOrAGroup(sender,
                lang, e.Message?.From?.LanguageCode, e.Message?.From?.Username, e.Message?.From?.Id,
                e.Message?.From?.FirstName, e.Message?.From?.LastName, e.Message.Chat.Id, e.Message.Chat.Type);

            await sender.DeleteMessageAsync(e.Message.Chat.Id, e.Message.MessageId, null);
        }

        private static async Task HelpPrivate(TelegramBotAbstract sender, MessageEventArgs e)
        {
            const string text = "<i>Lista di funzioni</i>:\n" +
                                //"\n📑 Sistema di recensioni dei corsi (per maggiori info /help_review)\n" +
                                //"\n🔖 Link ai materiali nei gruppi (per maggiori info /help_material)\n" +
                                "\n🙋 <a href='https://polinetwork.github.io/it/faq/index.html'>" +
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
                                   "\n🙋 <a href='https://polinetwork.github.io/it/faq/index.html'>" +
                                   "FAQ (frequently asked questions)</a>\n" +
                                   "\n🏫 Find free rooms /rooms\n" +
                                   //"\n🕶️ Anonymous posting system (for more info /help_anon)\n" +
                                   //"\n🎙️ Record of lessons (for more info /help_record)\n" +
                                   "\n👥 Recommended groups /groups\n" +
                                   "\n⚠ Have you already read our network rules? /rules\n" +
                                   "\n✍ To contact us /contact";

            var text2 = new Language(new Dictionary<string, string>
            {
                {"en", textEng},
                {"it", text}
            });
            await SendMessage.SendMessageInPrivate(sender, e.Message.From.Id,
                e.Message.From.LanguageCode,
                e.Message.From.Username, text2, ParseMode.Html, null);
        }

        private static async Task ContactUs(TelegramBotAbstract telegramBotClient, MessageEventArgs e)
        {
            await DeleteMessage.DeleteIfMessageIsNotInPrivate(telegramBotClient, e);
            var lang2 = new Language(new Dictionary<string, string>
            {
                {"it", telegramBotClient.GetContactString()},
                {"en", telegramBotClient.GetContactString()}
            });
            await telegramBotClient.SendTextMessageAsync(e.Message.Chat.Id,
                lang2, e.Message.Chat.Type, e.Message.From.LanguageCode,
                ParseMode.Html,
                new ReplyMarkupObject(ReplyMarkupEnum.REMOVE), e.Message.From.Username
            );
        }

        private static async Task ForceCheckInviteLinksAsync(TelegramBotAbstract sender, MessageEventArgs e)
        {
            long? n = null;
            try
            {
                n = await InviteLinks.FillMissingLinksIntoDB_Async(sender, e);
            }
            catch (Exception e2)
            {
                await NotifyUtil.NotifyOwners(new ExceptionNumbered(e2), sender);
            }

            if (n == null)
                return;

            var text2 = new Language(new Dictionary<string, string>
            {
                {"en", "I have updated n=" + n + " links"},
                {"it", "Ho aggiornato n=" + n + " link"}
            });
            await SendMessage.SendMessageInPrivate(sender, e.Message.From.Id,
                e.Message.From.LanguageCode,
                e.Message.From.Username, text2,
                ParseMode.Html,
                e.Message.MessageId);
        }

        private static async Task Start(TelegramBotAbstract telegramBotClient, MessageEventArgs e)
        {
            await DeleteMessage.DeleteIfMessageIsNotInPrivate(telegramBotClient, e);
            var lang2 = new Language(new Dictionary<string, string>
            {
                {
                    "it", "Ciao! 👋\n" +
                          "\nScrivi /help per la lista completa delle mie funzioni 👀\n" +
                          "\nVisita anche il nostro sito " + telegramBotClient.GetWebSite()
                },
                {
                    "en", "Hi! 👋\n" +
                          "\nWrite /help for the complete list of my functions👀\n" +
                          "\nAlso visit our site " + telegramBotClient.GetWebSite()
                }
            });
            await telegramBotClient.SendTextMessageAsync(e.Message.Chat.Id,
                lang2,
                e.Message.Chat.Type, replyMarkupObject: new ReplyMarkupObject(ReplyMarkupEnum.REMOVE),
                lang: e.Message.From.LanguageCode, username: e.Message.From.Username, parseMode: ParseMode.Html
            );
        }

        public static void BanMessageActions(TelegramBotAbstract telegramBotClient, MessageEventArgs e)
        {
            NotifyUtil.NotifyOwnersBanAction(telegramBotClient, e, e.Message.LeftChatMember?.Id, e.Message.LeftChatMember?.Username);
        }
    }
}