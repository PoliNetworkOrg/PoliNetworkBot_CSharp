using System;
using Telegram.Bot.Types.Enums;

namespace PoliNetworkBot_CSharp.Code.Utils
{
    public static class ChatTypeUtil
    {
        public static ChatType? GetChatTypeFromString(object o)
        {
            if (o == null)
                return null;

            var s = o.ToString();

            if (string.IsNullOrEmpty(s))
                return null;

            try
            {
                Enum.TryParse(typeof(ChatType), s, result: out var r);

                switch (r)
                {
                    case null:
                        return null;
                    case ChatType r2:
                        return r2;
                }
            }
            catch
            {
                //ignored
            }

            return null;
        }
    }
}