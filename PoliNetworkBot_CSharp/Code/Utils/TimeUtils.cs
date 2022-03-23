#region

using PoliNetworkBot_CSharp.Code.Enums;
using System;
using System.Threading.Tasks;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils
{
    internal static class TimeUtils
    {
        public static async Task ExecuteAtLaterTime(TimeSpan time, Action task)
        {
            try
            {
                await Task.Delay(time);
                task.Invoke();
            }
            catch (Exception ex)
            {
                Logger.WriteLine(ex, LogSeverityLevel.ERROR);
            }
        }
    }
}