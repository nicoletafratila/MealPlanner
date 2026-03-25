using System.Drawing;

namespace Common.Constants.Tests
{
    [TestFixture]
    public class ColorsTests
    {
        [Test]
        public void GetBackgroundColors_WhenCounterIsZero_ReturnsEmptyList()
        {
            var result = Colors.GetBackgroundColors(0, Color.Black);

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);
        }

        [Test]
        public void GetBackgroundColors_WhenCounterIsNegative_ReturnsEmptyList()
        {
            var result = Colors.GetBackgroundColors(-5, Color.Black);

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);
        }

        [Test]
        public void GetBackgroundColors_ReturnsCorrectNumberOfColors()
        {
            const int count = 5;
            var result = Colors.GetBackgroundColors(count, Color.Blue);

            Assert.That(result, Has.Count.EqualTo(count));
        }

        [Test]
        public void GetBackgroundColors_FormatsColorsAsHexStrings()
        {
            var result = Colors.GetBackgroundColors(3, Color.Green);

            Assert.That(result, Has.All.Matches<string>(c =>
                c.StartsWith("#") && c.Length == 7));
        }

        [Test]
        public void GetBackgroundColors_ForBlack_ProducesExpectedGradient()
        {
            // For baseColor Black and counter=3 the math is:
            // factor 1   -> #FFFFFF
            // factor 2/3 -> #AAAAAA
            // factor 1/3 -> #555555
            var result = Colors.GetBackgroundColors(3, Color.Black);

            var expected = new List<string> { "#FFFFFF", "#AAAAAA", "#555555" };

            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void GetBackgroundColors_ForWhite_ReturnsAllWhite()
        {
            var result = Colors.GetBackgroundColors(4, Color.White);

            Assert.That(result, Has.Count.EqualTo(4));
            Assert.That(result, Has.All.EqualTo("#FFFFFF"));
        }
    }
}