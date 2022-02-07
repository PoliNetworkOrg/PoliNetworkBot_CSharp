﻿#region

using PoliNetworkBot_CSharp.Code.Enums;
using System;

#endregion

namespace PoliNetworkBot_CSharp.Code.Objects.InfoBot
{
    [Serializable]
    public class BotDisguisedAsUserBotInfo : BotInfoAbstract
    {
        public string SessionUserId { get; set; }

        internal new static bool SetIsBot(BotTypeApi v)
        {
            return false;
        }

        internal new static BotTypeApi? IsBot()
        {
            return BotTypeApi.DISGUISED_BOT;
        }

        internal long? GetUserId()
        {
            try
            {
                return Convert.ToInt64(KeyValuePairs[ConstConfigBot.UserId]);
            }
            catch
            {
                return null;
            }
        }

        internal long? GetApiId()
        {
            try
            {
                return Convert.ToInt64(KeyValuePairs[ConstConfigBot.ApiId]);
            }
            catch
            {
                return null;
            }
        }

        internal string GetApiHash()
        {
            try
            {
                return KeyValuePairs[ConstConfigBot.ApiHash].ToString();
            }
            catch
            {
                return null;
            }
        }

        private string GenerateSessionUserId_From_UserId()
        {
            var id = GetUserId();
            return id == null ? null : Convert.ToString(id.Value, 16).ToLower();
        }

        internal bool SetApiId(string v)
        {
            try
            {
                KeyValuePairs[ConstConfigBot.ApiId] = Convert.ToInt64(v);
                return true;
            }
            catch
            {
                ;
            }

            return false;
        }

        internal string GetSessionUserId()
        {
            if (string.IsNullOrEmpty(SessionUserId)) SessionUserId = GenerateSessionUserId_From_UserId();

            return SessionUserId;
        }

        internal void SetApiHash(string v)
        {
            KeyValuePairs[ConstConfigBot.ApiHash] = v;
        }

        internal bool SetUserId(string v)
        {
            try
            {
                KeyValuePairs[ConstConfigBot.UserId] = Convert.ToInt64(v);
                return true;
            }
            catch
            {
                ;
            }

            return false;
        }
    }
}