using System.Threading.Tasks;
using Microsoft.VisualBasic.CompilerServices;

namespace PoliNetworkBot_CSharp.Test;

internal static class Test
{
    public static async Task MainTest()
    {
        const string path = @"C:\path\db.json";
        await Code.Utils.Restore.RestoreDbUtil.RestoreDbMethod(path);
    }
}