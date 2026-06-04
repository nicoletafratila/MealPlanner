using RecipeBook.Shared.Models;

namespace Common.Pagination.Tests
{
    [TestFixture]
    public class PagedListTests
    {
        [Test]
        public void DefaultCtor_Initializes_Empty_Items_And_Metadata()
        {
            // Act
            var pagedList = new PagedList<RecipeModel>();

            // Assert
            using (Assert.EnterMultipleScope())
            {
                using (Assert.EnterMultipleScope())
                {
                    Assert.That(pagedList.Metadata, Is.Not.Null);
                    Assert.That(pagedList.Items, Is.Not.Null);
                }
                using (Assert.EnterMultipleScope())
                {
                    Assert.That(pagedList.Items, Is.Empty);
                    Assert.That(pagedList.Count, Is.EqualTo(0));
                }
                Assert.That(pagedList.HasItems, Is.False);
            }
        }

        [Test]
        public void Ctor_Sets_Items_And_Metadata()
        {
            // Arrange
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();
            var items = new[]
            {
                new RecipeModel { Id = id1 },
                new RecipeModel { Id = id2 }
            };
            var metadata = new Metadata();

            // Act
            var pagedList = new PagedList<RecipeModel>(items, metadata);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                using (Assert.EnterMultipleScope())
                {
                    Assert.That(pagedList.Metadata, Is.SameAs(metadata));
                    Assert.That(pagedList.Items, Is.Not.Null);
                }
                Assert.That(pagedList.Items, Has.Count.EqualTo(2));
                using (Assert.EnterMultipleScope())
                {
                    Assert.That(pagedList.Items[0].Id, Is.EqualTo(id1));
                    Assert.That(pagedList.Items[1].Id, Is.EqualTo(id2));
                    Assert.That(pagedList.Count, Is.EqualTo(2));
                }
                Assert.That(pagedList.HasItems, Is.True);
            }
        }

        [Test]
        public void Ctor_Throws_When_Metadata_Is_Null()
        {
            // Arrange
            IEnumerable<RecipeModel> items = [new RecipeModel()];
            Metadata? metadata = null;

            // Act & Assert
            Assert.That(
                () => new PagedList<RecipeModel>(items, metadata!),
                Throws.TypeOf<ArgumentNullException>()
                      .With.Property(nameof(ArgumentNullException.ParamName)).EqualTo("metadata"));
        }

        [Test]
        public void Ctor_Throws_When_Items_Is_Null()
        {
            // Arrange
            IEnumerable<RecipeModel>? items = null;
            var metadata = new Metadata();

            // Act & Assert
            Assert.That(
                () => new PagedList<RecipeModel>(items!, metadata),
                Throws.TypeOf<ArgumentNullException>()
                      .With.Property(nameof(ArgumentNullException.ParamName)).EqualTo("items"));
        }

        [Test]
        public void Empty_Creates_Empty_List_With_Metadata()
        {
            // Act
            var pagedList = new PagedList<RecipeModel>();

            // Assert
            using (Assert.EnterMultipleScope())
            {
                using (Assert.EnterMultipleScope())
                {
                    Assert.That(pagedList.Metadata, Is.Not.Null);
                    Assert.That(pagedList.Metadata.PageNumber, Is.EqualTo(1));
                    Assert.That(pagedList.Metadata.PageSize, Is.EqualTo(1));
                    Assert.That(pagedList.Metadata.TotalCount, Is.EqualTo(0));
                    Assert.That(pagedList.Metadata.TotalPages, Is.EqualTo(0));

                    Assert.That(pagedList.Items, Is.Empty);
                    Assert.That(pagedList.Count, Is.EqualTo(0));
                }

                Assert.That(pagedList.HasItems, Is.False);
            }
        }
    }
}