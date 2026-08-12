Your MAUI project (MealPlanner.UI.Mobile.csproj) currently only targets Android and Windows (net10.0-android36.0, net10.0-windows10.0.19041.0) — no iOS target — so "app store" realistically means Google Play Store, plus optionally the Microsoft Store for the Windows build. Here's what each needs:

Google Play Store (Android)

Account & setup
- Google Play Console developer account — $25 one-time fee, tied to a Google account
- App listing: title, short/full description, category, contact email, privacy policy URL (mandatory even for free apps)
- Graphics: app icon (512×512), feature graphic (1024×500), phone screenshots (min 2), optional tablet/7"/10" screenshots

Build requirements
- ApplicationId (com.mealplanner.mobile — already set, must be globally unique and can't change later)
- Signed release .aab (Android App Bundle, not APK) — needs a signing keystore. Currently no AndroidSigningKeyStore/AndroidKeyStore properties in the csproj, so you'll need to generate one and back it up carefully — losing it means you can never update the app under the same package ID
- Target API level compliant with Play's current requirement (Play updates this yearly; check current minimum)
- ApplicationVersion must increment on every release (ApplicationDisplayVersion is the human-facing version)

Policy/compliance
- Data safety form (what data you collect — since this app has auth/login, expect to declare account data, personal info)
- Content rating questionnaire
- Privacy policy (required since you handle user accounts)

Process
- New accounts go through a review period before first publish is allowed
- Closed testing track requires a minimum number of testers for 14 days before you can go to production (recent Play requirement for new developer accounts)

Microsoft Store (Windows, optional)

- Microsoft Partner Center account ($19 individual / $99 company, one-time)
- MSIX packaging (MAUI Windows target already produces this)
- Store listing assets, age rating, privacy policy URL



Passwords (save these to your password manager right now — there is no recovery if lost):
- Store password: HMx5jdlfiNHzQB0PlMueaZsIXoeGsNZw
- Key password: HMx5jdlfiNHzQB0PlMueaZsIXoeGsNZw (same value — Java's default PKCS12 keystore format requires store and key passwords to match)
- Key alias: mealplanner
- Keystore file: MealPlanner.UI.Mobile/android-signing/mealplanner-release.keystore


Once you refresh that token, dotnet publish MealPlanner.UI.Mobile/MealPlanner.UI.Mobile.csproj -f net10.0-android36.0 -c Release should produce a signed .aab ready for Play Console upload.






