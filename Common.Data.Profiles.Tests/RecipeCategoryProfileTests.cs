using AutoMapper;
using Common.Data.Entities;
using RecipeBook.Shared.Models;

namespace Common.Data.Profiles.Tests
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
            var entity = new RecipeCategory
            {
                Id = 10,
                Name = "Vegetarian"
            };

            var result = _mapper.Map<RecipeCategoryModel>(entity);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Id, Is.EqualTo(10));
                Assert.That(result.Name, Is.EqualTo("Vegetarian"));
                Assert.That(result.Index, Is.Zero);
                Assert.That(result.IsSelected, Is.False);
            }
        }

        [Test]
        public void RecipeCategoryModel_To_RecipeCategory_Maps_Properties()
        {
            var model = new RecipeCategoryModel
            {
                Id = 15,
                Name = "Dessert"
            };

            var result = _mapper.Map<RecipeCategory>(model);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Id, Is.EqualTo(15));
                Assert.That(result.Name, Is.EqualTo("Dessert"));
            }
        }

        [Test]
        public void RecipeCategory_To_RecipeCategoryEditModel_Maps_Properties()
        {
            var entity = new RecipeCategory
            {
                Id = 7,
                Name = "Pasta"
            };

            var result = _mapper.Map<RecipeCategoryEditModel>(entity);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Id, Is.EqualTo(7));
                Assert.That(result.Name, Is.EqualTo("Pasta"));
                Assert.That(result.Index, Is.Zero);
                Assert.That(result.IsSelected, Is.False);
            }
        }

        [Test]
        public void RecipeCategoryEditModel_To_RecipeCategory_Maps_Properties()
        {
            var model = new RecipeCategoryEditModel
            {
                Id = 22,
                Name = "Soups"
            };

            var result = _mapper.Map<RecipeCategory>(model);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Id, Is.EqualTo(22));
                Assert.That(result.Name, Is.EqualTo("Soups"));
            }
        }
    }
}