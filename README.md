# MealPlanner

#Package Manager Console
Default project: Services\MealPlanner.Api
PM> Install-Package Microsoft.EntityFrameworkCore.Tools
PM> Add-migration InitialMealPlanner
PM> Update-database "InitialMealPlanner"
PM> Script-migration

Add-Migration -Verbose -Context MealPlannerLogsDbContext InitialCreate 
Add-Migration -Verbose -Context MealPlannerDbContext InitialCreate 




MealPlanner
navigation, breadcrumps
unit conversions
edit product categorie
edit recipe categories
edit units
design interfata