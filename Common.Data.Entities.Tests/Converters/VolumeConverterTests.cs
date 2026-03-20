using Common.Constants.Units;
using Common.Data.Entities.Converters;

namespace Common.Data.Entities.Tests.Converters
{
    [TestFixture]
    public class VolumeConverterTests
    {
        [Test]
        public void Convert_tbsp_to_tsp_Works()
        {
            var result = VolumeConverter.Convert(1m, VolumeUnit.tbsp, VolumeUnit.tsp);
            Assert.That(result, Is.EqualTo(3m));
        }

        [Test]
        public void Convert_tsp_to_tbsp_Works()
        {
            var result = VolumeConverter.Convert(3m, VolumeUnit.tsp, VolumeUnit.tbsp);
            Assert.That(result, Is.EqualTo(1m));
        }

        [Test]
        public void Convert_cup_to_tsp_Works()
        {
            var result = VolumeConverter.Convert(1m, VolumeUnit.cup, VolumeUnit.tsp);
            Assert.That(result, Is.EqualTo(48m));
        }

        [Test]
        public void Convert_tsp_to_cup_Works()
        {
            var result = VolumeConverter.Convert(96m, VolumeUnit.tsp, VolumeUnit.cup);
            Assert.That(result, Is.EqualTo(2m)); // 96 tsp = 2 cups
        }

        [Test]
        public void Convert_Same_Unit_Returns_Original_Value()
        {
            var result = VolumeConverter.Convert(10m, VolumeUnit.tsp, VolumeUnit.tsp);
            Assert.That(result, Is.EqualTo(10m));
        }
    }
}