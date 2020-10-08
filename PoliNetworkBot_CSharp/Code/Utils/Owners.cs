namespace PoliNetworkBot_CSharp.Code.Utils
{
    internal class Owners
    {
        internal static bool CheckIfOwner(long id)
        {
            foreach (var x in Code.Data.GlobalVariables.Owners)
            {
                if (x.Item1 == id)
                    return true;
            }

            return false;
        }
    }
}