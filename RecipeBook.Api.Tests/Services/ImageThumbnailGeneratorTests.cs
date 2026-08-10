using RecipeBook.Api.Services;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace RecipeBook.Api.Tests.Services
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
            using var image = new Image<Rgba32>(800, 800);
            using var ms = new MemoryStream();
            image.SaveAsPng(ms);
            var source = ms.ToArray();

            var thumbnail = ImageThumbnailGenerator.CreateThumbnail(source, maxDimension: 200, quality: 75);

            Assert.That(thumbnail, Is.Not.Null);
            using var thumbnailImage = Image.Load(thumbnail);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(thumbnailImage.Width, Is.LessThanOrEqualTo(200));
                Assert.That(thumbnailImage.Height, Is.LessThanOrEqualTo(200));
                Assert.That(thumbnail!.Length, Is.LessThan(source.Length));
            }
        }
    }
}
