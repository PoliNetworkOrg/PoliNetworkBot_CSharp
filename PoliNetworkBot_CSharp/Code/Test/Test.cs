﻿using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Utils.Restore;

namespace PoliNetworkBot_CSharp.Code.Test;

internal static class Test
{
    public static async Task MainTest()
    {
        await RestoreDbUtil.RestoreDb();
    }
}