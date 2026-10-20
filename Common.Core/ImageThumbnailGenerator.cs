using SkiaSharp;

namespace Common.Core
{
    public static class ImageThumbnailGenerator
    {
        public static byte[]? CreateThumbnail(byte[]? source, int maxDimension = 200, int quality = 75)
        {
            if (source is null || source.Length == 0)
                return null;

            using var original = SKBitmap.Decode(source);
            if (original is null)
                return null;

            var scale = Math.Min(1.0, Math.Min((double)maxDimension / original.Width, (double)maxDimension / original.Height));
            var width = Math.Max(1, (int)Math.Round(original.Width * scale));
            var height = Math.Max(1, (int)Math.Round(original.Height * scale));

            using var resized = original.Resize(new SKImageInfo(width, height), SKSamplingOptions.Default);
            using var image = SKImage.FromBitmap(resized ?? original);
            using var data = image.Encode(SKEncodedImageFormat.Jpeg, quality);
            return data.ToArray();
        }
    }
}
