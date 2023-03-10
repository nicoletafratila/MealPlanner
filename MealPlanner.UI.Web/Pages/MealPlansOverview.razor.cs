﻿using Common.Api;
using MealPlanner.Shared.Models;
using MealPlanner.UI.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace MealPlanner.UI.Web.Pages
{
    public partial class MealPlansOverview
    {
        public IList<MealPlanModel> MealPlans { get; set; } = new List<MealPlanModel>();
        public MealPlanModel MealPlan { get; set; } = new MealPlanModel();

        [Inject]
        public IMealPlanService MealPlanService { get; set; }

        [Inject]
        public NavigationManager NavigationManager { get; set; }

        [Inject]
        public IJSRuntime JSRuntime { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await RefreshAsync();
        }

        protected void New()
        {
            NavigationManager.NavigateTo($"mealplanedit/");
        }

        protected void Update(MealPlanModel item)
        {
            NavigationManager.NavigateTo($"mealplanedit/{item.Id}");
        }

        protected async Task DeleteAsync(MealPlanModel item)
        {
            if (item != null)
            {
                if (!await JSRuntime.Confirm($"Are you sure you want to delete the meal plan: '{item.Name}'?"))
                    return;

                await MealPlanService.DeleteAsync(item.Id);
                await RefreshAsync();
            }
        }

        protected async Task RefreshAsync()
        {
            MealPlans = await MealPlanService.GetAllAsync();
        }
    }
}
