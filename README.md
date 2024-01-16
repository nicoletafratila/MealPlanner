# MealPlanner

#Package Manager Console
Default project: Services\MealPlanner.Api
PM> Install-Package Microsoft.EntityFrameworkCore.Tools
PM> Add-migration InitialMealPlanner
PM> Update-database "InitialMealPlanner"
PM> Script-migration

The database for Game API will be created automatically, but if you want to change the structure, you need to add new migrations. You can use the following script to add a new migration for Trivia database for example: dotnet ef migrations add Initial --verbose --project "UpdateWithFullPathToProject\Services\Game\Game.Infrastructure\Game.Infrastructure.csproj" --startup-project "UpdateWithFullPathToProject\Services\Game\Game.API\Game.API.csproj" --context TriviaDbContext
The database for Identity API will be created automatically, but if you want to change the structure, you need to add new migrations by running .\Services\Indentity\Identity.API\buildschema.bat script

MealPlanner
-

card for fas display of recipe
edit product categorie
edit recipe categories
edit units