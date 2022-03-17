using PoliNetworkBot_CSharp.Code.Enums;
using System;
using System.Threading.Tasks;

namespace PoliNetworkBot_CSharp.Test
{
    internal static class Test
    {
        private static async Task Main2(string[] args)
        {
            new MessageAllowedStatus(MessageAllowedStatusEnum.PENDING, new TimeSpan(4, 0, 0));
        }
    }
}