using System.IO;

namespace PoliNetworkBot_CSharp.Code.Utils
{
    internal class DirectoryUtils
    {
        internal static void CreateDirectory(string v)
        {
            try
            {
                Directory.CreateDirectory(v);
            }
            catch
            {
                ;
            }
        }
    }
}