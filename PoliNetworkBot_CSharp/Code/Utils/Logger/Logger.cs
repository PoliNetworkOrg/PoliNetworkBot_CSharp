#region

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Web;
using JsonPolimi_Core_nf.Tipi;
using PoliNetworkBot_CSharp.Code.Data.Constants;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.TelegramMedia;
using Telegram.Bot.Types.Enums;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils.Logger;

public static class Logger
{
    private const string DataLogPath = Paths.Data.Log;
    private const string LogSeparator = "#@#LOG ENTRY#@#";
    private static readonly Dictionary<long, TelegramBotAbstract?> Subscribers = new();
    private static readonly BufferBlock<MessageQueue> Buffer = new();
    private static readonly object LogFileLock = new();

    private static readonly object PrintLogLock = new();

    private static DateTime? _lastTimeSentAutomaticLog;

    internal static async Task MainMethodAsync()
    {
        while (await Buffer.OutputAvailableAsync())
            try
            {
                var messageToBeSent = Buffer.Receive();
                var escaped = HttpUtility.HtmlEncode(messageToBeSent.Text);
                var text = new Language(new Dictionary<string, string?>
                {
                    { "un", escaped }
                });
                if (messageToBeSent.Key.Value != null)
                    await messageToBeSent.Key.Value.SendTextMessageAsync(messageToBeSent.Key.Key, text,
                        messageToBeSent.ChatType, "un", ParseMode.Html,
                        null, null, null, splitMessage: true);
            }
            catch (Exception e)
            {
                CriticalError(e, null);
            }
    }

    private static void CriticalError(Exception e, object? log)
    {
        try
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
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    public static void WriteLine(object? log, LogSeverityLevel logSeverityLevel = LogSeverityLevel.INFO)
    {
        if (log == null || string.IsNullOrEmpty(log.ToString()))
            return;
        try
        {
            Console.WriteLine(logSeverityLevel + " | " + log);
            var log1 = log.ToString();
            if (Directory.Exists("../data/") == false) Directory.CreateDirectory("../data/");

            if (!File.Exists(DataLogPath)) File.WriteAllText(DataLogPath, "");

            lock (LogFileLock)
            {
                File.AppendAllLinesAsync(DataLogPath, new[]
                {
                    "#@#LOG ENTRY#@#" + GetTime()
                                      + " | " + logSeverityLevel + " | " + log1
                });
            }

            foreach (var subscriber in Subscribers.Where(subscriber => subscriber.Value != null))
                if (log1 != null)
                    Buffer.Post(
                        new MessageQueue(subscriber,
                            log1,
                            ChatType.Group,
                            ParseMode.Html)
                    );
            _ = SendLogIfOversize();
        }
        catch (Exception e)
        {
            CriticalError(e, log);
        }
    }

    private static Task SendLogIfOversize()
    {
        var size = new FileInfo(DataLogPath).Length;
        if (!(size > 50e6)) return Task.CompletedTask;
        try
        {
            var bots = BotUtil.GetBotFromType(BotTypeApi.REAL_BOT, BotStartMethods.Moderation.Item1);
            if (bots is { Count: < 1 }) throw new Exception("No REAL_BOT to send Log");

            if (bots != null) PrintLog(bots[0], new List<long?> { GroupsConstants.BackupGroup }, null);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        return Task.CompletedTask;
    }

    private static string GetTime()
    {
        return DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
    }

    private static async Task Subscribe(long? fromId, TelegramBotAbstract? telegramBotAbstract,
        MessageEventArgs? messageEventArgs)
    {
        if (fromId == null)
            return;

        try
        {
            Subscribers.TryAdd(fromId.Value, telegramBotAbstract);
        }
        catch (Exception? e)
        {
            await NotifyUtil.NotifyOwnerWithLog2(e, telegramBotAbstract, messageEventArgs);
        }
    }

    private static void Unsubscribe(long? fromId)
    {
        if (fromId == null)
            return;

        try
        {
            Subscribers.Remove(fromId.Value);
        }
        catch (Exception e)
        {
            CriticalError(e, fromId);
        }
    }

    private static void PrintLog(TelegramBotAbstract? sender, List<long?> sendTo, MessageEventArgs? messageEventArgs)
    {
        lock (PrintLogLock)
        {
            try
            {
                const string path = Paths.Data.Log;

                List<string>? text = null;
                try
                {
                    lock (LogFileLock)
                    {
                        text = File.ReadAllLines(path).ToList();
                    }
                }
                catch (Exception? e)
                {
                    WriteLine(e, LogSeverityLevel.CRITICAL);
                }

                if (text is { Count: <= 1 })
                    EmptyLog(sender, sendTo);
                else
                    PrintLog2(sendTo, sender, path);
            }
            catch (Exception? e)
            {
                NotifyUtil.NotifyOwnerWithLog2(e, sender, messageEventArgs).Wait();
            }
        }
    }

    private static void PrintLog2(List<long?> sendTo, TelegramBotAbstract? sender, string path)
    {
        string file;
        lock (LogFileLock)
        {
            file = File.ReadAllText(path);
        }

        file = string.Join("", file.Split(LogSeparator)); //remove "#@#LOG ENTRY#@#" from all the lines
        var encoding = Encoding.UTF8;

        var text2 = new Language(new Dictionary<string, string?>
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

        lock (LogFileLock)
        {
            File.WriteAllText(path, "\n");
        }
    }

    private static void EmptyLog(TelegramBotAbstract? sender, List<long?> sendTo)
    {
        var text = new Language(new Dictionary<string, string?>
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
        string log;
        lock (LogFileLock)
        {
            log = File.ReadAllText(DataLogPath);
        }

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

    public static void AutomaticLog()
    {
        while (true)
        {
            try
            {
                if (_lastTimeSentAutomaticLog == null || _lastTimeSentAutomaticLog.Value.AddDays(7) <= DateTime.Now)
                {
                    _lastTimeSentAutomaticLog = DateTime.Now;
                    AutomaticLog2();
                }
            }
            catch
            {
                // ignored
            }

            Thread.Sleep(1000 * 60 * 60 * 24);
        }
        // ReSharper disable once FunctionNeverReturns
    }

    private static void AutomaticLog2()
    {
        var bots = BotUtil.GetBotFromType(BotTypeApi.REAL_BOT, BotStartMethods.Moderation.Item1);
        if (bots == null || bots.Count == 0)
            return;
        PrintLog(bots.First(), new List<long?> { GroupsConstants.BackupGroup }, null);
    }

    public static void GetLog(TelegramBotAbstract? sender, MessageEventArgs e)
    {
        var sendTo = GetLogTo(e);
        PrintLog(sender, sendTo, e);
    }

    public static List<long?> GetLogTo(MessageEventArgs e)
    {
        return new List<long?> { e.Message?.From?.Id, GroupsConstants.BackupGroup };
    }

    public static async Task<bool> SubscribeCommand(MessageEventArgs? e, TelegramBotAbstract? sender)
    {
        if (e == null)
            return false;
        
        await Logger.Subscribe(e.Message.From?.Id, sender, e);
        return true;
    }

    public static Task<bool> UnsubscribeCommand(MessageEventArgs? e, TelegramBotAbstract? sender)
    {
        if (e == null) 
            return Task.FromResult(false);
        Logger.Unsubscribe(e.Message.From?.Id);
        return Task.FromResult(true);
    }

    public static Task GetLogCommand(MessageEventArgs? arg1, TelegramBotAbstract? arg2)
    {
        if (arg1 != null) Logger.GetLog(arg2, arg1);
        return Task.CompletedTask;
    }
}