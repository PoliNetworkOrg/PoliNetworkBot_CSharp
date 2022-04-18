#region

using System.IO;
using Newtonsoft.Json.Linq;
using PoliNetworkBot_CSharp.Code.Data.Constants;

#endregion

namespace PoliNetworkBot_CSharp.Code.Config;

internal static class GitHubConfig
{
    private const string Path = Paths.Info.GitHubConfigInfo;

    private static string GetInfo(string param)
    {
        var json = File.ReadAllText(Path);
        var data = JObject.Parse(json);
        return (string)data[param];
    }

    public static string GetUser()
    {
        return GetInfo("user");
    }

    public static object GetEmail()
    {
        return GetInfo("email");
    }

    public static object GetPassword()
    {
        return GetInfo("password");
    }

    public static object GetRepo()
    {
        return GetInfo("data_repo");
    }

    public static object GetRemote()
    {
        return GetInfo("remote_repo");
    }
}