using System;
using System.IO;
using Newtonsoft.Json;
using PoliNetworkBot_CSharp.Code.Data.Constants;
using PoliNetworkBot_CSharp.Code.Data.Variables;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Utils.Logger;
using SampleNuGet.Objects;

namespace PoliNetworkBot_CSharp.Code.Config;

public class DbConfigClass
{
    
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
        Logger.WriteLine("Database failed to initialize, we generated an empty file to fill");
    }
}