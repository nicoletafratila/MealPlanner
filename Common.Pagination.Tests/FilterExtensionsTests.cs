using BlazorBootstrap;
using RecipeBook.Shared.Models;

namespace Common.Pagination.Tests
{
    [TestFixture]
    public class FilterExtensionsTests
    {
        private sealed record DateItem(DateTime? CreatedAt);

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
        public void NotEquals_String_FiltersCorrectly()
        {
            var data = new[]
            {
                new RecipeModel { Name = "Alice" },
                new RecipeModel { Name = "Bob" },
                new RecipeModel { Name = "Alice" }
            };

            var filter = new FilterItem(
                propertyName: nameof(RecipeModel.Name),
                value: "Alice",
                @operator: FilterOperator.NotEquals,
                stringComparison: StringComparison.OrdinalIgnoreCase);

            var predicate = filter.ConvertFilterItemToFunc<RecipeModel>();
            var result = data.Where(predicate).Select(x => x.Name).ToArray();

            Assert.That(result, Is.EquivalentTo(new[] { "Bob" }));
        }

        [Test]
        public void GreaterThan_Int_FiltersCorrectly()
        {
            var data = new[]
            {
                new RecipeModel { Id = 1 },
                new RecipeModel { Id = 3 },
                new RecipeModel { Id = 5 }
            };

            var filter = new FilterItem(
                propertyName: nameof(RecipeModel.Id),
                value: "3",
                @operator: FilterOperator.GreaterThan,
                stringComparison: StringComparison.OrdinalIgnoreCase);

            var predicate = filter.ConvertFilterItemToFunc<RecipeModel>();
            var result = data.Where(predicate).Select(x => x.Id).ToArray();

            Assert.That(result, Is.EquivalentTo(new[] { 5 }));
        }

        [Test]
        public void GreaterThanOrEquals_Int_FiltersCorrectly()
        {
            var data = new[]
            {
                new RecipeModel { Id = 1 },
                new RecipeModel { Id = 3 },
                new RecipeModel { Id = 5 }
            };

            var filter = new FilterItem(
                propertyName: nameof(RecipeModel.Id),
                value: "3",
                @operator: FilterOperator.GreaterThanOrEquals,
                stringComparison: StringComparison.OrdinalIgnoreCase);

            var predicate = filter.ConvertFilterItemToFunc<RecipeModel>();
            var result = data.Where(predicate).Select(x => x.Id).ToArray();

            Assert.That(result, Is.EquivalentTo(new[] { 3, 5 }));
        }

        [Test]
        public void LessThan_Int_FiltersCorrectly()
        {
            var data = new[]
            {
                new RecipeModel { Id = 1 },
                new RecipeModel { Id = 3 },
                new RecipeModel { Id = 5 }
            };

            var filter = new FilterItem(
                propertyName: nameof(RecipeModel.Id),
                value: "3",
                @operator: FilterOperator.LessThan,
                stringComparison: StringComparison.OrdinalIgnoreCase);

            var predicate = filter.ConvertFilterItemToFunc<RecipeModel>();
            var result = data.Where(predicate).Select(x => x.Id).ToArray();

            Assert.That(result, Is.EquivalentTo(new[] { 1 }));
        }

        [Test]
        public void LessThanOrEquals_Int_FiltersCorrectly()
        {
            var data = new[]
            {
                new RecipeModel { Id = 1 },
                new RecipeModel { Id = 3 },
                new RecipeModel { Id = 5 }
            };

            var filter = new FilterItem(
                propertyName: nameof(RecipeModel.Id),
                value: "3",
                @operator: FilterOperator.LessThanOrEquals,
                stringComparison: StringComparison.OrdinalIgnoreCase);

            var predicate = filter.ConvertFilterItemToFunc<RecipeModel>();
            var result = data.Where(predicate).Select(x => x.Id).ToArray();

            Assert.That(result, Is.EquivalentTo(new[] { 1, 3 }));
        }

        [Test]
        public void GreaterThanOrEquals_NullableDateTime_ExcludesNulls_AndFiltersCorrectly()
        {
            var threshold = new DateTime(2026, 5, 10);
            var data = new[]
            {
                new DateItem(new DateTime(2026, 5, 9)),
                new DateItem(new DateTime(2026, 5, 10)),
                new DateItem(new DateTime(2026, 5, 11)),
                new DateItem(null)
            };

            var filter = new FilterItem(
                propertyName: nameof(DateItem.CreatedAt),
                value: threshold.ToString(),
                @operator: FilterOperator.GreaterThanOrEquals,
                stringComparison: StringComparison.OrdinalIgnoreCase);

            var predicate = filter.ConvertFilterItemToFunc<DateItem>();
            var result = data.Where(predicate).Select(x => x.CreatedAt).ToArray();

            Assert.That(result, Is.EquivalentTo(new DateTime?[]
            {
                new DateTime(2026, 5, 10),
                new DateTime(2026, 5, 11)
            }));
        }

        [Test]
        public void LessThan_NullableDateTime_ExcludesNulls_AndFiltersCorrectly()
        {
            var threshold = new DateTime(2026, 5, 18);
            var data = new[]
            {
                new DateItem(new DateTime(2026, 5, 10)),
                new DateItem(new DateTime(2026, 5, 18)),
                new DateItem(new DateTime(2026, 5, 20)),
                new DateItem(null)
            };

            var filter = new FilterItem(
                propertyName: nameof(DateItem.CreatedAt),
                value: threshold.ToString(),
                @operator: FilterOperator.LessThan,
                stringComparison: StringComparison.OrdinalIgnoreCase);

            var predicate = filter.ConvertFilterItemToFunc<DateItem>();
            var result = data.Where(predicate).Select(x => x.CreatedAt).ToArray();

            Assert.That(result, Is.EquivalentTo(new DateTime?[] { new DateTime(2026, 5, 10) }));
        }

        [Test]
        public void UnsupportedOperator_Throws_NotSupportedException()
        {
            var filter = new FilterItem(
                propertyName: nameof(RecipeModel.Id),
                value: "1",
                @operator: (FilterOperator)999,
                stringComparison: StringComparison.OrdinalIgnoreCase);

            Assert.That(
                () => filter.ConvertFilterItemToFunc<RecipeModel>(),
                Throws.TypeOf<NotSupportedException>());
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