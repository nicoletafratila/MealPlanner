using System.ComponentModel.DataAnnotations;
using Blazored.Modal;
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
        public BlazoredModalInstance? ModalInstance { get; set; }

        public IModalController? ModalController { get; set; }

        [Required]
        public string? MealPlanId { get; set; }

        public PagedList<MealPlanModel>? MealPlans { get; set; } = new([], new Metadata());

        [Inject]
        public IMealPlanService? MealPlanService { get; set; }

        protected override async Task OnInitializedAsync()
        {
            var result = await MealPlanService.SearchAsync();
            MealPlans = result ?? new PagedList<MealPlanModel>([], new Metadata());
        }

        protected override void OnParametersSet()
        {
            if (ModalController is null && ModalInstance is not null)
            {
                ModalController = new BlazoredModalController(ModalInstance);
            }
        }

        private async Task SaveAsync()
        {
            await ModalController!.CloseAsync(MealPlanId);
        }

        private async Task CancelAsync()
        {
            await ModalController!.CancelAsync();
        }
    }
}
