#region

using System.Linq;
using PoliNetworkBot_CSharp.Code.Data;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils;

internal static class Owners
{
    internal static bool CheckIfOwner(long? id)
    {
        return GlobalVariables.Owners != null && id != null && GlobalVariables.Owners.Any(x => x.Id == id);
    }
}