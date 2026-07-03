namespace Common.Constants
{
    /// <summary>
    /// Query parameter names used by the API search and filter endpoints.
    /// Shared between ServiceBase (HTTP client) and API controllers (server).
    /// </summary>
    public static class ApiQueryParams
    {
        public const string SearchRoute = "search";
        public const string EditRoute = "edit";
        public const string Filters = "filters";
        public const string Sorting = "sorting";
        public const string PageSize = "pageSize";
        public const string PageNumber = "pageNumber";
        public const string Id = "id";
        public const string CategoryIds = "categoryIds";
        public const string RecipeId = "recipeId";
        public const string ShopId = "shopId";
        public const string MealPlanId = "mealPlanId";
        public const string Username = "username";
        public const string UserId = "userId";
    }
}
