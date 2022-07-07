#region

using PoliNetworkBot_CSharp.Code.Enums;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils;

internal static class SpamTypeUtil
{
    internal static SpamType? Merge(SpamType? spamType1, SpamType? spamType2)
    {
        if (spamType1 is null)
            return spamType2;

        if (spamType2 is null)
            return spamType1;

        return spamType1 switch
        {
            SpamType.ALL_GOOD => spamType2,
            _ => spamType1
        };
    }
}