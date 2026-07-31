using System.Globalization;
using MealPlanner.UI.Mobile.Converters;

namespace MealPlanner.UI.Mobile.Tests.Converters
{
    [TestFixture]
    public class RankToBadgeTextConverterTests
    {
        private readonly RankToBadgeTextConverter _converter = new();

        [Test]
        public void Convert_Rank1_ReturnsGoldMedalEmoji()
        {
            var result = _converter.Convert(1, typeof(string), null, CultureInfo.InvariantCulture);

            Assert.That(result, Is.EqualTo("🥇"));
        }

        [Test]
        public void Convert_Rank2_ReturnsSilverMedalEmoji()
        {
            var result = _converter.Convert(2, typeof(string), null, CultureInfo.InvariantCulture);

            Assert.That(result, Is.EqualTo("🥈"));
        }

        [Test]
        public void Convert_Rank3_ReturnsBronzeMedalEmoji()
        {
            var result = _converter.Convert(3, typeof(string), null, CultureInfo.InvariantCulture);

            Assert.That(result, Is.EqualTo("🥉"));
        }

        [Test]
        public void Convert_RankOutsideTopThree_ReturnsRankNumberAsString()
        {
            var result = _converter.Convert(4, typeof(string), null, CultureInfo.InvariantCulture);

            Assert.That(result, Is.EqualTo("4"));
        }

        [Test]
        public void Convert_NonIntValue_ReturnsEmptyString()
        {
            var result = _converter.Convert("1", typeof(string), null, CultureInfo.InvariantCulture);

            Assert.That(result, Is.EqualTo(string.Empty));
        }

        [Test]
        public void Convert_Null_ReturnsEmptyString()
        {
            var result = _converter.Convert(null, typeof(string), null, CultureInfo.InvariantCulture);

            Assert.That(result, Is.EqualTo(string.Empty));
        }

        [Test]
        public void ConvertBack_Always_ThrowsNotImplementedException()
        {
            Assert.Throws<NotImplementedException>(() =>
                _converter.ConvertBack("🥇", typeof(int), null, CultureInfo.InvariantCulture));
        }
    }
}
