using AutoMapper;
using Common.Data.Entities;
using RecipeBook.Shared.Models;

namespace Common.Data.Profiles.Tests
{
    [TestFixture]
    public class ProductCategoryProfileTests
    {
        private IMapper _mapper = null!;

        [SetUp]
        public void Setup()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<ProductCategoryProfile>();
            });

            config.AssertConfigurationIsValid();
            _mapper = config.CreateMapper();
        }

        [Test]
        public void ProductCategory_To_ProductCategoryModel_Maps_Properties()
        {
            var entity = new ProductCategory
            {
                Id = 10,
                Name = "Beverages"
            };

            var result = _mapper.Map<ProductCategoryModel>(entity);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Id, Is.EqualTo(10));
                Assert.That(result.Name, Is.EqualTo("Beverages"));

                Assert.That(result.Index, Is.Zero);
                Assert.That(result.IsSelected, Is.False);
            }
        }

        [Test]
        public void ProductCategoryModel_To_ProductCategory_Maps_Properties()
        {
            var model = new ProductCategoryModel
            {
                Id = 22,
                Name = "Grains"
            };

            var result = _mapper.Map<ProductCategory>(model);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Id, Is.EqualTo(22));
                Assert.That(result.Name, Is.EqualTo("Grains"));
            }
        }

        [Test]
        public void ProductCategory_To_ProductCategoryEditModel_Maps_Properties()
        {
            var entity = new ProductCategory
            {
                Id = 7,
                Name = "Spices"
            };

            var result = _mapper.Map<ProductCategoryEditModel>(entity);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Id, Is.EqualTo(7));
                Assert.That(result.Name, Is.EqualTo("Spices"));

                Assert.That(result.Index, Is.Zero);
                Assert.That(result.IsSelected, Is.False);
            }
        }

        [Test]
        public void ProductCategoryEditModel_To_ProductCategory_Maps_Properties()
        {
            var model = new ProductCategoryEditModel
            {
                Id = 99,
                Name = "Dairy"
            };

            var result = _mapper.Map<ProductCategory>(model);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Id, Is.EqualTo(99));
                Assert.That(result.Name, Is.EqualTo("Dairy"));
            }
        }
    }
}