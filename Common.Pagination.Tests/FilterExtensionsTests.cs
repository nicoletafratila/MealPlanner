using BlazorBootstrap;
using RecipeBook.Shared.Models;

namespace Common.Pagination.Tests
{
    [TestFixture]
    public class FilterExtensionsTests
    {
        [Test]
        public void ConvertFilterItemToFunc_Throws_When_Filter_Is_Null()
        {
            FilterItem? filter = null;

            Assert.That(
                () => filter!.ConvertFilterItemToFunc<RecipeModel>(),
                Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void ConvertFilterItemToFunc_Throws_When_PropertyName_Is_Empty()
        {
            var filter = new FilterItem(
                propertyName: "",
                value: "1",
                @operator: FilterOperator.Equals,
                stringComparison: StringComparison.OrdinalIgnoreCase);

            Assert.That(
              () => filter.ConvertFilterItemToFunc<RecipeModel>(),
              Throws.TypeOf<ArgumentException>().With.Message.Contains("PropertyName cannot be null or empty."));
        }

        [Test]
        public void ConvertFilterItemToFunc_Throws_When_Property_Not_Found()
        {
            var filter = new FilterItem(
                propertyName: "DoesNotExist",
                value: "1",
                @operator: FilterOperator.Equals,
                stringComparison: StringComparison.OrdinalIgnoreCase);

            Assert.That(
                () => filter.ConvertFilterItemToFunc<RecipeModel>(),
                Throws.TypeOf<ArgumentException>()
                      .With.Message.Contains("DoesNotExist"));
        }

        [Test]
        public void Equals_Int_Works_With_Exact_Type()
        {
            var data = new[]
            {
                new RecipeModel { Name = "John", RecipeCategoryName = "a" },
                new RecipeModel { Name = "Jane", RecipeCategoryName = "b" }
            };

            var filter = new FilterItem(
                propertyName: nameof(RecipeModel.RecipeCategoryName),
                value: "a",
                @operator: FilterOperator.Equals,
                stringComparison: StringComparison.OrdinalIgnoreCase);

            var predicate = filter.ConvertFilterItemToFunc<RecipeModel>();
            var result = data.Where(predicate).ToArray();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Has.Length.EqualTo(1));
                Assert.That(result[0].Name, Is.EqualTo("John"));
            }
        }

        [Test]
        public void Equals_Int_Works_With_Convertible_String_Value()
        {
            var data = new[]
            {
                new RecipeModel { Name = "John", RecipeCategoryName = "a" },
                new RecipeModel { Name = "Jane", RecipeCategoryName = "b" }
            };

            var filter = new FilterItem(
                propertyName: nameof(RecipeModel.RecipeCategoryName),
                value: "b",
                @operator: FilterOperator.Equals,
                stringComparison: StringComparison.OrdinalIgnoreCase);

            var predicate = filter.ConvertFilterItemToFunc<RecipeModel>();
            var result = data.Where(predicate).ToArray();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Has.Length.EqualTo(1));
                Assert.That(result[0].Name, Is.EqualTo("Jane"));
            }
        }

        [Test]
        public void Equals_Enum_Works_With_String_Value_CaseInsensitive()
        {
            var data = new[]
            {
                new RecipeModel { Name = "John", RecipeCategoryName = "A" },
                new RecipeModel { Name = "Jane", RecipeCategoryName = "b" }
            };

            var filter = new FilterItem(
                propertyName: nameof(RecipeModel.RecipeCategoryName),
                value: "a",
                @operator: FilterOperator.Equals,
                stringComparison: StringComparison.OrdinalIgnoreCase);

            var predicate = filter.ConvertFilterItemToFunc<RecipeModel>();
            var result = data.Where(predicate).ToArray();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Has.Length.EqualTo(1));
                Assert.That(result[0].Name, Is.EqualTo("John"));
            }
        }

        [Test]
        public void Contains_Works_CaseInsensitive_And_Ignores_Null_Values()
        {
            var data = new[]
            {
                new RecipeModel { Name = "John", RecipeCategoryName = "A" },
                new RecipeModel { Name = null, RecipeCategoryName = "b" },
                new RecipeModel { Name = "Joanna",  RecipeCategoryName = "c" }
            };

            var filter = new FilterItem(
                propertyName: nameof(RecipeModel.Name),
                value: "an",
                @operator: FilterOperator.Contains,
                stringComparison: StringComparison.OrdinalIgnoreCase);

            var predicate = filter.ConvertFilterItemToFunc<RecipeModel>();
            var result = data.Where(predicate).Select(x => x.Name).ToArray();

            Assert.That(result, Is.EquivalentTo(new[] { "Joanna" }));
        }

        [Test]
        public void StartsWith_Works_CaseInsensitive()
        {
            var data = new[]
            {
                new RecipeModel { Name = "Alice" },
                new RecipeModel { Name = "alex" },
                new RecipeModel { Name = "Bob" }
            };

            var filter = new FilterItem(
                propertyName: nameof(RecipeModel.Name),
                value: "al",
                @operator: FilterOperator.StartsWith,
                stringComparison: StringComparison.OrdinalIgnoreCase);

            var predicate = filter.ConvertFilterItemToFunc<RecipeModel>();
            var result = data.Where(predicate).Select(x => x.Name).ToArray();

            Assert.That(result, Is.EquivalentTo(new[] { "Alice", "alex" }));
        }

        [Test]
        public void EndsWith_Works_CaseInsensitive()
        {
            var data = new[]
            {
                new RecipeModel { Name = "Alice" },
                new RecipeModel { Name = "Nice" },
                new RecipeModel { Name = "Bob" }
            };

            var filter = new FilterItem(
                propertyName: nameof(RecipeModel.Name),
                value: "ice",
                @operator: FilterOperator.EndsWith,
                stringComparison: StringComparison.OrdinalIgnoreCase);

            var predicate = filter.ConvertFilterItemToFunc<RecipeModel>();
            var result = data.Where(predicate).Select(x => x.Name).ToArray();

            Assert.That(result, Is.EquivalentTo(new[] { "Alice", "Nice" }));
        }

        [Test]
        public void String_Operator_On_NonString_Property_Throws()
        {
            var filter = new FilterItem(
                propertyName: nameof(RecipeModel.Id),
                value: "3",
                @operator: FilterOperator.Contains,
                stringComparison: StringComparison.OrdinalIgnoreCase);

            Assert.That(
                () => filter.ConvertFilterItemToFunc<RecipeModel>(),
                Throws.TypeOf<NotSupportedException>().With.Message.Contains("only supported for string properties"));
        }

        [Test]
        public void String_Operator_With_Null_Value_Throws()
        {
            var filter = new FilterItem(
                propertyName: nameof(RecipeModel.Name),
                value: null,
                @operator: FilterOperator.Contains,
                stringComparison: StringComparison.OrdinalIgnoreCase);

            Assert.That(
                () => filter.ConvertFilterItemToFunc<RecipeModel>(),
                Throws.TypeOf<ArgumentException>()
                      .With.Message.Contains("cannot be null"));
        }
    }
}