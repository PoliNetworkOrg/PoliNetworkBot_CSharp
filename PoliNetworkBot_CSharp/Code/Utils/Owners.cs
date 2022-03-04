#region

using System.Linq;
using PoliNetworkBot_CSharp.Code.Data;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils
{
    internal class Owners
    {
        internal static bool CheckIfOwner(long id)
        {
            return GlobalVariables.Owners.Any(x => x.Item1 == id);
        }
    }
}