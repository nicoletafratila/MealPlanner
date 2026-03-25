using System.Drawing;

namespace Common.Constants.Tests
{
    [TestFixture]
    public class ColorsTests
    {
        private static int Brightness(Color c) => (c.R + c.G + c.B) / 3;

        [Test]
        public void GetBackgroundColors_WhenCounterIsZero_ReturnsEmptyList()
        {
            var result = Colors.GetBackgroundColors(0, Color.SteelBlue);

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);
        }

        [Test]
        public void GetBackgroundColors_WhenCounterIsNegative_ReturnsEmptyList()
        {
            var result = Colors.GetBackgroundColors(-5, Color.SteelBlue);

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);
        }

        [Test]
        public void GetBackgroundColors_WhenCounterIsOne_ReturnsDarkOliveGreen()
        {
            var result = Colors.GetBackgroundColors(1, Color.SteelBlue);

            Assert.That(result, Has.Count.EqualTo(1));
            var color = ColorTranslator.FromHtml(result[0]);

            Assert.That(color.ToArgb(), Is.EqualTo(Color.DarkOliveGreen.ToArgb()));
        }

        [Test]
        public void GetBackgroundColors_FirstColorIsDarkOliveGreen()
        {
            var result = Colors.GetBackgroundColors(5, Color.SteelBlue);

            var first = ColorTranslator.FromHtml(result[0]);

            Assert.That(first.ToArgb(), Is.EqualTo(Color.DarkOliveGreen.ToArgb()));
        }

        [Test]
        public void GetBackgroundColors_ColorsAreHexFormatted()
        {
            var result = Colors.GetBackgroundColors(5, Color.SteelBlue);

            Assert.That(result, Has.All.Matches<string>(hex =>
                hex.StartsWith("#") && hex.Length == 7));
        }

        [Test]
        public void GetBackgroundColors_BrightnessIncreasesFromMostUsedToLeastUsed()
        {
            var baseColor = Color.SteelBlue;
            var result = Colors.GetBackgroundColors(6, baseColor);

            var parsed = new List<Color>();
            foreach (var hex in result)
            {
                parsed.Add(ColorTranslator.FromHtml(hex));
            }

            // Ensure strictly non-decreasing brightness
            for (var i = 0; i < parsed.Count - 1; i++)
            {
                var b1 = Brightness(parsed[i]);
                var b2 = Brightness(parsed[i + 1]);
                Assert.That(b2, Is.GreaterThanOrEqualTo(b1),
                    $"Color at index {i + 1} should be at least as bright as index {i}.");
            }
        }

        [Test]
        public void GetBackgroundColors_LightestColorIsNotWhite()
        {
            var baseColor = Color.SteelBlue;
            var result = Colors.GetBackgroundColors(5, baseColor);

            var last = ColorTranslator.FromHtml(result[^1]);
            Assert.That(last, Is.Not.EqualTo(Color.White));
        }

        [Test]
        public void GetBackgroundColors_LightestColorIsBrighterThanBase()
        {
            var baseColor = Color.SteelBlue;
            var baseBrightness = Brightness(baseColor);

            var result = Colors.GetBackgroundColors(5, baseColor);
            var last = ColorTranslator.FromHtml(result[^1]);

            Assert.That(Brightness(last), Is.GreaterThan(baseBrightness));
        }
    }
}