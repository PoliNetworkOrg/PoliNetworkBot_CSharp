#region

using PoliNetworkBot_CSharp.Code.Data;
using System.Linq;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils;

internal class Owners
{
    internal static bool CheckIfOwner(long id)
    {
        return GlobalVariables.Owners.Any(x => x.id == id);
    }
}