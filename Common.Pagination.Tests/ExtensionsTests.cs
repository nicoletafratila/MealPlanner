using RecipeBook.Shared.Models;

namespace Common.Pagination.Tests
{
    [TestFixture]
    public class ExtensionsToPagedListTests
    {
        [Test]
        public void ToPagedList_Throws_When_Source_Is_Null()
        {
            IEnumerable<RecipeModel>? source = null;

            Assert.That(
                () => Extensions.ToPagedList(source!, 1, 10),
                Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void ToPagedList_Throws_When_PageNumber_Is_Invalid()
        {
            var source = Enumerable.Range(1, 5)
                .Select(i => new RecipeModel { Name = i.ToString() });

            Assert.That(
                () => source.ToPagedList(0, 10),
                Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void ToPagedList_Throws_When_PageSize_Is_Invalid()
        {
            var source = Enumerable.Range(1, 5)
                .Select(i => new RecipeModel { Name = i.ToString() });

            Assert.That(
                () => source.ToPagedList(1, 0),
                Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void ToPagedList_First_Page_Fills_Correct_Items_Metadata_And_Indexes()
        {
            // Arrange
            var source = Enumerable.Range(1, 25)
                .Select(i => new RecipeModel { Name = i.ToString() });

            const int pageNumber = 1;
            const int pageSize = 10;

            // Act
            var paged = source.ToPagedList(pageNumber, pageSize);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(paged.Items, Has.Count.EqualTo(10));
                Assert.Multiple(() =>
                {
                    Assert.That(paged.Items.Select(x => x.Name), Is.EqualTo(Enumerable.Range(1, 10).Select(i => i.ToString())));
                    Assert.That(paged.Metadata.PageNumber, Is.EqualTo(pageNumber));
                    Assert.That(paged.Metadata.PageSize, Is.EqualTo(pageSize));
                    Assert.That(paged.Metadata.TotalCount, Is.EqualTo(25));
                    Assert.That(paged.Metadata.TotalPages, Is.EqualTo(3));
                    Assert.That(paged.Items.First().Index, Is.EqualTo(1));
                    Assert.That(paged.Items.Last().Index, Is.EqualTo(10));
                });
            }
        }

        [Test]
        public void ToPagedList_Middle_Page_Fills_Correct_Items_And_Indexes()
        {
            var source = Enumerable.Range(1, 25)
                .Select(i => new RecipeModel { Name = i.ToString() });

            const int pageNumber = 2;
            const int pageSize = 10;

            var paged = source.ToPagedList(pageNumber, pageSize);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(paged.Items, Has.Count.EqualTo(10));
                Assert.Multiple(() =>
                {
                    Assert.That(paged.Items.Select(x => x.Name), Is.EqualTo(Enumerable.Range(11, 10).Select(i => i.ToString())));
                    Assert.That(paged.Items.First().Index, Is.EqualTo(11));
                    Assert.That(paged.Items.Last().Index, Is.EqualTo(20));
                });
            }
        }

        [Test]
        public void ToPagedList_Last_Page_Partial_Fills_Correct_Items_And_Indexes()
        {
            var source = Enumerable.Range(1, 25)
                .Select(i => new RecipeModel { Name = i.ToString() });

            const int pageNumber = 3;
            const int pageSize = 10;

            var paged = source.ToPagedList(pageNumber, pageSize);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(paged.Items, Has.Count.EqualTo(5));
                Assert.Multiple(() =>
                {
                    Assert.That(paged.Items.Select(x => x.Name), Is.EqualTo(Enumerable.Range(21, 5).Select(i => i.ToString())));
                    Assert.That(paged.Items.First().Index, Is.EqualTo(21));
                    Assert.That(paged.Items.Last().Index, Is.EqualTo(25));
                });
            }
        }

        [Test]
        public void ToPagedList_PageBeyondEnd_Returns_Empty_Items_But_Metadata_Still_Computed()
        {
            var source = Enumerable.Range(1, 10)
                .Select(i => new RecipeModel { Name = i.ToString() });

            const int pageNumber = 3;
            const int pageSize = 10;

            var paged = source.ToPagedList(pageNumber, pageSize);

            using (Assert.EnterMultipleScope())
            {
                Assert.Multiple(() =>
                {
                    Assert.That(paged.Items, Is.Empty);
                    Assert.That(paged.Metadata.TotalCount, Is.EqualTo(10));
                    Assert.That(paged.Metadata.TotalPages, Is.EqualTo(1));
                    Assert.That(paged.Metadata.PageNumber, Is.EqualTo(pageNumber));
                });
            }
        }

        [Test]
        public void ToPagedList_Does_Not_Reenumerate_Source_More_Than_Necessary_When_IList()
        {
            var list = new List<RecipeModel>
            {
                new() { Name = "1" },
                new() { Name = "2" },
                new() { Name = "3" }
            };

            var paged = list.ToPagedList(1, 2);

            using (Assert.EnterMultipleScope())
            {
                Assert.Multiple(() =>
                {
                    Assert.That(paged.Items.Select(x => x.Name), Is.EqualTo(new[] { "1", "2" }));
                    Assert.That(paged.Metadata.TotalCount, Is.EqualTo(3));
                });
            }
        }
    }
}
