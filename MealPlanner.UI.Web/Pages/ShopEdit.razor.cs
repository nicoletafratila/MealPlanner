using BlazorBootstrap;
using MealPlanner.Shared.Models;
using MealPlanner.UI.Web.Services;
using Microsoft.AspNetCore.Components;

namespace MealPlanner.UI.Web.Pages
{
    public partial class ShopEdit
    {
        [Parameter]
        public string? Id { get; set; }
        public EditShopModel? Shop { get; set; }

        [Inject]
        public IShopService? ShopService { get; set; }

        [Inject]
        public IProductCategoryService? ProductCategoriesService { get; set; }

        [Inject]
        public NavigationManager? NavigationManager { get; set; }

        [CascadingParameter(Name = "MessageComponent")]
        protected IMessageComponent? MessageComponent { get; set; }

        protected ConfirmDialog dialog = default!;

        protected override async Task OnInitializedAsync()
        {
            _ = int.TryParse(Id, out var id);

            if (id == 0)
            {
                var categories = await ProductCategoriesService!.GetAllAsync();
                Shop = new EditShopModel(categories);
            }
            else
            {
                Shop = await ShopService!.GetEditAsync(id);
            }
        }

        private async void SaveAsync()
        {
            var response = Shop!.Id == 0 ? await ShopService!.AddAsync(Shop) : await ShopService!.UpdateAsync(Shop);
            if (!string.IsNullOrWhiteSpace(response))
            {
                MessageComponent!.ShowError(response);
            }
            else
            {
                MessageComponent!.ShowInfo("Data has been saved successfully");
                NavigateToOverview();
            }
        }

        private async void DeleteAsync()
        {
            if (Shop!.Id != 0)
            {
                var options = new ConfirmDialogOptions
                {
                    YesButtonText = "OK",
                    YesButtonColor = ButtonColor.Success,
                    NoButtonText = "Cancel",
                    NoButtonColor = ButtonColor.Danger
                };
                var confirmation = await dialog.ShowAsync(
                        title: "Are you sure you want to delete this?",
                        message1: "This will delete the record. Once deleted can not be rolled back.",
                        message2: "Do you want to proceed?",
                        confirmDialogOptions: options);

                if (!confirmation)
                    return;

                var response = await ShopService!.DeleteAsync(Shop.Id);
                if (!string.IsNullOrWhiteSpace(response))
                {
                    MessageComponent!.ShowError(response);
                }
                else
                {
                    MessageComponent!.ShowInfo("Data has been deleted successfully");
                    NavigateToOverview();
                }
            }
        }

        private bool CanMoveUp(ShopDisplaySequenceModel item)
        {
            return Shop!.DisplaySequence!.IndexOf(item) - 1 >= 0;
        }

        private void MoveUp(ShopDisplaySequenceModel item)
        {
            int index = Shop!.DisplaySequence!.IndexOf(item);
            Shop.DisplaySequence.RemoveAt(index);
            if (index - 1 >= 0)
            {
                Shop.DisplaySequence.Insert(index - 1, item);
            }
            UpdateDisplaySeqenceValues();
        }

        private bool CanMoveDown(ShopDisplaySequenceModel item)
        {
            return Shop!.DisplaySequence!.IndexOf(item) + 2 <= Shop!.DisplaySequence.Count;
        }

        private void MoveDown(ShopDisplaySequenceModel item)
        {
            int index = Shop!.DisplaySequence!.IndexOf(item);
            Shop.DisplaySequence.RemoveAt(index);
            if (index + 1 <= Shop!.DisplaySequence.Count)
            {
                Shop.DisplaySequence.Insert(index + 1, item);
            }
            UpdateDisplaySeqenceValues();
        }

        private void UpdateDisplaySeqenceValues()
        {
            for (int i = 0; i < Shop!.DisplaySequence!.Count; i++)
            {
                Shop.DisplaySequence[i].Value = i + 1;
            }
        }

        private void NavigateToOverview()
        {
            NavigationManager!.NavigateTo("/shopsoverview");
        }
    }
}
