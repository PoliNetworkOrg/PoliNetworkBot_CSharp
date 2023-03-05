#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Devart.Data.SQLite;
using MySql.Data.MySqlClient;
using PoliNetworkBot_CSharp.Code.Bots.Moderation.Dispatcher;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.DbObject;
using PoliNetworkBot_CSharp.Code.Objects.TelegramBotAbstract;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils.DatabaseUtils;

public static class Database
{
    public static int? Execute(string? query, DbConfigConnection? dbConfigConnection,
        Dictionary<string, object?>? args = null)
    {
        Logger.Logger.WriteLine(query, LogSeverityLevel.DATABASE_QUERY); //todo metti gli args

        return ExecuteSlave(query, dbConfigConnection, args);
    }

    public static int? ExecuteUnlogged(string? query, DbConfigConnection? dbConfigConnection,
        Dictionary<string, object?>? args = null)
    {
        return ExecuteSlave(query, dbConfigConnection, args);
    }

    private static int? ExecuteSlave(string? query, DbConfigConnection? dbConfigConnection,
        Dictionary<string, object?>? args = null)
    {
        if (dbConfigConnection == null)
            return 0;
        var connectionWithLock = dbConfigConnection.GetMySqlConnection();
        return connectionWithLock.ExecuteSlave(query, dbConfigConnection, args);
    }

    public static DataTable? ExecuteSelect(string? query, DbConfigConnection? dbConfigConnection,
        Dictionary<string, object?>? args = null)
    {
        Logger.Logger.WriteLine(query, LogSeverityLevel.DATABASE_QUERY); //todo metti gli args

        return ExecuteSelectSlave(query, dbConfigConnection, args);
    }


    public static DataTable? ExecuteSelectUnlogged(string? query, DbConfigConnection? dbConfigConnection,
        Dictionary<string, object?>? args = null)
    {
        return ExecuteSelectSlave(query, dbConfigConnection, args);
    }

    private static DataTable? ExecuteSelectSlave(string? query, DbConfigConnection? dbConfigConnection,
        Dictionary<string, object?>? args = null)
    {
        var connectionWithLock = dbConfigConnection?.GetMySqlConnection();
        return connectionWithLock?.ExecuteSelectSlave(query, dbConfigConnection, args);
    }



    internal static object? GetFirstValueFromDataTable(DataTable? dt)
    {
        if (dt == null)
            return null;

        try
        {
            return dt.Rows[0].ItemArray[0];
        }
        catch
        {
            return null;
        }
    }

    public static long? GetIntFromColumn(DataRow dr, string columnName)
    {
        var o = dr[columnName];
        if (o is null or DBNull)
            return null;

        try
        {
            return Convert.ToInt64(o);
        }
        catch
        {
            return null;
        }
    }

    public static async Task<CommandExecutionState> QueryBotExec(MessageEventArgs? e, TelegramBotAbstract? sender)
    {
        _ = await CommandDispatcher.QueryBot(true, e, sender);
        return CommandExecutionState.SUCCESSFUL;
    }

    public static async Task<CommandExecutionState> QueryBotSelect(MessageEventArgs? e, TelegramBotAbstract? sender)
    {
        _ = await CommandDispatcher.QueryBot(false, e, sender);
        return CommandExecutionState.SUCCESSFUL;
    }
}