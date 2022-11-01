#region

using System;
using System.IO;
using Newtonsoft.Json;
using PoliNetworkBot_CSharp.Code.Data;
using PoliNetworkBot_CSharp.Code.Data.Constants;
using PoliNetworkBot_CSharp.Code.Data.Variables;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Utils.Logger;

#endregion

namespace PoliNetworkBot_CSharp.Code.Config;

[Serializable]
[JsonObject(MemberSerialization.Fields)]
public class DbConfig
{
    public string? Database;
    public string? Host;
    public string? Password;
    public int Port;
    public string? User;

    public static void InitializeDbConfig()
    {
        if (File.Exists(Paths.Info.DbConfig))
        {
            try
            {
                var text = File.ReadAllText(Paths.Info.DbConfig);
                var deserializeObject = JsonConvert.DeserializeObject<DbConfig?>(text);
                if (deserializeObject != null)
                    GlobalVariables.DbConfig = new DbConfigConnection(deserializeObject);
            }
            catch (Exception? ex)
            {
                Logger.WriteLine(ex);
            }

            if (GlobalVariables.DbConfig == null) GenerateDbConfigEmpty();
        }
        else
        {
            GenerateDbConfigEmpty();
        }
    }

    private static void GenerateDbConfigEmpty()
    {
        GlobalVariables.DbConfig = new DbConfigConnection(null);
        var x = JsonConvert.SerializeObject(GlobalVariables.DbConfig.GetDbConfig());
        File.WriteAllText(Paths.Info.DbConfig, x);
        Logger.WriteLine("Initialized DBConfig to empty!", LogSeverityLevel.CRITICAL);
        throw new Exception("Database failed to initialize, we generated an empty file to fill");
    }

    public string GetConnectionString()
    {
        return "server='" + Host + "';user='" + User + "';database='" + Database + "';port=" + Port + ";password='" +
               Password + "'";
    }
}