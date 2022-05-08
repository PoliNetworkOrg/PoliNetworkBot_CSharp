using System.Collections.Generic;

namespace PoliNetworkBot_CSharp.Code.Data.Constants;

public static class Channels
{
    public static class Assoc
    {
        public const long PoliAssociazioni = -1001314601927;
        public const long AssociazioniPolimi = -1001776530133;

        public static IEnumerable<long> GetChannels()
        {
            return new[] {PoliAssociazioni, AssociazioniPolimi};
        }
    }
}