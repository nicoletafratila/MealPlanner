# MealPlanner

#Package Manager Console
Default project: Services\MealPlanner.Api
PM> Install-Package Microsoft.EntityFrameworkCore.Tools
PM> EntityFrameworkCore\Add-Migration InitialCreate
PM> dotnet ef migrations add Update_MealPlans_AddDates --project MealPlanner.Api
PM> Update-database "InitialCreate"
PM> Script-migration





unit tests on what is missing, except mobile app




filtru nu mai merge la retete
schimba ids in guid
warning cu automapper
Error occurred while getting package vulnerability data: Unable to load the service index for source https://suvoda-005514975141.d.codeartifact.eu-west-1.amazonaws.com/nuget/sv-framework-nuget/v3/index.json.
create the weeks menu on first click on the recipe if not yet created
update on the banner after weeks menu is created
allow same recipe multiple times on the menu
selectare de limba si traduceri

