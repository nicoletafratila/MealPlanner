using System.Collections.ObjectModel;
using Common.Pagination;
using MealPlanner.Services.Http;
using MealPlanner.Shared.Models;
using RecipeBook.Services.Http;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Mobile.Services
{
    public sealed class ReferenceDataCacheService(
        IRecipeCategoryService categoryService,
        IUnitService unitService,
        IProductService productService,
        IProductCategoryService productCategoryService,
        IShopService shopService,
        IRecipeService recipeService)
    {
        private static readonly TimeSpan StableCacheDuration = TimeSpan.FromMinutes(3);
        private static readonly TimeSpan VolatileCacheDuration = TimeSpan.FromSeconds(60);
        private static readonly List<SortingModel> NameSorting =
            [new SortingModel { PropertyName = "Name", Direction = SortDirection.Ascending }];

        private readonly SemaphoreSlim _lock = new(1, 1);
        private DateTimeOffset _stableLoadedAt = DateTimeOffset.MinValue;
        private DateTimeOffset _productsLoadedAt = DateTimeOffset.MinValue;
        private DateTimeOffset _recipesLoadedAt = DateTimeOffset.MinValue;

        public ObservableCollection<RecipeCategoryModel> Categories { get; } = [];
        public ObservableCollection<UnitModel> Units { get; } = [];

        // Raw categories only — no synthetic "All categories" entry. Consumers that need a filter
        // Picker with an "All" option build their own decorated view over this shared list, since
        // some consumers (e.g. assigning a product's category) must never offer that option.
        public ObservableCollection<ProductCategoryModel> ProductCategories { get; } = [];
        public ObservableCollection<ShopModel> Shops { get; } = [];
        public ObservableCollection<ProductModel> Products { get; } = [];
        public ObservableCollection<RecipeModel> Recipes { get; } = [];

        public async Task EnsureLoadedAsync()
        {
            if (DateTimeOffset.UtcNow - _stableLoadedAt < StableCacheDuration) return;

            await _lock.WaitAsync();
            try
            {
                if (DateTimeOffset.UtcNow - _stableLoadedAt < StableCacheDuration) return;

                var catTask = categoryService.SearchAsync(new QueryParameters<RecipeCategoryModel> { PageSize = 100, Sorting = NameSorting });
                var unitTask = unitService.SearchAsync(new QueryParameters<UnitModel> { PageSize = 100, Sorting = NameSorting });
                var prodCatTask = productCategoryService.SearchAsync(new QueryParameters<ProductCategoryModel> { PageSize = 200, Sorting = NameSorting });
                var shopTask = shopService.SearchAsync(new QueryParameters<ShopModel> { PageSize = 200, Sorting = NameSorting });
                await Task.WhenAll(catTask, unitTask, prodCatTask, shopTask);

                Replace(Categories, catTask.Result?.Items);
                Replace(Units, unitTask.Result?.Items);
                Replace(Shops, shopTask.Result?.Items);
                Replace(ProductCategories, prodCatTask.Result?.Items);

                _stableLoadedAt = DateTimeOffset.UtcNow;
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task EnsureProductsLoadedAsync()
        {
            if (DateTimeOffset.UtcNow - _productsLoadedAt < VolatileCacheDuration) return;

            await _lock.WaitAsync();
            try
            {
                if (DateTimeOffset.UtcNow - _productsLoadedAt < VolatileCacheDuration) return;

                var result = await productService.SearchAsync(new QueryParameters<ProductModel>
                {
                    PageSize = 500,
                    Sorting = NameSorting,
                    Filters = [new FilterItem("ThumbnailOnly", true, FilterOperator.Equals)]
                });
                Replace(Products, result?.Items);
                _productsLoadedAt = DateTimeOffset.UtcNow;
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task EnsureRecipesLoadedAsync()
        {
            if (DateTimeOffset.UtcNow - _recipesLoadedAt < VolatileCacheDuration) return;

            await _lock.WaitAsync();
            try
            {
                if (DateTimeOffset.UtcNow - _recipesLoadedAt < VolatileCacheDuration) return;

                var result = await recipeService.SearchAsync(new QueryParameters<RecipeModel>
                {
                    PageSize = 500,
                    Sorting = NameSorting,
                    Filters = [new FilterItem("ThumbnailOnly", true, FilterOperator.Equals)]
                });
                Replace(Recipes, result?.Items);
                _recipesLoadedAt = DateTimeOffset.UtcNow;
            }
            finally
            {
                _lock.Release();
            }
        }

        public void InvalidateProducts() => _productsLoadedAt = DateTimeOffset.MinValue;

        public void InvalidateRecipes() => _recipesLoadedAt = DateTimeOffset.MinValue;

        public void InvalidateShops() => _stableLoadedAt = DateTimeOffset.MinValue;

        private static void Replace<T>(ObservableCollection<T> target, IEnumerable<T>? items)
        {
            target.Clear();
            if (items is null) return;
            foreach (var item in items)
                target.Add(item);
        }
    }
}
