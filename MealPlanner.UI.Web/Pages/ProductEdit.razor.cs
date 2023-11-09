﻿using MealPlanner.UI.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Pages
{
    public partial class ProductEdit
    {
        [Parameter]
        public string? Id { get; set; }

        private string? _categoryId;
        public string? CategoryId
        {
            get
            {
                return _categoryId;
            }
            set
            {
                if (_categoryId != value)
                {
                    _categoryId = value;
                    Product!.ProductCategoryId = int.Parse(_categoryId!);
                }
            }
        }

        private string? _unitId;
        public string? UnitId
        {
            get
            {
                return _unitId;
            }
            set
            {
                if (_unitId != value)
                {
                    _unitId = value;
                    Product!.UnitId = int.Parse(_unitId!);
                }
            }
        }

        public EditProductModel? Product { get; set; }
        public IList<ProductCategoryModel>? Categories { get; set; }
        public IList<UnitModel>? Units { get; set; }
        private long maxFileSize = 1024L * 1024L * 1024L * 3L;

        [Inject]
        public IProductService? ProductService { get; set; }

        [Inject]
        public IProductCategoryService? CategoryService { get; set; }

        [Inject]
        public IUnitService? UnitService { get; set; }

        [Inject]
        public NavigationManager? NavigationManager { get; set; }

        [Inject]
        public IJSRuntime? JSRuntime { get; set; }

        [CascadingParameter(Name = "ErrorComponent")]
        protected IErrorComponent? ErrorComponent { get; set; }

        protected override async Task OnInitializedAsync()
        {
            int.TryParse(Id, out var id);
            Categories = await CategoryService!.GetAllAsync();
            Units = await UnitService!.GetAllAsync();

            if (id == 0)
            {
                Product = new EditProductModel();
            }
            else
            {
                Product = await ProductService!.GetEditAsync(int.Parse(Id!));
            }

            CategoryId = Product!.ProductCategoryId.ToString();
            UnitId = Product.UnitId.ToString();
        }

        protected async Task SaveAsync()
        {
            var response = Product!.Id == 0 ? await ProductService!.AddAsync(Product) : await ProductService!.UpdateAsync(Product);
            if (!string.IsNullOrWhiteSpace(response))
            {
                ErrorComponent!.ShowError("Error", response);
            }
            else
            {
                NavigateToOverview();
            }
        }

        protected async Task DeleteAsync()
        {
            if (Product!.Id != 0)
            {
                if (!await JSRuntime!.Confirm($"Are you sure you want to delete the product: '{Product.Name}'?"))
                    return;

                var response = await ProductService!.DeleteAsync(Product.Id);
                if (!string.IsNullOrWhiteSpace(response))
                {
                    ErrorComponent!.ShowError("Error", response);
                }
                else
                {
                    NavigateToOverview();
                }
            }
        }

        protected void NavigateToOverview()
        {
            NavigationManager!.NavigateTo("/productsoverview");
        }

        private async Task OnInputFileChangeAsync(InputFileChangeEventArgs e)
        {
            try
            {
                if (e.File != null)
                {
                    Stream stream = e.File.OpenReadStream(maxAllowedSize: 1024 * 300);
                    MemoryStream ms = new MemoryStream();
                    await stream.CopyToAsync(ms);
                    stream.Close();
                    Product!.ImageContent = ms.ToArray();
                }
                StateHasChanged();
            }
            catch (Exception)
            {
                ErrorComponent!.ShowError("Error", $"File size exceeds the limit. Maximum allowed size is <strong>{maxFileSize / (1024 * 1024)} MB</strong>.");
                return;
            }
        }
    }
}
