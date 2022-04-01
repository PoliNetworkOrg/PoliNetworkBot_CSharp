#region

using System.Linq;
using PoliNetworkBot_CSharp.Code.Data;
using Telegram.Bot.Types;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils;

internal class Creators
{
    internal static bool? CheckIfIsCreatorOrSubCreator(ChatMember chatMember)
    {
        if (chatMember?.User == null)
            return null;

        if (string.IsNullOrEmpty(chatMember.User.Username))
            return false;

        return GlobalVariables.Creators.ToList().Any(x => x.Matches(chatMember.User))
               ||
               GlobalVariables.SubCreators.ToList().Any(x => x.Matches(chatMember.User));
    }
}