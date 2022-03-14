#region

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Web;
using JsonPolimi_Core_nf.Tipi;
using PoliNetworkBot_CSharp.Code.Bots.Anon;
using PoliNetworkBot_CSharp.Code.Data.Constants;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.TelegramMedia;
using Telegram.Bot.Types.Enums;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils
{
    public static class Logger
    {
        private const string DataLogPath = Paths.Data.Log;
        private const string LogSeparator = "#@#LOG ENTRY#@#";
        private static readonly Dictionary<long, TelegramBotAbstract> Subscribers = new();
        private static readonly BufferBlock<MessageQueue> Buffer = new();
        private static readonly object Lock = new();

        private static readonly object PrintLogLock = new();

        internal static async Task MainMethodAsync()
        {
            while (await Buffer.OutputAvailableAsync())
                try
                {
                    var messageToBeSent = Buffer.Receive();
                    var escaped = HttpUtility.HtmlEncode(messageToBeSent.text);
                    var text = new Language(new Dictionary<string, string>
                    {
                        { "un", escaped }
                    });
                    await messageToBeSent.key.Value.SendTextMessageAsync(messageToBeSent.key.Key, text,
                        messageToBeSent.ChatType, "un", ParseMode.Html,
                        null, null, null, splitMessage: true);
                }
                catch (Exception e)
                {
                    CriticalError(e, null);
                }
        }

        private static void CriticalError(Exception e, object log)
        {
            Console.WriteLine("#############1#############");
            Console.WriteLine("CRITICAL ERROR IN LOGGER APPLICATION! NOTIFY ASAP!");
            Console.WriteLine(e);
            Console.WriteLine("#############2#############");
            if (log == null)
                Console.WriteLine("[null]");
            else
                Console.WriteLine(log);
            Console.WriteLine("#############3#############");
        }

        public static void WriteLine(object log, LogSeverityLevel logSeverityLevel = LogSeverityLevel.INFO)
        {
            if (log == null || string.IsNullOrEmpty(log.ToString()))
                return;
            try
            {
                Console.WriteLine(logSeverityLevel + " | " + log);
                var log1 = log.ToString();
                if (Directory.Exists("../data/") == false) Directory.CreateDirectory("../data/");

                if (!File.Exists(DataLogPath)) File.WriteAllText(DataLogPath, "");
                lock (Lock)
                {
                    File.AppendAllLinesAsync(DataLogPath, new[]
                    {
                        "#@#LOG ENTRY#@#" + GetTime()
                                          + " | " + logSeverityLevel + " | " + log1
                    });
                }

                foreach (var subscriber in Subscribers)
                    Buffer.Post(
                        new MessageQueue(subscriber,
                            log1,
                            ChatType.Group,
                            ParseMode.Html)
                    );
            }
            catch (Exception e)
            {
                CriticalError(e, log);
            }
        }

        private static string GetTime()
        {
            return DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
        }

        public static async Task Subscribe(long fromId, TelegramBotAbstract telegramBotAbstract,
            MessageEventArgs messageEventArgs)
        {
            try
            {
                Subscribers.TryAdd(fromId, telegramBotAbstract);
            }
            catch (Exception e)
            {
                await NotifyUtil.NotifyOwners(e, telegramBotAbstract, messageEventArgs);
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
                CriticalError(e, fromId);
            }
        }

        public static void PrintLog(TelegramBotAbstract sender, List<long> sendTo, MessageEventArgs messageEventArgs)
        {
            lock (PrintLogLock)
            {
                try
                {
                    const string path = Paths.Data.Log;

                    List<string> text = null;
                    try
                    {
                        text = File.ReadAllLines(path).ToList();
                    }
                    catch (Exception e)
                    {
                        WriteLine(e, LogSeverityLevel.CRITICAL);
                    }

                    if (text is { Count: <= 1 })
                        EmptyLog(sender, sendTo);
                    else
                        PrintLog2(sendTo, sender, path);
                }
                catch (Exception e)
                {
                    NotifyUtil.NotifyOwners(e, sender, messageEventArgs).Wait();
                }
            }
        }

        private static void PrintLog2(List<long> sendTo, TelegramBotAbstract sender, string path)
        {
            var file = File.ReadAllText(path);
            file = string.Join("", file.Split(LogSeparator)); //remove "#@#LOG ENTRY#@#" from all the lines
            var encoding = Encoding.UTF8;

            var text2 = new Language(new Dictionary<string, string>
            {
                { "it", "LOG:" }
            });

            foreach (var sendToSingle in sendTo)
            {
                var peer = new PeerAbstract(sendToSingle, ChatType.Private);

                var stream = new MemoryStream(encoding.GetBytes(file));

                SendMessage.SendFileAsync(new TelegramFile(stream, "log.log",
                        null, "application/octet-stream"), peer,
                    text2, TextAsCaption.BEFORE_FILE,
                    sender, null, "it", null, true).Wait();
            }

            File.WriteAllText(path, "\n");
        }

        private static void EmptyLog(TelegramBotAbstract sender, List<long> sendTo)
        {
            var text = new Language(new Dictionary<string, string>
            {
                { "en", "No log available." }
            });

            foreach (var sendToSingle in sendTo)
                SendMessage.SendMessageInPrivate(sender, sendToSingle, "en",
                    null, text, ParseMode.Html, null).Wait();
        }

        internal static void Log(EventoConLog eventoLog)
        {
            var (item1, item2) = eventoLog.GetLog();
            for (var i = 0; i < item2; i++) WriteLine(item1[i]);
        }

        public static bool ContainsCriticalErrors(out string s)
        {
            var log = File.ReadAllText(DataLogPath);
            var entries = log.Split(LogSeparator).ToList();
            s = "";
            var toReturn = false;
            foreach (var entry in entries)
                try
                {
                    var severityLevel = entry[(GetTime().Length + 3)..];
                    if (!severityLevel.StartsWith(LogSeverityLevel.NOTICE.ToString()) &&
                        !severityLevel.StartsWith(LogSeverityLevel.WARNING.ToString()) &&
                        !severityLevel.StartsWith(LogSeverityLevel.CRITICAL.ToString()) &&
                        !severityLevel.StartsWith(LogSeverityLevel.EMERGENCY.ToString())) continue;
                    s += entry;
                    toReturn = true;
                }
                catch (ArgumentOutOfRangeException)
                {
                }

            return toReturn;
        }

        public static class GroupsFixLog
        {
            private static List<string> _bothNull = new();
            private static Dictionary<long, KeyValuePair<string, Exception>> _newNull = new();
            private static List<string> _nameChange = new();
            private static List<string> _oldNull = new();
            private static int _countFixed;
            private static int _countIgnored;

            public static void SendLog(TelegramBotAbstract telegramBotAbstract, MessageEventArgs messageEventArgs,
                GroupsFixLogUpdatedEnum groupsFixLogUpdatedEnum = GroupsFixLogUpdatedEnum.ALL)
            {
                var message = "Groups Fix Log:";
                message += "\n\n";
                if (groupsFixLogUpdatedEnum == GroupsFixLogUpdatedEnum.ALL)
                {
                    message += "-NewTitle null && OldTitle null: [Group ID]";
                    message += "\n";
                    message = _bothNull.Aggregate(message, (current, group) => current + group + "\n");
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
                message = _nameChange.Aggregate(message, (current, nameChange) => current + nameChange + "\n");
                message += "\n\n";
                message += "Fixed: " + _countFixed;
                message += "\nIgnored (already ok): " + _countIgnored;

                var escaped = HttpUtility.HtmlEncode(message);
                WriteLine(message);
                NotifyUtil.NotifyOwners(escaped, telegramBotAbstract, messageEventArgs);
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
                foreach (var exception in newNull.Values.Cast<Exception>()
                             .Where(exception => !exceptionTypes.Contains(exception.GetType())))
                    exceptionTypes.Add(exception.GetType());

                foreach (var exceptionType in exceptionTypes)
                {
                    toReturn += "#" + exceptionType + ":\n";
                    toReturn += "[GroupId , oldTitle]\n";
                    toReturn = newNull.Where(group => newNull[group.Key].Value.GetType() == exceptionType)
                        .Aggregate(toReturn,
                            (current, group) => current + group.Key + " , " + group.Value.Key + "\n");
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