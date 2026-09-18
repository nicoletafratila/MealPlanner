using System.Linq.Expressions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;

namespace MealPlanner.UI.Web.Shared
{
    public partial class ComboBox<TItem, TValue> : ComponentBase, IDisposable
    {
        private FieldIdentifier _fieldIdentifier;
        private string _filterText = string.Empty;

        [CascadingParameter]
        private EditContext? CurrentEditContext { get; set; }

        [Parameter]
        public string Id { get; set; } = default!;

        [Parameter]
        public string CssClass { get; set; } = "form-select text-start d-flex align-items-center";

        [Parameter]
        public IEnumerable<TItem>? Items { get; set; }

        [Parameter, EditorRequired]
        public Func<TItem, TValue> IdSelector { get; set; } = default!;

        [Parameter, EditorRequired]
        public Func<TItem, string> TextSelector { get; set; } = default!;

        [Parameter]
        public TValue Value { get; set; } = default!;

        [Parameter]
        public EventCallback<TValue> ValueChanged { get; set; }

        [Parameter]
        public string? Placeholder { get; set; }

        [Parameter]
        public bool ShowClearButton { get; set; }

        [Parameter]
        public Expression<Func<TValue>>? For { get; set; }

        private string FilterText => _filterText;

        private IEnumerable<TItem> FilteredItems =>
            Items is null
                ? []
                : string.IsNullOrWhiteSpace(_filterText)
                    ? Items
                    : Items.Where(item => TextSelector(item).Contains(_filterText, StringComparison.OrdinalIgnoreCase));

        private string SelectedText
        {
            get
            {
                if (Items is not null)
                {
                    foreach (var item in Items)
                    {
                        if (EqualityComparer<TValue>.Default.Equals(IdSelector(item), Value))
                            return TextSelector(item);
                    }
                }

                return Placeholder ?? string.Empty;
            }
        }

        private bool HasValue => !EqualityComparer<TValue>.Default.Equals(Value, default!);

        private string CssClassValue =>
            For is null || CurrentEditContext is null
                ? CssClass
                : $"{CssClass} {CurrentEditContext.FieldCssClass(_fieldIdentifier)}";

        protected override void OnParametersSet()
        {
            if (For is null || CurrentEditContext is null)
                return;

            var newFieldIdentifier = FieldIdentifier.Create(For);
            if (newFieldIdentifier.Equals(_fieldIdentifier))
                return;

            Unsubscribe();
            _fieldIdentifier = newFieldIdentifier;
            CurrentEditContext.OnValidationStateChanged += HandleValidationStateChanged;
        }

        private void OnFilterInput(ChangeEventArgs e)
        {
            _filterText = e.Value?.ToString() ?? string.Empty;
        }

        private async Task SelectAsync(TItem item)
        {
            var newValue = IdSelector(item);
            _filterText = string.Empty;
            Value = newValue;
            await ValueChanged.InvokeAsync(newValue);
        }

        private async Task ClearAsync()
        {
            var newValue = default(TValue)!;
            _filterText = string.Empty;
            Value = newValue;
            await ValueChanged.InvokeAsync(newValue);
        }

        private void HandleValidationStateChanged(object? sender, ValidationStateChangedEventArgs e)
            => StateHasChanged();

        private void Unsubscribe()
        {
            if (CurrentEditContext is not null)
                CurrentEditContext.OnValidationStateChanged -= HandleValidationStateChanged;
        }

        public void Dispose() => Unsubscribe();
    }
}
