using Blazored.Modal.Services;
using Blazored.Modal;
using MealPlanner.Shared.Models;
using MealPlanner.UI.Web.Services;
using Microsoft.AspNetCore.Components;
using System.ComponentModel.DataAnnotations;

namespace MealPlanner.UI.Web.Pages
{
    public partial class MealPlanSelection : IComponent
    {
        [Required]
        public string? MealPlanId { get; set; }

        public IList<MealPlanModel>? MealPlans { get; set; }

        [Inject]
        public IMealPlanService? MealPlanService { get; set; }

        [CascadingParameter]
        protected BlazoredModalInstance BlazoredModal { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            MealPlans = await MealPlanService!.GetAllAsync();
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
