using PoliNetworkBot_CSharp.Code.Enums;

namespace PoliNetworkBot_CSharp.Code.Utils
{
    internal class SpamTypeUtil
    {
        internal static SpamType Merge(SpamType spamType1, SpamType spamType2)
        {
            switch (spamType1)
            {
                case SpamType.ALL_GOOD:
                {
                    return spamType2;
                }

                default:
                {
                    return spamType1;
                }
            }
        }
    }
}