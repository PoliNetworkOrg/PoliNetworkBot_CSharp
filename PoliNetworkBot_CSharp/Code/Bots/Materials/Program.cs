using Bot;
using Bot.Enums;
using BotUtils = PoliNetworkBot_CSharp.Code.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using System.Management.Automation;
using System.Threading.Tasks;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json;
using PoliNetworkBot_CSharp.Code.Bots.Anon;
using PoliNetworkBot_CSharp.Code.Bots.Materials.Enums;
using PoliNetworkBot_CSharp.Code.Bots.Materials.Global;
using PoliNetworkBot_CSharp.Code.Bots.Materials.Utils;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects;
using Config = PoliNetworkBot_CSharp.Code.Bots.Materials.Utils.Config;
using File = System.IO.File;
using PoliNetworkBot_CSharp.Code.Bots.Moderation;

namespace PoliNetworkBot_CSharp.Code.Bots.Materials
{
    [Serializable]
    public class Program
    {
        public static Dictionary<long, Conversation> UsersConversations = new(); //inizializzazione del dizionario <utente, Conversation>
        
        public static Dictionary<string, string> DictPaths = new (); //inizializzazione del dizionario <ID univoco file, stringa documento>

        
        private static object _lock1 = new  ();
        public static Utils.Config Config;
        private static long _logGroup = -1001399914655;

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
            
            DictPaths ??= Deserialize<Dictionary<string, string>>(File.Open(Data.Constants.Paths.Data.PoliMaterialsDictPaths,
                    FileMode.Open));

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

                        if (e.Message.Text == "/start")
                        {
                            await GeneraStartAsync(e);
                        }

                        Console.WriteLine(e.Message.Text);
                        if (!UsersConversations.ContainsKey(e.Message.From.Id))
                        {
                            await GeneraStartAsync(e);
                        }

                        var state = UsersConversations[e.Message.From.Id].getStato();

                        switch (state)
                        {
                            case stati.start:
                                await GestisciStartAsync(e, telegramBotClient);
                                break;
                            case stati.Scuola:
                                await GestisciScuolaAsync(e, telegramBotClient);
                                break;
                            case stati.Corso:
                                await GestisciCorsoAsync(e, telegramBotClient);
                                break;
                            case stati.Cartella:
                                await GestisciCartellaAsync(e, telegramBotClient);
                                break;
                            case stati.AttesaFile:
                                await GestisciFileAsync(e, telegramBotClient);
                                break;
                            case stati.newCartella:
                                await GestisciNewCartellaAsync(e, telegramBotClient);
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    } catch (Exception ex)
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
                    BinaryFormatter bin = new BinaryFormatter();
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
            Object ret = CreateInstance<Object>();
            try
            {
                using (stream)
                {
                    // create BinaryFormatter
                    BinaryFormatter bin = new BinaryFormatter();
                    // deserialize the collection (Employee) from file (stream)
                    ret = (Object) bin.Deserialize(stream);
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
            return (TObject) Activator.CreateInstance(typeof(TObject));
        }

        private static void GitHandler(CallbackQueryEventArgs e, TelegramBotAbstract sender)
        {
            lock (_lock1)
            {
                var callbackQuery = e.CallbackQuery;
                String[] callbackdata = callbackQuery.Data.Split("|");
                long fromId = Int64.Parse(callbackdata[1]);
                string directory;
                if (!DictPaths.TryGetValue(callbackdata[2], out directory))
                    throw new Exception("Errore nel dizionario dei Path in GITHANDLER!");
                string[] a = directory.Split("/");
                directory = "";
                for (int i = 0; i < a.Length - 1; i++)
                {
                    directory = directory + a[i] + "/";
                }

                string[] b = directory.Split("'");
                directory = "";
                for (int i = 0; i < b.Length; i++)
                {
                    directory = directory + b[i] + "\'\'";
                }

                string logMessage = "Log for message ID: " + e.CallbackQuery.From.Id;
                directory = directory.Substring(0, directory.Length - 2);
                try
                {
                    logMessage += "\n\n";
                    logMessage += "To Directory: " + directory;
                    logMessage += "\n";
                    logMessage += "Git Directory: " + GetGit(directory);
                    logMessage += "\n";
                    using (PowerShell powershell = PowerShell.Create())
                    {
                        string dirCd = "/" + GetRoot(directory) + "/" + GetCorso(directory) + "/" + GetGit(directory) + "/";
                        logMessage += DoScript(powershell, "cd " + dirCd, true) + "\n";
                        logMessage += DoScript(powershell, "git pull", true) + "\n";
                        logMessage += DoScript(powershell, "git add . --ignore-errors", true)+ "\n\n";

                        string diff = DoScript(powershell, "git ls-files --others --exclude-standard", true, ", ");
                        if (diff.EndsWith(", ")) diff = diff[..^2];
                        logMessage += "Git diff: " + diff + "\n";
                        
                        string commit = @"git commit -m 'git commit by bot updated file: " + diff +
                                        @"' --author=""PoliBot <polinetwork2@gmail.com>""";

                        logMessage += "Commit results: " + DoScript(powershell, commit, true);

                        string push = @"git push https://polibot:" + Config.Password +
                                      "@gitlab.com/polinetwork/" + GetGit(directory) + @".git --all";
                        
                        logMessage += "Push Result: " + DoScript(powershell, push, true);
                        
                        var dict = new Dictionary<string, string>
                        {
                            { "en", "File sent for approval" },
                            { "it", "File inviato per approvazione"}
                        };
                        var text = new Language(dict);

                        sender.SendTextMessageAsync(_logGroup, 
                            text, ChatType.Group, "uni", ParseMode.Html, null, null);
                        
                        powershell.Stop();
                    }
                }
                catch (Exception ex)
                {
                    BotUtils.Logger.WriteLine(ex);
                    BotUtils.NotifyUtil.NotifyOwners(ex, sender);
                }
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

        private static object GetGit(string directory)
        {
            return directory.Split("/").GetValue(3);
        }
        
        public static async void BotOnCallbackQueryReceived(object sender1,
            CallbackQueryEventArgs callbackQueryEventArgs)
        {
            TelegramBotClient sender2 = null;
            if (sender1 is TelegramBotClient s2)
            {
                sender2 = s2;
            }
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
            String[] callbackdata = callbackQuery.Data.Split("|");
            long FromId = Int64.Parse(callbackdata[1]);
            string fileNameWithPath;
            if (!DictPaths.TryGetValue(callbackdata[2], out fileNameWithPath))
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
                    string nameApprover = callbackQuery.From.FirstName;
                    if (nameApprover.Length > 1)
                    {
                        nameApprover = nameApprover[0].ToString();
                    }

                    await sender.AnswerCallbackQueryAsync(callbackQueryId: callbackQuery.Id,
                        text: $"Modification Accepted"); //Mostra un messaggio all'utente
                    
                    var message = sender.EditMessageTextAsync(callbackQuery.Message.Chat.Id,
                        callbackQuery.Message.MessageId, "<b>MERGED</b> by " + nameApprover,
                        ParseMode.Html); //modifica il messaggio in modo che non sia più riclickabile
                    
                    if (callbackQuery.Message.ReplyToMessage.Document.FileSize > 20000000)
                    {
                        var dict = new Dictionary<string, string>
                        {
                            { "en", "Can't upload " + callbackQuery.Message.ReplyToMessage.Document.FileName +
                                    ". file size exceeds maximum allowed size. You can upload it manually from GitLab." },
                            { "it", "Il file " + callbackQuery.Message.ReplyToMessage.Document.FileName +
                                    " supera il peso massimo consentito. Puoi caricarlo a mano da GitLab."}
                        };
                
                        var text = new Language(dict);
                        await sender.SendTextMessageAsync(
                            ChannelsForApproval.GetChannel(UsersConversations[FromId].getcorso()), text, ChatType.Private,
                            callbackQuery.Message.From.LanguageCode, ParseMode.Html, null, null);
                    }

                    var fileName = fileNameWithPath;
                    var fileOnlyName = fileName.Substring(Config.RootDir.Length);
                    try
                    {
                        int endOfPath = fileName.Split(@"/").Last().Split(@"/").Last().Length;
                        //string a = fileName.ToCharArray().Take(fileName.Length - endOfPath).ToString();
                        Directory.CreateDirectory(fileName.Substring(0, fileName.Length - endOfPath));
                        await using FileStream fileStream = File.OpenWrite(fileName);
                        var tupleFileStream = await sender.DownloadFileAsync(callbackQuery.Message.ReplyToMessage.Document);
                        await tupleFileStream.Item2.CopyToAsync(fileStream);
                        fileStream.Close();
                        var dict = new Dictionary<string, string>
                        {
                            { "en", "File Saved in " + fileOnlyName + "\n" },
                            { "it", "File salvato in " + fileOnlyName + "\n" },

                        };
                        var text = new Language(dict); 
                        await sender.SendTextMessageAsync(FromId, text, ChatType.Private,
                            callbackQuery.From.LanguageCode, ParseMode.Html, null, null);
                    }
                    catch (Exception exception)
                    {
                        var dict = new Dictionary<string, string>
                        {
                            { "en", @"Couldn't save the file. Bot only support files up to 20 MB, 
                                    although you can open a Pull Request on GitLab to upload it or ask an Admin to do it. "},
                            { "it", "Impossibile salvare il file. Il bot supporta solo file fino a 20 MB, puoi aprire una " +
                                    "pull request su GitLab per caricarlo o chiedere a un amministratore di farlo per te." },

                        };
                        var text = new Language(dict);
                        await sender.SendTextMessageAsync(FromId, text, ChatType.Private, callbackQuery.From.LanguageCode, 
                            ParseMode.Html, null, null);
                        
                    }

                    GitHandler(callbackQueryEventArgs, sender);
                }
                    break;
                case "n":
                    try
                    {
                        string nameApprover = callbackQuery.From.FirstName;
                        if (nameApprover.Length > 1)
                        {
                            nameApprover = nameApprover[0].ToString();
                        }
                        string fileOnlyName = callbackQuery.Message.ReplyToMessage.Document.FileName;
                        await sender.AnswerCallbackQueryAsync(callbackQueryId: callbackQuery.Id,
                            text: $"Modification Denied");
                        await sender.EditMessageTextAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId,
                            "<b>DENIED</b> by " + nameApprover,
                            ParseMode.Html); //modifica il messaggio in modo che non sia più riclickabile
                        
                        var dict = new Dictionary<string, string>
                        {
                            { "en", "The file: " + fileOnlyName + " was rejected by an admin" },                              
                            { "it", "Il file: " + fileOnlyName + " è stato rifiutato da un admin" }

                        };
                        var text = new Language(dict);
                        await sender.SendTextMessageAsync(FromId, text, ChatType.Private, callbackQuery.From.LanguageCode, 
                            ParseMode.Html, null, null);
                    }
                    catch (Exception exception)
                    {
                        var dict = new Dictionary<string, string>
                        {
                            { "en", "Couldn't save the file." },                              
                            { "it", "Non è stato possibile salvare il file." },                              
     

                        };
                        var text = new Language(dict);
                        await sender.SendTextMessageAsync(FromId, text, ChatType.Private, callbackQuery.From.LanguageCode, 
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
                BotUtils.NotifyUtil.NotifyOwners(ex, bot);
                return false;
            }
        }

        private static async Task GestisciNewCartellaAsync(MessageEventArgs e, TelegramBotAbstract telegramBotAbstract)
        {
            if(e.Message.Text.Contains("/") || e.Message.Text.Contains(@"\"))
            {
                await GeneraStartAsync(e);
                return;
            }
            UsersConversations[e.Message.From.Id].scesoDiUnLivello(e.Message.Text);
            await GenerateFolderKeyboard(e, telegramBotAbstract);
            UsersConversations[e.Message.From.Id].setStato(stati.Cartella);
        }

        private static async Task GenerateFolderKeyboard(MessageEventArgs e, TelegramBotAbstract telegramBotAbstract)
        {
            var replyKeyboard = Keyboards.GetPathsKeyboard(e.Message.From.Id);
            await InviaCartellaAsync(e, replyKeyboard, telegramBotAbstract);
        }

        private static async Task GestisciFileAsync(MessageEventArgs e, TelegramBotAbstract telegramBotAbstract)
        {
            //gestisce l'arrivo del messaggio dall'utente
            if (e.Message.Photo != null)
            {
                var dict = new Dictionary<string, string>
                {
                    { "en", "Photos can only be sent without compression" },                              
                    { "it", "Le immagini sono accettate solo se inviate senza compressione" },
                };
                var text = new Language(dict);
                await telegramBotAbstract.SendTextMessageAsync(e.Message.From.Id, text, ChatType.Private, e.Message.From.LanguageCode, 
                    ParseMode.Html, null, null);
                return;
            }

            if (e.Message?.Document == null)
            {
                var dict = new Dictionary<string, string>
                {
                    { "en", "Going back to the main menu." },                              
                    { "it", "Ritorno al menu principale." },
                };
                var text = new Language(dict);
                await telegramBotAbstract.SendTextMessageAsync(e.Message.From.Id, text, ChatType.Private, e.Message.From.LanguageCode, 
                    ParseMode.Html, null, null);
                await generaStartOnBackAndNull(e, telegramBotAbstract);
                return;
            }
            
            var file = Config.RootDir + UsersConversations[e.Message.From.Id].getcorso().ToLower() + "/" +
                       UsersConversations[e.Message.From.Id].getPercorso() + "/" + e.Message.Document.FileName;
            BotUtils.Logger.WriteLine("File requested: " + file);
            string FileUniqueAndGit = e.Message.Document.FileUniqueId + GetGit(file);
            Boolean fileAlreadyPresent = false;
            string oldPath = null;
            if (!DictPaths.TryAdd(FileUniqueAndGit, file))
            {
                //Verifica anti-SPAM, da attivare se servisse
                if (DictPaths.TryGetValue(FileUniqueAndGit, out oldPath))
                {
                    fileAlreadyPresent = true;
                }
                else
                {
                    throw new Exception("Fatal error while handling path dictionary");
                }
            }

            ;
            try
            {
                Serialize(DictPaths, System.IO.File.Open("/home/ubuntu/bot/dictPath.bin", FileMode.Create));
            }
            catch (Exception exception)
            {
                await BotUtils.NotifyUtil.NotifyOwners(exception, telegramBotAbstract);
            }

            List<InlineKeyboardButton> inlineKeyboardButton = new List<InlineKeyboardButton>()
            {
                InlineKeyboardButton.WithCallbackData(text : "Yes", callbackData : "y|" + e.Message.From.Id + "|" + FileUniqueAndGit),
                InlineKeyboardButton.WithCallbackData(text : "No", callbackData: "n|" + e.Message.From.Id + "|" + FileUniqueAndGit),
            };
            
            InlineKeyboardMarkup inlineKeyboardMarkup = new InlineKeyboardMarkup(inlineKeyboardButton);
            
            if ((!fileAlreadyPresent) || (oldPath != null))
            {
                var dict = new Dictionary<string, string>
                {
                    { "en", "File sent for approval" },
                    { "it", "File inviato per approvazione"}
                };
                var text = new Language(dict);

                await telegramBotAbstract.SendTextMessageAsync(e.Message.Chat.Id, text, ChatType.Private,
                    e.Message.From.LanguageCode,
                    ParseMode.Html, null, null);
                
                MessageSentResult messageFw = await telegramBotAbstract.ForwardMessageAsync(e.Message.MessageId,
                    e.Message.Chat.Id,
                    ChannelsForApproval.GetChannel(UsersConversations[e.Message.From.Id].getcorso()));

                var dict1 = new Dictionary<string, string>
                {
                    { "uni", "Approvi l'inserimento del documento in " + UsersConversations[e.Message.From.Id].getcorso() + "/" +
                            UsersConversations[e.Message.From.Id].getPercorso() + " ?" },
                };
                var text1 = new Language(dict);

                var queryAw = await telegramBotAbstract.SendTextMessageAsync(
                    ChannelsForApproval.GetChannel(UsersConversations[e.Message.From.Id].getcorso()),
                    text1, ChatType.Group, e.Message.From.LanguageCode, ParseMode.Html, 
                    new ReplyMarkupObject(inlineKeyboardMarkup), null, messageFw.GetMessageID()); //aggiunge sotto la InlineKeyboard per la selezione del what to do
            }
            else
            {
                throw new Exception(
                    "Fatal error while handling path dictionary -> fileAlreadyPresent && oldPath != null");
            }
                
            Thread.Sleep(200);
        }
        
        
        
        private static async Task GeneraStartAsync(MessageEventArgs e)
        {
            if (!UsersConversations.ContainsKey(e.Message.From.Id))
            {
                Conversation conv = new Conversation();
                UsersConversations.TryAdd(e.Message.From.Id, conv);
            }
            else
            {
                UsersConversations[e.Message.From.Id].setStato(stati.start);
                UsersConversations[e.Message.From.Id].resetPercorso();
            }
        }

        private static void generaStartOnCallback(CallbackQueryEventArgs e)
        {
            if (!UsersConversations.ContainsKey(e.CallbackQuery.Message.From.Id))
            {
                Conversation conv = new Conversation();
                UsersConversations.TryAdd(e.CallbackQuery.Message.From.Id,
                    conv); //aggiunge una conversazione al dizionario, questa parte è WIP
            }
            else
            {
                UsersConversations[e.CallbackQuery.Message.From.Id].setStato(stati.start);
            }
        }

        private static async Task GestisciCartellaAsync(MessageEventArgs e, TelegramBotAbstract sender)
        {
            if (e.Message.Text == null)
            {
                await generaStartOnBackAndNull(e, sender);
                return;
            }
            
            if (e.Message.Document != null)
            {
                var dict = new Dictionary<string, string>
                {
                    { "en", "File received. Send your files (can be multiple). Write anything to go back to the main menu" },
                    { "it", "File ricevuto. Invia tutti i file che vuoi caricare in questa cartella, scrivi qualsiasi cosa per tornare al menu"}
                };
                var text = new Language(dict);

                await sender.SendTextMessageAsync(e.Message.Chat.Id, text, ChatType.Private,
                    e.Message.From.LanguageCode,
                    ParseMode.Html, null, null);
                
                UsersConversations[e.Message.From.Id].setStato(stati.AttesaFile);
                await GestisciFileAsync(e, sender);
                return;
            }

            if (e.Message.Text.StartsWith("🆗"))
            {
                UsersConversations[e.Message.From.Id].setStato(stati.AttesaFile);
                
                var dict = new Dictionary<string, string>
                {
                    { "en", "Send your files (can be multiple). Write anything to go back to the main menu" },
                    { "it", "Invia tutti i file che vuoi caricare in questa cartella, scrivi qualsiasi cosa per tornare al menu"}
                };
                var text = new Language(dict);

                await sender.SendTextMessageAsync(e.Message.Chat.Id, text, ChatType.Private,
                    e.Message.From.LanguageCode,
                    ParseMode.Html, null, null);
            }
            else if (e.Message.Text.StartsWith("🔙"))
            {
                await GestisciCorsoAsync(e, sender);
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
                        { "it", "Cartella non trovata, usa il bottone per crearne una nuova"}
                    };
                    var text = new Language(dict);

                    await sender.SendTextMessageAsync(e.Message.Chat.Id, text, ChatType.Private,
                        e.Message.From.LanguageCode,
                        ParseMode.Html, null, null);
                }
                else
                {
                    UsersConversations[e.Message.From.Id].scesoDiUnLivello(e.Message.Text);
                    var replyKeyboard = Keyboards.GetPathsKeyboard(e.Message.From.Id);
                    await InviaCartellaAsync(e, replyKeyboard, sender);
                }
            }
        }

        private static bool VerificaSottoCartelle(MessageEventArgs e)
        {
            string[] sottoCartelle = Keyboards.GetDir(e.Message.From.Id);
            foreach (string a in sottoCartelle)
            {
                if (a.Split(@"/").Last().Equals(e.Message.Text.Split(@"/").Last())) return true;
            }

            return false;
        }

        private static async Task GeneraCartellaAsync(MessageEventArgs e, TelegramBotAbstract sender)
        {
            UsersConversations[e.Message.From.Id].setStato(stati.newCartella);
            var dict = new Dictionary<string, string>
            {
                { "en", "Write the name of the new folder" },
                { "it", "Scrivi il nome della cartella"}
            };
            var text = new Language(dict);

            await sender.SendTextMessageAsync(e.Message.Chat.Id, text, ChatType.Private,
                e.Message.From.LanguageCode,
                ParseMode.Html, null, null);
        }

        private static async Task GestisciStartAsync(MessageEventArgs e, TelegramBotAbstract telegramBotAbstract)
        {
            UsersConversations[e.Message.From.Id].setStato(stati.Scuola);
            var replyKeyboard = Keyboards.GetKeyboardSchools();
            var dict = new Dictionary<string, string>
            {
                { "en", "Choose a school" },
                { "it", "Scegli una scuola"}
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

        private static async Task GestisciCorsoAsync(MessageEventArgs e, TelegramBotAbstract sender)
        {
            UsersConversations[e.Message.From.Id].resetPercorso();
            if (e.Message.Text == null 
                || e.Message.Text.StartsWith("🔙") 
                || !Navigator.CorsoHandler(UsersConversations[e.Message.From.Id], e.Message.Text))
            {
                if (!Navigator.CorsoHandler(UsersConversations[e.Message.From.Id], e.Message.Text))
                {
                    var dict = new Dictionary<string, string>
                    {
                        { "en", "Unknown path. Going back to beginning. Use the Keyboard to navigate the folders." },
                        { "it", "Percorso sconosciuto, ritorno all'inizio. Usa il tastierino per navigare tra le cartelle."}
                    };
                    if (e.Message.Text.StartsWith("🔙"))
                    {
                        dict = new Dictionary<string, string>
                        {
                            { "en", "Going back to beginning." },
                            { "it", "Ritorno all'inizio."}
                        };
                    }
                    var text = new Language(dict);
                    await sender.SendTextMessageAsync(e.Message.Chat.Id, text, ChatType.Private,
                        e.Message.From.LanguageCode,
                        ParseMode.Html, null, null);
                }
                await GestisciStartAsync(e, sender);
                return;
            }

            try
            {
                var replyKeyboard = Keyboards.GetPathsKeyboard(e.Message.From.Id);
                if (replyKeyboard.Count == 0)
                {
                    throw new Exception("No paths for folder " + UsersConversations[e.Message.From.Id].getcorso());
                }
                await InviaCartellaAsync(e, replyKeyboard, sender);
            }
            catch (Exception ex)
            {
                var dict = new Dictionary<string, string>
                {
                    { "en", "The folder you have selected is not available" },
                    { "it", "La cartella non è disponibile."}
                };
                var text = new Language(dict);
                await sender.SendTextMessageAsync(e.Message.Chat.Id, text, ChatType.Private,
                    e.Message.From.LanguageCode,
                    ParseMode.Html, null, null);
                await BotUtils.NotifyUtil.NotifyOwners(ex, sender, e);
            }

            
        }

        private static async Task generaStartOnBackAndNull(MessageEventArgs e, TelegramBotAbstract telegramBotAbstract)
        {
            await GeneraStartAsync(e);
            await BotClient_OnMessageAsync2Async(telegramBotAbstract, e);
        }

        private static async Task InviaCartellaAsync(MessageEventArgs e,
            List<List<Language>> replyKeyboard, TelegramBotAbstract telegramBotAbstract)
        {
            if (replyKeyboard == null)
                return;
            
            var dict = new Dictionary<string, string>
            {
                { "en", "Choose a path" },
                { "it", "Seleziona un percorso"}
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

        private static async Task GestisciScuolaAsync(MessageEventArgs e, TelegramBotAbstract telegramBotAbstract)
        {
            
            if (e.Message.Text == null || !Navigator.ScuolaHandler(UsersConversations[e.Message.From.Id], e.Message.Text))
            {
                var dict = new Dictionary<string, string>
                {
                    { "en", "Unknown path. Going back to beginning. Use the Keyboard to navigate the folders." },
                    { "it", "Percorso sconosciuto. Ritorno al menu principale. Usa il tastierino per navigare tra le cartelle."}
                };
                var text = new Language(dict);
                await telegramBotAbstract.SendTextMessageAsync(e.Message.Chat.Id, text, ChatType.Private,
                    e.Message.From.LanguageCode,
                    ParseMode.Html, null, null);
                
                await generaStartOnBackAndNull(e, telegramBotAbstract);
                return;
            }
            
            var replyKeyboard = Keyboards.GetKeyboardCorsi(UsersConversations[e.Message.From.Id].getScuola());
            var replyMarkupObject = new ReplyMarkupObject(
                new ReplyMarkupOptions(
                    BotUtils.KeyboardMarkup.OptionsStringToKeyboard(replyKeyboard, e.Message.From.LanguageCode)
                )
            );
            var dict1 = new Dictionary<string, string>
            {
                { "en", "Chosen " + UsersConversations[e.Message.From.Id].getScuola() },
                { "it", "Selezionata " + UsersConversations[e.Message.From.Id].getScuola()}
            };
            var text1 = new Language(dict1);
            await telegramBotAbstract.SendTextMessageAsync(e.Message.Chat.Id, text1, ChatType.Private,
                e.Message.From.LanguageCode,
                ParseMode.Html, replyMarkupObject, null);

        }

        private static object ReconEnum(string text, Type type)
        {
            if (string.IsNullOrEmpty(text))
            {
                return null;
            }

            try
            {
                object r;
                Enum.TryParse(type, text, out r);
                return r;
            }
            catch
            {
                ;
            }

            return null;
        }

        private static string DoScript(PowerShell powershell, string script, bool debug, string separator = "\n")
        {
            return CommandDispatcher.DoScript(powershell, script, debug).Aggregate("", (current, s) => current + s + separator);
        }
    }
}
