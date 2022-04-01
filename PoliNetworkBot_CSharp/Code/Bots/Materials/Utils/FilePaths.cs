#region

using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using Newtonsoft.Json;
using PoliNetworkBot_CSharp.Code.Data.Constants;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Utils;
using PoliNetworkBot_CSharp.Code.Utils.Logger;

#endregion

namespace PoliNetworkBot_CSharp.Code.Bots.Materials.Utils;

[Serializable]
[JsonObject(MemberSerialization.Fields)]
public static class FilePaths
{
    private static SQLiteConnection _con = Initialize();

    /// <summary>
    /// Initialize connection
    /// </summary>
    /// <returns></returns>
    public static SQLiteConnection Initialize()
    {
        try
        {
            SQLiteConnection con;
            if (!File.Exists(Paths.Data.MaterialDbPath))
            {
                File.WriteAllText(Paths.Data.MaterialDbPath, "");
                con = new SQLiteConnection(Paths.MaterialDb);
                SqLite.Execute("CREATE TABLE FilePaths (" +
                               "id INTEGER PRIMARY KEY AUTOINCREMENT, " +
                               "file_and_git NVARCHAR(250)," +
                               "location NVARCHAR(250)" +
                               ") ", con);
                con.Open();
            }
            else
            {
                con = new SQLiteConnection(Paths.MaterialDb);
                con.Open();
            }
            
            return con;
        }
        catch (Exception ex)
        {
            Logger.WriteLine(ex, LogSeverityLevel.CRITICAL);
            throw new Exception("Error while initializing material db");
        }
    }

    public static bool TryGetValue(string fileAndGit, out string output)
    {
        const string q1 = "SELECT location FROM FilePaths WHERE file_and_git = @v";
        var d = new Dictionary<string, object>
        {
            { "@v", fileAndGit }
        };
        var data = SqLite.ExecuteSelect(q1, _con, d);
        var value = SqLite.GetFirstValueFromDataTable(data);
        if (value == null)
        {
            output = null;
            return false;
        }

        output = value.ToString();
        return true;
    }

    public static bool TryAdd(string fileUniqueAndGit, string file)
    {
        try
        {
            const string q = "INSERT INTO FilePaths (file_and_git, location) VALUES (@file_and_git, @path)";
            var keyValuePairs = new Dictionary<string, object>
            {
                {"@file_and_git", fileUniqueAndGit},
                {"@path", file}

            };
            SqLite.Execute(q, _con,  keyValuePairs);
            return true;
        }
        catch (Exception ex)
        {
            Logger.WriteLine(ex);
            throw new Exception("Cannot add file to materialdb!");
        }
    }
}