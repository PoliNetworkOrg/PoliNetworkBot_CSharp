#region

using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.TelegramBotAbstract;
using SampleNuGet.Objects;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils;

internal static class Info
{
    internal static async Task<TargetUserObject> GetTargetUserIdAsync(TargetUserObject target,
        TelegramBotAbstract? telegramBotAbstract)
    {
        return await target.GetTargetUserId(telegramBotAbstract);
    }

    public static async Task<UserIdFound?> GetIdFromUsernameAsync(string? target,
        TelegramBotAbstract? telegramBotAbstract)
    {
        if (telegramBotAbstract != null) return await telegramBotAbstract.GetIdFromUsernameAsync(target);
        return null;
    }
}