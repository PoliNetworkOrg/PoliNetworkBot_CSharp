﻿#region

using System;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Media.Editing;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
//using Unigram.Common;

#endregion

namespace Minista.Helpers
{
    public static class ImageHelper
    {
        public static async Task<(int Width, int Height)> GetScaleAsync(StorageFile file, int requestedMinSide = 1280,
            Rect? crop = null)
        {
            var props = await file.Properties.GetImagePropertiesAsync();
            var width = props.Width;
            var height = props.Height;

            if (crop.HasValue)
            {
                width = (uint)crop.Value.Width;
                height = (uint)crop.Value.Height;
            }

            if (width > requestedMinSide || height > requestedMinSide)
            {
                var ratioX = (double)requestedMinSide / width;
                var ratioY = (double)requestedMinSide / height;
                var ratio = Math.Min(ratioX, ratioY);

                return ((int)(width * ratio), (int)(height * ratio));
            }

            return ((int)width, (int)height);
        }


        /// <summary>
        ///     Resizes and crops source file image so that resized image width/height are not larger than
        ///     <param name="requestedMinSide"></param>
        /// </summary>
        /// <param name="sourceFile">Source StorageFile</param>
        /// <param name="resizedImageFile">Target StorageFile</param>
        /// <param name="requestedMinSide">Max width/height of the output image</param>
        /// <param name="quality">JPEG compression quality (0.77 for pictures, 0.87 for thumbnails)</param>
        /// <returns></returns>
        public static async Task<StorageFile> ScaleJpegAsync(StorageFile sourceFile, StorageFile resizedImageFile,
            int requestedMinSide = 1280, double quality = 0.77)
        {
            using (var imageStream = await sourceFile.OpenReadAsync())
            {
                var decoder = await BitmapDecoder.CreateAsync(imageStream);
                //if (decoder.FrameCount > 1)
                //{
                //    throw new InvalidCastException();
                //}

                var originalPixelWidth = decoder.PixelWidth;
                var originalPixelHeight = decoder.PixelHeight;

                using (var resizedStream = await resizedImageFile.OpenAsync(FileAccessMode.ReadWrite))
                {
                    BitmapTransform transform;

                    if (decoder.PixelWidth > requestedMinSide || decoder.PixelHeight > requestedMinSide)
                    {
                        var ratioX = (double)requestedMinSide / originalPixelWidth;
                        var ratioY = (double)requestedMinSide / originalPixelHeight;
                        var ratio = Math.Min(ratioX, ratioY);

                        var width = (uint)(originalPixelWidth * ratio);
                        var height = (uint)(originalPixelHeight * ratio);

                        transform = new BitmapTransform
                        {
                            ScaledWidth = width,
                            ScaledHeight = height,
                            InterpolationMode = BitmapInterpolationMode.Linear
                        };
                    }
                    else
                    {
                        transform = new BitmapTransform();
                    }

                    var pixelData = await decoder.GetSoftwareBitmapAsync(decoder.BitmapPixelFormat,
                        decoder.BitmapAlphaMode, transform, ExifOrientationMode.RespectExifOrientation,
                        ColorManagementMode.DoNotColorManage);

                    var propertySet = new BitmapPropertySet();
                    var qualityValue = new BitmapTypedValue(quality, PropertyType.Single);
                    propertySet.Add("ImageQuality", qualityValue);

                    var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, resizedStream);
                    encoder.SetSoftwareBitmap(pixelData);
                    await encoder.FlushAsync();
                }
            }

            return resizedImageFile;
        }

        public static async Task<StorageFile> TranscodeAsync(IRandomAccessStream imageStream,
            StorageFile resizedImageFile, Guid encoderId)
        {
            var decoder = await BitmapDecoder.CreateAsync(imageStream);
            //if (decoder.FrameCount > 1)
            //{
            //    throw new InvalidCastException();
            //}

            var originalPixelWidth = decoder.PixelWidth;
            var originalPixelHeight = decoder.PixelHeight;

            using (var resizedStream = await resizedImageFile.OpenAsync(FileAccessMode.ReadWrite))
            {
                var pixelData = await decoder.GetSoftwareBitmapAsync(decoder.BitmapPixelFormat, decoder.BitmapAlphaMode,
                    new BitmapTransform(), ExifOrientationMode.RespectExifOrientation,
                    ColorManagementMode.DoNotColorManage);

                var encoder = await BitmapEncoder.CreateAsync(encoderId, resizedStream);
                encoder.SetSoftwareBitmap(pixelData);
                await encoder.FlushAsync();
            }

            return resizedImageFile;
        }

        public static async Task<ImageSource> GetPreviewBitmapAsync(StorageFile sourceFile, int requestedMinSide = 1280)
        {
            try
            {
                if (sourceFile.ContentType.Equals("video/mp4"))
                {
                    var props = await sourceFile.Properties.GetVideoPropertiesAsync();
                    var composition = new MediaComposition();
                    var clip = await MediaClip.CreateFromFileAsync(sourceFile);

                    composition.Clips.Add(clip);

                    using (var imageStream = await composition.GetThumbnailAsync(TimeSpan.Zero, (int)props.GetWidth(),
                               (int)props.GetHeight(), VideoFramePrecision.NearestKeyFrame))
                    {
                        return await GetPreviewBitmapAsync(imageStream, requestedMinSide);
                    }
                }

                using (var imageStream = await sourceFile.OpenReadAsync())
                {
                    return await GetPreviewBitmapAsync(imageStream, requestedMinSide);
                }
            }
            catch
            {
                return null;
            }
        }

        public static async Task<ImageSource> GetPreviewBitmapAsync(IRandomAccessStream imageStream,
            int requestedMinSide = 1280)
        {
            var decoder = await BitmapDecoder.CreateAsync(imageStream);
            if (decoder.BitmapPixelFormat == BitmapPixelFormat.Bgra8)
            {
                var originalPixelWidth = decoder.PixelWidth;
                var originalPixelHeight = decoder.PixelHeight;

                BitmapTransform transform;

                if (decoder.PixelWidth > requestedMinSide || decoder.PixelHeight > requestedMinSide)
                {
                    var ratioX = (double)requestedMinSide / originalPixelWidth;
                    var ratioY = (double)requestedMinSide / originalPixelHeight;
                    var ratio = Math.Min(ratioX, ratioY);

                    var width = (uint)(originalPixelWidth * ratio);
                    var height = (uint)(originalPixelHeight * ratio);

                    transform = new BitmapTransform
                    {
                        ScaledWidth = width,
                        ScaledHeight = height,
                        InterpolationMode = BitmapInterpolationMode.Linear
                    };
                }
                else
                {
                    transform = new BitmapTransform();
                }

                var bitmap = await decoder.GetSoftwareBitmapAsync(decoder.BitmapPixelFormat,
                    BitmapAlphaMode.Premultiplied, transform, ExifOrientationMode.RespectExifOrientation,
                    ColorManagementMode.DoNotColorManage);
                var bitmapImage = new SoftwareBitmapSource();
                await bitmapImage.SetBitmapAsync(bitmap);

                return bitmapImage;
            }
            else
            {
                var bitmap = new BitmapImage();
                await bitmap.SetSourceAsync(imageStream);

                return bitmap;
            }
        }

        public static async Task<Color[]> GetAccentAsync(StorageFile sourceFile)
        {
            var result = new Color[2];

            try
            {
                using (var imageStream = await sourceFile.OpenReadAsync())
                {
                    var decoder = await BitmapDecoder.CreateAsync(imageStream);
                    var transform = new BitmapTransform
                    {
                        ScaledWidth = 1,
                        ScaledHeight = 1
                    };

                    var pixelData = await decoder.GetPixelDataAsync(decoder.BitmapPixelFormat, decoder.BitmapAlphaMode,
                        transform, ExifOrientationMode.RespectExifOrientation, ColorManagementMode.DoNotColorManage);
                    var detach = pixelData.DetachPixelData();

                    var hsv = ColorsHelper.RgbToHsv(detach[2], detach[1], detach[0]);
                    hsv[1] = Math.Min(1.0, hsv[1] + 0.05 + 0.1 * (1.0 - hsv[1]));
                    hsv[2] = Math.Max(0, hsv[2] * 0.65);

                    var rgb = ColorsHelper.HsvToRgb(hsv[0], hsv[1], hsv[2]);
                    result[0] = Color.FromArgb(0x66, rgb[0], rgb[1], rgb[2]);
                    result[1] = Color.FromArgb(0x88, rgb[0], rgb[1], rgb[2]);
                }
            }
            catch
            {
            }

            if (result[0] == null) result[0] = Color.FromArgb(0x66, 0x7A, 0x8A, 0x96);
            if (result[1] == null) result[1] = Color.FromArgb(0x88, 0x7A, 0x8A, 0x96);

            return result;
        }

        public static async Task<StorageFile> CropAsync(StorageFile sourceFile, StorageFile file, Rect cropRectangle,
            int min = 1280, int max = 0, double quality = 0.77)
        {
            if (file == null)
                file = await ApplicationData.Current.TemporaryFolder.CreateFileAsync("crop.jpg",
                    CreationCollisionOption.ReplaceExisting);

            using (var fileStream = await OpenReadAsync(sourceFile))
            using (var outputStream = await file.OpenAsync(FileAccessMode.ReadWrite))
            {
                var decoder = await BitmapDecoder.CreateAsync(fileStream);
                var cropWidth = (double)decoder.PixelWidth;
                var cropHeight = (double)decoder.PixelHeight;

                if (decoder.PixelWidth > 1280 || decoder.PixelHeight > 1280)
                {
                    var ratioX = 1280 / cropWidth;
                    var ratioY = 1280 / cropHeight;
                    var ratio = Math.Min(ratioX, ratioY);

                    cropWidth = cropWidth * ratio;
                    cropHeight = cropHeight * ratio;
                }

                var (scaledCrop, scaledSize) = Scale(cropRectangle, new Size(cropWidth, cropHeight),
                    new Size(decoder.PixelWidth, decoder.PixelHeight), min, max);

                var bounds = new BitmapBounds();
                bounds.X = (uint)scaledCrop.X;
                bounds.Y = (uint)scaledCrop.Y;
                bounds.Width = (uint)scaledCrop.Width;
                bounds.Height = (uint)scaledCrop.Height;

                var transform = new BitmapTransform();
                transform.ScaledWidth = (uint)scaledSize.Width;
                transform.ScaledHeight = (uint)scaledSize.Height;
                transform.Bounds = bounds;
                transform.InterpolationMode = BitmapInterpolationMode.Linear;

                var pixelData = await decoder.GetSoftwareBitmapAsync(decoder.BitmapPixelFormat, decoder.BitmapAlphaMode,
                    transform, ExifOrientationMode.RespectExifOrientation, ColorManagementMode.DoNotColorManage);

                var propertySet = new BitmapPropertySet();
                var qualityValue = new BitmapTypedValue(quality, PropertyType.Single);
                propertySet.Add("ImageQuality", qualityValue);

                var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, outputStream);
                encoder.SetSoftwareBitmap(pixelData);
                await encoder.FlushAsync();
            }

            return file;
        }

        public static (Rect, Size) Scale(Rect rect, Size start, Size size, int min, int max)
        {
            var width = rect.Width * size.Width / start.Width;
            var height = rect.Height * size.Height / start.Height;

            if (width > min || height > min)
            {
                var ratioX = min / width;
                var ratioY = min / height;
                var ratio = Math.Min(ratioX, ratioY);

                width = width * ratio;
                height = height * ratio;
            }

            if (width < max || height < max)
            {
                var ratioX = max / width;
                var ratioY = max / height;
                var ratio = Math.Min(ratioX, ratioY);

                width = width * ratio;
                height = height * ratio;
            }

            var ratioW = start.Width * width / rect.Width;
            var ratioH = start.Height * height / rect.Height;

            var x = rect.X * ratioW / start.Width;
            var y = rect.Y * ratioH / start.Height;
            var w = rect.Width * ratioW / start.Width;
            var h = rect.Height * ratioH / start.Height;

            return (new Rect(x, y, w, h), new Size(ratioW, ratioH));
        }

        public static async Task<ImageSource> CropAndPreviewAsync(StorageFile sourceFile, Rect cropRectangle)
        {
            if (sourceFile.ContentType.Equals("video/mp4"))
            {
                var props = await sourceFile.Properties.GetVideoPropertiesAsync();
                var composition = new MediaComposition();
                var clip = await MediaClip.CreateFromFileAsync(sourceFile);

                composition.Clips.Add(clip);

                using (var imageStream = await composition.GetThumbnailAsync(TimeSpan.Zero, (int)props.GetWidth(),
                           (int)props.GetHeight(), VideoFramePrecision.NearestKeyFrame))
                {
                    return await CropAndPreviewAsync(imageStream, cropRectangle);
                }
            }

            using (var imageStream = await sourceFile.OpenReadAsync())
            {
                return await CropAndPreviewAsync(imageStream, cropRectangle);
            }
        }

        public static async Task<ImageSource> CropAndPreviewAsync(IRandomAccessStream imageStream, Rect cropRectangle)
        {
            var decoder = await BitmapDecoder.CreateAsync(imageStream);
            var bounds = new BitmapBounds();
            bounds.X = (uint)cropRectangle.X;
            bounds.Y = (uint)cropRectangle.Y;
            bounds.Width = (uint)cropRectangle.Width;
            bounds.Height = (uint)cropRectangle.Height;

            var transform = ComputeScalingTransformForSourceImage(decoder);
            transform.Bounds = bounds;

            var pixelData = await decoder.GetSoftwareBitmapAsync(decoder.BitmapPixelFormat, decoder.BitmapAlphaMode,
                transform, ExifOrientationMode.RespectExifOrientation, ColorManagementMode.DoNotColorManage);

            var propertySet = new BitmapPropertySet();
            var qualityValue = new BitmapTypedValue(0.77, PropertyType.Single);
            propertySet.Add("ImageQuality", qualityValue);

            var bitmap = await decoder.GetSoftwareBitmapAsync(decoder.BitmapPixelFormat, BitmapAlphaMode.Premultiplied,
                transform, ExifOrientationMode.RespectExifOrientation, ColorManagementMode.DoNotColorManage);
            var bitmapImage = new SoftwareBitmapSource();
            await bitmapImage.SetBitmapAsync(bitmap);

            return bitmapImage;
        }

        public static async Task<IRandomAccessStream> OpenReadAsync(StorageFile sourceFile)
        {
            if (sourceFile.ContentType.Equals("video/mp4"))
            {
                var props = await sourceFile.Properties.GetVideoPropertiesAsync();
                var composition = new MediaComposition();
                var clip = await MediaClip.CreateFromFileAsync(sourceFile);

                composition.Clips.Add(clip);

                return await composition.GetThumbnailAsync(TimeSpan.Zero, (int)props.GetWidth(), (int)props.GetHeight(),
                    VideoFramePrecision.NearestKeyFrame);
            }

            return await sourceFile.OpenReadAsync();
        }

        public static BitmapTransform ComputeScalingTransformForSourceImage(BitmapDecoder sourceDecoder)
        {
            var transform = new BitmapTransform();

            if (sourceDecoder.PixelWidth > 1280 || sourceDecoder.PixelHeight > 1280)
            {
                var ratioX = (double)1280 / sourceDecoder.PixelWidth;
                var ratioY = (double)1280 / sourceDecoder.PixelHeight;
                var ratio = Math.Min(ratioX, ratioY);

                transform.ScaledWidth = (uint)(sourceDecoder.PixelWidth * ratio);
                transform.ScaledHeight = (uint)(sourceDecoder.PixelHeight * ratio);
            }

            return transform;
        }

        //public static async Task<TLPhotoSizeBase> GetVideoThumbnailAsync(StorageFile file, VideoProperties props, VideoTransformEffectDefinition effect)
        //{
        //    double originalWidth = props.GetWidth();
        //    double originalHeight = props.GetHeight();

        //    if (effect != null && !effect.CropRectangle.IsEmpty)
        //    {
        //        file = await CropAsync(file, effect.CropRectangle);
        //        originalWidth = effect.CropRectangle.Width;
        //        originalHeight = effect.CropRectangle.Height;
        //    }

        //    TLPhotoSizeBase result;
        //    var desiredName = string.Format("{0}_{1}_{2}.jpg", "fileLocation.VolumeId", "fileLocation.LocalId", "fileLocation.Secret");
        //    var desiredFile = await FileUtils.CreateTempFileAsync(desiredName);

        //    using (var fileStream = await OpenReadAsync(file))
        //    using (var outputStream = await desiredFile.OpenAsync(FileAccessMode.ReadWrite))
        //    {
        //        var decoder = await BitmapDecoder.CreateAsync(fileStream);

        //        double ratioX = (double)90 / originalWidth;
        //        double ratioY = (double)90 / originalHeight;
        //        double ratio = Math.Min(ratioX, ratioY);

        //        uint width = (uint)(originalWidth * ratio);
        //        uint height = (uint)(originalHeight * ratio);

        //        var transform = new BitmapTransform();
        //        transform.ScaledWidth = width;
        //        transform.ScaledHeight = height;
        //        transform.InterpolationMode = BitmapInterpolationMode.Linear;

        //        if (effect != null)
        //        {
        //            transform.Flip = effect.Mirror == MediaMirroringOptions.Horizontal ? BitmapFlip.Horizontal : BitmapFlip.None;
        //        }

        //        var pixelData = await decoder.GetSoftwareBitmapAsync(decoder.BitmapPixelFormat, decoder.BitmapAlphaMode, transform, ExifOrientationMode.RespectExifOrientation, ColorManagementMode.DoNotColorManage);

        //        var propertySet = new BitmapPropertySet();
        //        var qualityValue = new BitmapTypedValue(0.77, PropertyType.Single);
        //        propertySet.Add("ImageQuality", qualityValue);

        //        var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, outputStream);
        //        encoder.SetSoftwareBitmap(pixelData);
        //        await encoder.FlushAsync();

        //        result = new TLPhotoSize
        //        {
        //            W = (int)width,
        //            H = (int)height,
        //            Size = (int)outputStream.Size,
        //            Type = string.Empty
        //        };
        //    }

        //    return result;
        //}

        //public static async Task<TLPhotoSizeBase> GetFileThumbnailAsync(StorageFile file)
        //{
        //    //file = await Package.Current.InstalledLocation.GetFileAsync("Assets\\Thumb.jpg");

        //    //var imageProps = await file.Properties.GetImagePropertiesAsync();
        //    var videoProps = await file.Properties.GetVideoPropertiesAsync();
        //    if (videoProps.Duration > TimeSpan.Zero && videoProps.Width > 0 && videoProps.Height > 0)
        //    {
        //        return await GetVideoThumbnailAsync(file, videoProps, null);
        //    }

        //    //if (imageProps.Width > 0 || videoProps.Width > 0)
        //    //{
        //    try
        //    {
        //        using (var thumb = await file.GetThumbnailAsync(ThumbnailMode.ListView, 96, ThumbnailOptions.ResizeThumbnail))
        //        {
        //            if (thumb != null && thumb.Type == ThumbnailType.Image)
        //            {
        //                var randomStream = thumb as IRandomAccessStream;

        //                var originalWidth = (int)thumb.OriginalWidth;
        //                var originalHeight = (int)thumb.OriginalHeight;

        //                if (thumb.ContentType != "image/jpeg")
        //                {
        //                    var memoryStream = new InMemoryRandomAccessStream();
        //                    var bitmapDecoder = await BitmapDecoder.CreateAsync(thumb);
        //                    var pixelDataProvider = await bitmapDecoder.GetPixelDataAsync();
        //                    var bitmapEncoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, memoryStream);
        //                    bitmapEncoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore, bitmapDecoder.PixelWidth, bitmapDecoder.PixelHeight, bitmapDecoder.DpiX, bitmapDecoder.DpiY, pixelDataProvider.DetachPixelData());
        //                    await bitmapEncoder.FlushAsync();
        //                    randomStream = memoryStream;
        //                }

        //                var desiredName = string.Format("{0}_{1}_{2}.jpg", "fileLocation.VolumeId", "fileLocation.LocalId", "fileLocation.Secret");
        //                var desiredFile = await FileUtils.CreateTempFileAsync(desiredName);

        //                var buffer = new Windows.Storage.Streams.Buffer(Convert.ToUInt32(randomStream.Size));
        //                var buffer2 = await randomStream.ReadAsync(buffer, buffer.Capacity, InputStreamOptions.None);
        //                using (var stream = await desiredFile.OpenAsync(FileAccessMode.ReadWrite))
        //                {
        //                    await stream.WriteAsync(buffer2);
        //                    stream.Dispose();
        //                }

        //                var result = new TLPhotoSize
        //                {
        //                    W = originalWidth,
        //                    H = originalHeight,
        //                    Size = (int)randomStream.Size,
        //                    Type = string.Empty
        //                };

        //                randomStream.Dispose();
        //                return result;
        //            }
        //        }
        //    }
        //    catch
        //    {
        //        return new TLPhotoSizeEmpty();
        //    }
        //    //}

        //    return null;
        //}
    }

    public static class ColorsHelper
    {
        public static Color AlphaBlend(Color color1, Color color2)
        {
            return AlphaBlend(color1, color2, color2.A);
        }

        public static Color AlphaBlend(Color color1, Color color2, byte alpha)
        {
            var factor = alpha / 255f;
            var red = (byte)(color1.R * (1 - factor) + color2.R * factor);
            var green = (byte)(color1.G * (1 - factor) + color2.G * factor);
            var blue = (byte)(color1.B * (1 - factor) + color2.B * factor);
            return Color.FromArgb(0xFF, red, green, blue);
        }

        public static double[] RgbToHsv(byte r, byte g, byte b)
        {
            var rf = r / 255.0;
            var gf = g / 255.0;
            var bf = b / 255.0;
            var max = rf > gf && rf > bf ? rf : gf > bf ? gf : bf;
            var min = rf < gf && rf < bf ? rf : gf < bf ? gf : bf;
            double h, s;
            var d = max - min;
            s = max == 0 ? 0 : d / max;
            if (max == min)
            {
                h = 0;
            }
            else
            {
                if (rf > gf && rf > bf)
                    h = (gf - bf) / d + (gf < bf ? 6 : 0);
                else if (gf > bf)
                    h = (bf - rf) / d + 2;
                else
                    h = (rf - gf) / d + 4;
                h /= 6;
            }

            return new[] { h, s, max };
        }

        public static byte[] HsvToRgb(double h, double s, double v)
        {
            double r = 0, g = 0, b = 0;
            double i = (int)Math.Floor(h * 6);
            var f = h * 6 - i;
            var p = v * (1 - s);
            var q = v * (1 - f * s);
            var t = v * (1 - (1 - f) * s);
            switch ((int)i % 6)
            {
                case 0:
                    r = v;
                    g = t;
                    b = p;
                    break;
                case 1:
                    r = q;
                    g = v;
                    b = p;
                    break;
                case 2:
                    r = p;
                    g = v;
                    b = t;
                    break;
                case 3:
                    r = p;
                    g = q;
                    b = v;
                    break;
                case 4:
                    r = t;
                    g = p;
                    b = v;
                    break;
                case 5:
                    r = v;
                    g = p;
                    b = q;
                    break;
            }

            return new[] { (byte)(r * 255), (byte)(g * 255), (byte)(b * 255) };
        }
    }
}