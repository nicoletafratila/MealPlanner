# MealPlanner

#Package Manager Console
Default project: Services\MealPlanner.Api
PM> Install-Package Microsoft.EntityFrameworkCore.Tools
PM> EntityFrameworkCore\Add-Migration InitialCreate
PM> Update-database "InitialCreate"
PM> Script-migration




adauga authorize la toate endpoints
muta tot in Sessionstorage
cookie vs jwt
@if (SignInManager.IsSignedIn(User))
{
    <div @onclick="ToggleNavMenu" class="nav-menu">
        <div class="nav-item px-3">
            <MudNavLink class="nav-link" href="/">
                <span class="oi oi-list-rich" aria-hidden="true"></span> Home
            </MudNavLink>
        </div>

home page, authorized or not
click pe meniu fara auth te duce la home

 @if (context.User.Identity?.IsAuthenticated != true || !context.User.IsInRole("admin"))
 manageusers vizibil doar pt admin


click dreapta sa adauge reteta la meniu
display la statistics e taiat
la cautare cand nu gaseste, aseaza cutia la mijloc si scoate background gri
organizeaza pe features
swagger
user module
pagini in foldere
muta pe maui
