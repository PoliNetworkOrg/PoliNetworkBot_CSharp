using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Utils.Restore;

namespace PoliNetworkBot_CSharp.Test;

internal static class Test
{
    public static async Task MainTest()
    {
        const string? path = @"C:\Users\User\Downloads\Telegram Desktop\db (129).json";
        await  RestoreDbUtil.RestoreDb();
    }
}