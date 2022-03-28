#region

using System;
using PoliNetworkBot_CSharp.Code.Enums;

#endregion

namespace PoliNetworkBot_CSharp.Test;

internal static class Test
{
    private static void Main2(string[] args)
    {
        var messageAllowedStatus = new MessageAllowedStatus(MessageAllowedStatusEnum.PENDING, new TimeSpan(4, 0, 0));
    }
}