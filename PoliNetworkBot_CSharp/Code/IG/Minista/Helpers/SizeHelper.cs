#region

using System.Collections.Generic;

#endregion

namespace Minista.Helpers
{
    /// <summary>
    ///     When I share a photo on Instagram, what's the image resolution?
    ///     When you share a photo on Instagram, regardless of whether you're using Instagram for iOS or Android,
    ///     we make sure to upload it at the best quality resolution possible (up to a width of 1080 pixels).
    ///     When you share a photo that has a width between 320 and 1080 pixels, we keep that photo at its original resolution
    ///     as long as the photo's aspect ratio is between 1.91:1 and 4:5 (a height between 566 and 1350 pixels with a width of
    ///     1080 pixels)
    ///     . If the aspect ratio of your photo isn't supported, it will be cropped to fit a supported ratio. If you share a
    ///     photo at
    ///     a lower resolution, we enlarge it to a width of 320 pixels. If you share a photo at a higher resolution, we size it
    ///     down to
    ///     a width of 1080 pixels.
    ///     If you want to make sure that your photo is shared with a width of 1080 pixels
    ///     Upload a photo with a width of at least 1080 pixels with an aspect ratio between 1.91:1 and 4:5.
    ///     Make sure you're using a phone with a high-quality camera as different phones have cameras of varying qualities.
    /// </summary>
    internal class SizeHelper
    {
        private const int MIN_WIDTH = 320;
        private const int MAX_WIDTH = 1080;
        private const int THUMBNAIL_SIZE = 292;

        public readonly List<int> Sizes = new()
        {
            1080,
            1024,
            960,
            800,
            768,
            720,
            640,
            612,
            600,
            546,
            500,
            468,
            480,
            400,
            320
        };
        //int CalculateHeight(double height)
        //{

        //}
    }
}