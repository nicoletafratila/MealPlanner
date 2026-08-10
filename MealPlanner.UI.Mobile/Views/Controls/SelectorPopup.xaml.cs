using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Views;

namespace MealPlanner.UI.Mobile.Views.Controls
{
    public partial class SelectorPopup : Popup<object>
    {
        private readonly IReadOnlyList<SelectorItem> _allItems;
        private readonly ObservableCollection<SelectorItem> _filteredItems;
        private bool _isClosing;

        public SelectorPopup(IReadOnlyList<SelectorItem> items, string header, string searchPlaceholder, string emptyText)
        {
            InitializeComponent();
            _allItems = items;
            _filteredItems = new ObservableCollection<SelectorItem>(items);
            ItemsView.ItemsSource = _filteredItems;
            HeaderLabel.Text = header;
            ItemSearch.Placeholder = searchPlaceholder;
            EmptyLabel.Text = emptyText;
        }

        private void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
        {
            var query = e.NewTextValue?.Trim();
            var matches = string.IsNullOrWhiteSpace(query)
                ? _allItems
                : _allItems.Where(i => i.Name.Contains(query, StringComparison.OrdinalIgnoreCase));

            _filteredItems.Clear();
            foreach (var item in matches)
                _filteredItems.Add(item);
        }

        private async void OnItemSelected(object? sender, SelectionChangedEventArgs e)
        {
            if (_isClosing) return;
            if (e.CurrentSelection.FirstOrDefault() is not SelectorItem item) return;

            _isClosing = true;
            await CloseAsync(item.Value, CancellationToken.None);
        }
    }
}
