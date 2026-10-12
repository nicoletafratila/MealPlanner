/*
  Copies all menus (MealPlans) from one user account to another, re-linking each
  copied menu's recipes to the recipes that already exist for the target user
  (created earlier via the recipe/product "Share" feature).

  Recipes are NOT deduplicated by the Share feature's handlers (RecipeBook.Api
  Features/Recipe/Commands/Share and ShareAll) - every shared recipe gets a brand
  new row for the target user, named identically to the source UNLESS a name
  collision occurred during the share, in which case it was suffixed
  " (Copy)", " (Copy 2)", " (Copy 3)", etc. There is no persisted old-id -> new-id
  mapping table, so this script matches recipes the same way the Share feature's
  RecipeRepository.SearchAsync does: by case-insensitive Name, scoped to the
  target user's RecipeCategory (matched by case-insensitive category Name, since
  RecipeCategories are also per-user), trying the plain name first and then the
  "(Copy)" / "(Copy N)" suffixes as a fallback.

  SAFETY: this script runs inside an explicit transaction and ROLLBACKs by default.
  Review the diagnostic result sets (especially #UnmatchedRecipes) before changing
  the final ROLLBACK to COMMIT.
*/

SET NOCOUNT ON;

DECLARE @SourceUserId NVARCHAR(450) = N'<SOURCE-USER-ID>';
DECLARE @TargetUserId NVARCHAR(450) = N'<TARGET-USER-ID>';

IF @SourceUserId = N'<SOURCE-USER-ID>' OR @TargetUserId = N'<TARGET-USER-ID>'
BEGIN
    RAISERROR('Set @SourceUserId and @TargetUserId before running this script.', 16, 1);
    RETURN;
END;

BEGIN TRANSACTION;

-- ---------------------------------------------------------------------------
-- 1. Build the recipe mapping: old Recipes.Id (source user) -> new Recipes.Id
--    (target user), matched by category Name + recipe Name (with "(Copy)"/
--    "(Copy N)" fallback), mirroring RecipeRepository.SearchAsync.
-- ---------------------------------------------------------------------------
;WITH Numbers AS (
    SELECT TOP (50) ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) - 1 AS n
    FROM sys.all_objects
),
Candidates AS (
    SELECT
        r.Id            AS OldRecipeId,
        rc.Name         AS SourceCategoryName,
        num.n,
        CASE
            WHEN num.n = 0 THEN r.Name
            WHEN num.n = 1 THEN r.Name + N' (Copy)'
            ELSE r.Name + N' (Copy ' + CAST(num.n AS NVARCHAR(10)) + N')'
        END AS CandidateName
    FROM Recipes r
    JOIN RecipeCategories rc ON rc.Id = r.RecipeCategoryId
    CROSS JOIN Numbers num
    WHERE r.UserId = @SourceUserId
),
Matched AS (
    SELECT
        c.OldRecipeId,
        tr.Id AS NewRecipeId,
        ROW_NUMBER() OVER (PARTITION BY c.OldRecipeId ORDER BY c.n) AS rn
    FROM Candidates c
    JOIN RecipeCategories trc
        ON trc.UserId = @TargetUserId
       AND LOWER(trc.Name) = LOWER(c.SourceCategoryName)
    JOIN Recipes tr
        ON tr.UserId = @TargetUserId
       AND tr.RecipeCategoryId = trc.Id
       AND LOWER(tr.Name) = LOWER(c.CandidateName)
)
SELECT OldRecipeId, NewRecipeId
INTO #RecipeMap
FROM Matched
WHERE rn = 1;

-- ---------------------------------------------------------------------------
-- 2. Diagnostics: recipes used in the source user's menus that could NOT be
--    matched to a target-user recipe. Review these before committing - the
--    corresponding menu items will simply be skipped (not copied).
-- ---------------------------------------------------------------------------
SELECT DISTINCT
    r.Id                AS OldRecipeId,
    r.Name              AS RecipeName,
    rc.Name             AS CategoryName
INTO #UnmatchedRecipes
FROM MealPlanRecipes mpr
JOIN MealPlans mp ON mp.Id = mpr.MealPlanId AND mp.UserId = @SourceUserId
JOIN Recipes r ON r.Id = mpr.RecipeId
JOIN RecipeCategories rc ON rc.Id = r.RecipeCategoryId
WHERE NOT EXISTS (SELECT 1 FROM #RecipeMap rm WHERE rm.OldRecipeId = r.Id);

SELECT * FROM #UnmatchedRecipes ORDER BY CategoryName, RecipeName;

-- ---------------------------------------------------------------------------
-- 3. Copy the menus (MealPlans) themselves. Skips menus the target user
--    already has (by Name) so the script is safe to re-run.
-- ---------------------------------------------------------------------------
DECLARE @MealPlanMap TABLE (OldMealPlanId UNIQUEIDENTIFIER, NewMealPlanId UNIQUEIDENTIFIER);

MERGE INTO MealPlans AS tgt
USING (
    SELECT mp.Id, mp.Name, mp.CreatedAt, mp.UpdatedAt
    FROM MealPlans mp
    WHERE mp.UserId = @SourceUserId
      AND NOT EXISTS (
          SELECT 1 FROM MealPlans existing
          WHERE existing.UserId = @TargetUserId
            AND existing.Name = mp.Name
      )
) AS src
ON 1 = 0
WHEN NOT MATCHED THEN
    INSERT (Id, UserId, Name, CreatedAt, UpdatedAt)
    VALUES (NEWID(), @TargetUserId, src.Name, src.CreatedAt, src.UpdatedAt)
OUTPUT src.Id, inserted.Id
INTO @MealPlanMap (OldMealPlanId, NewMealPlanId);

SELECT COUNT(*) AS MealPlansCopied FROM @MealPlanMap;

-- ---------------------------------------------------------------------------
-- 4. Copy the menu/recipe links (MealPlanRecipes) for the newly copied menus,
--    pointing each one at the matched target-user recipe. Links whose recipe
--    could not be matched (see #UnmatchedRecipes) are skipped.
-- ---------------------------------------------------------------------------
INSERT INTO MealPlanRecipes (Id, MealPlanId, RecipeId)
SELECT
    NEWID(),
    map.NewMealPlanId,
    rm.NewRecipeId
FROM MealPlanRecipes mpr
JOIN @MealPlanMap map ON map.OldMealPlanId = mpr.MealPlanId
JOIN #RecipeMap rm ON rm.OldRecipeId = mpr.RecipeId;

SELECT COUNT(*) AS MealPlanRecipesCopied FROM MealPlanRecipes mpr
JOIN @MealPlanMap map ON map.NewMealPlanId = mpr.MealPlanId;

DROP TABLE #RecipeMap;
DROP TABLE #UnmatchedRecipes;

-- ---------------------------------------------------------------------------
-- Review the result sets above. If everything looks right, change ROLLBACK to
-- COMMIT and re-run (or just run `COMMIT TRANSACTION;` now in the same session).
-- ---------------------------------------------------------------------------
ROLLBACK TRANSACTION;
-- COMMIT TRANSACTION;
