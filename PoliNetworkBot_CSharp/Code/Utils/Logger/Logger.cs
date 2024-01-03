#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
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
using PoliNetworkBot_CSharp.Code.Data.Variables;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Enums.Log;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.Exceptions;
using PoliNetworkBot_CSharp.Code.Objects.Log;
using PoliNetworkBot_CSharp.Code.Objects.TelegramBotAbstract;
using PoliNetworkBot_CSharp.Code.Utils.DatabaseUtils;
using PoliNetworkBot_CSharp.Code.Utils.Notify;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

#pragma warning disable CS0162

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils.Logger;

public static class Logger
{
    private const string DataLogPath = Paths.Data.Log;
    private const string LogSeparator = "#@#LOG ENTRY#@#";

    private const int ChunckSize = 100;
    private static readonly Dictionary<long, TelegramBotAbstract?> Subscribers = new();
    private static readonly BufferBlock<MessageQueue> Buffer = new();
    private static readonly object LogFileLock = new();
    private static int _linesCount;

    private static readonly object PrintLogLock = new();

    private static DateTime? _lastTimeSentAutomaticLog;
    public static bool EnableSelfManagedLogger { get; set; }

    internal static async Task MainMethodAsync()
    {
        if (!EnableSelfManagedLogger) return;
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
                {
<<<<<<< HEAD
                    var messageOptions = new TelegramBotAbstract.MessageOptions
=======
                    TelegramBotAbstract.MessageOptions messageOptions = new TelegramBotAbstract.MessageOptions()
>>>>>>> dev
                    {
                        ChatId = messageToBeSent.Key.Key,
                        Text = text,
                        ChatType = messageToBeSent.ChatType,
                        SplitMessage = true
                    };
                    await messageToBeSent.Key.Value.SendTextMessageAsync(messageOptions);
                }
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
            Console.WriteLine(logSeverityLevel + " | " + DateTime.Now.ToString("O") + " | " + log);
            if (!EnableSelfManagedLogger) return;
            var log1 = log.ToString();
            if (Directory.Exists("./data/") == false) Directory.CreateDirectory("./data/");

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

            try
            {
                const string? q1 =
                    "CALL `insert_log`(@id, @severity, @stacktrace, @content)";

                Database.ExecuteUnlogged(q1, GlobalVariables.DbConfig, new Dictionary<string, object?>
                {
                    { "@id", GlobalVariables.Bots?.Values.First()?.GetId() ?? 0 },
                    { "@severity", Enum.GetName(typeof(LogSeverityLevel), logSeverityLevel) },
                    { "@content", log1 },
                    { "@stacktrace", Environment.StackTrace }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            _linesCount++;
            SendLogIfOversize();
        }
        catch (Exception e)
        {
            CriticalError(e, log);
        }
    }

    private static void SendLogIfOversize()
    {
        var toSendLogOversize = GetIfLogOversize();
        if (!toSendLogOversize)
            return;

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
    }


    private static bool GetIfLogOversize()
    {
        var size = new FileInfo(DataLogPath).Length;
        return size > 50e6 || _linesCount > 500;
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
            await NotifyUtil.NotifyOwnerWithLog2(e, telegramBotAbstract, EventArgsContainer.Get(messageEventArgs));
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

                SendLogGeneral(sender, sendTo, messageEventArgs, path);

                SendLogDb(sender, sendTo);
            }
            catch (Exception? e)
            {
                NotifyUtil.NotifyOwnerWithLog2(e, sender, EventArgsContainer.Get(messageEventArgs)).Wait();
            }
        }
    }

    private static void SendLogDb(TelegramBotAbstract? sender, List<long?> sendTo)
    {
        var botId = sender?.GetId();
        if (botId == null) return;
        var count = GetCountLogDb(botId.Value);
        if (count == null)
            return;

        var size = count.Value / (decimal)ChunckSize;
        var howManyFiles = (int)Math.Ceiling(size);

        var lastIndex = howManyFiles - 1;

        //we will skip first files
        var startWithLastOnes = Math.Max(0, lastIndex - 3);

        for (var i = startWithLastOnes; i < howManyFiles; i++)
            try
            {
                SendLogDbChunk(sender, sendTo, botId.Value, i);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
    }

    private static void SendLogDbChunk(TelegramBotAbstract? sender, List<long?> sendTo, long botId, int chunckIndex)
    {
        var chunckSize = ChunckSize * chunckIndex;
        var q1 = "SELECT * " +
                 "FROM LogTable X " +
                 "WHERE X.bot_id = 0 OR X.bot_id = @bot_id " +
                 "ORDER BY X.when_insert ASC " +
                 $"LIMIT {ChunckSize},{chunckSize}";

        var dictionary = new Dictionary<string, object?>
        {
            { "@bot_id", botId }
        };
        var data = Database.ExecuteSelectUnlogged(q1, GlobalVariables.DbConfig,
            dictionary);
        var dbLogFileContent = GetFileContentFromDataTable(data);
        if (string.IsNullOrEmpty(dbLogFileContent)) return;
        dbLogFileContent = dbLogFileContent.Trim();
        if (string.IsNullOrEmpty(dbLogFileContent)) return;
        var textToSendBefore = "LOG (bot " + botId + ") from db:";
        const string applicationOctetStream = "application/octet-stream";
        var stringOrStream = new StringOrStream { StringValue = dbLogFileContent };
        LoggerSendFile.SendFiles(sendTo, stringOrStream, sender, textToSendBefore,
            applicationOctetStream, "log_db_" + chunckIndex + "_bot_" + botId + ".log");
    }

    private static int? GetCountLogDb(long botId)
    {
        const string q1 = "SELECT COUNT(*) " +
                          "FROM LogTable X " +
                          "WHERE X.bot_id = 0 OR X.bot_id = @bot_id " +
                          "ORDER BY X.when_insert ASC";

        var dictionary = new Dictionary<string, object?>
        {
            { "@bot_id", botId }
        };
        var data = Database.ExecuteSelectUnlogged(q1, GlobalVariables.DbConfig,
            dictionary);

        var o = Database.GetFirstValueFromDataTable(data);
        if (o == null)
            return null;

        try
        {
            return Convert.ToInt32(o);
        }
        catch
        {
            return null;
        }
    }

    private static void SendLogGeneral(TelegramBotAbstract? sender, List<long?> sendTo,
        MessageEventArgs? messageEventArgs,
        string path)
    {
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

        PrintLog3(text, sender, sendTo, messageEventArgs, path, "LOG general:", "log_general.log");
    }

    private static string? GetFileContentFromDataTable(DataTable? data)
    {
        if (data == null)
            return null;

        StringBuilder sb = new();
        foreach (DataRow row in data.Rows)
        {
            sb.Append(GetDbLogRow(row));
            sb.Append("\\n--------------------\\n");
        }

        return sb.ToString();
    }


    private static string GetDbLogRow(DataRow dr)
    {
        var r = "";
        var cols = dr.Table.Columns;
        for (var i = 0; i < cols.Count; i++)
        {
            var text = cols[i].Caption;
            var content = ContentToString(dr.ItemArray[i]) ?? "[null]";

            if (content == "")
                content = "[empty]";

            r += text + ": " + content + "\n";
        }

        return r.Trim();
    }

    private static string? ContentToString(object? drItem)
    {
        return drItem switch
        {
            null => null,
            DateTime dt => dt.ToString("dd/MM/yyyy HH:mm:ss zz"),
            _ => drItem.ToString()
        };
    }

    private static void PrintLog3(IReadOnlyCollection<string>? text, TelegramBotAbstract? sender, List<long?> sendTo,
        MessageEventArgs? messageEventArgs, string path, string textToSendBefore, string fileName)
    {
        if (DetectEmptyLog(text))
            EmptyLog(sender, sendTo, EventArgsContainer.Get(messageEventArgs));
        else
            PrintLog2(sendTo, sender, path, textToSendBefore, fileName);


        _linesCount = 0;
    }

    private static bool DetectEmptyLog(IReadOnlyCollection<string>? text)
    {
        return text == null || text.All(x => string.IsNullOrEmpty(x.Trim()));
    }


    private static void PrintLog2(List<long?> sendTo, TelegramBotAbstract? sender, string path, string textToSendBefore,
        string fileName)
    {
        string file;
        lock (LogFileLock)
        {
            file = File.ReadAllText(path);
        }

        file = string.Join("", file.Split(LogSeparator)); //remove "#@#LOG ENTRY#@#" from all the lines

        const string applicationOctetStream = "application/octet-stream";
        var stringOrStream = new StringOrStream { StringValue = file };
        var done = LoggerSendFile.SendFiles(sendTo, stringOrStream, sender, textToSendBefore, applicationOctetStream,
            fileName);
        if (done <= 0 || sendTo.Count <= 0)
            return;

        lock (LogFileLock)
        {
            File.WriteAllText(path, "\n");
        }
    }

    private static void EmptyLog(TelegramBotAbstract? sender, List<long?> sendTo, EventArgsContainer eventArgsContainer)
    {
        var text = new Language(new Dictionary<string, string?>
        {
            { "en", "No log available." }
        });

        foreach (var sendToSingle in sendTo)
            SendMessage.SendMessageInPrivate(sender, sendToSingle, "en",
                null, text, ParseMode.Html, null, InlineKeyboardMarkup.Empty(), eventArgsContainer).Wait();
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
        if (!EnableSelfManagedLogger) return;
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
        if (!EnableSelfManagedLogger) return;
        var sendTo = GetLogTo(e);
        PrintLog(sender, sendTo, e);
    }

    public static List<long?> GetLogTo(MessageEventArgs e)
    {
        return new List<long?> { e.Message.From?.Id, GroupsConstants.BackupGroup };
    }

    public static async Task<CommandExecutionState> SubscribeCommand(MessageEventArgs? e, TelegramBotAbstract? sender)
    {
        if (!EnableSelfManagedLogger) return await Task.FromResult(CommandExecutionState.ERROR_NOT_ENABLED);

        if (e == null)
            return CommandExecutionState.ERROR_DEFAULT;

        await Subscribe(e.Message.From?.Id, sender, e);
        return CommandExecutionState.UNMET_CONDITIONS;
    }

    public static Task<CommandExecutionState> UnsubscribeCommand(MessageEventArgs? e, TelegramBotAbstract? sender)
    {
        if (!EnableSelfManagedLogger) return Task.FromResult(CommandExecutionState.ERROR_NOT_ENABLED);

        if (e == null)
            return Task.FromResult(CommandExecutionState.ERROR_DEFAULT);
        Unsubscribe(e.Message.From?.Id);
        return Task.FromResult(CommandExecutionState.SUCCESSFUL);
    }

    public static Task<CommandExecutionState> GetLogCommand(MessageEventArgs? arg1, TelegramBotAbstract? arg2)
    {
        if (!EnableSelfManagedLogger) return Task.FromResult(CommandExecutionState.ERROR_NOT_ENABLED);
        if (arg1 != null) GetLog(arg2, arg1);
        return Task.FromResult(CommandExecutionState.SUCCESSFUL);
    }

    [SuppressMessage("ReSharper", "HeuristicUnreachableCode")]
    public static void WriteLogComplete(List<object?> values, TelegramBotAbstract? telegramBotAbstract, string caption)
    {
        switch (LogCostants.LogComplete)
        {
            case LogCompleteModeEnum.FILE:
            case LogCompleteModeEnum.GROUP:
            {
                var x = new LogObject(values);
                WriteLogComplete2(x, telegramBotAbstract, caption);
                break;
            }
            case LogCompleteModeEnum.NONE:
                break;

            default:
                return;
        }
    }

    [SuppressMessage("ReSharper", "HeuristicUnreachableCode")]
    private static void WriteLogComplete2(LogObject logObject, TelegramBotAbstract? telegramBotAbstract, string caption)
    {
        switch (LogCostants.LogComplete)
        {
            case LogCompleteModeEnum.FILE:
                WriteLogCompleteFile(logObject);
                break;
            case LogCompleteModeEnum.GROUP:
                NotifyLog.SendInGroup(logObject, telegramBotAbstract, caption);
                break;
            case LogCompleteModeEnum.NONE:
                break;

            default:
                return;
        }
    }


    private static void WriteLogCompleteFile(LogObject logObject)
    {
        WriteLine(logObject.GetStringToLog());
    }
}