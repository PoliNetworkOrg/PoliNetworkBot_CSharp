using Newtonsoft.Json;
using PoliNetworkBot_CSharp.Code.Data.Constants;
using PoliNetworkBot_CSharp.Code.Objects;
using System;
using System.IO;

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