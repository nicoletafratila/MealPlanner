# MealPlanner

#Package Manager Console
Default project: Services\MealPlanner.Api
PM> Install-Package Microsoft.EntityFrameworkCore.Tools
PM> EntityFrameworkCore\Add-Migration InitialCreate
PM> dotnet ef migrations add Update_MealPlans_AddDates --project MealPlanner.Api
PM> Update-database "InitialCreate"
PM> Script-migration



forgot pass de pe mobile app
mesajele de eroare sa fie foar textul



The edit / selection / statistics pages (RecipeEdit, ProductEdit, MealPlanEdit, ShoppingListEdit, RecipeSelection, ShopSelection, MealPlanSelection, RecipeCategoriesOverview, RecipeStatistics, ProductStatistics) also call SearchAsync, but in their OnInitialized load path — a different code path, with varied
shapes (multiple calls, property assignment, some with no MessageComponent). They aren't the grid-render crash you reported. They'd still throw if a backend is down — but handling them cleanly is a separate, larger change.
Want me to extend the same helper to those init-path pages too? It's doable but touches more varied code, so I kept it out of this pass unless you want it.

