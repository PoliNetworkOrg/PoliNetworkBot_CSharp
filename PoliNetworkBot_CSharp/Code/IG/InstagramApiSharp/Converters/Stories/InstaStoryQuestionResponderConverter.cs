#region

using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Classes.ResponseWrappers;
using InstagramApiSharp.Helpers;
using System;

#endregion

namespace InstagramApiSharp.Converters
{
    internal class
        InstaStoryQuestionResponderConverter : IObjectConverter<InstaStoryQuestionResponder,
            InstaStoryQuestionResponderResponse>
    {
        public InstaStoryQuestionResponderResponse SourceObject { get; set; }

        public InstaStoryQuestionResponder Convert()
        {
            if (SourceObject == null) throw new ArgumentNullException("Source object");

            var responder = new InstaStoryQuestionResponder
            {
                HasSharedResponse = SourceObject.HasSharedResponse ?? false,
                Id = SourceObject.Id,
                ResponseText = SourceObject.Response,
                Time = (SourceObject.Ts ?? DateTime.UtcNow.ToUnixTime()).FromUnixTimeSeconds()
            };

            if (SourceObject.User != null)
                responder.User = ConvertersFabric.GetUserShortConverter(SourceObject.User).Convert();

            return responder;
        }
    }
}