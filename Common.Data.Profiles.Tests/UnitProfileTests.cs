using AutoMapper;
using Common.Data.Entities;
using RecipeBook.Shared.Models;

namespace Common.Data.Profiles.Tests
{
    public class UnitProfileTests
    {
        private IMapper _mapper;

        [SetUp]
        public void Setup()
        {
            var config = new MapperConfiguration(c =>
            {
                c.AddProfile<UnitProfile>();
            });
            _mapper = config.CreateMapper();
        }

        [Test]
        public void Unit_To_UnitModel_Maps_All_Properties()
        {
            var unit = new Unit
            {
                Id = 1,
                Name = "Gram",
            };

            var result = _mapper.Map<UnitModel>(unit);

            Assert.Multiple(() =>
            {
                Assert.That(result.Id, Is.EqualTo(unit.Id));
                Assert.That(result.Name, Is.EqualTo(unit.Name));
            });
        }

        [Test]
        public void UnitModel_To_Unit_Maps_All_Properties()
        {
            var model = new UnitModel
            {
                Id = 10,
                Name = "Milliliter",
            };

            var result = _mapper.Map<Unit>(model);

            Assert.Multiple(() =>
            {
                Assert.That(result.Id, Is.EqualTo(model.Id));
                Assert.That(result.Name, Is.EqualTo(model.Name));
            });
        }

        [Test]
        public void Unit_To_UnitEditModel_Maps_All_Properties()
        {
            var unit = new Unit
            {
                Id = 2,
                Name = "Cup",
            };

            var result = _mapper.Map<UnitEditModel>(unit);

            Assert.Multiple(() =>
            {
                Assert.That(result.Id, Is.EqualTo(unit.Id));
                Assert.That(result.Name, Is.EqualTo(unit.Name));
            });
        }

        [Test]
        public void UnitEditModel_To_Unit_Maps_All_Properties()
        {
            var model = new UnitEditModel
            {
                Id = 3,
                Name = "Tablespoon",
            };

            var result = _mapper.Map<Unit>(model);

            Assert.Multiple(() =>
            {
                Assert.That(result.Id, Is.EqualTo(model.Id));
                Assert.That(result.Name, Is.EqualTo(model.Name));
            });
        }

        [Test]
        public void Null_SourceValue_Does_Not_Overwrite_Destination()
        {
            var model = new UnitModel
            {
                Id = 5,
                Name = null,                // null should NOT overwrite
            };

            var destination = new Unit
            {
                Id = 5,
                Name = "Liter",
            };

            _mapper.Map(model, destination);

            Assert.That(destination.Name, Is.EqualTo("Liter"), "Null source should NOT overwrite existing value.");
        }

        [Test]
        public void AutoMapper_Configuration_Is_Valid()
        {
            Assert.That(() => _mapper.ConfigurationProvider.AssertConfigurationIsValid(), Throws.Nothing);
        }
    }
}