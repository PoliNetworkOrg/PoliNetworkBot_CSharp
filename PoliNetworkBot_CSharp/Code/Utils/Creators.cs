#region

using System.Linq;
using PoliNetworkBot_CSharp.Code.Data.Variables;
using Telegram.Bot.Types;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils;

internal static class Creators
{
    internal static bool? CheckIfIsCreatorOrSubCreator(ChatMember? chatMember)
    {
        if (chatMember?.User == null)
            return null;

        if (string.IsNullOrEmpty(chatMember.User.Username))
            return false;

        return GlobalVariables.SubCreators != null && GlobalVariables.Creators != null &&
               (GlobalVariables.Creators.ToList().Any(x => x.Matches(chatMember.User))
                ||
                GlobalVariables.SubCreators.ToList().Any(x => x.Matches(chatMember.User)));
    }
}