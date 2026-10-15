using AutoMapper;
using MealPlanner.Data.Entities;
using MealPlanner.Shared.Models;
using Microsoft.Extensions.Logging.Abstractions;

namespace MealPlanner.Data.Profiles.Tests
{
    [TestFixture]
    public class ShopProfileTests
    {
        private IMapper _mapper = null!;

        [SetUp]
        public void SetUp()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<ShopProfile>();
                cfg.AddProfile<ShopDisplaySequenceProfile>();
                cfg.AddProfile<RecipeBook.Data.Profiles.ProductCategoryProfile>();
            }, NullLoggerFactory.Instance);

            config.AssertConfigurationIsValid();
            _mapper = config.CreateMapper();
        }

        [Test]
        public void Shop_To_ShopModel_Maps_And_Ignores_Index_And_IsSelected()
        {
            var shop = new Shop
            {
                Id = Guid.NewGuid(),
                Name = "Test Shop",
            };

            var result = _mapper.Map<ShopModel>(shop);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result.Id, Is.EqualTo(shop.Id));
                Assert.That(result.Name, Is.EqualTo(shop.Name));
                Assert.That(result.Index, Is.Zero);
                Assert.That(result.IsSelected, Is.False);
            }
        }

        [Test]
        public void Shop_To_ShopModel_Maps_ImageUrl_And_ThumbnailUrl_AsBase64DataUris()
        {
            var shop = new Shop
            {
                Id = Guid.NewGuid(),
                Name = "Test Shop",
                ImageContent = [1, 2, 3],
                ImageThumbnail = [4, 5]
            };

            var result = _mapper.Map<ShopModel>(shop);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.ImageUrl, Is.EqualTo($"data:image/jpg;base64,{Convert.ToBase64String(shop.ImageContent)}"));
                Assert.That(result.ThumbnailUrl, Is.EqualTo($"data:image/jpg;base64,{Convert.ToBase64String(shop.ImageThumbnail)}"));
            }
        }

        [Test]
        public void Shop_To_ShopModel_EmptyImageArrays_MapsToNullUrls_NotEmptyDataUris()
        {
            // An entity whose ImageContent/ImageThumbnail ended up as an empty array (e.g. via
            // AutoMapper's null -> empty array default on the way in) must still surface as a null
            // URL, not a non-null "data:image/jpg;base64," with no payload.
            var shop = new Shop
            {
                Id = Guid.NewGuid(),
                Name = "Test Shop",
                ImageContent = [],
                ImageThumbnail = []
            };

            var result = _mapper.Map<ShopModel>(shop);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.ImageUrl, Is.Null);
                Assert.That(result.ThumbnailUrl, Is.Null);
            }
        }

        [Test]
        public void Shop_To_ShopModel_NullImages_MapsToNullUrls()
        {
            var shop = new Shop
            {
                Id = Guid.NewGuid(),
                Name = "Test Shop",
                ImageContent = null,
                ImageThumbnail = null
            };

            var result = _mapper.Map<ShopModel>(shop);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.ImageUrl, Is.Null);
                Assert.That(result.ThumbnailUrl, Is.Null);
            }
        }

        [Test]
        public void Shop_To_ShopEditModel_Maps_ImageUrl_AndPassesThroughImageContent()
        {
            var shop = new Shop
            {
                Id = Guid.NewGuid(),
                Name = "Test Shop",
                ImageContent = [9, 8, 7]
            };

            var result = _mapper.Map<ShopEditModel>(shop);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.ImageUrl, Is.EqualTo($"data:image/jpg;base64,{Convert.ToBase64String(shop.ImageContent)}"));
                Assert.That(result.ImageContent, Is.EqualTo(shop.ImageContent).AsCollection);
            }
        }

        [Test]
        public void Shop_To_ShopEditModel_NullImageContent_MapsToNullImageUrl_AndNullImageContent()
        {
            var shop = new Shop
            {
                Id = Guid.NewGuid(),
                Name = "Test Shop",
                ImageContent = null
            };

            var result = _mapper.Map<ShopEditModel>(shop);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.ImageUrl, Is.Null);
                // AutoMapper's default array mapper maps a null source array to an empty array, not
                // null - harmless, since ImageUrl (asserted above) and ImageThumbnailGenerator both
                // treat empty the same as null.
                Assert.That(result.ImageContent, Is.Null.Or.Empty);
            }
        }

        [Test]
        public void ShopEditModel_To_Shop_PassesThroughImageContent()
        {
            var editModel = new ShopEditModel
            {
                ImageContent = [1, 2, 3],
                DisplaySequence = []
            };

            var result = _mapper.Map<Shop>(editModel);

            Assert.That(result.ImageContent, Is.EqualTo(editModel.ImageContent).AsCollection);
        }

        [Test]
        public void ShopEditModel_To_Shop_NullImageContent_MapsToEmptyArray_NotTreatedAsAnImage()
        {
            // Regression guard: AutoMapper maps a null source array to an empty array (not null) here.
            // That's fine as long as downstream consumers (ImageUrl/ThumbnailUrl ForMembers,
            // ImageThumbnailGenerator) treat empty the same as null - which they do - so a "no image"
            // shop still ends up with a null ImageUrl / null ImageThumbnail end-to-end.
            var editModel = new ShopEditModel
            {
                ImageContent = null,
                DisplaySequence = []
            };

            var result = _mapper.Map<Shop>(editModel);

            Assert.That(result.ImageContent, Is.Null.Or.Empty);
        }

        [Test]
        public void ShopModel_To_Shop_Ignores_DisplaySequence_On_Destination()
        {
            var originalDisplaySequence = new List<ShopDisplaySequence>
            {
                new() { Value = 10 },
                new() { Value = 20 }
            };

            var destShop = new Shop
            {
                DisplaySequence = new List<ShopDisplaySequence>(originalDisplaySequence)
            };

            var sourceModel = new ShopModel
            {
                Id = Guid.NewGuid(),
                Name = "Updated Shop",
                Index = 5,
                IsSelected = true
            };

            var mappedShop = _mapper.Map(sourceModel, destShop);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(mappedShop.Id, Is.EqualTo(sourceModel.Id));
                Assert.That(mappedShop.Name, Is.EqualTo(sourceModel.Name));
                Assert.That(mappedShop.DisplaySequence, Is.Not.Null);
                Assert.That(mappedShop.DisplaySequence!, Has.Count.EqualTo(originalDisplaySequence.Count));
                Assert.That(mappedShop.DisplaySequence!.Select(x => x.Value), Is.EqualTo(originalDisplaySequence.Select(x => x.Value)));
            }
        }

        [Test]
        public void Shop_To_ShopEditModel_Orders_By_Value_And_Sets_Index()
        {
            var shop = new Shop
            {
                DisplaySequence =
                [
                    new() { Value = 3 },
                    new() { Value = 1 },
                    new() { Value = 2 }
                ]
            };

            var result = _mapper.Map<ShopEditModel>(shop);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.DisplaySequence, Has.Count.EqualTo(3));
                Assert.That(result.DisplaySequence![0].Value, Is.EqualTo(1));
                Assert.That(result.DisplaySequence![1].Value, Is.EqualTo(2));
                Assert.That(result.DisplaySequence![2].Value, Is.EqualTo(3));
                Assert.That(result.DisplaySequence![0].Index, Is.EqualTo(1));
                Assert.That(result.DisplaySequence![1].Index, Is.EqualTo(2));
                Assert.That(result.DisplaySequence![2].Index, Is.EqualTo(3));
            }
        }

        [Test]
        public void ShopEditModel_To_Shop_Maps_DisplaySequence()
        {
            var editModel = new ShopEditModel
            {
                DisplaySequence =
                [
                    new ShopDisplaySequenceEditModel { Value = 1 },
                    new ShopDisplaySequenceEditModel { Value = 2 }
                ]
            };

            var result = _mapper.Map<Shop>(editModel);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.DisplaySequence, Is.Not.Null);
                Assert.That(result.DisplaySequence!, Has.Count.EqualTo(2));
                Assert.That(result.DisplaySequence!.Select(s => s.Value), Is.EqualTo(new[] { 1, 2 }).AsCollection);
            }
        }
    }
}
