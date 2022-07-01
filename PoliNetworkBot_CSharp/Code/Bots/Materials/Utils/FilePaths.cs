#region

using System;
using System.Collections.Generic;
using System.Data;
using Newtonsoft.Json;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Utils;
using PoliNetworkBot_CSharp.Code.Utils.Logger;

#endregion

namespace PoliNetworkBot_CSharp.Code.Bots.Materials.Utils;

[Serializable]
[JsonObject(MemberSerialization.Fields)]
public static class FilePaths
{
    public static bool TryGetValue(string fileAndGit, TelegramBotAbstract? telegramBotAbstract, out string? output)
    {
        const string? q1 = "SELECT location FROM FilePaths WHERE file_and_git = @v";
        var d = new Dictionary<string, object?>
        {
            { "@v", fileAndGit }
        };
        var data = Database.ExecuteSelect(q1, telegramBotAbstract?.DbConfig, d);
        var value = Database.GetFirstValueFromDataTable(data);
        if (value == null)
        {
            output = null;
            return false;
        }

        output = value.ToString();
        return true;
    }

    public static bool TryAdd(string fileUniqueAndGit, TelegramBotAbstract? telegramBotAbstract, string? file)
    {
        try
        {
            const string? q = "INSERT INTO FilePaths (file_and_git, location) VALUES (@file_and_git, @path)";
            var keyValuePairs = new Dictionary<string, object?>
            {
                { "@file_and_git", fileUniqueAndGit },
                { "@path", file }
            };
            Database.Execute(q, telegramBotAbstract?.DbConfig, keyValuePairs);
            return true;
        }
        catch (Exception? ex)
        {
            Logger.WriteLine(ex);
            throw new Exception("Cannot add file to materialdb!");
        }
    }
}