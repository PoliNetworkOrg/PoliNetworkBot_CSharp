#region

using System;
using System.Collections.Generic;
using System.IO;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using PoliNetworkBot_CSharp.Code.Data;
using PoliNetworkBot_CSharp.Code.Data.Constants;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects.InfoBot;
using PoliNetworkBot_CSharp.Code.Utils.Logger;

#endregion

namespace PoliNetworkBot_CSharp.Code.Config;

[Serializable]
[JsonObject(MemberSerialization.Fields)]
public class DbConfig
{
    public string User;
    public string Password;
    public int Port;
    public string Database;
    public string Host;

    public static void InitializeDbConfig()
    {
        if (File.Exists(Paths.Info.DbConfig))
        {
            GlobalVariables.DbConfig = JsonConvert.DeserializeObject<DbConfig>(Paths.Info.DbConfig);
        }
        else
        {
            GlobalVariables.DbConfig = new DbConfig();
            var x = JsonConvert.SerializeObject(GlobalVariables.DbConfig);
            File.WriteAllText(Paths.Info.DbConfig, x);
            Logger.WriteLine("Initialized DBConfig to empty!", LogSeverityLevel.CRITICAL);
        }
        GlobalVariables.DbConnection = new MySqlConnection(GlobalVariables.DbConfig.GetConnectionString());
        
    }

    public string GetConnectionString()
    { /*todo*/
        return "server=localhost;user=bot;database=polinetwork;port=3306;password=temp";
    }
}