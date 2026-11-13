using SkiaSharp;

namespace Common.Core.Tests
{
    [TestFixture]
    public class ImageThumbnailGeneratorTests
    {
        [Test]
        public void CreateThumbnail_NullSource_ReturnsNull()
        {
            Assert.That(ImageThumbnailGenerator.CreateThumbnail(null), Is.Null);
        }

        [Test]
        public void CreateThumbnail_EmptySource_ReturnsNull()
        {
            Assert.That(ImageThumbnailGenerator.CreateThumbnail([]), Is.Null);
        }

        [Test]
        public void CreateThumbnail_ValidImage_ReturnsSmallerImage()
        {
            using var bitmap = new SKBitmap(800, 800);
            using var image = SKImage.FromBitmap(bitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            var source = data.ToArray();

            var thumbnail = ImageThumbnailGenerator.CreateThumbnail(source, maxDimension: 200, quality: 75);

            Assert.That(thumbnail, Is.Not.Null);
            using var thumbnailImage = SKBitmap.Decode(thumbnail);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(thumbnailImage.Width, Is.LessThanOrEqualTo(200));
                Assert.That(thumbnailImage.Height, Is.LessThanOrEqualTo(200));
                Assert.That(thumbnail!.Length, Is.LessThan(source.Length));
            }
        }
    }
}
