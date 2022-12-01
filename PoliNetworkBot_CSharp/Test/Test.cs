using System.Threading.Tasks;
using Microsoft.VisualBasic.CompilerServices;

namespace PoliNetworkBot_CSharp.Test;

internal static class Test
{
    public static async Task MainTest()
    {
        const string path = @"C:\Users\eliam\OneDrive - Politecnico di Milano\ssh\db.json";
        await Code.Utils.Restore.RestoreDbUtil.RestoreDbMethod(path);
    }
}