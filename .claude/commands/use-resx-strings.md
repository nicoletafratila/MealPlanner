---
name: use-resx-strings
description: Move hardcoded user-facing strings in C# and XAML files into the appropriate .resx resource file
---

Never hardcode user-facing strings — in C# code **or in MAUI XAML**. Any message, error, validation text, label, page title, button caption, or placeholder that a user will read must live in the appropriate `.resx` file and be referenced via the generated resource class.

## When this applies

### C# files (`.cs`)

- `SetError("...")` / `SetSuccess("...")` / `SetWarning("...")`
- `.WithMessage("...")` in FluentValidation validators
- `ErrorMessage = "..."` / `ErrorMessageResourceName` in DataAnnotation attributes
- Any string literal with English words passed as a method argument or assigned to a message-related property

### MAUI XAML files (`.xaml`, `MealPlanner.UI.Mobile`)

- `Text="..."`, `Title="..."`, `Placeholder="..."`, `ToolTipProperties.Text="..."`, `SemanticProperties.Description/Hint="..."` — any attribute whose value is a literal string (i.e. does NOT start with `{Binding` / `{x:Static` / another markup extension) and contains readable words.
- Emoji/symbol-only values (`"✕"`, `"🗑"`) do not need to move.

Do **not** try to bulk-migrate the entire pre-existing mobile app in one pass — only fix the strings in the file(s) you are editing.

## How to move a string to resx

### 1. Find the right .resx file

The file to use depends on where the string lives:

| Location | Resx file to use |
|---|---|
| `RecipeBook.Api/Features/<X>/` | `RecipeBook.Api/Features/<X>/Resources/<X>Messages.resx` |
| `MealPlanner.Api/Features/<X>/` | `MealPlanner.Api/Features/<X>/Resources/<X>Messages.resx` |
| `Identity.Api/Features/<X>/` | `Identity.Api/Features/<X>/Resources/<X>Messages.resx` |
| `RecipeBook.Shared/` | `RecipeBook.Shared/Resources/RecipeBookSharedMessages.resx` |
| `MealPlanner.Shared/` | `MealPlanner.Shared/Resources/MealPlannerSharedMessages.resx` |
| `MealPlanner.UI.Web/Pages/<X>/` | `MealPlanner.UI.Web/Pages/<X>/Resources/<X>.resx` |
| `MealPlanner.UI.Mobile/**/*.xaml` | `MealPlanner.UI.Mobile/Resources/Strings/AppStrings.resx` |
| Mobile ViewModels (`.cs`) / cross-domain | closest Shared resx (RecipeBook or MealPlanner), else `AppStrings.resx` |

Always check if the string (or a semantically identical one) already exists in the resx before adding a new entry.

### 2. Add the entry (if new)

Insert a `<data>` block in PascalCase, describing the concept — NOT the raw text:

```xml
<data name="RecipeCategoryRequired" xml:space="preserve">
  <value>Please select a category for the recipe.</value>
</data>
```

Good key names: `RecipeCategoryRequired`, `SaveFailed`, `DuplicateIngredientError`  
Bad key names: `PleaseSelectCategory`, `Error1`, `Message`

### 3. Reference the key in code

Add the resource namespace to the using block (each on its own line):

```csharp
using RecipeBook.Shared.Resources;
```

Replace the hardcoded string with the generated property:

```csharp
// Before
SetError("Please select a category.");

// After
SetError(RecipeBookSharedMessages.RecipeCategoryRequired);
```

For DataAnnotation attributes use `nameof` + `typeof`:

```csharp
[Range(0, int.MaxValue,
    ErrorMessageResourceName = nameof(RecipeBookSharedMessages.IngredientQuantityPositive),
    ErrorMessageResourceType = typeof(RecipeBookSharedMessages))]
```

### 4. Apply

Use the Edit tool to update both the `.resx` file and the `.cs` file. Confirm the key compiles (the designer-generated class is auto-updated by the build).

## How to move a string in MAUI XAML

The mobile app (`MealPlanner.UI.Mobile`) keeps all XAML strings in a single resource file: `Resources/Strings/AppStrings.resx` (generated class `MealPlanner.UI.Mobile.Resources.Strings.AppStrings`, `internal` via `ResXFileCodeGenerator` — this is a UI executable per CLAUDE.md).

### 1. Add the entry to `AppStrings.resx` (if new)

Same PascalCase, concept-based key rules as above. Check for an existing equivalent key first.

### 2. Reference it from XAML with `x:Static`

Declare the namespace once on the root element:

```xml
xmlns:strings="clr-namespace:MealPlanner.UI.Mobile.Resources.Strings"
```

Then replace the literal:

```xml
<!-- Before -->
<Label Text="This week's menu:" />

<!-- After -->
<Label Text="{x:Static strings:AppStrings.CurrentMenuLabel}" />
```

### 3. Wire new resx files into the csproj (only when creating `AppStrings.resx` from scratch)

```xml
<ItemGroup>
  <EmbeddedResource Update="Resources\Strings\AppStrings.resx">
    <Generator>ResXFileCodeGenerator</Generator>
    <LastGenOutput>AppStrings.Designer.cs</LastGenOutput>
  </EmbeddedResource>
  <Compile Update="Resources\Strings\AppStrings.Designer.cs">
    <DesignTime>True</DesignTime>
    <AutoGen>True</AutoGen>
    <DependentUpon>AppStrings.resx</DependentUpon>
  </Compile>
</ItemGroup>
```

If Visual Studio is not generating `AppStrings.Designer.cs`, author it by hand as an `internal` strongly-typed class whose `ResourceManager` base name is `MealPlanner.UI.Mobile.Resources.Strings.AppStrings`.
