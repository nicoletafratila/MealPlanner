using Common.Constants.Units;
using Common.Data.Entities.Converters;

namespace Common.Data.Entities.Tests.Converters
{
    [TestFixture]
    public class UnitConverterTests
    {
        [Test]
        public void Convert_Weight_kg_to_gr_Delegates_To_MassConverter()
        {
            var from = new Unit { Name = "kg", UnitType = UnitType.Weight };
            var to = new Unit { Name = "gr", UnitType = UnitType.Weight };

            var result = UnitConverter.Convert(1m, from, to);

            Assert.That(result, Is.EqualTo(1000m));
        }

        [Test]
        public void Convert_Liquid_l_to_ml_Delegates_To_LiquidConverter()
        {
            var from = new Unit { Name = "l", UnitType = UnitType.Liquid };
            var to = new Unit { Name = "ml", UnitType = UnitType.Liquid };

            var result = UnitConverter.Convert(2m, from, to);

            Assert.That(result, Is.EqualTo(2000m));
        }

        [Test]
        public void Convert_Volume_tbsp_to_tsp_Delegates_To_VolumeConverter()
        {
            var from = new Unit { Name = "tbsp", UnitType = UnitType.Volume };
            var to = new Unit { Name = "tsp", UnitType = UnitType.Volume };

            var result = UnitConverter.Convert(2m, from, to);

            Assert.That(result, Is.EqualTo(6m));
        }

        [Test]
        public void Convert_Piece_Is_Identity()
        {
            var from = new Unit { Name = "piece", UnitType = UnitType.Piece };
            var to = new Unit { Name = "piece", UnitType = UnitType.Piece };

            var result = UnitConverter.Convert(5m, from, to);

            Assert.That(result, Is.EqualTo(5m));
        }

        [Test]
        public void Convert_Throws_When_UnitTypes_Differ()
        {
            var from = new Unit { Name = "kg", UnitType = UnitType.Weight };
            var to = new Unit { Name = "ml", UnitType = UnitType.Liquid };

            Assert.That(
                () => UnitConverter.Convert(1m, from, to),
                Throws.TypeOf<InvalidOperationException>()
                      .With.Message.Contains("Cannot convert from unit type"));
        }

        [Test]
        public void Convert_Throws_When_Unit_Is_Null()
        {
            var from = new Unit { Name = "kg", UnitType = UnitType.Weight };

            Assert.That(
                () => UnitConverter.Convert(1m, null!, from),
                Throws.TypeOf<ArgumentNullException>());

            Assert.That(
                () => UnitConverter.Convert(1m, from, null!),
                Throws.TypeOf<ArgumentNullException>());
        }
    }
}