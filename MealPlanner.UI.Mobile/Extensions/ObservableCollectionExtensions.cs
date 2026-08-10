using System.Collections.ObjectModel;

namespace MealPlanner.UI.Mobile.Extensions
{
    public static class ObservableCollectionExtensions
    {
        public static void Replace<T>(this ObservableCollection<T> collection, IEnumerable<T> items)
        {
            collection.Clear();
            foreach (var item in items)
                collection.Add(item);
        }
    }
}
