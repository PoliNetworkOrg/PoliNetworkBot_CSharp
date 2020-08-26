namespace PoliNetworkBot_CSharp.Code.Objects
{
    public class BotDisguisedAsUserBotInfo : BotInfoAbstract
    {
        internal new bool SetIsBot(Enums.BotTypeApi v)
        {
            return false;
        }

        internal new Enums.BotTypeApi? IsBot()
        {
            return Enums.BotTypeApi.DISGUISED_BOT;
        }

        public int? GetUserId()
        {
            throw new System.NotImplementedException();
        }

        public int GetApiId()
        {
            throw new System.NotImplementedException();
        }

        public string GetApiHash()
        {
            throw new System.NotImplementedException();
        }

        public string GetSessionUserId()
        {
            throw new System.NotImplementedException();
        }
    }
}