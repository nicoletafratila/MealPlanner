using CommunityToolkit.Mvvm.ComponentModel;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Mobile.ViewModels.RecipeBook
{
    public partial class RecipeIngredientEditViewModel : ObservableObject
    {
        public RecipeIngredientEditModel Model { get; } = new();

        [ObservableProperty]
        private ProductModel? _selectedProduct;

        [ObservableProperty]
        private UnitModel? _selectedUnit;

        [ObservableProperty]
        private decimal _quantity;

        public IReadOnlyList<ProductModel> Products { get; }
        public IReadOnlyList<UnitModel> Units { get; }

        public RecipeIngredientEditViewModel(IReadOnlyList<ProductModel> products, IReadOnlyList<UnitModel> units)
        {
            Products = products;
            Units = units;
        }

        public RecipeIngredientEditViewModel(RecipeIngredientEditModel model, IReadOnlyList<ProductModel> products, IReadOnlyList<UnitModel> units)
        {
            Model = model;
            Products = products;
            Units = units;
            _selectedProduct = products.FirstOrDefault(p => p.Id == model.ProductId);
            _selectedUnit = units.FirstOrDefault(u => u.Id == model.UnitId);
            _quantity = model.Quantity;
        }
    }
}
