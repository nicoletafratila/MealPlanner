# MealPlanner

#Package Manager Console
Default project: Services\MealPlanner.Api
PM> Install-Package Microsoft.EntityFrameworkCore.Tools
PM> EntityFrameworkCore\Add-Migration InitialCreate
PM> dotnet ef migrations add Update_MealPlans_AddDates --project MealPlanner.Api
PM> Update-database "InitialCreate"
PM> Script-migration






incearca iar cu db gol, coloane sunt schimbate
user management - edit user profile
cand se inregistreaza un user sa i se puna categories default
muta pe maui
