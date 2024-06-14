using Common.Data.DataContext;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MealPlanner.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            if (!migrationBuilder.TableExists<MealPlannerDbContext>("MealPlans"))
            {
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
            }

            if (!migrationBuilder.TableExists<MealPlannerDbContext>("ProductCategories"))
            {
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
            }

            if (!migrationBuilder.TableExists<MealPlannerDbContext>("RecipeCategories"))
            {
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
            }

            if (!migrationBuilder.TableExists<MealPlannerDbContext>("Shops"))
            {
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
            }

            if (!migrationBuilder.TableExists<MealPlannerDbContext>("Units"))
            {
                migrationBuilder.CreateTable(
                name: "Units",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Units", x => x.Id);
                });
            }

            if (!migrationBuilder.TableExists<MealPlannerDbContext>("Recipes"))
            {
                migrationBuilder.CreateTable(
                name: "Recipes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImageContent = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
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
                        onDelete: ReferentialAction.Cascade);
                });
            }

            if (!migrationBuilder.TableExists<MealPlannerDbContext>("ShopDisplaySequences"))
            {
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
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ShopDisplaySequences_Shops_ShopId",
                        column: x => x.ShopId,
                        principalTable: "Shops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
            }

            if (!migrationBuilder.TableExists<MealPlannerDbContext>("ShoppingLists"))
            {
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
                        onDelete: ReferentialAction.Cascade);
                });
            }

            if (!migrationBuilder.TableExists<MealPlannerDbContext>("Products"))
            {
                migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImageContent = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
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
                        onDelete: ReferentialAction.Cascade);
                });
            }

            if (!migrationBuilder.TableExists<MealPlannerDbContext>("MealPlanRecipes"))
            {
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
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MealPlanRecipes_Recipes_RecipeId",
                        column: x => x.RecipeId,
                        principalTable: "Recipes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
            }

            if (!migrationBuilder.TableExists<MealPlannerDbContext>("RecipeIngredients"))
            {
                migrationBuilder.CreateTable(
                name: "RecipeIngredients",
                columns: table => new
                {
                    RecipeId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<double>(type: "double(18,2)", precision: 18, scale: 2, nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecipeIngredients", x => new { x.RecipeId, x.ProductId });
                    table.ForeignKey(
                        name: "FK_RecipeIngredients_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RecipeIngredients_Recipes_RecipeId",
                        column: x => x.RecipeId,
                        principalTable: "Recipes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
            }

            if (!migrationBuilder.TableExists<MealPlannerDbContext>("ShoppingListProducts"))
            {
                migrationBuilder.CreateTable(
                name: "ShoppingListProducts",
                columns: table => new
                {
                    ShoppingListId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    Collected = table.Column<bool>(type: "bit", nullable: false),
                    DisplaySequence = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<double>(type: "double(18,2)", precision: 18, scale: 2, nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShoppingListProducts", x => new { x.ShoppingListId, x.ProductId });
                    table.ForeignKey(
                        name: "FK_ShoppingListProducts_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ShoppingListProducts_ShoppingLists_ShoppingListId",
                        column: x => x.ShoppingListId,
                        principalTable: "ShoppingLists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
            }

            if (!migrationBuilder.IndexExists<MealPlannerDbContext>("IX_MealPlanRecipes_RecipeId", "MealPlanRecipes"))
            {
                migrationBuilder.CreateIndex(
                name: "IX_MealPlanRecipes_RecipeId",
                table: "MealPlanRecipes",
                column: "RecipeId");
            }

            if (!migrationBuilder.IndexExists<MealPlannerDbContext>("IX_Products_ProductCategoryId", "Products"))
            {
                migrationBuilder.CreateIndex(
                    name: "IX_Products_ProductCategoryId",
                    table: "Products",
                    column: "ProductCategoryId");
            }

            if (!migrationBuilder.IndexExists<MealPlannerDbContext>("IX_RecipeIngredients_ProductId", "RecipeIngredients"))
            {
                migrationBuilder.CreateIndex(
                            name: "IX_RecipeIngredients_ProductId",
                            table: "RecipeIngredients",
                            column: "ProductId");
            }

            if (!migrationBuilder.IndexExists<MealPlannerDbContext>("IX_Recipes_RecipeCategoryId", "Recipes"))
            {
                migrationBuilder.CreateIndex(
                            name: "IX_Recipes_RecipeCategoryId",
                            table: "Recipes",
                            column: "RecipeCategoryId");
            }

            if (!migrationBuilder.IndexExists<MealPlannerDbContext>("IX_ShopDisplaySequences_ProductCategoryId", "ShopDisplaySequences"))
            {
                migrationBuilder.CreateIndex(
                            name: "IX_ShopDisplaySequences_ProductCategoryId",
                            table: "ShopDisplaySequences",
                            column: "ProductCategoryId");
            }

            if (!migrationBuilder.IndexExists<MealPlannerDbContext>("IX_ShoppingListProducts_ProductId", "ShoppingListProducts"))
            {
                migrationBuilder.CreateIndex(
                            name: "IX_ShoppingListProducts_ProductId",
                            table: "ShoppingListProducts",
                            column: "ProductId");
            }

            if (!migrationBuilder.IndexExists<MealPlannerDbContext>("IX_ShoppingLists_ShopId", "ShoppingLists"))
            {
                migrationBuilder.CreateIndex(
                            name: "IX_ShoppingLists_ShopId",
                            table: "ShoppingLists",
                            column: "ShopId");
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MealPlanRecipes");

            migrationBuilder.DropTable(
                name: "RecipeIngredients");

            migrationBuilder.DropTable(
                name: "ShopDisplaySequences");

            migrationBuilder.DropTable(
                name: "ShoppingListProducts");

            migrationBuilder.DropTable(
                name: "MealPlans");

            migrationBuilder.DropTable(
                name: "Recipes");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "ShoppingLists");

            migrationBuilder.DropTable(
                name: "RecipeCategories");

            migrationBuilder.DropTable(
                name: "ProductCategories");

            migrationBuilder.DropTable(
                name: "Units");

            migrationBuilder.DropTable(
                name: "Shops");
        }
    }
}
