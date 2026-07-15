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

        [ObservableProperty]
        private IReadOnlyList<UnitModel> _compatibleUnits = [];

        public IReadOnlyList<ProductModel> Products { get; }
        public IReadOnlyList<UnitModel> Units { get; }

        partial void OnSelectedProductChanged(ProductModel? value)
        {
            CompatibleUnits = FilterUnits(value);
            if (value?.BaseUnit is { } baseUnit)
                SelectedUnit = CompatibleUnits.FirstOrDefault(u => u.Id == baseUnit.Id);
        }

        private IReadOnlyList<UnitModel> FilterUnits(ProductModel? product) =>
            product?.BaseUnit is { } baseUnit
                ? Units.Where(u => u.UnitType == baseUnit.UnitType).ToList()
                : Units;

        public RecipeIngredientEditViewModel(IReadOnlyList<ProductModel> products, IReadOnlyList<UnitModel> units)
        {
            Products = products;
            Units = units;
            _compatibleUnits = units;
        }

        public RecipeIngredientEditViewModel(RecipeIngredientEditModel model, IReadOnlyList<ProductModel> products, IReadOnlyList<UnitModel> units)
        {
            Model = model;
            Products = products;
            Units = units;
            _selectedProduct = products.FirstOrDefault(p => p.Id == model.ProductId);
            _compatibleUnits = FilterUnits(_selectedProduct);
            _selectedUnit = _compatibleUnits.FirstOrDefault(u => u.Id == model.UnitId);
            _quantity = model.Quantity;
        }
    }
}
