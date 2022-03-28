namespace PoliNetworkBot_CSharp.Code.Bots.Materials;

public static class Paths
{
    public const string Db = "Data Source=data/db.db";

    public static class Info
    {
        public const string ConfigBotsInfo = "config/bots_info.txt";
        public const string ConfigUserBotsInfo = "config/userbots_info.txt";
        public const string ConfigBotDisguisedAsUserBotsInfo = "config/botdisguisedasuserbots_info.txt";
    }

    public static class Bin
    {
        public const string ConfigBot = "config/bots.bin";
        public const string ConfigUserbot = "config/userbots.bin";
        public const string ConfigBotDisguisedAsUserbot = "config/botdisguisedasuserbots.bin";
        public const string MessagesToDelete = "config/messagestodelete.bin";
    }

    public static class IG
    {
        public const string CREDENTIALS = "config/ig.txt";
    }
}