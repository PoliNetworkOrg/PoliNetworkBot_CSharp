using System;
using Telegram.Bot.Types;

namespace PoliNetworkBot_CSharp.Code.Utils
{
    internal class Creators
    {
        internal static bool? CheckIfIsCreatorOrSubCreator(ChatMember chatMember)
        {
            if (chatMember == null)
                return null;

            if (chatMember.User == null)
                return null;

            if (string.IsNullOrEmpty(chatMember.User.Username))
                return false;

            if (Data.GlobalVariables.Creators.Contains(chatMember.User.Username.ToLower()))
                return true;

            if (Data.GlobalVariables.SubCreators.Contains(chatMember.User.Username.ToLower()))
                return true;

            return false;
        }
    }
}