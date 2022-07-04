#region

using System.Collections.Generic;

#endregion

namespace PoliNetworkBot_CSharp.Code.Data.Constants;

public static class Channels
{
    public static class Assoc
    {
        private const long PoliAssociazioni = -1001314601927;
        private const long AssociazioniPolimi = -1001776530133;

        public static IEnumerable<long> GetChannels()
        {
            return new[] { PoliAssociazioni, AssociazioniPolimi };
        }
    }
}