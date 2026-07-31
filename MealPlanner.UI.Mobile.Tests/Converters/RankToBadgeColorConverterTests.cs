using System.Globalization;
using MealPlanner.UI.Mobile.Converters;

namespace MealPlanner.UI.Mobile.Tests.Converters
{
    [TestFixture]
    public class RankToBadgeColorConverterTests
    {
        private readonly RankToBadgeColorConverter _converter = new();

        [Test]
        public void Convert_Rank1_ReturnsGoldColor()
        {
            var result = (Color)_converter.Convert(1, typeof(Color), null, CultureInfo.InvariantCulture);

            Assert.That(result.ToArgbHex(), Is.EqualTo(Color.FromArgb("#FFD700").ToArgbHex()));
        }

        [Test]
        public void Convert_Rank2_ReturnsSilverColor()
        {
            var result = (Color)_converter.Convert(2, typeof(Color), null, CultureInfo.InvariantCulture);

            Assert.That(result.ToArgbHex(), Is.EqualTo(Color.FromArgb("#C0C0C0").ToArgbHex()));
        }

        [Test]
        public void Convert_Rank3_ReturnsBronzeColor()
        {
            var result = (Color)_converter.Convert(3, typeof(Color), null, CultureInfo.InvariantCulture);

            Assert.That(result.ToArgbHex(), Is.EqualTo(Color.FromArgb("#CD7F32").ToArgbHex()));
        }

        [Test]
        public void Convert_RankOutsideTopThree_ReturnsDefaultGrayColor()
        {
            var result = (Color)_converter.Convert(4, typeof(Color), null, CultureInfo.InvariantCulture);

            Assert.That(result.ToArgbHex(), Is.EqualTo(Color.FromArgb("#9E9E9E").ToArgbHex()));
        }

        [Test]
        public void Convert_NonIntValue_ReturnsDefaultGrayColor()
        {
            var result = (Color)_converter.Convert("1", typeof(Color), null, CultureInfo.InvariantCulture);

            Assert.That(result.ToArgbHex(), Is.EqualTo(Color.FromArgb("#9E9E9E").ToArgbHex()));
        }

        [Test]
        public void Convert_Null_ReturnsDefaultGrayColor()
        {
            var result = (Color)_converter.Convert(null, typeof(Color), null, CultureInfo.InvariantCulture);

            Assert.That(result.ToArgbHex(), Is.EqualTo(Color.FromArgb("#9E9E9E").ToArgbHex()));
        }

        [Test]
        public void ConvertBack_Always_ThrowsNotImplementedException()
        {
            Assert.Throws<NotImplementedException>(() =>
                _converter.ConvertBack(Color.FromArgb("#FFD700"), typeof(int), null, CultureInfo.InvariantCulture));
        }
    }
}
