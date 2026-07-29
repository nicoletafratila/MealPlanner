The edit / selection / statistics pages (RecipeEdit, ProductEdit, MealPlanEdit, ShoppingListEdit, RecipeSelection, ShopSelection, MealPlanSelection, RecipeCategoriesOverview, RecipeStatistics, ProductStatistics) also call SearchAsync, but in their OnInitialized load path — a different code path, with varied
shapes (multiple calls, property assignment, some with no MessageComponent). They aren't the grid-render crash you reported. They'd still throw if a backend is down — but handling them cleanly is a separate, larger change.
Want me to extend the same helper to those init-path pages too? It's doable but touches more varied code, so I kept it out of this pass unless you want it.




- MealPlanner.UI.Mobile/appsettings.json — the three BaseUrls now read https://LAN IP:PORT/ (was 10.0.2.2, the emulator-only alias).
- Identity.Api, MealPlanner.Api, RecipeBook.Api launchSettings.json — applicationUrl now binds 0.0.0.0 instead of localhost, so they accept connections from other devices on the LAN. This still works fine for local/emulator use since 0.0.0.0 includes loopback.
- Platforms/Android/Resources/xml/network_security_config.xml (new) — allows cleartext traffic and trusts user-installed certs for the LAN IP domain.
- AndroidManifest.xml — references the new network security config via android:networkSecurityConfig.

Before building for the phone, do a find-and-replace of the literal LAN IP placeholder (in appsettings.json and network_security_config.xml) with your PC's actual LAN IP (e.g. 192.168.1.50) — run ipconfig to get it. Two things to flag:

1. Windows Firewall: allow inbound TCP on 6001, 7201, 7249, or the phone won't reach the APIs at all.
2. HTTPS dev cert: the ASP.NET Core dev cert is issued for localhost/127.0.0.1 only — hitting it via your LAN IP will fail hostname validation regardless of the network security config. Simplest path for local testing is switching the mobile app's URLs to http:// instead of https:// (the cleartext permission is already in place), unless you want to generate/trust a cert valid for your LAN IP.