#region

using System.Collections.Generic;

#endregion

namespace InstagramApiSharp.Classes.Models
{
    public class InstaTranslate
    {
        public long Id { get; set; }

        public string Translation { get; set; }
    }

    public class InstaTranslateList : List<InstaTranslate>
    {
    }
}