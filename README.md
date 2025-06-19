# MealPlanner

#Package Manager Console
Default project: Services\MealPlanner.Api
PM> Install-Package Microsoft.EntityFrameworkCore.Tools
PM> EntityFrameworkCore\Add-Migration InitialCreate
PM> Update-database "InitialCreate"
PM> Script-migration





home page, authorized or not
click pe meniu fara auth te duce la home
click dreapta sa adauge reteta la meniu
pagini in foldere

 @if (context.User.Identity?.IsAuthenticated != true || !context.User.IsInRole("admin"))
 manageusers vizibil doar pt admin

display la statistics e taiat
la cautare cand nu gaseste, aseaza cutia la mijloc si scoate background gri
organizeaza pe features
swagger
user module
muta pe maui
