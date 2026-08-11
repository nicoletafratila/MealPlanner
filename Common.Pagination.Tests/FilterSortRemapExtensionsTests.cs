namespace Common.Pagination.Tests
{
    [TestFixture]
    public class FilterSortRemapExtensionsTests
    {
        [Test]
        public void RemapPropertyName_Filters_Null_ReturnsNull()
        {
            IEnumerable<FilterItem>? filters = null;

            var result = filters.RemapPropertyName("From", "To");

            Assert.That(result, Is.Null);
        }

        [Test]
        public void RemapPropertyName_Filters_RenamesMatchingEntry_CaseInsensitive()
        {
            var filters = new[]
            {
                new FilterItem("recipecategoryname", "Main", FilterOperator.Contains),
                new FilterItem("Name", "Cake", FilterOperator.Contains)
            };

            var result = filters.RemapPropertyName("RecipeCategoryName", "RecipeCategory.Name")!.ToArray();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Has.Length.EqualTo(2));
                Assert.That(result[0].PropertyName, Is.EqualTo("RecipeCategory.Name"));
                Assert.That(result[0].Value, Is.EqualTo("Main"));
                Assert.That(result[0].Operator, Is.EqualTo(FilterOperator.Contains));
                Assert.That(result[1].PropertyName, Is.EqualTo("Name"));
            }
        }

        [Test]
        public void RemapPropertyName_Filters_NoMatch_LeavesEntriesUnchanged()
        {
            var filters = new[] { new FilterItem("Name", "Cake", FilterOperator.Contains) };

            var result = filters.RemapPropertyName("DoesNotExist", "Whatever")!.ToArray();

            Assert.That(result[0].PropertyName, Is.EqualTo("Name"));
        }

        [Test]
        public void RemapPropertyName_Sorting_Null_ReturnsNull()
        {
            IEnumerable<SortingModel>? sorting = null;

            var result = sorting.RemapPropertyName("From", "To");

            Assert.That(result, Is.Null);
        }

        [Test]
        public void RemapPropertyName_Sorting_RenamesMatchingEntry_CaseInsensitive_PreservesDirection()
        {
            var sorting = new[]
            {
                new SortingModel { PropertyName = "productcategoryname", Direction = SortDirection.Descending }
            };

            var result = sorting.RemapPropertyName("ProductCategoryName", "ProductCategory.Name")!.ToArray();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Has.Length.EqualTo(1));
                Assert.That(result[0].PropertyName, Is.EqualTo("ProductCategory.Name"));
                Assert.That(result[0].Direction, Is.EqualTo(SortDirection.Descending));
            }
        }

        [Test]
        public void TryExtractBooleanFlag_NullFilters_ReturnsFalse_AndNullRemaining()
        {
            IEnumerable<FilterItem>? filters = null;

            var found = filters.TryExtractBooleanFlag("ThumbnailOnly", out var remaining);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(found, Is.False);
                Assert.That(remaining, Is.Null);
            }
        }

        [Test]
        public void TryExtractBooleanFlag_NoMatch_ReturnsFalse_AndOriginalReferenceUnchanged()
        {
            var filters = new List<FilterItem> { new("Name", "Cake", FilterOperator.Contains) };

            var found = filters.TryExtractBooleanFlag("ThumbnailOnly", out var remaining);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(found, Is.False);
                Assert.That(remaining, Is.SameAs(filters));
            }
        }

        [Test]
        public void TryExtractBooleanFlag_MatchTrue_ReturnsTrue_AndRemovesEntryFromRemaining()
        {
            var filters = new List<FilterItem>
            {
                new("Name", "Cake", FilterOperator.Contains),
                new("ThumbnailOnly", true, FilterOperator.Equals)
            };

            var found = filters.TryExtractBooleanFlag("thumbnailonly", out var remaining);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(found, Is.True);
                Assert.That(remaining!.Select(f => f.PropertyName), Is.EqualTo(new[] { "Name" }));
            }
        }

        [Test]
        public void TryExtractBooleanFlag_MatchFalse_ReturnsFalse_AndRemovesEntryFromRemaining()
        {
            var filters = new List<FilterItem> { new("ThumbnailOnly", false, FilterOperator.Equals) };

            var found = filters.TryExtractBooleanFlag("ThumbnailOnly", out var remaining);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(found, Is.False);
                Assert.That(remaining, Is.Empty);
            }
        }

        [Test]
        public void TryExtractBooleanFlag_MatchWithNonBooleanValue_ReturnsFalse_AndRemovesEntryFromRemaining()
        {
            var filters = new List<FilterItem> { new("ThumbnailOnly", "true", FilterOperator.Equals) };

            var found = filters.TryExtractBooleanFlag("ThumbnailOnly", out var remaining);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(found, Is.False);
                Assert.That(remaining, Is.Empty);
            }
        }
    }
}
