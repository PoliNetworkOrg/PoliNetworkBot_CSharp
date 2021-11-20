using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.TelegramMedia;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Telegram.Bot.Types.Enums;
using TeleSharp.TL;

namespace PoliNetworkBot_CSharp.Code.Utils
{
    public static class Logger
    {
        private static readonly Dictionary<long, TelegramBotAbstract> Subscribers = new();
        private static readonly BufferBlock<MessageQueue> Buffer = new();


        internal static async Task MainMethodAsync()
        {
            while (await Buffer.OutputAvailableAsync())
            {
                try
                {
                    var messageToBeSent = await Buffer.ReceiveAsync();
                    await messageToBeSent.key.Value.SendTextMessageAsync(messageToBeSent.key.Key, messageToBeSent.text, 
                        messageToBeSent.ChatType, messageToBeSent.Language, messageToBeSent.Parsemode, 
                        null, messageToBeSent.Username, null);
                }
                catch (Exception e)
                {
                    Console.WriteLine("ERROR IN LOGGER APPLICATION!");
                    Console.WriteLine(e);
                    Console.WriteLine("------");
                }
            }
        }

        public static void WriteLine(object log)
        {
            if (log == null || string.IsNullOrEmpty(log.ToString()))
                return;
            try
            {
                Console.WriteLine(log);
                foreach (KeyValuePair<long, TelegramBotAbstract> subscriber in Subscribers)
                {
                    var text = new Language(new Dictionary<string, string>
                    {
                        {"un", log.ToString()}
                    });

                    Buffer.Post(
                        new MessageQueue(
                                subscriber, text, ChatType.Private,
                                "dc", ParseMode.Default,
                                "un"
                            )
                        );
                }
                if (Directory.Exists("./data/") == false)
                {
                    Directory.CreateDirectory("./data/");
                }
                if (!File.Exists("./data/log.txt"))
                {
                    File.WriteAllText("./data/log.txt", "");
                }
                File.AppendAllLinesAsync("./data/log.txt", new[] { log.ToString() });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static async Task Subscribe(long fromId, TelegramBotAbstract telegramBotAbstract)
        {
            try
            {
                Subscribers.TryAdd(fromId, telegramBotAbstract);
            }
            catch (Exception e)
            {
                await NotifyUtil.NotifyOwners(e, telegramBotAbstract);
            }
        }

        public static void Unsubscribe(long fromId)
        {
            try
            {
                Subscribers.Remove(fromId);
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR IN LOGGER APPLICATION!");
                Console.WriteLine(e);
                Console.WriteLine("------");
            }
        }

        public static async Task PrintLog(TelegramBotAbstract sender, long sendTo)
        {
            try
            {
                const string path = "./data/log.txt";
                var file = await File.ReadAllBytesAsync(path);

                var stream = new MemoryStream(file);

                var text2 = new Language(new Dictionary<string, string>
                {
                    {"it", "LOG:"}
                });

                TLAbsInputPeer peer2 = new TLInputPeerUser { UserId = (int)sendTo };
                var peer = new Tuple<TLAbsInputPeer, long>(peer2, sendTo);

                await SendMessage.SendFileAsync(new TelegramFile(stream, "log.log",
                        null, "application/octet-stream"), peer,
                    text2, TextAsCaption.BEFORE_FILE,
                    sender, null, "it", null, true);

                await File.WriteAllTextAsync(path, "");
            }
            catch (Exception e)
            {
                await NotifyUtil.NotifyOwners(e, sender);
            }
        }

        public static class GroupsFixLog
        {
            private static List<string> _bothNull = new();
            private static Dictionary<long, KeyValuePair<string, Exception>> _newNull = new();
            private static List<string> _nameChange = new();
            private static List<string> _oldNull = new();
            private static int _countFixed = 0;
            private static int _countIgnored = 0;

            public static void SendLog(TelegramBotAbstract telegramBotAbstract)
            {
                var message = "Groups Fix Log:";
                message += "\n\n";
                message += "-NewTitle null && OldTitle null: [Group ID]";
                message += "\n";
                message = _bothNull.Aggregate(message, (current, @group) => current + @group + "\n");
                message += "\n\n";
                message += "-NewTitle null:";
                message += "\n";
                message += HandleNewTitleNull(_newNull);
                message += "\n\n";
                message += "-OldTitle null: [newTitle]";
                message += "\n";
                message = _oldNull.Aggregate(message, (current, newTitle) => current + newTitle + "\n");
                message += "\n\n";
                message += "-Name Changed: [oldTitle [->] newTitle]";
                message += "\n";
                message = _nameChange.Aggregate(message, (current, nameChange) => current + (nameChange + "\n"));
                message += "\n\n";
                message += "Fixed: " + _countFixed;
                message += "\nIgnored (already ok): " + _countIgnored;
                
                Logger.WriteLine(message);
                Utils.NotifyUtil.NotifyOwners(message, telegramBotAbstract);
                Reset();
            }

            private static void Reset()
            {
                _bothNull = new List<string>();
                _newNull = new Dictionary<long, KeyValuePair<string, Exception>>();
                _oldNull = new List<string>();
                _nameChange = new List<string>();
                _countFixed = 0;
                _countIgnored = 0;
            }

            private static string HandleNewTitleNull(Dictionary<long, KeyValuePair<string, Exception>> newNull)
            {
                var toReturn ="";
                var exceptionTypes = new List<Exception>();
                foreach (var (_, exception) in newNull.Values)
                {
                    if(!exceptionTypes.Contains(exception))
                        exceptionTypes.Add(exception);
                }

                foreach (var exceptionType in exceptionTypes)
                {
                    toReturn += exceptionType.GetType() + ":\n";
                    toReturn += "[GroupId , oldTitle]";
                    foreach (var groupId in newNull.Keys)
                    {
                        if(newNull[groupId].Value == exceptionType)
                            toReturn += groupId + " , " + groupId;
                    }
                }

                return toReturn;
            }

            public static void OldNullNewNull(long? item1Id, long tableRowId)
            {
                if (item1Id != null && item1Id != tableRowId)
                {
                    _bothNull.Add(tableRowId + " but GetChat responded with ID: " + item1Id);
                    return;
                }
                _bothNull.Add(tableRowId.ToString());
            }

            public static void NewNull(long id, string oldTitle, Exception exception)
            {
                _newNull.TryAdd(id, new KeyValuePair<string, Exception>(oldTitle, exception));
            }
            
            public static void OldNull(string newTitle)
            {
                _oldNull.Add(newTitle ?? "[NULL VALUE]");
            }

            public static void NameChange(string oldTitle, string newTitle)
            {
                _nameChange.Add(oldTitle + " [->] " + newTitle);
                _countFixed++;
            }
            
            
            public static void CountIgnored()
            {
                _countIgnored++;
            }
        }
    }
}