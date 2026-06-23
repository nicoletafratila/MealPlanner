using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;
using RecipeBook.Data.Entities;
using RecipeBook.Shared.Models;

namespace RecipeBook.Data.Profiles.Tests
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
            }, NullLoggerFactory.Instance);
            _mapper = config.CreateMapper();
        }

        [Test]
        public void Unit_To_UnitModel_Maps_All_Properties()
        {
            var unit = new Unit
            {
                Id = Guid.NewGuid(),
                Name = "Gram",
            };

            var result = _mapper.Map<UnitModel>(unit);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Id, Is.EqualTo(unit.Id));
                Assert.That(result.Name, Is.EqualTo(unit.Name));
            }
        }

        [Test]
        public void UnitModel_To_Unit_Maps_All_Properties()
        {
            var model = new UnitModel
            {
                Id = Guid.NewGuid(),
                Name = "Milliliter",
            };

            var result = _mapper.Map<Unit>(model);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Id, Is.EqualTo(model.Id));
                Assert.That(result.Name, Is.EqualTo(model.Name));
            }
        }

        [Test]
        public void Unit_To_UnitEditModel_Maps_All_Properties()
        {
            var unit = new Unit
            {
                Id = Guid.NewGuid(),
                Name = "Cup",
            };

            var result = _mapper.Map<UnitEditModel>(unit);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Id, Is.EqualTo(unit.Id));
                Assert.That(result.Name, Is.EqualTo(unit.Name));
            }
        }

        [Test]
        public void UnitEditModel_To_Unit_Maps_All_Properties()
        {
            var model = new UnitEditModel
            {
                Id = Guid.NewGuid(),
                Name = "Tablespoon",
            };

            var result = _mapper.Map<Unit>(model);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Id, Is.EqualTo(model.Id));
                Assert.That(result.Name, Is.EqualTo(model.Name));
            }
        }

        [Test]
        public void Null_SourceValue_Does_Not_Overwrite_Destination()
        {
            var sharedId = Guid.NewGuid();
            var model = new UnitModel
            {
                Id = sharedId,
                Name = null!,
            };

            var destination = new Unit
            {
                Id = sharedId,
                Name = "Liter",
            };

            _mapper.Map(model, destination);

            Assert.That(destination.Name, Is.EqualTo("Liter"), "Null source should NOT overwrite existing value.");
        }
    }
}
