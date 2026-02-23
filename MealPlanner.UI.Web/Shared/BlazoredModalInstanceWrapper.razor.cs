using Blazored.Modal;
using MealPlanner.UI.Web.Pages;
using Microsoft.AspNetCore.Components;

namespace MealPlanner.UI.Web.Shared
{
    public partial class BlazoredModalInstanceWrapper
    {
        [CascadingParameter] 
        private BlazoredModalInstance BlazoredModal { get; set; } = default!;
        private IModalController _controller = default!;

        protected override void OnInitialized()
        {
            _controller = new BlazoredModalController(BlazoredModal);
        }
    }
}
