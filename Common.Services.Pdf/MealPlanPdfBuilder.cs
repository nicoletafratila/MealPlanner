using System.Text;
using System.Text.RegularExpressions;
using MealPlanner.Shared.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using RecipeBook.Shared.Models;
using SkiaSharp;

namespace Common.Services.Pdf
{
    public class MealPlanPdfBuilder : IPdfBuilder<MealPlanEditModel>
    {
        private const int DefaultColumnCount = 3;
        private const float PageMargin = 20;
        private const float HeaderHeightEstimate = 50;
        private const float FooterHeightEstimate = 30;
        private const float DefaultCardHeight = 150;
        private const float MinCardHeight = 50;
        private const float MaxCardHeight = 220;
        private const float MinColumnWidth = 55;
        private const float CardHorizontalChrome = 28;
        private const float ImageMaxWidthFraction = 0.75f;
        private const int NameMaxLines = 2;
        private const int ImageColumnWeight = 2;

        static MealPlanPdfBuilder()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public byte[] Build(MealPlanEditModel mealPlan)
        {
            var recipes = mealPlan.Recipes ?? [];
            if (recipes.Count == 0)
                return GeneratePdf(mealPlan, recipes, DefaultColumnCount, DefaultCardHeight, PageSizes.A4.Landscape().Width - (2 * PageMargin));

            var pageSize = PageSizes.A4.Landscape();
            var availableContentHeight = pageSize.Height - (2 * PageMargin) - HeaderHeightEstimate - FooterHeightEstimate;
            var availableContentWidth = pageSize.Width - (2 * PageMargin);
            var maxRowsWithoutImage = Math.Max(1, (int)(availableContentHeight / MinCardHeight));
            var maxColumns = Math.Max(DefaultColumnCount, Math.Min(recipes.Count, (int)(availableContentWidth / MinColumnWidth)));
            var columns = Math.Max(DefaultColumnCount, Math.Min(maxColumns, (int)Math.Ceiling(recipes.Count / (double)maxRowsWithoutImage)));

            byte[] pdfBytes;
            while (true)
            {
                var cardHeight = CalculateCardHeight(recipes.Count, columns, availableContentHeight);
                var cardWidth = availableContentWidth / columns;
                pdfBytes = GeneratePdf(mealPlan, recipes, columns, cardHeight, cardWidth);

                if (CountPages(pdfBytes) <= 1 || columns >= maxColumns)
                    break;

                columns++;
            }

            return pdfBytes;
        }

        private static float CalculateCardHeight(int recipeCount, int columns, float availableContentHeight)
        {
            var rows = (int)Math.Ceiling(recipeCount / (double)columns);
            var maxRowHeight = availableContentHeight / rows;

            return Math.Clamp(maxRowHeight, MinCardHeight, MaxCardHeight);
        }

        private static byte[] GeneratePdf(MealPlanEditModel mealPlan, IList<RecipeModel> recipes, int columns, float cardHeight, float cardWidth)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(PageMargin);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header()
                        .PaddingBottom(10)
                        .Text(mealPlan.Name)
                        .FontSize(20)
                        .SemiBold();

                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(tableColumns =>
                        {
                            for (var i = 0; i < columns; i++)
                                tableColumns.RelativeColumn();
                        });

                        foreach (var recipe in recipes)
                        {
                            table.Cell().Element(cell => ComposeRecipeCard(cell, recipe, cardHeight, cardWidth));
                        }
                    });

                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.CurrentPageNumber();
                        text.Span(" / ");
                        text.TotalPages();
                    });
                });
            });

            return document.GeneratePdf();
        }

        private static int CountPages(byte[] pdfBytes)
        {
            var text = Encoding.Latin1.GetString(pdfBytes);
            return Regex.Count(text, @"/Type\s*/Page(?!s)");
        }

        private static void ComposeRecipeCard(IContainer container, RecipeModel recipe, float cardHeight, float cardWidth)
        {
            container
                .Height(cardHeight)
                .Padding(5)
                .Border(1)
                .BorderColor(Colors.Grey.Lighten2)
                .Padding(8)
                .Row(row =>
                {
                    row.RelativeItem().Column(column =>
                    {
                        column.Item().Text(recipe.Name).Bold().FontSize(12).ClampLines(NameMaxLines, "…");

                        if (!string.IsNullOrWhiteSpace(recipe.RecipeCategoryName))
                        {
                            column.Item().PaddingTop(2).Text(recipe.RecipeCategoryName).FontSize(9).FontColor(Colors.Grey.Darken1).ClampLines(1, "…");
                        }
                    });

                    var imageBytes = TryDecodeDataUriImage(recipe.ImageUrl);
                    var imageWidth = CalculateImageWidth(imageBytes, cardHeight, cardWidth);

                    if (imageBytes is not null && imageWidth is not null)
                    {
                        // Size the image slot to its own aspect ratio (given the fixed card height) so the
                        // name column reclaims any width the image doesn't need, instead of leaving it blank.
                        // The width is computed upfront (rather than left to auto-sizing) and capped, so an
                        // unusually wide image can never squeeze the name column to nothing or overflow the row.
                        row.ConstantItem(imageWidth.Value).PaddingLeft(8).AlignRight().Image(imageBytes!).FitArea();
                    }
                    else
                    {
                        row.RelativeItem(ImageColumnWeight).PaddingLeft(8).Background(Colors.Grey.Lighten3);
                    }
                });
        }

        private static float? CalculateImageWidth(byte[]? imageBytes, float cardHeight, float cardWidth)
        {
            if (imageBytes is null)
                return null;

            using var bitmap = SKBitmap.Decode(imageBytes);
            if (bitmap is null || bitmap.Height <= 0 || bitmap.Width <= 0)
                return null;

            var aspectRatio = (float)bitmap.Width / bitmap.Height;
            var naturalWidth = cardHeight * aspectRatio;
            var maxImageWidth = Math.Max(1, (cardWidth - CardHorizontalChrome) * ImageMaxWidthFraction);

            return Math.Min(naturalWidth, maxImageWidth);
        }

        private static byte[]? TryDecodeDataUriImage(string? imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl) || !imageUrl.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
                return null;

            var commaIndex = imageUrl.IndexOf(',');
            if (commaIndex < 0)
                return null;

            try
            {
                return Convert.FromBase64String(imageUrl[(commaIndex + 1)..]);
            }
            catch (FormatException)
            {
                return null;
            }
        }
    }
}
