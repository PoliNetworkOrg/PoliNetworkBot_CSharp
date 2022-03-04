#region

using System;
using System.Linq;
using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Classes.ResponseWrappers;

#endregion

namespace InstagramApiSharp.Converters
{
    internal class
        InstaHighlightShortListConverter : IObjectConverter<InstaHighlightShortList, InstaHighlightShortListResponse>
    {
        public InstaHighlightShortListResponse SourceObject { get; set; }

        public InstaHighlightShortList Convert()
        {
            if (SourceObject == null) throw new ArgumentNullException("Source object");

            var highlight = new InstaHighlightShortList
            {
                MaxId = SourceObject.MaxId ?? string.Empty,
                MoreAvailable = SourceObject.MoreAvailable,
                ResultsCount = SourceObject.ResultsCount
            };
            if (SourceObject.Items != null && SourceObject.Items.Any())
                foreach (var item in SourceObject.Items)
                    try
                    {
                        highlight.Items.Add(ConvertersFabric.Instance.GetSingleHighlightShortConverter(item).Convert());
                    }
                    catch
                    {
                    }

            return highlight;
        }
    }
}