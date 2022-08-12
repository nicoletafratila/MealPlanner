using MealPlanner.App.Services;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

builder.Services.AddSingleton<IRecipeBookApiConfig, RecipeBookApiConfig>();

builder.Services.AddHttpClient<IRecipeService, RecipeService>()
    .ConfigureHttpClient((serviceProvider, httpClient) =>
    {
        var clientConfig = serviceProvider.GetRequiredService<IRecipeBookApiConfig>();
        httpClient.BaseAddress = clientConfig.BaseUrl;
        httpClient.Timeout = TimeSpan.FromSeconds(clientConfig.Timeout);
        httpClient.DefaultRequestHeaders.Add("User-Agent", "BlahAgent");
        httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
    })
    .SetHandlerLifetime(TimeSpan.FromMinutes(5))    // Default is 2 mins
    .ConfigurePrimaryHttpMessageHandler(x =>
        new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
            UseCookies = false,
            AllowAutoRedirect = false,
            UseDefaultCredentials = true,
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