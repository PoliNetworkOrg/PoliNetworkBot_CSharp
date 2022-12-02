using System.Threading.Tasks;

namespace PoliNetworkBot_CSharp.Test;

internal static class Test
{
    public static async Task MainTest()
    {
        const string path = @"C:\Users\User\Downloads\Telegram Desktop\db (129).json";
        await Code.Utils.Restore.RestoreDbUtil.RestoreDbMethod(path);
    }
}