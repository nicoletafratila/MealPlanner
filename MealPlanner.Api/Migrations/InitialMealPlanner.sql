IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [MealPlans] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NULL,
    CONSTRAINT [PK_MealPlans] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [ProductCategories] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NULL,
    CONSTRAINT [PK_ProductCategories] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [RecipeCategories] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NULL,
    [DisplaySequence] int NOT NULL,
    CONSTRAINT [PK_RecipeCategories] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [ShoppingLists] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NULL,
    [ShopId] int NOT NULL,
    CONSTRAINT [PK_ShoppingLists] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Shops] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NULL,
    CONSTRAINT [PK_Shops] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Units] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NULL,
    CONSTRAINT [PK_Units] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Recipes] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NULL,
    [ImageContent] varbinary(max) NULL,
    [RecipeCategoryId] int NOT NULL,
    CONSTRAINT [PK_Recipes] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Recipes_RecipeCategories_RecipeCategoryId] FOREIGN KEY ([RecipeCategoryId]) REFERENCES [RecipeCategories] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [ShopDisplaySequences] (
    [ShopId] int NOT NULL,
    [ProductCategoryId] int NOT NULL,
    [Value] int NOT NULL,
    CONSTRAINT [PK_ShopDisplaySequences] PRIMARY KEY ([ShopId], [ProductCategoryId]),
    CONSTRAINT [FK_ShopDisplaySequences_ProductCategories_ProductCategoryId] FOREIGN KEY ([ProductCategoryId]) REFERENCES [ProductCategories] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_ShopDisplaySequences_Shops_ShopId] FOREIGN KEY ([ShopId]) REFERENCES [Shops] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Products] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NULL,
    [ImageContent] varbinary(max) NULL,
    [UnitId] int NOT NULL,
    [ProductCategoryId] int NOT NULL,
    CONSTRAINT [PK_Products] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Products_ProductCategories_ProductCategoryId] FOREIGN KEY ([ProductCategoryId]) REFERENCES [ProductCategories] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Products_Units_UnitId] FOREIGN KEY ([UnitId]) REFERENCES [Units] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [MealPlanRecipes] (
    [MealPlanId] int NOT NULL,
    [RecipeId] int NOT NULL,
    CONSTRAINT [PK_MealPlanRecipes] PRIMARY KEY ([MealPlanId], [RecipeId]),
    CONSTRAINT [FK_MealPlanRecipes_MealPlans_MealPlanId] FOREIGN KEY ([MealPlanId]) REFERENCES [MealPlans] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_MealPlanRecipes_Recipes_RecipeId] FOREIGN KEY ([RecipeId]) REFERENCES [Recipes] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [RecipeIngredients] (
    [RecipeId] int NOT NULL,
    [ProductId] int NOT NULL,
    [Quantity] decimal(18,2) NOT NULL,
    CONSTRAINT [PK_RecipeIngredients] PRIMARY KEY ([RecipeId], [ProductId]),
    CONSTRAINT [FK_RecipeIngredients_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_RecipeIngredients_Recipes_RecipeId] FOREIGN KEY ([RecipeId]) REFERENCES [Recipes] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [ShoppingListProducts] (
    [ShoppingListId] int NOT NULL,
    [ProductId] int NOT NULL,
    [Quantity] decimal(18,2) NOT NULL,
    [Collected] bit NOT NULL,
    [DisplaySequence] int NOT NULL,
    CONSTRAINT [PK_ShoppingListProducts] PRIMARY KEY ([ShoppingListId], [ProductId]),
    CONSTRAINT [FK_ShoppingListProducts_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_ShoppingListProducts_ShoppingLists_ShoppingListId] FOREIGN KEY ([ShoppingListId]) REFERENCES [ShoppingLists] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_MealPlanRecipes_RecipeId] ON [MealPlanRecipes] ([RecipeId]);
GO

CREATE INDEX [IX_Products_ProductCategoryId] ON [Products] ([ProductCategoryId]);
GO

CREATE INDEX [IX_Products_UnitId] ON [Products] ([UnitId]);
GO

CREATE INDEX [IX_RecipeIngredients_ProductId] ON [RecipeIngredients] ([ProductId]);
GO

CREATE INDEX [IX_Recipes_RecipeCategoryId] ON [Recipes] ([RecipeCategoryId]);
GO

CREATE INDEX [IX_ShopDisplaySequences_ProductCategoryId] ON [ShopDisplaySequences] ([ProductCategoryId]);
GO

CREATE INDEX [IX_ShoppingListProducts_ProductId] ON [ShoppingListProducts] ([ProductId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20240110133358_InitialMealPlanner', N'8.0.0');
GO

COMMIT;
GO

