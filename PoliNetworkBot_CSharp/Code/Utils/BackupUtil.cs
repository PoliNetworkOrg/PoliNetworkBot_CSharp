using PoliNetworkBot_CSharp.Code.Objects;

namespace PoliNetworkBot_CSharp.Code.Utils
{
    internal class BackupUtil
    {
        internal static void BackupBeforeReboot()
        {
            MessagesStore.BackupToFile();
            CallbackUtils.CallbackUtils.callBackDataFull.BackupToFile();
        }
    }
}