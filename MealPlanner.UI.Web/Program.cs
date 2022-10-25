using MealPlanner.UI.Web.Configs;
using MealPlanner.UI.Web.Services;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

builder.Services.AddSingleton<IApiConfig, RecipeBookApiConfig>();
builder.Services.AddSingleton<IApiConfig, MealPlannerApiConfig>();
builder.Services.AddSingleton<IQuantityCalculator, QuantityCalculator>();

builder.Services.AddHttpClient<IRecipeService, RecipeService>()
    .ConfigureHttpClient((serviceProvider, httpClient) =>
    {
        var clientConfig = serviceProvider.GetServices<IApiConfig>().Where(item => item.Name == "RecipeBook").Single();
        httpClient.BaseAddress = clientConfig.BaseUrl;
        httpClient.Timeout = TimeSpan.FromSeconds(clientConfig.Timeout);
    });

builder.Services.AddHttpClient<IMealPlanService, MealPlanService>()
    .ConfigureHttpClient((serviceProvider, httpClient) =>
    {
        var clientConfig = serviceProvider.GetServices<IApiConfig>().Where(item => item.Name == "MealPlanner").Single();
        httpClient.BaseAddress = clientConfig.BaseUrl;
        httpClient.Timeout = TimeSpan.FromSeconds(clientConfig.Timeout);
    });

builder.Services.AddHttpClient<IShoppingListService, ShoppingListService>()
    .ConfigureHttpClient((serviceProvider, httpClient) =>
    {
        var clientConfig = serviceProvider.GetServices<IApiConfig>().Where(item => item.Name == "MealPlanner").Single();
        httpClient.BaseAddress = clientConfig.BaseUrl;
        httpClient.Timeout = TimeSpan.FromSeconds(clientConfig.Timeout);
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();