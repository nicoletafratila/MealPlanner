namespace MealPlanner.App.Services
{
    public interface IRecipeBookApiConfig
    {
        public Uri BaseUrl { get; set; }
        public int Timeout { get; set; }
    }
}
