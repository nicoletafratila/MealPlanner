using Common.Constants.Units;
using Common.Data.Entities.Converters;

namespace Common.Data.Entities.Tests.Converters
{
    [TestFixture]
    public class MassConverterTests
    {
        [Test]
        public void Convert_kg_to_gr_Works()
        {
            var result = MassConverter.Convert(1m, MassUnit.kg, MassUnit.gr);
            Assert.That(result, Is.EqualTo(1000m));
        }

        [Test]
        public void Convert_gr_to_kg_Works()
        {
            var result = MassConverter.Convert(1000m, MassUnit.gr, MassUnit.kg);
            Assert.That(result, Is.EqualTo(1m));
        }

        [Test]
        public void Convert_Same_Unit_Returns_Original_Value()
        {
            var result = MassConverter.Convert(123.45m, MassUnit.kg, MassUnit.kg);
            Assert.That(result, Is.EqualTo(123.45m));
        }
    }
}