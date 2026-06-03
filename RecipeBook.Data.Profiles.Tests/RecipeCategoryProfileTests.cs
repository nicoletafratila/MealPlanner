using AutoMapper;
using RecipeBook.Data.Entities;
using RecipeBook.Shared.Models;

namespace RecipeBook.Data.Profiles.Tests
{
    [TestFixture]
    public class RecipeCategoryProfileTests
    {
        private IMapper _mapper = null!;

        [SetUp]
        public void Setup()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<RecipeCategoryProfile>();
            });

            config.AssertConfigurationIsValid();
            _mapper = config.CreateMapper();
        }

        [Test]
        public void RecipeCategory_To_RecipeCategoryModel_Maps_Properties()
        {
            var id = Guid.NewGuid();
            var entity = new RecipeCategory
            {
                Id = id,
                Name = "Vegetarian"
            };

            var result = _mapper.Map<RecipeCategoryModel>(entity);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Id, Is.EqualTo(id));
                Assert.That(result.Name, Is.EqualTo("Vegetarian"));
                Assert.That(result.Index, Is.Zero);
                Assert.That(result.IsSelected, Is.False);
            }
        }

        [Test]
        public void RecipeCategoryModel_To_RecipeCategory_Maps_Properties()
        {
            var id = Guid.NewGuid();
            var model = new RecipeCategoryModel
            {
                Id = id,
                Name = "Dessert"
            };

            var result = _mapper.Map<RecipeCategory>(model);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Id, Is.EqualTo(id));
                Assert.That(result.Name, Is.EqualTo("Dessert"));
            }
        }

        [Test]
        public void RecipeCategory_To_RecipeCategoryEditModel_Maps_Properties()
        {
            var id = Guid.NewGuid();
            var entity = new RecipeCategory
            {
                Id = id,
                Name = "Pasta"
            };

            var result = _mapper.Map<RecipeCategoryEditModel>(entity);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Id, Is.EqualTo(id));
                Assert.That(result.Name, Is.EqualTo("Pasta"));
                Assert.That(result.Index, Is.Zero);
                Assert.That(result.IsSelected, Is.False);
            }
        }

        [Test]
        public void RecipeCategoryEditModel_To_RecipeCategory_Maps_Properties()
        {
            var id = Guid.NewGuid();
            var model = new RecipeCategoryEditModel
            {
                Id = id,
                Name = "Soups"
            };

            var result = _mapper.Map<RecipeCategory>(model);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Id, Is.EqualTo(id));
                Assert.That(result.Name, Is.EqualTo("Soups"));
            }
        }
    }
}
