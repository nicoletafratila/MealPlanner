using Common.Data.DataContext;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MealPlanner.Api.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            if (!migrationBuilder.TableExists<MealPlannerDbContext>("AspNetRoles"))
                migrationBuilder.CreateTable(
                    name: "AspNetRoles",
                    columns: table => new
                    {
                        Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                        Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                        NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                        ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                    });

            if (!migrationBuilder.TableExists<MealPlannerDbContext>("AspNetUsers"))
                migrationBuilder.CreateTable(
                    name: "AspNetUsers",
                    columns: table => new
                    {
                        Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                        FirstName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                        LastName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                        ProfilePicture = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                        IsActive = table.Column<bool>(type: "bit", nullable: false),
                        UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                        NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                        Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                        NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                        EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                        PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                        SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                        ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                        PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                        PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                        TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                        LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                        LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                        AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                    });

            if (!migrationBuilder.TableExists<MealPlannerDbContext>("Logs"))
                migrationBuilder.CreateTable(
                    name: "Logs",
                    columns: table => new
                    {
                        Id = table.Column<int>(type: "int", nullable: false)
                            .Annotation("SqlServer:Identity", "1, 1"),
                        Message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                        MessageTemplate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                        Level = table.Column<string>(type: "nvarchar(max)", nullable: true),
                        TimeStamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                        Exception = table.Column<string>(type: "nvarchar(max)", nullable: true),
                        Properties = table.Column<string>(type: "nvarchar(max)", nullable: true)
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_Logs", x => x.Id);
                    });

            if (!migrationBuilder.TableExists<MealPlannerDbContext>("MealPlans"))
                migrationBuilder.CreateTable(
                    name: "MealPlans",
                    columns: table => new
                    {
                        Id = table.Column<int>(type: "int", nullable: false)
                            .Annotation("SqlServer:Identity", "1, 1"),
                        Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_MealPlans", x => x.Id);
                    });

            if (!migrationBuilder.TableExists<MealPlannerDbContext>("ProductCategories"))
                migrationBuilder.CreateTable(
                    name: "ProductCategories",
                    columns: table => new
                    {
                        Id = table.Column<int>(type: "int", nullable: false)
                            .Annotation("SqlServer:Identity", "1, 1"),
                        Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_ProductCategories", x => x.Id);
                    });

            if (!migrationBuilder.TableExists<MealPlannerDbContext>("RecipeCategories"))
                migrationBuilder.CreateTable(
                    name: "RecipeCategories",
                    columns: table => new
                    {
                        Id = table.Column<int>(type: "int", nullable: false)
                            .Annotation("SqlServer:Identity", "1, 1"),
                        Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                        DisplaySequence = table.Column<int>(type: "int", nullable: false)
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_RecipeCategories", x => x.Id);
                    });

            if (!migrationBuilder.TableExists<MealPlannerDbContext>("Shops"))
                migrationBuilder.CreateTable(
                    name: "Shops",
                    columns: table => new
                    {
                        Id = table.Column<int>(type: "int", nullable: false)
                            .Annotation("SqlServer:Identity", "1, 1"),
                        Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_Shops", x => x.Id);
                    });

            if (!migrationBuilder.TableExists<MealPlannerDbContext>("Units"))
                migrationBuilder.CreateTable(
                    name: "Units",
                    columns: table => new
                    {
                        Id = table.Column<int>(type: "int", nullable: false)
                            .Annotation("SqlServer:Identity", "1, 1"),
                        Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                        UnitType = table.Column<int>(type: "int", nullable: false)
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_Units", x => x.Id);
                    });

            // --- Tables with FK dependencies ---
            if (!migrationBuilder.TableExists<MealPlannerDbContext>("AspNetRoleClaims"))
                migrationBuilder.CreateTable(
                    name: "AspNetRoleClaims",
                    columns: table => new
                    {
                        Id = table.Column<int>(type: "int", nullable: false)
                            .Annotation("SqlServer:Identity", "1, 1"),
                        RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                        ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                        ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                        table.ForeignKey(
                            name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                            column: x => x.RoleId,
                            principalTable: "AspNetRoles",
                            principalColumn: "Id",
                            onDelete: ReferentialAction.NoAction);
                    });

            if (!migrationBuilder.TableExists<MealPlannerDbContext>("AspNetUserClaims"))
                migrationBuilder.CreateTable(
                    name: "AspNetUserClaims",
                    columns: table => new
                    {
                        Id = table.Column<int>(type: "int", nullable: false)
                            .Annotation("SqlServer:Identity", "1, 1"),
                        UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                        ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                        ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                        table.ForeignKey(
                            name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                            column: x => x.UserId,
                            principalTable: "AspNetUsers",
                            principalColumn: "Id",
                            onDelete: ReferentialAction.NoAction);
                    });

            if (!migrationBuilder.TableExists<MealPlannerDbContext>("AspNetUserLogins"))
                migrationBuilder.CreateTable(
                    name: "AspNetUserLogins",
                    columns: table => new
                    {
                        LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                        ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                        ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                        UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                        table.ForeignKey(
                            name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                            column: x => x.UserId,
                            principalTable: "AspNetUsers",
                            principalColumn: "Id",
                            onDelete: ReferentialAction.NoAction);
                    });

            if (!migrationBuilder.TableExists<MealPlannerDbContext>("AspNetUserRoles"))
                migrationBuilder.CreateTable(
                    name: "AspNetUserRoles",
                    columns: table => new
                    {
                        UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                        RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                        table.ForeignKey(
                            name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                            column: x => x.RoleId,
                            principalTable: "AspNetRoles",
                            principalColumn: "Id",
                            onDelete: ReferentialAction.NoAction);
                        table.ForeignKey(
                            name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                            column: x => x.UserId,
                            principalTable: "AspNetUsers",
                            principalColumn: "Id",
                            onDelete: ReferentialAction.NoAction);
                    });

            if (!migrationBuilder.TableExists<MealPlannerDbContext>("AspNetUserTokens"))
                migrationBuilder.CreateTable(
                    name: "AspNetUserTokens",
                    columns: table => new
                    {
                        UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                        LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                        Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                        Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                        table.ForeignKey(
                            name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                            column: x => x.UserId,
                            principalTable: "AspNetUsers",
                            principalColumn: "Id",
                            onDelete: ReferentialAction.NoAction);
                    });

            if (!migrationBuilder.TableExists<MealPlannerDbContext>("Recipes"))
                migrationBuilder.CreateTable(
                    name: "Recipes",
                    columns: table => new
                    {
                        Id = table.Column<int>(type: "int", nullable: false)
                            .Annotation("SqlServer:Identity", "1, 1"),
                        Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                        ImageContent = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                        Source = table.Column<string>(type: "nvarchar(max)", nullable: true),
                        RecipeCategoryId = table.Column<int>(type: "int", nullable: false)
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_Recipes", x => x.Id);
                        table.ForeignKey(
                            name: "FK_Recipes_RecipeCategories_RecipeCategoryId",
                            column: x => x.RecipeCategoryId,
                            principalTable: "RecipeCategories",
                            principalColumn: "Id",
                            onDelete: ReferentialAction.NoAction);
                    });

            if (!migrationBuilder.TableExists<MealPlannerDbContext>("ShopDisplaySequences"))
                migrationBuilder.CreateTable(
                    name: "ShopDisplaySequences",
                    columns: table => new
                    {
                        ShopId = table.Column<int>(type: "int", nullable: false),
                        ProductCategoryId = table.Column<int>(type: "int", nullable: false),
                        Value = table.Column<int>(type: "int", nullable: false)
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_ShopDisplaySequences", x => new { x.ShopId, x.ProductCategoryId });
                        table.ForeignKey(
                            name: "FK_ShopDisplaySequences_ProductCategories_ProductCategoryId",
                            column: x => x.ProductCategoryId,
                            principalTable: "ProductCategories",
                            principalColumn: "Id",
                            onDelete: ReferentialAction.NoAction);
                        table.ForeignKey(
                            name: "FK_ShopDisplaySequences_Shops_ShopId",
                            column: x => x.ShopId,
                            principalTable: "Shops",
                            principalColumn: "Id",
                            onDelete: ReferentialAction.NoAction);
                    });

            if (!migrationBuilder.TableExists<MealPlannerDbContext>("ShoppingLists"))
                migrationBuilder.CreateTable(
                    name: "ShoppingLists",
                    columns: table => new
                    {
                        Id = table.Column<int>(type: "int", nullable: false)
                            .Annotation("SqlServer:Identity", "1, 1"),
                        Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                        ShopId = table.Column<int>(type: "int", nullable: false)
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_ShoppingLists", x => x.Id);
                        table.ForeignKey(
                            name: "FK_ShoppingLists_Shops_ShopId",
                            column: x => x.ShopId,
                            principalTable: "Shops",
                            principalColumn: "Id",
                            onDelete: ReferentialAction.NoAction);
                    });

            if (!migrationBuilder.TableExists<MealPlannerDbContext>("Products"))
                migrationBuilder.CreateTable(
                    name: "Products",
                    columns: table => new
                    {
                        Id = table.Column<int>(type: "int", nullable: false)
                            .Annotation("SqlServer:Identity", "1, 1"),
                        Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                        ImageContent = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                        BaseUnitId = table.Column<int>(type: "int", nullable: false),
                        ProductCategoryId = table.Column<int>(type: "int", nullable: false)
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_Products", x => x.Id);
                        table.ForeignKey(
                            name: "FK_Products_ProductCategories_ProductCategoryId",
                            column: x => x.ProductCategoryId,
                            principalTable: "ProductCategories",
                            principalColumn: "Id",
                            onDelete: ReferentialAction.NoAction);
                        table.ForeignKey(
                            name: "FK_Products_Units_BaseUnitId",
                            column: x => x.BaseUnitId,
                            principalTable: "Units",
                            principalColumn: "Id",
                            onDelete: ReferentialAction.NoAction);
                    });

            if (!migrationBuilder.TableExists<MealPlannerDbContext>("MealPlanRecipes"))
                migrationBuilder.CreateTable(
                    name: "MealPlanRecipes",
                    columns: table => new
                    {
                        MealPlanId = table.Column<int>(type: "int", nullable: false),
                        RecipeId = table.Column<int>(type: "int", nullable: false)
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_MealPlanRecipes", x => new { x.MealPlanId, x.RecipeId });
                        table.ForeignKey(
                            name: "FK_MealPlanRecipes_MealPlans_MealPlanId",
                            column: x => x.MealPlanId,
                            principalTable: "MealPlans",
                            principalColumn: "Id",
                            onDelete: ReferentialAction.NoAction);
                        table.ForeignKey(
                            name: "FK_MealPlanRecipes_Recipes_RecipeId",
                            column: x => x.RecipeId,
                            principalTable: "Recipes",
                            principalColumn: "Id",
                            onDelete: ReferentialAction.NoAction);
                    });

            if (!migrationBuilder.TableExists<MealPlannerDbContext>("RecipeIngredients"))
                migrationBuilder.CreateTable(
                    name: "RecipeIngredients",
                    columns: table => new
                    {
                        RecipeId = table.Column<int>(type: "int", nullable: false),
                        ProductId = table.Column<int>(type: "int", nullable: false),
                        Quantity = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                        UnitId = table.Column<int>(type: "int", nullable: false)
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_RecipeIngredients", x => new { x.RecipeId, x.ProductId });
                        table.ForeignKey(
                            name: "FK_RecipeIngredients_Products_ProductId",
                            column: x => x.ProductId,
                            principalTable: "Products",
                            principalColumn: "Id",
                            onDelete: ReferentialAction.NoAction);
                        table.ForeignKey(
                            name: "FK_RecipeIngredients_Recipes_RecipeId",
                            column: x => x.RecipeId,
                            principalTable: "Recipes",
                            principalColumn: "Id",
                            onDelete: ReferentialAction.NoAction);
                        table.ForeignKey(
                            name: "FK_RecipeIngredients_Units_UnitId",
                            column: x => x.UnitId,
                            principalTable: "Units",
                            principalColumn: "Id",
                            onDelete: ReferentialAction.NoAction);
                    });

            if (!migrationBuilder.TableExists<MealPlannerDbContext>("ShoppingListProducts"))
                migrationBuilder.CreateTable(
                    name: "ShoppingListProducts",
                    columns: table => new
                    {
                        ShoppingListId = table.Column<int>(type: "int", nullable: false),
                        ProductId = table.Column<int>(type: "int", nullable: false),
                        Quantity = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                        UnitId = table.Column<int>(type: "int", nullable: false),
                        Collected = table.Column<bool>(type: "bit", nullable: false),
                        DisplaySequence = table.Column<int>(type: "int", nullable: false)
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_ShoppingListProducts", x => new { x.ShoppingListId, x.ProductId });
                        table.ForeignKey(
                            name: "FK_ShoppingListProducts_Products_ProductId",
                            column: x => x.ProductId,
                            principalTable: "Products",
                            principalColumn: "Id",
                            onDelete: ReferentialAction.NoAction);
                        table.ForeignKey(
                            name: "FK_ShoppingListProducts_ShoppingLists_ShoppingListId",
                            column: x => x.ShoppingListId,
                            principalTable: "ShoppingLists",
                            principalColumn: "Id",
                            onDelete: ReferentialAction.NoAction);
                        table.ForeignKey(
                            name: "FK_ShoppingListProducts_Units_UnitId",
                            column: x => x.UnitId,
                            principalTable: "Units",
                            principalColumn: "Id",
                            onDelete: ReferentialAction.NoAction);
                    });

            // --- Indexes ---
            if (!migrationBuilder.IndexExists<MealPlannerDbContext>("IX_AspNetRoleClaims_RoleId", "AspNetRoleClaims"))
                migrationBuilder.CreateIndex("IX_AspNetRoleClaims_RoleId", "AspNetRoleClaims", "RoleId");

            if (!migrationBuilder.IndexExists<MealPlannerDbContext>("RoleNameIndex", "AspNetRoles"))
                migrationBuilder.CreateIndex("RoleNameIndex", "AspNetRoles", "NormalizedName", unique: true, filter: "[NormalizedName] IS NOT NULL");

            if (!migrationBuilder.IndexExists<MealPlannerDbContext>("IX_AspNetUserClaims_UserId", "AspNetUserClaims"))
                migrationBuilder.CreateIndex("IX_AspNetUserClaims_UserId", "AspNetUserClaims", "UserId");

            if (!migrationBuilder.IndexExists<MealPlannerDbContext>("IX_AspNetUserLogins_UserId", "AspNetUserLogins"))
                migrationBuilder.CreateIndex("IX_AspNetUserLogins_UserId", "AspNetUserLogins", "UserId");

            if (!migrationBuilder.IndexExists<MealPlannerDbContext>("IX_AspNetUserRoles_RoleId", "AspNetUserRoles"))
                migrationBuilder.CreateIndex("IX_AspNetUserRoles_RoleId", "AspNetUserRoles", "RoleId");

            if (!migrationBuilder.IndexExists<MealPlannerDbContext>("EmailIndex", "AspNetUsers"))
                migrationBuilder.CreateIndex("EmailIndex", "AspNetUsers", "NormalizedEmail");

            if (!migrationBuilder.IndexExists<MealPlannerDbContext>("UserNameIndex", "AspNetUsers"))
                migrationBuilder.CreateIndex("UserNameIndex", "AspNetUsers", "NormalizedUserName", unique: true, filter: "[NormalizedUserName] IS NOT NULL");

            if (!migrationBuilder.IndexExists<MealPlannerDbContext>("IX_MealPlanRecipes_RecipeId", "MealPlanRecipes"))
                migrationBuilder.CreateIndex("IX_MealPlanRecipes_RecipeId", "MealPlanRecipes", "RecipeId");

            if (!migrationBuilder.IndexExists<MealPlannerDbContext>("IX_Products_BaseUnitId", "Products"))
                migrationBuilder.CreateIndex("IX_Products_BaseUnitId", "Products", "BaseUnitId");

            if (!migrationBuilder.IndexExists<MealPlannerDbContext>("IX_Products_ProductCategoryId", "Products"))
                migrationBuilder.CreateIndex("IX_Products_ProductCategoryId", "Products", "ProductCategoryId");

            if (!migrationBuilder.IndexExists<MealPlannerDbContext>("IX_RecipeIngredients_ProductId", "RecipeIngredients"))
                migrationBuilder.CreateIndex("IX_RecipeIngredients_ProductId", "RecipeIngredients", "ProductId");

            if (!migrationBuilder.IndexExists<MealPlannerDbContext>("IX_RecipeIngredients_UnitId", "RecipeIngredients"))
                migrationBuilder.CreateIndex("IX_RecipeIngredients_UnitId", "RecipeIngredients", "UnitId");

            if (!migrationBuilder.IndexExists<MealPlannerDbContext>("IX_Recipes_RecipeCategoryId", "Recipes"))
                migrationBuilder.CreateIndex("IX_Recipes_RecipeCategoryId", "Recipes", "RecipeCategoryId");

            if (!migrationBuilder.IndexExists<MealPlannerDbContext>("IX_ShopDisplaySequences_ProductCategoryId", "ShopDisplaySequences"))
                migrationBuilder.CreateIndex("IX_ShopDisplaySequences_ProductCategoryId", "ShopDisplaySequences", "ProductCategoryId");

            if (!migrationBuilder.IndexExists<MealPlannerDbContext>("IX_ShoppingListProducts_ProductId", "ShoppingListProducts"))
                migrationBuilder.CreateIndex("IX_ShoppingListProducts_ProductId", "ShoppingListProducts", "ProductId");

            if (!migrationBuilder.IndexExists<MealPlannerDbContext>("IX_ShoppingListProducts_UnitId", "ShoppingListProducts"))
                migrationBuilder.CreateIndex("IX_ShoppingListProducts_UnitId", "ShoppingListProducts", "UnitId");

            if (!migrationBuilder.IndexExists<MealPlannerDbContext>("IX_ShoppingLists_ShopId", "ShoppingLists"))
                migrationBuilder.CreateIndex("IX_ShoppingLists_ShopId", "ShoppingLists", "ShopId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "AspNetRoleClaims");
            migrationBuilder.DropTable(name: "AspNetUserClaims");
            migrationBuilder.DropTable(name: "AspNetUserLogins");
            migrationBuilder.DropTable(name: "AspNetUserRoles");
            migrationBuilder.DropTable(name: "AspNetUserTokens");
            migrationBuilder.DropTable(name: "Logs");
            migrationBuilder.DropTable(name: "MealPlanRecipes");
            migrationBuilder.DropTable(name: "RecipeIngredients");
            migrationBuilder.DropTable(name: "ShopDisplaySequences");
            migrationBuilder.DropTable(name: "ShoppingListProducts");
            migrationBuilder.DropTable(name: "AspNetRoles");
            migrationBuilder.DropTable(name: "AspNetUsers");
            migrationBuilder.DropTable(name: "MealPlans");
            migrationBuilder.DropTable(name: "Recipes");
            migrationBuilder.DropTable(name: "Products");
            migrationBuilder.DropTable(name: "ShoppingLists");
            migrationBuilder.DropTable(name: "RecipeCategories");
            migrationBuilder.DropTable(name: "ProductCategories");
            migrationBuilder.DropTable(name: "Units");
            migrationBuilder.DropTable(name: "Shops");
        }
    }
}