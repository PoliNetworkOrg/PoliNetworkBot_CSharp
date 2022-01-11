using JsonPolimi_Core_nf.Tipi;
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
        private static readonly Object Lock = new();

        internal static async Task MainMethodAsync()
        {
            while (await Buffer.OutputAvailableAsync())
            {
                try
                {
                    var messageToBeSent = Buffer.Receive();
                    string escaped = System.Web.HttpUtility.HtmlEncode(messageToBeSent.text);
                    var text = new Language(new Dictionary<string, string>
                    {
                        {"un", escaped}
                    });
                    await messageToBeSent.key.Value.SendTextMessageAsync(messageToBeSent.key.Key, text,
                        messageToBeSent.ChatType, "un", ParseMode.Html,
                        null, null, null, splitMessage: true);
                }
                catch (Exception e)
                {
                    Console.WriteLine("##########################");
                    Console.WriteLine("CRITICAL ERROR IN LOGGER APPLICATION! NOTIFY ASAP!");
                    Console.WriteLine(e);
                    Console.WriteLine("##########################");
                }
            }
        }

        public static void WriteLine(object log, LogSeverityLevel logSeverityLevel = LogSeverityLevel.INFO)
        {
            if (log == null || string.IsNullOrEmpty(log.ToString()))
                return;
            try
            {
                Console.WriteLine(logSeverityLevel + " | " + log);
                string log1 = log.ToString();
                foreach (KeyValuePair<long, TelegramBotAbstract> subscriber in Subscribers)
                {
                    Buffer.Post(
                        new MessageQueue(subscriber,
                                log1,
                                ChatType.Group,
                                ParseMode.Html)
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
                lock (Lock)
                {
                    File.AppendAllLinesAsync("./data/log.txt", new[] { DateTime.Now + " | " + logSeverityLevel + " | " + log1 });
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("##########################");
                Console.WriteLine("CRITICAL ERROR IN LOGGER APPLICATION! NOTIFY ASAP!");
                Console.WriteLine(e);
                Console.WriteLine("##########################");
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
                Console.WriteLine("##########################");
                Console.WriteLine("CRITICAL ERROR IN LOGGER APPLICATION! NOTIFY ASAP!");
                Console.WriteLine(e);
                Console.WriteLine("##########################");
            }
        }

        public static async Task PrintLog(TelegramBotAbstract sender, long sendTo)
        {
            try
            {
                const string path = "./data/log.txt";
                
                var file = await File.ReadAllBytesAsync(path);
                
                var stream = new MemoryStream(file);
                
                if (stream.Length==0)
                {
                    var text = new Language(new Dictionary<string, string>
                    {
                        {"en", "No log available."}
                    });

                    await SendMessage.SendMessageInPrivate(sender, sendTo, "en",
                        null, text, ParseMode.Html, null);
                    return;
                }
                 
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

            public static void SendLog(TelegramBotAbstract telegramBotAbstract,
                GroupsFixLogUpdatedEnum groupsFixLogUpdatedEnum = GroupsFixLogUpdatedEnum.ALL)
            {
                var message = "Groups Fix Log:";
                message += "\n\n";
                if (groupsFixLogUpdatedEnum == GroupsFixLogUpdatedEnum.ALL)
                {
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
                }
                message += "-Name Changed: [oldTitle [->] newTitle]";
                message += "\n";
                message = _nameChange.Aggregate(message, (current, nameChange) => current + (nameChange + "\n"));
                message += "\n\n";
                message += "Fixed: " + _countFixed;
                message += "\nIgnored (already ok): " + _countIgnored;

                string escaped = System.Web.HttpUtility.HtmlEncode(message);
                Logger.WriteLine(message);
                Utils.NotifyUtil.NotifyOwners(escaped, telegramBotAbstract);
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
                var toReturn = "";
                var exceptionTypes = new List<Type>();
                foreach (var (_, exception) in newNull.Values)
                {
                    if (!exceptionTypes.Contains(exception.GetType()))
                        exceptionTypes.Add(exception.GetType());
                }

                foreach (var exceptionType in exceptionTypes)
                {
                    toReturn += "#" + exceptionType + ":\n";
                    toReturn += "[GroupId , oldTitle]\n";
                    foreach (var group in newNull)
                    {
                        if (newNull[group.Key].Value.GetType() == exceptionType)
                            toReturn += group.Key + " , " + group.Value.Key + "\n";
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

        internal static void Log(EventoConLog eventoLog)
        {
            var log = eventoLog.GetLog();
            for (int i = 0; i < log.Item2; i++)
            {
                Logger.WriteLine(log.Item1[i]);
            }
        }
    }
}