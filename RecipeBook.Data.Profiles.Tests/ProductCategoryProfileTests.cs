using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;
using RecipeBook.Data.Entities;
using RecipeBook.Shared.Models;

namespace RecipeBook.Data.Profiles.Tests
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
            }, NullLoggerFactory.Instance);

            config.AssertConfigurationIsValid();
            _mapper = config.CreateMapper();
        }

        [Test]
        public void ProductCategory_To_ProductCategoryModel_Maps_Properties()
        {
            var id = Guid.NewGuid();
            var entity = new ProductCategory
            {
                Id = id,
                Name = "Beverages"
            };

            var result = _mapper.Map<ProductCategoryModel>(entity);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Id, Is.EqualTo(id));
                Assert.That(result.Name, Is.EqualTo("Beverages"));

                Assert.That(result.Index, Is.Zero);
                Assert.That(result.IsSelected, Is.False);
            }
        }

        [Test]
        public void ProductCategoryModel_To_ProductCategory_Maps_Properties()
        {
            var id = Guid.NewGuid();
            var model = new ProductCategoryModel
            {
                Id = id,
                Name = "Grains"
            };

            var result = _mapper.Map<ProductCategory>(model);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Id, Is.EqualTo(id));
                Assert.That(result.Name, Is.EqualTo("Grains"));
            }
        }

        [Test]
        public void ProductCategory_To_ProductCategoryEditModel_Maps_Properties()
        {
            var id = Guid.NewGuid();
            var entity = new ProductCategory
            {
                Id = id,
                Name = "Spices"
            };

            var result = _mapper.Map<ProductCategoryEditModel>(entity);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Id, Is.EqualTo(id));
                Assert.That(result.Name, Is.EqualTo("Spices"));

                Assert.That(result.Index, Is.Zero);
                Assert.That(result.IsSelected, Is.False);
            }
        }

        [Test]
        public void ProductCategoryEditModel_To_ProductCategory_Maps_Properties()
        {
            var id = Guid.NewGuid();
            var model = new ProductCategoryEditModel
            {
                Id = id,
                Name = "Dairy"
            };

            var result = _mapper.Map<ProductCategory>(model);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Id, Is.EqualTo(id));
                Assert.That(result.Name, Is.EqualTo("Dairy"));
            }
        }
    }
}
