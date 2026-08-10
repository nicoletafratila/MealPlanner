using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace RecipeBook.Api.Services
{
    /// <summary>
    /// Generates small JPEG thumbnails from stored recipe/product images, so list and
    /// search endpoints can return a lightweight image instead of re-encoding the full
    /// upload (which can be several MB) for every row.
    /// </summary>
    public static class ImageThumbnailGenerator
    {
        public static byte[]? CreateThumbnail(byte[]? source, int maxDimension = 200, int quality = 75)
        {
            if (source is null || source.Length == 0)
                return null;

            using var image = Image.Load(source);
            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Mode = ResizeMode.Max,
                Size = new Size(maxDimension, maxDimension)
            }));

            using var output = new MemoryStream();
            image.Save(output, new JpegEncoder { Quality = quality });
            return output.ToArray();
        }
    }
}
