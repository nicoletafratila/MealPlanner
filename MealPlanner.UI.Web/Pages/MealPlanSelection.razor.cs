using System.ComponentModel.DataAnnotations;
using Blazored.Modal;
using Blazored.Modal.Services;
using Common.Pagination;
using MealPlanner.Shared.Models;
using MealPlanner.UI.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;

namespace MealPlanner.UI.Web.Pages
{
    [Authorize]
    public partial class MealPlanSelection : IComponent
    {
        [CascadingParameter]
        protected BlazoredModalInstance blazoredModal { get; set; } = default!;

        [Required]
        public string? MealPlanId { get; set; }

        public PagedList<MealPlanModel>? MealPlans { get; set; }

        [Inject]
        public IMealPlanService? MealPlanService { get; set; }

        protected override async Task OnInitializedAsync()
        {
            MealPlans = await MealPlanService!.SearchAsync();
            blazoredModal.SetTitle("Select a meal plan");
        }

        private async Task SaveAsync()
        {
            await blazoredModal.CloseAsync(ModalResult.Ok(MealPlanId));
        }

        private async Task CancelAsync()
        {
            await blazoredModal.CancelAsync();
        }
    }
}
