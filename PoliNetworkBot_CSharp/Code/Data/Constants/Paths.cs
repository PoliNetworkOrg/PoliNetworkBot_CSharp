namespace PoliNetworkBot_CSharp.Code.Data.Constants;

public static class Paths
{
    public const string Db = "Data Source=" + Data.DbPath;
    public const string MaterialDb = "Data Source=" + Data.MaterialDbPath;
    

    public static class Data
    {
        public const string Log = "../data/log.txt";
        public const string PoliNetworkWebsiteData = "../data/polinetworkWebsiteData";
        public const string DbPath = "../data/db.db";
        public const string MaterialDbPath = "../data/matdb.db";
        public const string MessageStore = "../data/MessageStore.bin";
        public const string CallbackData = "../data/Callback.bin";
    }

    public static class Config
    {
        public const string PoliMaterialsConfig = "../config/materialbotconfig.json";
    }

    public static class Info
    {
        public const string ConfigBotsInfo = "../config/bots_info.json";
        public const string GitHubConfigInfo = "../config/git_info.json";
        public const string ConfigUserBotsInfo = "../config/userbots_info.json";
        public const string ConfigBotDisguisedAsUserBotsInfo = "../config/botdisguisedasuserbots_info.json";
        public const string DbConfig = "../config/dbconfig.json";
    }

    public static class Bin
    {
        public const string MessagesToDelete = "../config/messagestodelete.bin";
    }

    public static class IG
    {
        public const string CREDENTIALS = "../config/ig.txt";
    }

}