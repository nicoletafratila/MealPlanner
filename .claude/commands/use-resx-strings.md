---
name: use-resx-strings
description: Move hardcoded user-facing strings in C# files into the appropriate .resx resource file
---

Never hardcode user-facing strings in C# code. Any message, error, validation text, or label that a user will read must live in the appropriate `.resx` file and be referenced via the generated resource class.

## When this applies

- `SetError("...")` / `SetSuccess("...")` / `SetWarning("...")`
- `.WithMessage("...")` in FluentValidation validators
- `ErrorMessage = "..."` / `ErrorMessageResourceName` in DataAnnotation attributes
- Any string literal with English words passed as a method argument or assigned to a message-related property

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
| Mobile ViewModels / cross-domain | closest Shared resx (RecipeBook or MealPlanner) |

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
