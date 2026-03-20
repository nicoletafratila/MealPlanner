using AutoMapper;
using Common.Data.Entities;
using MealPlanner.Shared.Models;
using RecipeBook.Shared.Models;

namespace Common.Data.Profiles.Tests
{
    public class ShopDisplaySequenceProfileTests
    {
        private IMapper _mapper = null!;

        [SetUp]
        public void Setup()
        {
            var config = new MapperConfiguration(c =>
            {
                c.AddProfile<ShopDisplaySequenceProfile>();
                c.AddProfile<ProductCategoryProfile>();
            });

            config.AssertConfigurationIsValid();
            _mapper = config.CreateMapper();
        }

        [Test]
        public void ShopDisplaySequence_To_ShopDisplaySequenceEditModel_Maps_All_Properties()
        {
            var entity = new ShopDisplaySequence
            {
                ShopId = 10,
                ProductCategoryId = 20,
                Value = 3
            };

            var result = _mapper.Map<ShopDisplaySequenceEditModel>(entity);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.ShopId, Is.EqualTo(entity.ShopId));
                Assert.That(result.ProductCategory, Is.Null);
                Assert.That(result.Value, Is.EqualTo(entity.Value));

                Assert.That(result.ProductCategory, Is.Null);

                Assert.That(result.Index, Is.Zero);
                Assert.That(result.IsSelected, Is.False);
            }
        }

        [Test]
        public void ShopDisplaySequenceEditModel_To_ShopDisplaySequence_Maps_All_Properties()
        {
            var model = new ShopDisplaySequenceEditModel
            {
                ShopId = 15,
                Value = 7,
                ProductCategory = new ProductCategoryModel { Id = 30 }
            };

            var result = _mapper.Map<ShopDisplaySequence>(model);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.ShopId, Is.EqualTo(model.ShopId));
                Assert.That(result.ProductCategoryId, Is.EqualTo(model.ProductCategory.Id));
                Assert.That(result.Value, Is.EqualTo(model.Value));

                Assert.That(result.Shop, Is.Null);
                Assert.That(result.ProductCategory, Is.Null);
            }
        }

        [Test]
        public void Null_SourceValue_Does_Not_Overwrite_Destination()
        {
            var model = new ShopDisplaySequenceEditModel
            {
                ShopId = 1,
                Value = 10,
                ProductCategory = null
            };

            var destination = new ShopDisplaySequence
            {
                ShopId = 1,
                Value = 2,
                ProductCategoryId = 5,
                Shop = new Shop { Name = "Existing Shop" },
                ProductCategory = new ProductCategory { Name = "Existing Category" }
            };

            _mapper.Map(model, destination);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(destination.ShopId, Is.EqualTo(model.ShopId));
                Assert.That(destination.Value, Is.EqualTo(model.Value));
                Assert.That(destination.ProductCategoryId, Is.Zero);

                Assert.That(destination.Shop!.Name, Is.EqualTo("Existing Shop"));
                Assert.That(destination.ProductCategory!.Name, Is.EqualTo("Existing Category"));
            }
        }
    }
}