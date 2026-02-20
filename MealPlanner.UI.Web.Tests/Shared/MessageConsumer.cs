using Common.UI;
using Microsoft.AspNetCore.Components;

namespace MealPlanner.UI.Web.Tests.Shared
{
    public class MessageConsumer : ComponentBase
    {
        [CascadingParameter(Name = "MessageComponent")]
        public IMessageComponent? MessageComponent { get; set; }

        [Parameter]
        public string Message { get; set; } = "Child message";

        protected override void OnInitialized()
        {
            MessageComponent?.ShowInfo(Message);
        }
    }
}
