using Common.Constants.Units;
using Common.Data.Entities.Converters;

namespace Common.Data.Entities.Tests.Converters
{
    [TestFixture]
    public class LiquidConverterTests
    {
        [Test]
        public void Convert_l_to_ml_Works()
        {
            var result = LiquidConverter.Convert(1m, LiquidUnit.l, LiquidUnit.ml);
            Assert.That(result, Is.EqualTo(1000m));
        }

        [Test]
        public void Convert_ml_to_l_Works()
        {
            var result = LiquidConverter.Convert(500m, LiquidUnit.ml, LiquidUnit.l);
            Assert.That(result, Is.EqualTo(0.5m));
        }

        [Test]
        public void Convert_Same_Unit_Returns_Original_Value()
        {
            var result = LiquidConverter.Convert(42m, LiquidUnit.l, LiquidUnit.l);
            Assert.That(result, Is.EqualTo(42m));
        }
    }
}