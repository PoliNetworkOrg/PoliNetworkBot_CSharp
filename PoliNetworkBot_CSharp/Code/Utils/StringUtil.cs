#region

using PoliNetworkBot_CSharp.Code.Objects;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils
{
    public class StringUtil
    {
        public static string NotNull(Language caption, string lang)
        {
            if (caption == null)
                return "";
            return caption.Select(lang);
        }
    }
}