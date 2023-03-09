﻿using Common.Api;
using MealPlanner.UI.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Pages
{
    public partial class IngredientEdit
    {
        [Parameter]
        public string Id { get; set; }

        private string _ingredientCategoryId;
        public string IngredientCategoryId
        {
            get
            {
                return _ingredientCategoryId;
            }
            set
            {
                if (_ingredientCategoryId != value)
                {
                    _ingredientCategoryId = value;
                    Ingredient.IngredientCategoryId = int.Parse(_ingredientCategoryId);
                }
            }
        }

        private string _unitId;
        public string UnitId
        {
            get
            {
                return _unitId;
            }
            set
            {
                if (_unitId != value)
                {
                    _unitId = value;
                    Ingredient.UnitId = int.Parse(_unitId);
                }
            }
        }

        public EditIngredientModel Ingredient { get; set; } = new EditIngredientModel();
        public List<IngredientCategoryModel> Categories { get; set; } = new List<IngredientCategoryModel>();
        public List<UnitModel> Units { get; set; } = new List<UnitModel>();

        [Inject]
        public IIngredientService IngredientService { get; set; }

        [Inject]
        public IIngredientCategoryService CategoryService { get; set; }

        [Inject]
        public IUnitService UnitService { get; set; }

        [Inject]
        public NavigationManager NavigationManager { get; set; }

        [Inject]
        public IJSRuntime JSRuntime { get; set; }

        protected override async Task OnInitializedAsync()
        {
            int.TryParse(Id, out var id);
            Categories = (await CategoryService.GetAll()).ToList();
            Units = (await UnitService.GetAll()).ToList();

            if (id == 0)
            {
                Ingredient = new EditIngredientModel();
            }
            else
            {
                Ingredient = await IngredientService.GetAsync(int.Parse(Id));
            }

            IngredientCategoryId = Ingredient.IngredientCategoryId.ToString();
            UnitId = Ingredient.UnitId.ToString();
        }

        protected async Task Save()
        {
            if (Ingredient.Id == 0)
            {
                var addedEntity = await IngredientService.AddAsync(Ingredient);
                if (addedEntity != null)
                {
                    NavigateToOverview();
                }
            }
            else
            {
                await IngredientService.UpdateAsync(Ingredient);
                NavigateToOverview();
            }
        }

        protected async Task Delete()
        {
            if (Ingredient.Id != 0)
            {
                if (!await JSRuntime.Confirm($"Are you sure you want to delete the ingredient: '{Ingredient.Name}'?"))
                    return;

                await IngredientService.DeleteAsync(Ingredient.Id);
                NavigateToOverview();
            }
        }

        protected void NavigateToOverview()
        {
            NavigationManager.NavigateTo("/ingredientsoverview");
        }
    }
}