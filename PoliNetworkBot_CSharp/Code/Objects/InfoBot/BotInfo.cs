#region

using PoliNetworkBot_CSharp.Code.Bots.Anon;
using PoliNetworkBot_CSharp.Code.Enums;
using System;
using System.Collections.Generic;
using Telegram.Bot.Types.Enums;

#endregion

namespace PoliNetworkBot_CSharp.Code.Objects.InfoBot
{
    [Serializable]
    public class BotInfo : BotInfoAbstract
    {
        internal new static bool SetIsBot(BotTypeApi v)
        {
            return false;
        }

        internal new static BotTypeApi? IsBot()
        {
            return BotTypeApi.REAL_BOT;
        }

        internal UpdateType[] GetAllowedUpdates()
        {
            switch (KeyValuePairs[ConstConfigBot.OnMessages])
            {
                case "a":
                    {
                        var x = new List<UpdateType>
                    {
                        UpdateType.CallbackQuery,
                        UpdateType.Message,
                        UpdateType.InlineQuery,
                        UpdateType.ChosenInlineResult
                    };
                        return x.ToArray();
                    }
            }

            return null;
        }

        internal bool Callback()
        {
            return KeyValuePairs[ConstConfigBot.OnMessages] switch
            {
                "a" => true,
                _ => false
            };
        }

        internal EventHandler<CallbackQueryEventArgs> GetCallbackEvent()
        {
            return KeyValuePairs[ConstConfigBot.OnMessages] switch
            {
                "a" => MainAnon.CallbackMethod,
                _ => null
            };
        }
    }
}