#region

using System.IO;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils
{
    internal static class DirectoryUtils
    {
        internal static void CreateDirectory(string v)
        {
            try
            {
                Directory.CreateDirectory(v);
            }
            catch
            {
                // ignored
            }
        }
    }
}