using System.Text;
using System.Text.RegularExpressions;
using MealPlanner.Shared.Models;
using RecipeBook.Shared.Models;

namespace Common.Services.Pdf.Tests
{
    public class MealPlanPdfBuilderTests
    {
        private static readonly string OnePixelJpegDataUri =
            "data:image/jpg;base64,/9j/4AAQSkZJRgABAQEAYABgAAD/2wBDAAMCAgICAgMCAgIDAwMDBAYEBAQEBAgGBgUGCQgKCgkICQkKDA8MCgsOCwkJDRENDg8QEBEQCgwSExIQEw8QEBD/2wBDAQMDAwQDBAgEBAgQCwkLEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBD/wAARCAABAAEDASIAAhEBAxEB/8QAHwAAAQUBAQEBAQEAAAAAAAAAAAECAwQFBgcICQoL/8QAtRAAAgEDAwIEAwUFBAQAAAF9AQIDAAQRBRIhMUEGE1FhByJxFDKBkaEII0KxwRVS0fAkM2JyggkKFhcYGRolJicoKSo0NTY3ODk6Q0RFRkdISUpTVFVWV1hZWmNkZWZnaGlqc3R1dnd4eXqDhIWGh4iJipKTlJWWl5iZmqKjpKWmp6ipqrKztLW2t7i5usLDxMXGx8jJytLT1NXW19jZ2uHi4+Tl5ufo6erx8vP09fb3+Pn6/8QAHwEAAwEBAQEBAQEBAQAAAAAAAAECAwQFBgcICQoL/8QAtREAAgECBAQDBAcFBAQAAQJ3AAECAxEEBSExBhJBUQdhcRMiMoEIFEKRobHBCSMzUvAVYnLRChYkNOEl8RcYGRomJygpKjU2Nzg5OkNERUZHSElKU1RVVldYWVpjZGVmZ2hpanN0dXZ3eHl6goOEhYaHiImKkpOUlZaXmJmaoqOkpaanqKmqsrO0tba3uLm6wsPExcbHyMnK0tPU1dbX2Nna4uPk5ebn6Onq8vP09fb3+Pn6/9oADAMBAAIRAxEAPwD+/KKKKACiiigD/9k=";

        private readonly MealPlanPdfBuilder _builder = new();

        [Test]
        public void Build_EmptyRecipeList_ReturnsValidPdf()
        {
            var mealPlan = new MealPlanEditModel { Name = "Weekly Menu", Recipes = [] };

            var bytes = _builder.Build(mealPlan);

            AssertIsValidPdf(bytes);
        }

        [Test]
        public void Build_NullRecipesCollection_ReturnsValidPdf()
        {
            var mealPlan = new MealPlanEditModel { Name = "Weekly Menu", Recipes = null };

            var bytes = _builder.Build(mealPlan);

            AssertIsValidPdf(bytes);
        }

        [Test]
        public void Build_SingleRecipeWithImage_ReturnsValidPdf()
        {
            var mealPlan = new MealPlanEditModel
            {
                Name = "Weekly Menu",
                Recipes =
                [
                    new RecipeModel(Guid.NewGuid(), "Pasta Carbonara")
                    {
                        ImageUrl = OnePixelJpegDataUri,
                        RecipeCategoryName = "Dinner"
                    }
                ]
            };

            var bytes = _builder.Build(mealPlan);

            AssertIsValidPdf(bytes);
        }

        [TestCase(1)]
        [TestCase(3)]
        [TestCase(6)]
        [TestCase(12)]
        [TestCase(20)]
        [TestCase(30)]
        [TestCase(50)]
        public void Build_VariousRecipeCountsWithImages_FitsOnOnePage(int recipeCount)
        {
            var recipes = Enumerable.Range(1, recipeCount)
                .Select(i => new RecipeModel(Guid.NewGuid(), $"Recipe {i}")
                {
                    ImageUrl = OnePixelJpegDataUri,
                    RecipeCategoryName = "Category " + i
                })
                .ToList();

            var mealPlan = new MealPlanEditModel { Name = "Big Menu", Recipes = recipes };

            var bytes = _builder.Build(mealPlan);

            AssertIsValidPdf(bytes);
            Assert.That(CountPages(bytes), Is.EqualTo(1), $"Expected a single page for {recipeCount} recipes.");
        }

        [TestCase(1)]
        [TestCase(12)]
        [TestCase(40)]
        public void Build_VariousRecipeCountsWithoutImages_FitsOnOnePage(int recipeCount)
        {
            var recipes = Enumerable.Range(1, recipeCount)
                .Select(i => new RecipeModel(Guid.NewGuid(), $"Recipe {i}") { RecipeCategoryName = "Category " + i })
                .ToList();

            var mealPlan = new MealPlanEditModel { Name = "Big Menu", Recipes = recipes };

            var bytes = _builder.Build(mealPlan);

            AssertIsValidPdf(bytes);
            Assert.That(CountPages(bytes), Is.EqualTo(1), $"Expected a single page for {recipeCount} recipes.");
        }

        [Test]
        public void Build_LongRecipeNames_WrapInsteadOfOverflowingAndStillFitOnOnePage()
        {
            var recipes = Enumerable.Range(1, 40)
                .Select(i => new RecipeModel(Guid.NewGuid(), $"A Really Long Recipe Name That Should Wrap Across Lines Number {i}")
                {
                    ImageUrl = OnePixelJpegDataUri,
                    RecipeCategoryName = "Category " + i
                })
                .ToList();

            var mealPlan = new MealPlanEditModel { Name = "Big Menu", Recipes = recipes };

            var bytes = _builder.Build(mealPlan);

            AssertIsValidPdf(bytes);
            Assert.That(CountPages(bytes), Is.EqualTo(1));
        }

        [Test]
        public void Build_ManyRecipesWithCategoryNames_CategoryWrapsInsteadOfOverflowingAndStillFitsOnOnePage()
        {
            // At high recipe counts the layout uses many narrow columns; even a short category
            // name can wrap across several lines in a narrow column unless clamped, which
            // previously overflowed the fixed card height and forced extra pages.
            var recipes = Enumerable.Range(1, 70)
                .Select(i => new RecipeModel(Guid.NewGuid(), $"Recipe {i}")
                {
                    ImageUrl = OnePixelJpegDataUri,
                    RecipeCategoryName = "Category " + i
                })
                .ToList();

            var mealPlan = new MealPlanEditModel { Name = "Big Menu", Recipes = recipes };

            var bytes = _builder.Build(mealPlan);

            AssertIsValidPdf(bytes);
            Assert.That(CountPages(bytes), Is.EqualTo(1));
        }

        [Test]
        public void Build_FewAndManyRecipes_BothFitOnOnePage()
        {
            var smallMealPlan = new MealPlanEditModel
            {
                Name = "Small Menu",
                Recipes =
                [
                    new RecipeModel(Guid.NewGuid(), "Pasta Carbonara") { ImageUrl = OnePixelJpegDataUri },
                    new RecipeModel(Guid.NewGuid(), "Greek Salad") { ImageUrl = OnePixelJpegDataUri }
                ]
            };

            var bigMealPlan = new MealPlanEditModel
            {
                Name = "Big Menu",
                Recipes = Enumerable.Range(1, 50)
                    .Select(i => new RecipeModel(Guid.NewGuid(), $"Recipe {i}") { ImageUrl = OnePixelJpegDataUri })
                    .ToList()
            };

            var smallMenuBytes = _builder.Build(smallMealPlan);
            var bigMenuBytes = _builder.Build(bigMealPlan);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(CountPages(smallMenuBytes), Is.EqualTo(1));
                Assert.That(CountPages(bigMenuBytes), Is.EqualTo(1));
            }
        }

        [Test]
        public void Build_ExtremeRecipeCount_DoesNotThrowEvenIfLayoutDegradesToMultiplePages()
        {
            var recipes = Enumerable.Range(1, 150)
                .Select(i => new RecipeModel(Guid.NewGuid(), $"Recipe {i}")
                {
                    ImageUrl = OnePixelJpegDataUri,
                    RecipeCategoryName = "Category " + i
                })
                .ToList();

            var mealPlan = new MealPlanEditModel { Name = "Enormous Menu", Recipes = recipes };

            byte[]? bytes = null;
            Assert.DoesNotThrow(() => bytes = _builder.Build(mealPlan));

            AssertIsValidPdf(bytes!);
            Assert.That(CountPages(bytes!), Is.GreaterThanOrEqualTo(1));
        }

        [Test]
        public void Build_RecipeWithMissingImageAndCategory_DoesNotThrow()
        {
            var mealPlan = new MealPlanEditModel
            {
                Name = "Weekly Menu",
                Recipes = [new RecipeModel(Guid.NewGuid(), "Mystery Soup")]
            };

            var bytes = _builder.Build(mealPlan);

            AssertIsValidPdf(bytes);
        }

        [Test]
        public void Build_RecipeWithMalformedImageDataUri_DoesNotThrow()
        {
            var mealPlan = new MealPlanEditModel
            {
                Name = "Weekly Menu",
                Recipes =
                [
                    new RecipeModel(Guid.NewGuid(), "Broken Image Recipe")
                    {
                        ImageUrl = "data:image/jpg;base64,not-valid-base64!!!"
                    }
                ]
            };

            var bytes = _builder.Build(mealPlan);

            AssertIsValidPdf(bytes);
        }

        private static void AssertIsValidPdf(byte[] bytes)
        {
            Assert.That(bytes, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(bytes, Is.Not.Empty);
                Assert.That(Encoding.ASCII.GetString(bytes, 0, 4), Is.EqualTo("%PDF"));
            }
        }

        private static int CountPages(byte[] pdfBytes)
        {
            var text = Encoding.Latin1.GetString(pdfBytes);
            return Regex.Count(text, @"/Type\s*/Page(?!s)");
        }
    }
}
