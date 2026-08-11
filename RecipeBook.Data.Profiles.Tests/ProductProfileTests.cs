using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;
using RecipeBook.Data.Entities;
using RecipeBook.Shared.Models;

namespace RecipeBook.Data.Profiles.Tests
{
    [TestFixture]
    public class ProductProfileTests
    {
        private IMapper _mapper = null!;

        [SetUp]
        public void Setup()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<ProductProfile>();
                cfg.AddProfile<UnitProfile>();
                cfg.AddProfile<ProductCategoryProfile>();
            }, NullLoggerFactory.Instance);

            config.AssertConfigurationIsValid();
            _mapper = config.CreateMapper();
        }

        [Test]
        public void Product_To_ProductModel_Maps_Properties()
        {
            var entity = new Product
            {
                Id = Guid.NewGuid(),
                Name = "Flour",
                ImageContent = [1, 2, 3],
                ImageThumbnail = [4, 5, 6],
                ProductCategory = new ProductCategory { Name = "Baking" }
            };

            var result = _mapper.Map<ProductModel>(entity);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Id, Is.EqualTo(entity.Id));
                Assert.That(result.Name, Is.EqualTo("Flour"));
                Assert.That(result.ImageUrl, Does.StartWith("data:image/jpg;base64,"));
                Assert.That(result.ThumbnailUrl, Does.StartWith("data:image/jpg;base64,"));
                Assert.That(result.Index, Is.Zero);
                Assert.That(result.IsSelected, Is.False);
            }
        }

        [Test]
        public void Product_To_ProductModel_WhenImageThumbnailNull_ReturnsNullThumbnailUrl()
        {
            var entity = new Product { Name = "Flour", ImageThumbnail = null };

            var result = _mapper.Map<ProductModel>(entity);

            Assert.That(result.ThumbnailUrl, Is.Null);
        }

        [Test]
        public void ProductModel_To_Product_Maps_Properties_And_Ignores_Navigation()
        {
            var model = new ProductModel
            {
                Id = Guid.NewGuid(),
                Name = "Sugar",
                ImageUrl = "ignored"
            };

            var result = _mapper.Map<Product>(model);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Id, Is.EqualTo(model.Id));
                Assert.That(result.Name, Is.EqualTo("Sugar"));
                Assert.That(result.ImageContent, Is.Null);
                Assert.That(result.ImageThumbnail, Is.Null);
                Assert.That(result.BaseUnit, Is.Null);
                Assert.That(result.ProductCategory, Is.Null);
            }
        }

        [Test]
        public void Product_To_ProductEditModel_Maps_Properties()
        {
            var entity = new Product
            {
                Id = Guid.NewGuid(),
                Name = "Yogurt",
                ImageContent = [5, 5, 5]
            };

            var result = _mapper.Map<ProductEditModel>(entity);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Id, Is.EqualTo(entity.Id));
                Assert.That(result.Name, Is.EqualTo("Yogurt"));
                Assert.That(result.ImageUrl, Does.StartWith("data:image/jpg;base64,"));
                Assert.That(result.Index, Is.Zero);
            }
        }

        [Test]
        public void ProductEditModel_To_Product_Maps_Properties_And_Ignores_Navigation()
        {
            var model = new ProductEditModel
            {
                Id = Guid.NewGuid(),
                Name = "Milk",
                ImageUrl = "ignored"
            };

            var result = _mapper.Map<Product>(model);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Id, Is.EqualTo(model.Id));
                Assert.That(result.Name, Is.EqualTo("Milk"));
                Assert.That(result.BaseUnit, Is.Null);
                Assert.That(result.ProductCategory, Is.Null);
            }
        }

        [Test]
        public void ImageUrl_Is_Generated_From_ImageContent()
        {
            var entity = new Product { ImageContent = [10, 20, 30] };
            var model = _mapper.Map<ProductModel>(entity);
            Assert.That(model.ImageUrl, Contains.Substring("data:image/jpg;base64,"));
        }

        [Test]
        public void Null_ImageContent_Does_Not_Throw_And_Returns_Null_ImageUrl()
        {
            var entity = new Product();
            var model = _mapper.Map<ProductEditModel>(entity);
            Assert.That(model.ImageUrl, Is.Null);
        }

        [Test]
        public void Product_To_ProductModel_WhenImageContentNull_ReturnsNullImageUrl()
        {
            var entity = new Product { Name = "Flour", ImageContent = null };

            var result = _mapper.Map<ProductModel>(entity);

            Assert.That(result.ImageUrl, Is.Null);
        }
    }
}
