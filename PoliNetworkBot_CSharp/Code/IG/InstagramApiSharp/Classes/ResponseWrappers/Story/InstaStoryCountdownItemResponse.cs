﻿#region

using Newtonsoft.Json;

#endregion

namespace InstagramApiSharp.Classes.ResponseWrappers
{
    public class InstaStoryCountdownItemResponse
    {
        [JsonProperty("x")] public float X { get; set; }

        [JsonProperty("y")] public float Y { get; set; }

        [JsonProperty("z")] public float Z { get; set; }

        [JsonProperty("width")] public float Width { get; set; }

        [JsonProperty("height")] public float Height { get; set; }

        [JsonProperty("rotation")] public float Rotation { get; set; }

        [JsonProperty("is_pinned")] public int IsPinned { get; set; }

        [JsonProperty("is_hidden")] public int IsHidden { get; set; }

        [JsonProperty("countdown_sticker")] public InstaStoryCountdownStickerItemResponse CountdownSticker { get; set; }
    }
}