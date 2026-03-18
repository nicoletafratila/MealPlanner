using BlazorBootstrap;
using RecipeBook.Shared.Models;

namespace Common.Pagination.Tests
{
    [TestFixture]
    public class QueryParametersTests
    {
        [Test]
        public void Ctor_Normalizes_PageNumber_And_PageSize()
        {
            // Act
            var parameters = new QueryParameters<RecipeModel>();

            // Assert: we only assert invariants we control
            using (Assert.EnterMultipleScope())
            {
                Assert.Multiple(() =>
                {
                    Assert.That(parameters.PageNumber, Is.GreaterThanOrEqualTo(1));
                    Assert.That(parameters.PageSize, Is.GreaterThan(0));
                });
            }
        }

        [Test]
        public void FromModel_Throws_When_Model_Is_Null()
        {
            SortingModel? model = null;

            Assert.That(
                () => QueryParameters<RecipeModel>.FromModel(model!),
                Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void FromModel_Throws_When_PropertyName_Is_Empty()
        {
            var model = new SortingModel
            {
                PropertyName = "",
                Direction = SortDirection.Ascending
            };

            Assert.That(
                () => QueryParameters<RecipeModel>.FromModel(model),
                Throws.TypeOf<InvalidOperationException>()
                      .With.Message.Contains("Sorting property name must be provided"));
        }

        [Test]
        public void FromModel_Throws_When_Property_Does_Not_Exist()
        {
            var model = new SortingModel
            {
                PropertyName = "DoesNotExist",
                Direction = SortDirection.Ascending
            };

            Assert.That(
                () => QueryParameters<RecipeModel>.FromModel(model),
                Throws.TypeOf<InvalidOperationException>()
                      .With.Message.Contains("DoesNotExist"));
        }

        [Test]
        public void FromModel_Throws_When_Property_Is_Not_IComparable()
        {
            var model = new SortingModel
            {
                PropertyName = "test",
                Direction = SortDirection.Ascending
            };

            Assert.That(
                () => QueryParameters<RecipeModel>.FromModel(model),
                Throws.TypeOf<InvalidOperationException>()
                      .With.Message.Contains("Property 'test' not found in type RecipeModel."));
        }

        [Test]
        public void FromModel_And_ToModel_RoundTrip_Preserves_Name_And_Direction_For_ValueType()
        {
            // Arrange
            var original = new SortingModel
            {
                PropertyName = nameof(RecipeModel.Id),
                Direction = SortDirection.Descending
            };

            // Act
            var sortingItem = QueryParameters<RecipeModel>.FromModel(original);
            var roundTripped = QueryParameters<RecipeModel>.ToModel(sortingItem);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.Multiple(() =>
                {
                    Assert.That(roundTripped.PropertyName, Is.EqualTo(nameof(RecipeModel.Id)));
                    Assert.That(roundTripped.Direction, Is.EqualTo(SortDirection.Descending));
                });
            }
        }

        [Test]
        public void FromModel_And_ToModel_RoundTrip_Normalizes_PropertyName_Casing()
        {
            // Arrange: intentionally wrong casing
            var original = new SortingModel
            {
                PropertyName = "name",
                Direction = SortDirection.Ascending
            };

            // Act
            var sortingItem = QueryParameters<RecipeModel>.FromModel(original);
            var roundTripped = QueryParameters<RecipeModel>.ToModel(sortingItem);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.Multiple(() =>
                {
                    Assert.That(roundTripped.PropertyName, Is.EqualTo(nameof(RecipeModel.Name)));
                    Assert.That(roundTripped.Direction, Is.EqualTo(SortDirection.Ascending));
                });
            }
        }
    }
}