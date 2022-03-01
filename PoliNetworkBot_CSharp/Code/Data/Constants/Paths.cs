namespace PoliNetworkBot_CSharp.Code.Data.Constants
{
    public static class Paths
    {
        public const string Db = "Data Source=data/db.db";

        public static class Info
        {
            public const string ConfigBotsInfo = "config/bots_info.json";
            public const string GitHubConfigInfo = "config/git_info.json";
            public const string ConfigUserBotsInfo = "config/userbots_info.json";
            public const string ConfigBotDisguisedAsUserBotsInfo = "config/botdisguisedasuserbots_info.json";
        }
        public static class Bin
        {
            public const string MessagesToDelete = "config/messagestodelete.bin";
        }

        public static class IG
        {
            public const string CREDENTIALS = "config/ig.txt";
        }
    }
}