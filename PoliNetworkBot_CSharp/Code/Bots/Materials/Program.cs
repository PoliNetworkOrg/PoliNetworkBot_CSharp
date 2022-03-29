#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using PoliNetworkBot_CSharp.Code.Bots.Anon;
using PoliNetworkBot_CSharp.Code.Bots.Materials.Enums;
using PoliNetworkBot_CSharp.Code.Bots.Materials.Global;
using PoliNetworkBot_CSharp.Code.Bots.Materials.Utils;
using PoliNetworkBot_CSharp.Code.Bots.Moderation;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using BotUtils = PoliNetworkBot_CSharp.Code.Utils;

#endregion

namespace PoliNetworkBot_CSharp.Code.Bots.Materials;

[Serializable]
public class Program
{
    public static Dictionary<long, Conversation>
        UsersConversations = new(); //inizializzazione del dizionario <utente, Conversation>

    public static Dictionary<string, string>
        DictPaths = new(); //inizializzazione del dizionario <ID univoco file, stringa documento>

    public static Dictionary<string, List<string>> ModifiedFilesInGitFolder = new();

    private static object _lock1 = new();
    public static Utils.Config Config;
    private const long LogGroup = -1001399914655;
    private static object _slowDownLock = new();

    public static async void BotClient_OnMessageAsync(object sender, MessageEventArgs e)
    {
        try
        {
            await BotClient_OnMessageAsync2Async(sender, e);
        }
        catch (Exception exception)
        {
            BotUtils.Logger.WriteLine(exception, LogSeverityLevel.CRITICAL);
        }
    }

    private static async Task BotClient_OnMessageAsync2Async(object sender, MessageEventArgs e)
    {
        Config ??= JsonConvert.DeserializeObject<Utils.Config>(
            await File.ReadAllTextAsync(Data.Constants.Paths.Config.PoliMaterialsConfig));

        DictPaths ??= Deserialize<Dictionary<string, string>>(File.Open(
            Data.Constants.Paths.Data.PoliMaterialsDictPaths,
            FileMode.OpenOrCreate));

        TelegramBotClient telegramBotClientBot = null;
        TelegramBotAbstract telegramBotClient = null;

        try
        {
            if (sender is TelegramBotClient tmp) telegramBotClientBot = tmp;

            if (telegramBotClientBot == null)
                return;

            telegramBotClient = TelegramBotAbstract.GetFromRam(telegramBotClientBot);

            {
                try
                {
                    if (e.Message.Text == "/start") GenerateStart(e);

                    BotUtils.Logger.WriteLine( "Message Arrived " + e.Message.From.Id + " : " + e.Message.Text);
                    if (!UsersConversations.ContainsKey(e.Message.From.Id)) GenerateStart(e);

                    var state = UsersConversations[e.Message.From.Id].GetState();

                    switch (state)
                    {
                        case UserState.START:
                            await HandleStartAsync(e, telegramBotClient);
                            break;

                        case UserState.SCHOOL:
                            await HandleSchoolAsync(e, telegramBotClient);
                            break;

                        case UserState.COURSE:
                            await HandleCourseAsync(e, telegramBotClient);
                            break;

                        case UserState.FOLDER:
                            await HandleFolderAsync(e, telegramBotClient);
                            break;

                        case UserState.WAITING_FILE:
                            await HandleFileAsync(e, telegramBotClient);
                            break;

                        case UserState.NEW_FOLDER:
                            await HandleNewFolderAsync(e, telegramBotClient);
                            break;

                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                catch (Exception ex)
                {
                    await BotUtils.NotifyUtil.NotifyOwners(ex, telegramBotClient, e);
                }
            }
        }
        catch (Exception ex)
        {
            BotUtils.Logger.WriteLine(ex, LogSeverityLevel.CRITICAL);
        }
    }

    public static void Serialize<TObject>(TObject dictionary, Stream stream)
    {
        try // try to serialize the collection to a file
        {
            using (stream)
            {
                // create BinaryFormatter
                var bin = new BinaryFormatter();
                // serialize the collection (EmployeeList1) to file (stream)
                bin.Serialize(stream, dictionary);
            }
        }
        catch (IOException)
        {
            Console.WriteLine("dict non esistente? ser");
        }
    }

    public static Object Deserialize<Object>(Stream stream) where Object : new()
    {
        var ret = CreateInstance<Object>();
        try
        {
            using (stream)
            {
                // create BinaryFormatter
                var bin = new BinaryFormatter();
                // deserialize the collection (Employee) from file (stream)
                ret = (Object)bin.Deserialize(stream);
            }
        }
        catch (IOException ex)
        {
            Console.WriteLine(ex);
        }

        return ret;
    }

    // function to create instance of T
    public static TObject CreateInstance<TObject>() where TObject : new()
    {
        return (TObject)Activator.CreateInstance(typeof(TObject));
    }

    private static async void GitHandler(CallbackQueryEventArgs e, TelegramBotAbstract sender)
    {
        try
        {
            string logMessage;
            lock (_lock1)
            {
                var callbackQuery = e.CallbackQuery;
                var callbackdata = callbackQuery.Data.Split("|");
                var fromId = long.Parse(callbackdata[1]);
                if (!DictPaths.TryGetValue(callbackdata[2], out var directory))
                    throw new Exception("Errore nel dizionario dei Path in GITHANDLER!");
                var a = directory.Split("/");
                directory = "";
                for (var i = 0; i < a.Length - 1; i++) directory = directory + a[i] + "/";

                var b = directory.Split("'");
                directory = b.Aggregate("", (current, t) => current + t + "\'\'");

                logMessage = "Log for message ID: " + e.CallbackQuery.From.Id;
                directory = directory[..^2];
            
                logMessage += "\n\n";
                logMessage += "To Directory: " + directory;
                logMessage += "\n";
                logMessage += "Git Directory: " + GetGit(directory);
                logMessage += "\n";
                using var powershell = PowerShell.Create();
                var dirCd = "/" + GetRoot(directory) + "/" + GetCorso(directory) + "/" + GetGit(directory) +
                            "/";
                logMessage += DoScript(powershell, "cd " + dirCd, true) + "\n";
                logMessage += DoScript(powershell, "git pull", true) + "\n";
                logMessage += DoScript(powershell, "git add . --ignore-errors", true) + "\n\n";

                ModifiedFilesInGitFolder.TryGetValue(GetGit(directory), out var diffList);
                
                var diff = (diffList ?? new List<string>{"--"}).Aggregate("", (current, s) => current + s + ", ");
                
                if (diff.EndsWith(", ")) diff = diff[..^2];
                logMessage += "Git diff: " + diff + "\n";

                ModifiedFilesInGitFolder.Remove(GetGit(directory));
                
                var commit = @"git commit -m 'git commit by bot updated file: " + diff +
                             @"' --author=""PoliBot <polinetwork2@gmail.com>""";

                logMessage += "Commit results: " + DoScript(powershell, commit, true) + "\n";

                var push = @"git push https://polibot:" + Config.Password +
                           "@gitlab.com/polinetwork/" + GetGit(directory) + @".git --all";

                BotUtils.Logger.WriteLine(DoScript(powershell, push, true));

                logMessage += "Push Executed";

                BotUtils.Logger.WriteLine(logMessage);
                
                powershell.Stop();
            }
            
            var dict = new Dictionary<string, string>
            {
                { "uni", "Log:\n\n" +  HttpUtility.HtmlEncode(logMessage) }
            };
            var text = new Language(dict);

            await sender.SendTextMessageAsync(LogGroup,
                text, ChatType.Group, "uni", ParseMode.Html, null, null); 
            
        } catch (Exception ex)
        {
            _ = BotUtils.NotifyUtil.NotifyOwners(ex, sender);
        }
    }

    private static object GetCorso(string directory)
    {
        return directory.Split("/").GetValue(2);
    }

    private static object GetRoot(string directory)
    {
        return directory.Split("/").GetValue(1);
    }

    private static string GetGit(string directory)
    {
        return directory.Split("/").ToList()[3];
    }

    public static async void BotOnCallbackQueryReceived(object sender1,
        CallbackQueryEventArgs callbackQueryEventArgs)
    {
        TelegramBotClient sender2 = null;
        if (sender1 is TelegramBotClient s2)
            sender2 = s2;
        else
            return;

        var sender = TelegramBotAbstract.GetFromRam(sender2);

        try
        {
            await BotOnCallbackQueryReceived2(sender, callbackQueryEventArgs);
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception.Message);
            await BotUtils.NotifyUtil.NotifyOwners(exception, sender);
        }
    }

    private static async Task BotOnCallbackQueryReceived2(TelegramBotAbstract sender,
        CallbackQueryEventArgs callbackQueryEventArgs)
    {
        var callbackQuery = callbackQueryEventArgs.CallbackQuery;
        var callbackdata = callbackQuery.Data.Split("|");
        var FromId = long.Parse(callbackdata[1]);
        if (!DictPaths.TryGetValue(callbackdata[2], out var fileNameWithPath))
            throw new Exception("Errore nel dizionario dei Path!");
        if (!UserIsAdmin(sender, callbackQuery.From.Id, callbackQueryEventArgs.CallbackQuery.Message.Chat.Id))
        {
            await sender.AnswerCallbackQueryAsync(callbackQuery.Id,
                "Modification Denied! You need to be admin of this channel");
            return;
        }

        switch (callbackdata[0]) // FORMATO: Y o N | ID PERSONA | ID MESSAGGIO (DEL DOC) | fileUniqueID
        {
            case "y":
            {
                var nameApprover = callbackQuery.From.FirstName;
                if (nameApprover.Length > 1) nameApprover = nameApprover[0].ToString();

                await sender.AnswerCallbackQueryAsync(callbackQuery.Id,
                    "Modification Accepted"); //Mostra un messaggio all'utente

                var message = sender.EditMessageTextAsync(callbackQuery.Message.Chat.Id,
                    callbackQuery.Message.MessageId, "<b>MERGED</b> by " + nameApprover,
                    ParseMode.Html); //modifica il messaggio in modo che non sia più riclickabile

                if (callbackQuery.Message.ReplyToMessage.Document.FileSize > 20000000)
                {
                    var dict = new Dictionary<string, string>
                    {
                        {
                            "en", "Can't upload " + callbackQuery.Message.ReplyToMessage.Document.FileName +
                                  ". file size exceeds maximum allowed size. You can upload it manually from GitLab."
                        },
                        {
                            "it", "Il file " + callbackQuery.Message.ReplyToMessage.Document.FileName +
                                  " supera il peso massimo consentito. Puoi caricarlo a mano da GitLab."
                        }
                    };

                    var text = new Language(dict);
                    await sender.SendTextMessageAsync(
                        ChannelsForApproval.GetChannel(UsersConversations[FromId].GetCourse()), text,
                        ChatType.Private,
                        callbackQuery.Message.From.LanguageCode, ParseMode.Html, null, null);
                }

                var fileOnlyName = fileNameWithPath[Config.RootDir.Length..];
                try
                {
                    var endOfPath = fileNameWithPath.Split(@"/").Last().Split(@"/").Last().Length;
                    //string a = fileName.ToCharArray().Take(fileName.Length - endOfPath).ToString();
                    Directory.CreateDirectory(fileNameWithPath[..^endOfPath]);
                    await using var fileStream = File.OpenWrite(fileNameWithPath);
                    var tupleFileStream =
                        await sender.DownloadFileAsync(callbackQuery.Message.ReplyToMessage.Document);
                    await tupleFileStream.Item2.CopyToAsync(fileStream);
                    fileStream.Close();
                    var dict = new Dictionary<string, string>
                    {
                        { "en", "File Saved in " + fileOnlyName + "\n" },
                        { "it", "File salvato in " + fileOnlyName + "\n" }
                    };
                    var text = new Language(dict);
                    await sender.SendTextMessageAsync(FromId, text, ChatType.Private,
                        callbackQuery.From.LanguageCode, ParseMode.Html, null, null);
                }
                catch (Exception exception)
                {
                    var dict = new Dictionary<string, string>
                    {
                        {
                            "en", @"Couldn't save the file. Bot only support files up to 20 MB,
                                    although you can open a Pull Request on GitLab to upload it or ask an Admin to do it. "
                        },
                        {
                            "it",
                            "Impossibile salvare il file. Il bot supporta solo file fino a 20 MB, puoi aprire una " +
                            "pull request su GitLab per caricarlo o chiedere a un amministratore di farlo per te."
                        }
                    };
                    var text = new Language(dict);
                    await sender.SendTextMessageAsync(FromId, text, ChatType.Private,
                        callbackQuery.From.LanguageCode,
                        ParseMode.Html, null, null);
                }

                GitHandler(callbackQueryEventArgs, sender);
            }
                break;

            case "n":
                try
                {
                    var nameApprover = callbackQuery.From.FirstName;
                    if (nameApprover.Length > 1) nameApprover = nameApprover[0].ToString();
                    var fileOnlyName = callbackQuery.Message.ReplyToMessage.Document.FileName;
                    await sender.AnswerCallbackQueryAsync(callbackQuery.Id,
                        "Modification Denied");
                    await sender.EditMessageTextAsync(callbackQuery.Message.Chat.Id,
                        callbackQuery.Message.MessageId,
                        "<b>DENIED</b> by " + nameApprover,
                        ParseMode.Html); //modifica il messaggio in modo che non sia più riclickabile

                    var dict = new Dictionary<string, string>
                    {
                        { "en", "The file: " + fileOnlyName + " was rejected by an admin" },
                        { "it", "Il file: " + fileOnlyName + " è stato rifiutato da un admin" }
                    };
                    var text = new Language(dict);
                    await sender.SendTextMessageAsync(FromId, text, ChatType.Private,
                        callbackQuery.From.LanguageCode,
                        ParseMode.Html, null, null);
                }
                catch (Exception exception)
                {
                    var dict = new Dictionary<string, string>
                    {
                        { "en", "Couldn't save the file." },
                        { "it", "Non è stato possibile salvare il file." }
                    };
                    var text = new Language(dict);
                    await sender.SendTextMessageAsync(FromId, text, ChatType.Private,
                        callbackQuery.From.LanguageCode,
                        ParseMode.Html, null, null);
                    await BotUtils.NotifyUtil.NotifyOwners(exception, sender);
                }

                break;
        }
    }

    private static bool UserIsAdmin(TelegramBotAbstract bot, long userId, long chatId)
    {
        try
        {
            var result = bot.IsAdminAsync(userId, chatId);
            if (result.Result.ContainsExceptions()) throw result.Result.GetFirstException();
            return result.Result.IsSuccess();
        }
        catch (Exception ex)
        {
            _ = BotUtils.NotifyUtil.NotifyOwners(ex, bot);
            return false;
        }
    }

    private static async Task HandleNewFolderAsync(MessageEventArgs e, TelegramBotAbstract telegramBotAbstract)
    {
        if (e.Message.Text.Contains('/') || e.Message.Text.Contains('\\'))
        {
            GenerateStart(e);
            return;
        }

        UsersConversations[e.Message.From.Id].PathDroppedOneLevel(e.Message.Text);
        await GenerateFolderKeyboard(e, telegramBotAbstract);
        UsersConversations[e.Message.From.Id].SetState(UserState.FOLDER);
    }

    private static async Task GenerateFolderKeyboard(MessageEventArgs e, TelegramBotAbstract telegramBotAbstract)
    {
        var replyKeyboard = Keyboards.GetPathsKeyboard(e.Message.From.Id);
        await SendFolderAsync(e, replyKeyboard, telegramBotAbstract);
    }

    private static async Task HandleFileAsync(MessageEventArgs e, TelegramBotAbstract telegramBotAbstract)
    {
        //gestisce l'arrivo del messaggio dall'utente
        if (e.Message.Photo != null)
        {
            var dict = new Dictionary<string, string>
            {
                { "en", "Photos can only be sent without compression" },
                { "it", "Le immagini sono accettate solo se inviate senza compressione" }
            };
            var text = new Language(dict);
            await telegramBotAbstract.SendTextMessageAsync(e.Message.From.Id, text, ChatType.Private,
                e.Message.From.LanguageCode,
                ParseMode.Html, null, null);
            return;
        }

        if (e.Message?.Document == null)
        {
            var dict = new Dictionary<string, string>
            {
                { "en", "Going back to the main menu." },
                { "it", "Ritorno al menu principale." }
            };
            var text = new Language(dict);
            await telegramBotAbstract.SendTextMessageAsync(e.Message.From.Id, text, ChatType.Private,
                e.Message.From.LanguageCode,
                ParseMode.Html, null, null);
            await GenerateStartOnBackAndNull(e, telegramBotAbstract);
            return;
        }

        var file = Config.RootDir + UsersConversations[e.Message.From.Id].GetCourse().ToLower() + "/" +
                   UsersConversations[e.Message.From.Id].GetPath() + "/" + e.Message.Document.FileName;
        BotUtils.Logger.WriteLine("File requested: " + file);
        var FileUniqueAndGit = e.Message.Document.FileUniqueId + GetGit(file);
        var fileAlreadyPresent = false;
        string oldPath = null;
        if (!DictPaths.TryAdd(FileUniqueAndGit, file))
        {
            //Verifica anti-SPAM, da attivare se servisse
            if (DictPaths.TryGetValue(FileUniqueAndGit, out oldPath))
                fileAlreadyPresent = true;
            else
                throw new Exception("Fatal error while handling path dictionary");
        }

        ModifiedFilesInGitFolder.TryGetValue(GetGit(file), out var filesInGit);
        filesInGit ??= new List<string>();
        filesInGit.Add(e.Message.Document.FileName);
        ModifiedFilesInGitFolder.Add(GetGit(file), filesInGit);   
        
        try
        {
            Serialize(DictPaths, File.Open(Data.Constants.Paths.Data.FilePaths, FileMode.Create));
        }
        catch (Exception exception)
        {
            await BotUtils.NotifyUtil.NotifyOwners(exception, telegramBotAbstract);
        }

        var inlineKeyboardButton = new List<InlineKeyboardButton>
        {
            InlineKeyboardButton.WithCallbackData("Yes", "y|" + e.Message.From.Id + "|" + FileUniqueAndGit),
            InlineKeyboardButton.WithCallbackData("No", "n|" + e.Message.From.Id + "|" + FileUniqueAndGit)
        };

        var inlineKeyboardMarkup = new InlineKeyboardMarkup(inlineKeyboardButton);

        if (!fileAlreadyPresent || oldPath != null)
        {
            var dict = new Dictionary<string, string>
            {
                { "en", "File sent for approval" },
                { "it", "File inviato per approvazione" }
            };
            var text = new Language(dict);

            await telegramBotAbstract.SendTextMessageAsync(e.Message.Chat.Id, text, ChatType.Private,
                e.Message.From.LanguageCode,
                ParseMode.Html, null, null);

            MessageSentResult messageFw;
            
            lock (_slowDownLock)
            {
                messageFw = telegramBotAbstract.ForwardMessageAsync(e.Message.MessageId,
                    e.Message.Chat.Id,
                    ChannelsForApproval.GetChannel(UsersConversations[e.Message.From.Id].GetCourse())).Result;
                
                Thread.Sleep(100);
                
                var approveMessage = new Dictionary<string, string>
                {
                    {
                        "uni", "Approvi l'inserimento del documento in " +
                               UsersConversations[e.Message.From.Id].GetCourse() + "/" +
                               UsersConversations[e.Message.From.Id].GetPath() + " ?"
                    }
                };
                var approveText = new Language(approveMessage);

                
                _ = telegramBotAbstract.SendTextMessageAsync(
                    ChannelsForApproval.GetChannel(UsersConversations[e.Message.From.Id].GetCourse()),
                    approveText, ChatType.Group, e.Message.From.LanguageCode, ParseMode.Html,
                    new ReplyMarkupObject(inlineKeyboardMarkup), null,
                    messageFw.GetMessageID()); //aggiunge sotto la InlineKeyboard per la selezione del what to do
                
                Thread.Sleep(100);
            }
        }
        else
        {
            throw new Exception(
                "Fatal error while handling path dictionary -> fileAlreadyPresent && oldPath != null");
        }
    }

    private static void GenerateStart(MessageEventArgs e)
    {
        if (!UsersConversations.ContainsKey(e.Message.From.Id))
        {
            var conv = new Conversation();
            UsersConversations.TryAdd(e.Message.From.Id, conv);
        }
        else
        {
            UsersConversations[e.Message.From.Id].SetState(UserState.START);
            UsersConversations[e.Message.From.Id].ResetPath();
        }
    }

    private static async Task HandleFolderAsync(MessageEventArgs e, TelegramBotAbstract sender)
    {
        if (e.Message.Text == null)
        {
            await GenerateStartOnBackAndNull(e, sender);
            return;
        }

        if (e.Message.Document != null)
        {
            var dict = new Dictionary<string, string>
            {
                {
                    "en",
                    "File received. Send your files (can be multiple). Write anything to go back to the main menu"
                },
                {
                    "it",
                    "File ricevuto. Invia tutti i file che vuoi caricare in questa cartella, scrivi qualsiasi cosa per tornare al menu"
                }
            };
            var text = new Language(dict);

            await sender.SendTextMessageAsync(e.Message.Chat.Id, text, ChatType.Private,
                e.Message.From.LanguageCode,
                ParseMode.Html, null, null);

            UsersConversations[e.Message.From.Id].SetState(UserState.WAITING_FILE);
            await HandleFileAsync(e, sender);
            return;
        }

        if (e.Message.Text.StartsWith("🆗"))
        {
            UsersConversations[e.Message.From.Id].SetState(UserState.WAITING_FILE);

            var dict = new Dictionary<string, string>
            {
                { "en", "Send your files (can be multiple). Write anything to go back to the main menu" },
                {
                    "it",
                    "Invia tutti i file che vuoi caricare in questa cartella, scrivi qualsiasi cosa per tornare al menu"
                }
            };
            var text = new Language(dict);

            await sender.SendTextMessageAsync(e.Message.Chat.Id, text, ChatType.Private,
                e.Message.From.LanguageCode,
                ParseMode.Html, null, null);
        }
        else if (e.Message.Text.StartsWith("🔙"))
        {
            await HandleCourseAsync(e, sender);
            await BotClient_OnMessageAsync2Async(sender, e);
        }
        else if (e.Message.Text.StartsWith("🆕"))
        {
            await GeneraCartellaAsync(e, sender);
        }
        else
        {
            if (!VerificaSottoCartelle(e))
            {
                var dict = new Dictionary<string, string>
                {
                    { "en", "Folder not recognized. Use the button to create a new one." },
                    { "it", "Cartella non trovata, usa il bottone per crearne una nuova" }
                };
                var text = new Language(dict);

                await sender.SendTextMessageAsync(e.Message.Chat.Id, text, ChatType.Private,
                    e.Message.From.LanguageCode,
                    ParseMode.Html, null, null);
            }
            else
            {
                UsersConversations[e.Message.From.Id].PathDroppedOneLevel(e.Message.Text);
                var replyKeyboard = Keyboards.GetPathsKeyboard(e.Message.From.Id);
                await SendFolderAsync(e, replyKeyboard, sender);
            }
        }
    }

    private static bool VerificaSottoCartelle(MessageEventArgs e)
    {
        var sottoCartelle = Keyboards.GetDir(e.Message.From.Id);
        return sottoCartelle.Any(a => a.Split(@"/").Last().Equals(e.Message.Text.Split(@"/").Last()));
    }

    private static async Task GeneraCartellaAsync(MessageEventArgs e, TelegramBotAbstract sender)
    {
        UsersConversations[e.Message.From.Id].SetState(UserState.NEW_FOLDER);
        var dict = new Dictionary<string, string>
        {
            { "en", "Write the name of the new folder" },
            { "it", "Scrivi il nome della cartella" }
        };
        var text = new Language(dict);

        await sender.SendTextMessageAsync(e.Message.Chat.Id, text, ChatType.Private,
            e.Message.From.LanguageCode,
            ParseMode.Html, null, null);
    }

    private static async Task HandleStartAsync(MessageEventArgs e, TelegramBotAbstract telegramBotAbstract)
    {
        UsersConversations[e.Message.From.Id].SetState(UserState.SCHOOL);
        var replyKeyboard = Keyboards.GetKeyboardSchools();
        var dict = new Dictionary<string, string>
        {
            { "en", "Choose a school" },
            { "it", "Scegli una scuola" }
        };
        var text = new Language(dict);
        var replyMarkupObject = new ReplyMarkupObject(
            new ReplyMarkupOptions(
                BotUtils.KeyboardMarkup.OptionsStringToKeyboard(replyKeyboard, e.Message.From.LanguageCode)
            )
        );
        await telegramBotAbstract.SendTextMessageAsync(e.Message.Chat.Id, text, ChatType.Private,
            e.Message.From.LanguageCode,
            ParseMode.Html, replyMarkupObject, null);
    }

    private static async Task HandleCourseAsync(MessageEventArgs e, TelegramBotAbstract sender)
    {
        UsersConversations[e.Message.From.Id].ResetPath();
        if (e.Message.Text == null
            || e.Message.Text.StartsWith("🔙")
            || !Navigator.CourseHandler(UsersConversations[e.Message.From.Id], e.Message.Text))
        {
            if (!Navigator.CourseHandler(UsersConversations[e.Message.From.Id], e.Message.Text))
            {
                var dict = new Dictionary<string, string>
                {
                    { "en", "Unknown path. Going back to beginning. Use the Keyboard to navigate the folders." },
                    {
                        "it",
                        "Percorso sconosciuto, ritorno all'inizio. Usa il tastierino per navigare tra le cartelle."
                    }
                };
                if (e.Message.Text.StartsWith("🔙"))
                    dict = new Dictionary<string, string>
                    {
                        { "en", "Going back to beginning." },
                        { "it", "Ritorno all'inizio." }
                    };
                var text = new Language(dict);
                await sender.SendTextMessageAsync(e.Message.Chat.Id, text, ChatType.Private,
                    e.Message.From.LanguageCode,
                    ParseMode.Html, null, null);
            }

            await HandleStartAsync(e, sender);
            return;
        }

        try
        {
            var replyKeyboard = Keyboards.GetPathsKeyboard(e.Message.From.Id);
            if (replyKeyboard.Count == 0)
                throw new Exception("No paths for folder " + UsersConversations[e.Message.From.Id].GetCourse());
            await SendFolderAsync(e, replyKeyboard, sender);
        }
        catch (Exception ex)
        {
            var dict = new Dictionary<string, string>
            {
                { "en", "The folder you have selected is not available" },
                { "it", "La cartella non è disponibile." }
            };
            var text = new Language(dict);
            await sender.SendTextMessageAsync(e.Message.Chat.Id, text, ChatType.Private,
                e.Message.From.LanguageCode,
                ParseMode.Html, null, null);
            await BotUtils.NotifyUtil.NotifyOwners(ex, sender, e);
        }
    }

    private static async Task GenerateStartOnBackAndNull(MessageEventArgs e, TelegramBotAbstract telegramBotAbstract)
    {
        GenerateStart(e);
        await BotClient_OnMessageAsync2Async(telegramBotAbstract, e);
    }

    private static async Task SendFolderAsync(MessageEventArgs e,
        List<List<Language>> replyKeyboard, TelegramBotAbstract telegramBotAbstract)
    {
        if (replyKeyboard == null)
            return;

        var dict = new Dictionary<string, string>
        {
            { "en", "Choose a path" },
            { "it", "Seleziona un percorso" }
        };
        var text = new Language(dict);
        var replyMarkupObject = new ReplyMarkupObject(
            new ReplyMarkupOptions(
                BotUtils.KeyboardMarkup.OptionsStringToKeyboard(replyKeyboard, e.Message.From.LanguageCode)
            )
        );
        await telegramBotAbstract.SendTextMessageAsync(e.Message.Chat.Id, text, ChatType.Private,
            e.Message.From.LanguageCode,
            ParseMode.Html, replyMarkupObject, null);
    }

    private static async Task HandleSchoolAsync(MessageEventArgs e, TelegramBotAbstract telegramBotAbstract)
    {
        if (e.Message.Text == null ||
            !Navigator.SchoolHandler(UsersConversations[e.Message.From.Id], e.Message.Text))
        {
            var dict = new Dictionary<string, string>
            {
                { "en", "Unknown path. Going back to beginning. Use the Keyboard to navigate the folders." },
                {
                    "it",
                    "Percorso sconosciuto. Ritorno al menu principale. Usa il tastierino per navigare tra le cartelle."
                }
            };
            var text = new Language(dict);
            await telegramBotAbstract.SendTextMessageAsync(e.Message.Chat.Id, text, ChatType.Private,
                e.Message.From.LanguageCode,
                ParseMode.Html, null, null);

            await GenerateStartOnBackAndNull(e, telegramBotAbstract);
            return;
        }

        var replyKeyboard = Keyboards.GetKeyboardCorsi(UsersConversations[e.Message.From.Id].GetSchool());
        var replyMarkupObject = new ReplyMarkupObject(
            new ReplyMarkupOptions(
                BotUtils.KeyboardMarkup.OptionsStringToKeyboard(replyKeyboard, e.Message.From.LanguageCode)
            )
        );
        var dict1 = new Dictionary<string, string>
        {
            { "en", "Chosen " + UsersConversations[e.Message.From.Id].GetSchool() },
            { "it", "Selezionata " + UsersConversations[e.Message.From.Id].GetSchool() }
        };
        var text1 = new Language(dict1);
        await telegramBotAbstract.SendTextMessageAsync(e.Message.Chat.Id, text1, ChatType.Private,
            e.Message.From.LanguageCode,
            ParseMode.Html, replyMarkupObject, null);
    }

    private static string DoScript(PowerShell powershell, string script, bool debug, string separator = "\n")
    {
        return CommandDispatcher.DoScript(powershell, script, debug)
            .Aggregate("", (current, s) => current + s + separator);
    }
}