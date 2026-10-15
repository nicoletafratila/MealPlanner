using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Identity.Services.Http;
using Identity.Shared.Models;
using MealPlanner.Services.Http;
using MealPlanner.Shared.Models;
using MealPlanner.Shared.Resources;
using MealPlanner.UI.Mobile.Services;
using Microsoft.Maui.Graphics.Platform;

namespace MealPlanner.UI.Mobile.ViewModels.MealPlans
{
    [QueryProperty(nameof(ShopId), "id")]
    public partial class ShopEditViewModel(
        IShopService shopService,
        IApplicationUserService applicationUserService,
        ReferenceDataCacheService lookupDataService) : BaseViewModel
    {
        private const float MaxImageDimension = 1024;
        private const float ImageQuality = 0.8f;

        [ObservableProperty]
        private string _shopId = string.Empty;

        [ObservableProperty]
        private ShopEditModel _model = new();

        [ObservableProperty]
        private bool _isNew;

        [ObservableProperty]
        private ObservableCollection<ShopDisplaySequenceEditModel> _displaySequence = [];

        // Plain string (not ImageSource) so this stays constructible/testable outside a running
        // MAUI app; the XAML binding resolves the "no image" fallback to a bundled asset.
        [ObservableProperty]
        private string? _shopImageUrl;

        partial void OnShopIdChanged(string value)
        {
            Guid.TryParse(value, out var id);
            IsNew = id == Guid.Empty;
            _ = LoadAsync();
        }

        [RelayCommand(AllowConcurrentExecutions = true)]
        private async Task LoadAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                if (IsNew)
                {
                    await lookupDataService.EnsureLoadedAsync();
                    Model = new ShopEditModel(lookupDataService.ProductCategories);
                }
                else
                {
                    Guid.TryParse(ShopId, out var id);
                    Model = await shopService.GetEditAsync(id) ?? new();
                }

                ShopImageUrl = Model.ImageUrl;

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

        [RelayCommand(AllowConcurrentExecutions = true)]
        private async Task PickImageAsync()
        {
            try
            {
                var results = await MediaPicker.Default.PickPhotosAsync();
                var result = results?.FirstOrDefault();
                if (result is null) return;
                await using var stream = await result.OpenReadAsync();
                Model.ImageContent = await ResizeImageAsync(stream);
                ShopImageUrl = $"data:image/jpg;base64,{Convert.ToBase64String(Model.ImageContent)}";
            }
            catch (Exception ex)
            {
                SetError(ex.Message);
            }
        }

        private static Task<byte[]> ResizeImageAsync(Stream stream) => Task.Run(() =>
        {
            using var image = PlatformImage.FromStream(stream);
            using var resized = image.Downsize(MaxImageDimension, disposeOriginal: false);
            using var output = new MemoryStream();
            resized.Save(output, ImageFormat.Jpeg, ImageQuality);
            return output.ToArray();
        });

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

        public async Task<List<ApplicationUserListModel>> GetShareUsersAsync()
            => (await applicationUserService.ListAsync())?.ToList() ?? [];

        public async Task ShareToUserAsync(string targetUserId)
        {
            if (IsNew || IsBusy || string.IsNullOrWhiteSpace(targetUserId)) return;
            ClearMessages();
            IsBusy = true;
            try
            {
                Guid.TryParse(ShopId, out var id);
                var result = await shopService.ShareAsync(id, targetUserId);
                if (result?.Succeeded == true)
                {
                    lookupDataService.InvalidateShops();
                    SetSuccess(Pages.MealPlans.Resources.ShopEditPage.ShareSucceeded);
                }
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
                if (result?.Succeeded == true)
                {
                    lookupDataService.InvalidateShops();
                    await Shell.Current.GoToAsync("..");
                }
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
