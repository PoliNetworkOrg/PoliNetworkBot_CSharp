﻿#region

using System;
using System.Globalization;
using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Classes.ResponseWrappers;
using InstagramApiSharp.Helpers;
using PoliNetworkBot_CSharp.Code.IG.InstagramApiSharp.Classes.Models.Media;

#endregion

namespace InstagramApiSharp.Converters;

internal class InstaMediaAlbumConverter : IObjectConverter<InstaMedia, InstaMediaAlbumResponse>
{
    public InstaMediaAlbumResponse SourceObject { get; set; }

    public InstaMedia Convert()
    {
        if (SourceObject == null) throw new ArgumentNullException("Source object");
        var media = new InstaMedia
        {
            InstaIdentifier = SourceObject.Media.InstaIdentifier,
            Code = SourceObject.Media.Code,
            Pk = SourceObject.Media.Pk,
            ClientCacheKey = SourceObject.Media.ClientCacheKey,
            CommentsCount = SourceObject.Media.CommentsCount,
            DeviceTimeStamp = DateTimeHelper.UnixTimestampToDateTime(SourceObject.Media.DeviceTimeStampUnixLike),
            HasLiked = SourceObject.Media.HasLiked,
            PhotoOfYou = SourceObject.Media.PhotoOfYou,
            TrackingToken = SourceObject.Media.TrackingToken,
            TakenAt = DateTimeHelper.UnixTimestampToDateTime(SourceObject.Media.TakenAtUnixLike),
            Height = SourceObject.Media.Height,
            LikesCount = SourceObject.Media.LikesCount,
            MediaType = SourceObject.Media.MediaType,
            FilterType = SourceObject.Media.FilterType,
            Width = SourceObject.Media.Width,
            HasAudio = SourceObject.Media.HasAudio,
            ViewCount = int.Parse(SourceObject.Media.ViewCount.ToString(CultureInfo.InvariantCulture)),
            IsCommentsDisabled = SourceObject.Media.IsCommentsDisabled
        };
        if (SourceObject.Media.CarouselMedia != null)
            media.Carousel = ConvertersFabric.GetCarouselConverter(SourceObject.Media.CarouselMedia)
                .Convert();
        if (SourceObject.Media.User != null)
            media.User = ConvertersFabric.GetUserConverter(SourceObject.Media.User).Convert();
        if (SourceObject.Media.Caption != null)
            media.Caption = ConvertersFabric.GetCaptionConverter(SourceObject.Media.Caption).Convert();
        if (SourceObject.Media.NextMaxId != null) media.NextMaxId = SourceObject.Media.NextMaxId;
        if (SourceObject.Media.Likers is { Count: > 0 })
            foreach (var liker in SourceObject.Media.Likers)
                media.Likers.Add(ConvertersFabric.GetUserShortConverter(liker).Convert());
        if (SourceObject.Media.UserTagList?.In is { Count: > 0 })
            foreach (var tag in SourceObject.Media.UserTagList.In)
                media.UserTags.Add(ConvertersFabric.GetUserTagConverter(tag).Convert());
        if (SourceObject.Media.PreviewComments != null)
            foreach (var comment in SourceObject.Media.PreviewComments)
                media.PreviewComments.Add(ConvertersFabric.GetCommentConverter(comment).Convert());
        if (SourceObject.Media.Location != null)
            media.Location = ConvertersFabric.GetLocationConverter(SourceObject.Media.Location).Convert();
        if (SourceObject.Media.Images?.Candidates == null) return media;
        foreach (var image in SourceObject.Media.Images.Candidates)
            media.Images.Add(new InstaImage(image.Url, int.Parse(image.Width), int.Parse(image.Height)));
        if (SourceObject.Media.Videos == null) return media;
        foreach (var video in SourceObject.Media.Videos)
            media.Videos.Add(new InstaVideo(video.Url, int.Parse(video.Width), int.Parse(video.Height),
                video.Type));
        return media;
    }
}