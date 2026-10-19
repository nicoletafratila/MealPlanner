using System.Globalization;
using MealPlanner.UI.Mobile.Converters;

namespace MealPlanner.UI.Mobile.Tests.Converters
{
    [TestFixture]
    public class PercentToProgressConverterTests
    {
        private readonly PercentToProgressConverter _converter = new();

        [Test]
        public void Convert_ZeroPercent_ReturnsZero()
        {
            var result = _converter.Convert(0, typeof(double), null, CultureInfo.InvariantCulture);

            Assert.That(result, Is.EqualTo(0.0));
        }

        [Test]
        public void Convert_FiftyPercent_ReturnsPointFive()
        {
            var result = _converter.Convert(50, typeof(double), null, CultureInfo.InvariantCulture);

            Assert.That(result, Is.EqualTo(0.5));
        }

        [Test]
        public void Convert_HundredPercent_ReturnsOne()
        {
            var result = _converter.Convert(100, typeof(double), null, CultureInfo.InvariantCulture);

            Assert.That(result, Is.EqualTo(1.0));
        }

        [Test]
        public void Convert_NonIntValue_ReturnsZero()
        {
            var result = _converter.Convert("50", typeof(double), null, CultureInfo.InvariantCulture);

            Assert.That(result, Is.EqualTo(0.0));
        }

        [Test]
        public void Convert_Null_ReturnsZero()
        {
            var result = _converter.Convert(null, typeof(double), null, CultureInfo.InvariantCulture);

            Assert.That(result, Is.EqualTo(0.0));
        }

        [Test]
        public void ConvertBack_Always_ThrowsNotImplementedException()
        {
            Assert.Throws<NotImplementedException>(() =>
                _converter.ConvertBack(0.5, typeof(int), null, CultureInfo.InvariantCulture));
        }
    }
}
