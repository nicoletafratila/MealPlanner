using System.Collections.ObjectModel;
using Common.Pagination;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MealPlanner.Services.Http;
using MealPlanner.Shared.Models;
using MealPlanner.Shared.Resources;
using RecipeBook.Services.Http;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Mobile.ViewModels.MealPlans
{
    [QueryProperty(nameof(ShopId), "id")]
    public partial class ShopEditViewModel(IShopService shopService, ProductCategoryService categoryService) : BaseViewModel
    {
        [ObservableProperty]
        private string _shopId = string.Empty;

        [ObservableProperty]
        private ShopEditModel _model = new();

        [ObservableProperty]
        private bool _isNew;

        [ObservableProperty]
        private ObservableCollection<ShopDisplaySequenceEditModel> _displaySequence = [];

        partial void OnShopIdChanged(string value)
        {
            Guid.TryParse(value, out var id);
            IsNew = id == Guid.Empty;
            _ = LoadAsync();
        }

        [RelayCommand(AllowConcurrentExecutions = true)]
        private async Task LoadAsync()
        {
            IsBusy = true;
            try
            {
                if (IsNew)
                {
                    var categories = await categoryService.SearchAsync(new QueryParameters<ProductCategoryModel> { PageSize = 200, Sorting = DefaultSorting });
                    Model = new ShopEditModel(categories?.Items ?? []);
                }
                else
                {
                    Guid.TryParse(ShopId, out var id);
                    Model = await shopService.GetEditAsync(id) ?? new();
                }

                DisplaySequence = new ObservableCollection<ShopDisplaySequenceEditModel>(Model.DisplaySequence ?? []);
            }
            catch (Exception ex)
            {
                SetError(ex.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private void MoveUp(ShopDisplaySequenceEditModel item)
        {
            var list = DisplaySequence.ToList();
            var index = list.IndexOf(item);
            if (index <= 0) return;

            list.RemoveAt(index);
            list.Insert(index - 1, item);
            Resequence(list);
        }

        [RelayCommand]
        private void MoveDown(ShopDisplaySequenceEditModel item)
        {
            var list = DisplaySequence.ToList();
            var index = list.IndexOf(item);
            if (index < 0 || index >= list.Count - 1) return;

            list.RemoveAt(index);
            list.Insert(index + 1, item);
            Resequence(list);
        }

        private void Resequence(List<ShopDisplaySequenceEditModel> list)
        {
            for (var i = 0; i < list.Count; i++)
                list[i].Value = i + 1;

            DisplaySequence = new ObservableCollection<ShopDisplaySequenceEditModel>(list);
        }

        [RelayCommand(AllowConcurrentExecutions = true)]
        private async Task DeleteAsync()
        {
            if (IsNew) return;
            var confirm = await Shell.Current.DisplayAlertAsync(
                Pages.MealPlans.Resources.ShopEditPage.DeleteConfirmTitle,
                Pages.MealPlans.Resources.ShopEditPage.DeleteConfirmMessage,
                Pages.MealPlans.Resources.ShopEditPage.DeleteButton,
                Pages.MealPlans.Resources.ShopEditPage.CancelButton);
            if (!confirm) return;
            IsBusy = true; ClearMessages();
            try
            {
                Guid.TryParse(ShopId, out var deleteId);
                var result = await shopService.DeleteAsync(deleteId);
                if (result?.Succeeded == true) await Shell.Current.GoToAsync("..");
                else SetError(result?.Message);
            }
            catch (Exception ex)
            {
                SetError(ex.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand(AllowConcurrentExecutions = true)]
        private async Task SaveAsync()
        {
            if (IsBusy) return;
            ClearMessages();

            if (string.IsNullOrWhiteSpace(Model.Name))
            {
                SetError(MealPlannerSharedMessages.ShopNameRequired);
                return;
            }

            Model.DisplaySequence = DisplaySequence.ToList();

            if (Model.DisplaySequence is null || Model.DisplaySequence.Count == 0)
            {
                SetError(MealPlannerSharedMessages.ShopRequiresCategoryOrder);
                return;
            }

            IsBusy = true;
            try
            {
                var result = IsNew ? await shopService.AddAsync(Model) : await shopService.UpdateAsync(Model);
                if (result?.Succeeded == true) await Shell.Current.GoToAsync("..");
                else SetError(result?.Message);
            }
            catch (Exception ex)
            {
                SetError(ex.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
