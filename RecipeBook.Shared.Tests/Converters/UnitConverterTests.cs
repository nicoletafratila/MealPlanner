using AutoMapper;
using Common.Data.Entities;
using Moq;
using RecipeBook.Shared.Converters;
using RecipeBook.Shared.Models;

namespace RecipeBook.Shared.Tests.Converters
{
    [TestFixture]
    public class UnitConverterTests
    {
        private Mock<IMapper> _mapperMock = null!;
        private Unit _capturedFromEntity = null!;
        private Unit _capturedToEntity = null!;
        private decimal _capturedValue;
        private decimal _convertResult;

        [SetUp]
        public void SetUp()
        {
            _mapperMock = new Mock<IMapper>(MockBehavior.Strict);

            // Configure UnitConverter to use our mock mapper
            UnitConverter.ConfigureMapperFactory(() => _mapperMock.Object);

            // Configure a fake converter that just records parameters and returns a fixed value
            _convertResult = 42.5m;
            UnitConverter.ConfigureConverter((value, from, to) =>
            {
                _capturedValue = value;
                _capturedFromEntity = from;
                _capturedToEntity = to;
                return _convertResult;
            });
        }

        [Test]
        public void Convert_MapsUnits_AndCallsConfiguredConverter()
        {
            // Arrange
            var fromModel = new UnitModel(1, "kg", Common.Constants.Units.UnitType.Weight);
            var toModel = new UnitModel(2, "g", Common.Constants.Units.UnitType.Weight);

            var fromEntity = new Unit { Id = 1, Name = "kg" };
            var toEntity = new Unit { Id = 2, Name = "g" };

            _mapperMock
                .Setup(m => m.Map<Unit>(fromModel))
                .Returns(fromEntity);

            _mapperMock
                .Setup(m => m.Map<Unit>(toModel))
                .Returns(toEntity);

            var value = 10m;

            // Act
            var result = UnitConverter.Convert(value, fromModel, toModel);

            // Assert
            Assert.Multiple(() =>
            {
                // Ensure mapper was called correctly
                _mapperMock.Verify(m => m.Map<Unit>(fromModel), Times.Once);
                _mapperMock.Verify(m => m.Map<Unit>(toModel), Times.Once);

                // Ensure our fake converter saw the mapped entities and value
                Assert.That(_capturedValue, Is.EqualTo(value));
                Assert.That(_capturedFromEntity, Is.SameAs(fromEntity));
                Assert.That(_capturedToEntity, Is.SameAs(toEntity));

                // Ensure the final result is what the converter returned
                Assert.That(result, Is.EqualTo(_convertResult));
            });
        }

        [Test]
        public void Convert_Throws_WhenMapperFactoryReturnsNull()
        {
            // Arrange
            UnitConverter.ConfigureMapperFactory(() => null!);

            var fromModel = new UnitModel(1, "kg", Common.Constants.Units.UnitType.Weight);
            var toModel = new UnitModel(2, "g", Common.Constants.Units.UnitType.Weight);

            // Act / Assert
            Assert.Throws<InvalidOperationException>(() =>
            {
                _ = UnitConverter.Convert(1m, fromModel, toModel);
            });
        }

        [Test]
        public void Convert_ThrowsArgumentNull_WhenFromUnitIsNull()
        {
            var toModel = new UnitModel(2, "g", Common.Constants.Units.UnitType.Weight);

            Assert.Throws<ArgumentNullException>(() =>
            {
                _ = UnitConverter.Convert(1m, null!, toModel);
            });
        }

        [Test]
        public void Convert_ThrowsArgumentNull_WhenToUnitIsNull()
        {
            var fromModel = new UnitModel(1, "kg", Common.Constants.Units.UnitType.Weight);

            Assert.Throws<ArgumentNullException>(() =>
            {
                _ = UnitConverter.Convert(1m, fromModel, null!);
            });
        }
    }
}