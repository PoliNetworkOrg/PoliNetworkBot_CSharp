#region

using PoliNetworkBot_CSharp.Code.Objects;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils;

internal class BackupUtil
{
    internal static void BackupBeforeReboot()
    {
        MessagesStore.BackupToFile();
        CallbackUtils.CallbackUtils.CallBackDataFull.BackupToFile();
    }
}