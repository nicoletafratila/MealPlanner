﻿// <auto-generated />
using System;
using Common.Data.DataContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace MealPlanner.Api.Migrations
{
    [DbContext(typeof(MealPlannerDbContext))]
    [Migration("20240605105153_Products_UnitId")]
    partial class Products_UnitId
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.3")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("Common.Data.Entities.Log", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Exception")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Level")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Message")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("MessageTemplate")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Properties")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("TimeStamp")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.ToTable("Logs");
                });

            modelBuilder.Entity("Common.Data.Entities.MealPlan", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("MealPlans");
                });

            modelBuilder.Entity("Common.Data.Entities.MealPlanRecipe", b =>
                {
                    b.Property<int>("MealPlanId")
                        .HasColumnType("int");

                    b.Property<int>("RecipeId")
                        .HasColumnType("int");

                    b.HasKey("MealPlanId", "RecipeId");

                    b.HasIndex("RecipeId");

                    b.ToTable("MealPlanRecipes");
                });

            modelBuilder.Entity("Common.Data.Entities.Product", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<byte[]>("ImageContent")
                        .HasColumnType("varbinary(max)");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("ProductCategoryId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("ProductCategoryId");

                    b.ToTable("Products");
                });

            modelBuilder.Entity("Common.Data.Entities.ProductCategory", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("ProductCategories");
                });

            modelBuilder.Entity("Common.Data.Entities.Recipe", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<byte[]>("ImageContent")
                        .HasColumnType("varbinary(max)");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("RecipeCategoryId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("RecipeCategoryId");

                    b.ToTable("Recipes");
                });

            modelBuilder.Entity("Common.Data.Entities.RecipeCategory", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("DisplaySequence")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("RecipeCategories");
                });

            modelBuilder.Entity("Common.Data.Entities.RecipeIngredient", b =>
                {
                    b.Property<int>("RecipeId")
                        .HasColumnType("int");

                    b.Property<int>("ProductId")
                        .HasColumnType("int");

                    b.Property<decimal>("Quantity")
                        .HasPrecision(18, 2)
                        .HasColumnType("decimal(18,2)");

                    b.Property<int>("UnitId")
                        .HasColumnType("int");

                    b.HasKey("RecipeId", "ProductId");

                    b.HasIndex("ProductId");

                    b.HasIndex("UnitId");

                    b.ToTable("RecipeIngredients");
                });

            modelBuilder.Entity("Common.Data.Entities.Shop", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Shops");
                });

            modelBuilder.Entity("Common.Data.Entities.ShopDisplaySequence", b =>
                {
                    b.Property<int>("ShopId")
                        .HasColumnType("int");

                    b.Property<int>("ProductCategoryId")
                        .HasColumnType("int");

                    b.Property<int>("Value")
                        .HasColumnType("int");

                    b.HasKey("ShopId", "ProductCategoryId");

                    b.HasIndex("ProductCategoryId");

                    b.ToTable("ShopDisplaySequences");
                });

            modelBuilder.Entity("Common.Data.Entities.ShoppingList", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("ShopId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("ShopId");

                    b.ToTable("ShoppingLists");
                });

            modelBuilder.Entity("Common.Data.Entities.ShoppingListProduct", b =>
                {
                    b.Property<int>("ShoppingListId")
                        .HasColumnType("int");

                    b.Property<int>("ProductId")
                        .HasColumnType("int");

                    b.Property<bool>("Collected")
                        .HasColumnType("bit");

                    b.Property<int>("DisplaySequence")
                        .HasColumnType("int");

                    b.Property<decimal>("Quantity")
                        .HasPrecision(18, 2)
                        .HasColumnType("decimal(18,2)");

                    b.Property<int>("UnitId")
                        .HasColumnType("int");

                    b.HasKey("ShoppingListId", "ProductId");

                    b.HasIndex("ProductId");

                    b.HasIndex("UnitId");

                    b.ToTable("ShoppingListProducts");
                });

            modelBuilder.Entity("Common.Data.Entities.Unit", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Units");
                });

            modelBuilder.Entity("Common.Data.Entities.MealPlanRecipe", b =>
                {
                    b.HasOne("Common.Data.Entities.MealPlan", "MealPlan")
                        .WithMany("MealPlanRecipes")
                        .HasForeignKey("MealPlanId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Common.Data.Entities.Recipe", "Recipe")
                        .WithMany()
                        .HasForeignKey("RecipeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("MealPlan");

                    b.Navigation("Recipe");
                });

            modelBuilder.Entity("Common.Data.Entities.Product", b =>
                {
                    b.HasOne("Common.Data.Entities.ProductCategory", "ProductCategory")
                        .WithMany()
                        .HasForeignKey("ProductCategoryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ProductCategory");
                });

            modelBuilder.Entity("Common.Data.Entities.Recipe", b =>
                {
                    b.HasOne("Common.Data.Entities.RecipeCategory", "RecipeCategory")
                        .WithMany()
                        .HasForeignKey("RecipeCategoryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("RecipeCategory");
                });

            modelBuilder.Entity("Common.Data.Entities.RecipeIngredient", b =>
                {
                    b.HasOne("Common.Data.Entities.Product", "Product")
                        .WithMany()
                        .HasForeignKey("ProductId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Common.Data.Entities.Recipe", "Recipe")
                        .WithMany("RecipeIngredients")
                        .HasForeignKey("RecipeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Common.Data.Entities.Unit", "Unit")
                        .WithMany()
                        .HasForeignKey("UnitId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Product");

                    b.Navigation("Recipe");

                    b.Navigation("Unit");
                });

            modelBuilder.Entity("Common.Data.Entities.ShopDisplaySequence", b =>
                {
                    b.HasOne("Common.Data.Entities.ProductCategory", "ProductCategory")
                        .WithMany()
                        .HasForeignKey("ProductCategoryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Common.Data.Entities.Shop", "Shop")
                        .WithMany("DisplaySequence")
                        .HasForeignKey("ShopId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ProductCategory");

                    b.Navigation("Shop");
                });

            modelBuilder.Entity("Common.Data.Entities.ShoppingList", b =>
                {
                    b.HasOne("Common.Data.Entities.Shop", "Shop")
                        .WithMany()
                        .HasForeignKey("ShopId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Shop");
                });

            modelBuilder.Entity("Common.Data.Entities.ShoppingListProduct", b =>
                {
                    b.HasOne("Common.Data.Entities.Product", "Product")
                        .WithMany()
                        .HasForeignKey("ProductId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Common.Data.Entities.ShoppingList", "ShoppingList")
                        .WithMany("Products")
                        .HasForeignKey("ShoppingListId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Common.Data.Entities.Unit", "Unit")
                        .WithMany()
                        .HasForeignKey("UnitId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Product");

                    b.Navigation("ShoppingList");

                    b.Navigation("Unit");
                });

            modelBuilder.Entity("Common.Data.Entities.MealPlan", b =>
                {
                    b.Navigation("MealPlanRecipes");
                });

            modelBuilder.Entity("Common.Data.Entities.Recipe", b =>
                {
                    b.Navigation("RecipeIngredients");
                });

            modelBuilder.Entity("Common.Data.Entities.Shop", b =>
                {
                    b.Navigation("DisplaySequence");
                });

            modelBuilder.Entity("Common.Data.Entities.ShoppingList", b =>
                {
                    b.Navigation("Products");
                });
#pragma warning restore 612, 618
        }
    }
}
