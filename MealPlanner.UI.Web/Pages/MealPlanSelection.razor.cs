using System.ComponentModel.DataAnnotations;
using BlazorBootstrap;
using Blazored.Modal;
using Blazored.Modal.Services;
using Common.Pagination;
using MealPlanner.Shared.Models;
using MealPlanner.UI.Web.Services;
using Microsoft.AspNetCore.Components;

namespace MealPlanner.UI.Web.Pages
{
    public partial class MealPlanSelection : IComponent
    {
        [Required]
        public string? MealPlanId { get; set; }

        public PagedList<MealPlanModel>? MealPlans { get; set; }

        [Inject]
        public IMealPlanService? MealPlanService { get; set; }

        [CascadingParameter]
        protected BlazoredModalInstance BlazoredModal { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            MealPlans = await MealPlanService!.SearchAsync();
            BlazoredModal.SetTitle("Select a meal plan");
        }

        private async void SaveAsync()
        {
            await BlazoredModal.CloseAsync(ModalResult.Ok(MealPlanId));
        }

        private async void CancelAsync()
        {
            await BlazoredModal.CancelAsync();
        }
    }
}
