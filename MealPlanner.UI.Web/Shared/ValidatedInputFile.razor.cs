using System.Linq.Expressions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace MealPlanner.UI.Web.Shared
{
    public partial class ValidatedInputFile : ComponentBase, IDisposable
    {
        private FieldIdentifier _fieldIdentifier;

        [CascadingParameter]
        private EditContext CurrentEditContext { get; set; } = default!;

        [Parameter]
        public string? Id { get; set; }

        [Parameter]
        public string CssClass { get; set; } = "form-control";

        [Parameter]
        public string? Accept { get; set; }

        [Parameter]
        public EventCallback<InputFileChangeEventArgs> OnChange { get; set; }

        [Parameter, EditorRequired]
        public Expression<Func<object>> For { get; set; } = default!;

        private string CssClassValue => $"{CssClass} {CurrentEditContext.FieldCssClass(_fieldIdentifier)}";

        protected override void OnInitialized()
        {
            _fieldIdentifier = FieldIdentifier.Create(For);
            CurrentEditContext.OnValidationStateChanged += HandleValidationStateChanged;
        }

        private void HandleValidationStateChanged(object? sender, ValidationStateChangedEventArgs e)
            => StateHasChanged();

        public void Dispose()
            => CurrentEditContext.OnValidationStateChanged -= HandleValidationStateChanged;
    }
}
