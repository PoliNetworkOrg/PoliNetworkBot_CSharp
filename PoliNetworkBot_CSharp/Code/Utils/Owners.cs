using PoliNetworkBot_CSharp.Code.Data;

namespace PoliNetworkBot_CSharp.Code.Utils
{
    internal class Owners
    {
        internal static bool CheckIfOwner(long id)
        {
            foreach (var x in GlobalVariables.Owners)
                if (x.Item1 == id)
                    return true;

            return false;
        }
    }
}