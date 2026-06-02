# MealPlanner

#Package Manager Console
Default project: Services\MealPlanner.Api
PM> Install-Package Microsoft.EntityFrameworkCore.Tools
PM> EntityFrameworkCore\Add-Migration InitialCreate
PM> dotnet ef migrations add Update_MealPlans_AddDates --project MealPlanner.Api
PM> Update-database "InitialCreate"
PM> Script-migration








schimba ids in guid
allow same recipe multiple times on the menu
