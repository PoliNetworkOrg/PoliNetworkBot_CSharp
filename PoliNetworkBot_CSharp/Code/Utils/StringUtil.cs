using PoliNetworkBot_CSharp.Code.Objects;

namespace PoliNetworkBot_CSharp.Code.Utils
{
    public class StringUtil
    {
        public static string NotNull(Language caption, string lang)
        {
   
            if (caption == null)
                return "";
            else
                return caption.Select(lang);
        }
    }
}