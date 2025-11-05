using System.ComponentModel.DataAnnotations;
using Blazored.Modal;
using Blazored.Modal.Services;
using Common.Pagination;
using MealPlanner.Shared.Models;
using MealPlanner.UI.Web.Services.MealPlans;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;

namespace MealPlanner.UI.Web.Pages.MealPlans
{
    [Authorize]
    public partial class MealPlanSelection : IComponent
    {
        [CascadingParameter]
        private BlazoredModalInstance BlazoredModal { get; set; } = default!;

        [Required]
        public string? MealPlanId { get; set; }

        public PagedList<MealPlanModel>? MealPlans { get; set; }

        [Inject]
        public IMealPlanService? MealPlanService { get; set; }

        protected override async Task OnInitializedAsync()
        {
            MealPlans = await MealPlanService!.SearchAsync();
        }

        private async Task SaveAsync()
        {
            await BlazoredModal.CloseAsync(ModalResult.Ok(MealPlanId));
        }

        private async Task CancelAsync()
        {
            await BlazoredModal.CancelAsync();
        }
    }
}
