using System.Linq.Expressions;
using BlazorBootstrap;
using RecipeBook.Shared.Models;

namespace Common.Pagination.Tests
{
    [TestFixture]
    public class EnumerableExtensionsApplySortingTests
    {
        private static SortingItem<RecipeModel> CreateSortingItem(
            string sortString,
            SortDirection direction)
        {
            Expression<Func<RecipeModel, IComparable>> keySelector = x => x.Name;
            return new SortingItem<RecipeModel>(sortString, keySelector, direction);
        }

        [Test]
        public void ApplySorting_Throws_When_Source_Is_Null()
        {
            IQueryable<RecipeModel>? source = null;
            var sorting = new[] { CreateSortingItem(nameof(RecipeModel.Name), SortDirection.Ascending) };

            Assert.That(
                () => source!.ApplySorting(sorting),
                Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void ApplySorting_Returns_Source_When_SortingItems_Is_Null()
        {
            var data = new[]
            {
                new RecipeModel { Id = 2, Name = "B" },
                new RecipeModel { Id = 1, Name = "A" }
            }.AsQueryable();

            var result = data.ApplySorting(null);

            Assert.That(result.ToArray(), Is.EqualTo(data.ToArray()));
        }

        [Test]
        public void ApplySorting_Returns_Source_When_SortingItems_Is_Empty()
        {
            var data = new[]
            {
                new RecipeModel { Id = 2, Name = "B" },
                new RecipeModel { Id = 1, Name = "A" }
            }.AsQueryable();

            var result = data.ApplySorting([]);

            Assert.That(result.ToArray(), Is.EqualTo(data.ToArray()));
        }

        [Test]
        public void ApplySorting_Throws_When_SortString_Is_Empty()
        {
            var data = new[]
            {
                new RecipeModel { Id = 1, Name = "A" }
            }.AsQueryable();

            var sorting = new[]
            {
                CreateSortingItem(string.Empty, SortDirection.Ascending)
            };

            Assert.That(
                () => data.ApplySorting(sorting),
                Throws.TypeOf<ArgumentException>()
                      .With.Message.Contains("SortString cannot be null or empty"));
        }

        [Test]
        public void ApplySorting_Throws_When_Property_Does_Not_Exist()
        {
            var data = new[]
            {
                new RecipeModel { Id = 1, Name = "A" }
            }.AsQueryable();

            var sorting = new[]
            {
                CreateSortingItem("DoesNotExist", SortDirection.Ascending)
            };

            Assert.That(
                () => data.ApplySorting(sorting),
                Throws.TypeOf<ArgumentException>()
                      .With.Message.Contains("DoesNotExist"));
        }

        [Test]
        public void ApplySorting_Sorts_By_Single_Property_Ascending()
        {
            var data = new[]
            {
                new RecipeModel { Id = 3, Name = "Charlie" },
                new RecipeModel { Id = 1, Name = "Alice" },
                new RecipeModel { Id = 2, Name = "Bob" }
            }.AsQueryable();

            var sorting = new[]
            {
                CreateSortingItem(nameof(RecipeModel.Name), SortDirection.Ascending)
            };

            var result = data.ApplySorting(sorting).ToArray();

            Assert.That(result.Select(x => x.Name), Is.EqualTo(new[] { "Alice", "Bob", "Charlie" }));
        }

        [Test]
        public void ApplySorting_Sorts_By_Single_Property_Descending()
        {
            var data = new[]
            {
                new RecipeModel { Id = 3, Name = "Charlie" },
                new RecipeModel { Id = 1, Name = "Alice" },
                new RecipeModel { Id = 2, Name = "Bob" }
            }.AsQueryable();

            var sorting = new[]
            {
                CreateSortingItem(nameof(RecipeModel.Name), SortDirection.Descending)
            };

            var result = data.ApplySorting(sorting).ToArray();

            Assert.That(result.Select(x => x.Name), Is.EqualTo(new[] { "Charlie", "Bob", "Alice" }));
        }

        [Test]
        public void ApplySorting_Is_CaseInsensitive_For_Property_Names()
        {
            var data = new[]
            {
                new RecipeModel { Id = 2, Name = "B" },
                new RecipeModel { Id = 1, Name = "A" }
            }.AsQueryable();

            var sorting = new[]
            {
                CreateSortingItem("name", SortDirection.Ascending)
            };

            var result = data.ApplySorting(sorting).ToArray();

            Assert.That(result.Select(x => x.Name), Is.EqualTo(new[] { "A", "B" }));
        }
    }
}