#region

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PoliNetworkBot_CSharp.Code.Objects.Exceptions;
using PoliNetworkBot_CSharp.Code.Objects.TelegramBotAbstract;
using PoliNetworkBot_CSharp.Code.Utils.Notify;
using SampleNuGet.Objects;
using SampleNuGet.Utils.DatabaseUtils;

#endregion

namespace PoliNetworkBot_CSharp.Code.Bots.Materials.Utils;

[Serializable]
[JsonObject(MemberSerialization.Fields)]
public static class FilePaths
{
    private static Dictionary<string, string?> _cache = new();

    public static bool TryGetValue(string? fileAndGit, TelegramBotAbstract? telegramBotAbstract, out string? output)
    {
        if (fileAndGit == null)
            throw new Exception("Exception in FilePaths!\nfileAndGit cannot be null here");
        if (_cache.TryGetValue(fileAndGit, out output)) return true;
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

    public static async Task<bool> TryAdd(string? fileUniqueAndGit, TelegramBotAbstract? telegramBotAbstract,
        string? file, EventArgsContainer eventArgsContainer)
    {
        try
        {
            if (fileUniqueAndGit == null)
                return false;
            _cache.Add(fileUniqueAndGit, file);
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
            await NotifyUtil.NotifyOwnersWithLog(ex, telegramBotAbstract, null, eventArgsContainer);
            return false;
        }
    }
}