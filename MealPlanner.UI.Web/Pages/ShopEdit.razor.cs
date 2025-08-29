using BlazorBootstrap;
using Common.Models;
using MealPlanner.Shared.Models;
using MealPlanner.UI.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;

namespace MealPlanner.UI.Web.Pages
{
    [Authorize]
    public partial class ShopEdit
    {
        private List<BreadcrumbItem>? NavItems { get; set; }

        [Parameter]
        public string? Id { get; set; }
        public ShopEditModel? Shop { get; set; }

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
            NavItems = new List<BreadcrumbItem>
            {
                new BreadcrumbItem{ Text = "Shops", Href ="/shopsoverview" },
                new BreadcrumbItem{ Text = "Shop", IsCurrentPage = true },
            };

            _ = int.TryParse(Id, out var id);
            if (id == 0)
            {
                var categories = await ProductCategoriesService!.SearchAsync();
                Shop = new ShopEditModel(categories!.Items);
            }
            else
            {
                Shop = await ShopService!.GetEditAsync(id);
            }
        }

        private async Task SaveAsync()
        {
            var response = Shop?.Id == 0 ? await ShopService!.AddAsync(Shop) : await ShopService!.UpdateAsync(Shop!);
            if (response != null && !response.Succeeded)
            {
                MessageComponent?.ShowError(response.Message!);
            }
            else
            {
                MessageComponent?.ShowInfo("Data has been saved successfully");
                NavigateToOverview();
            }
        }

        private async Task DeleteAsync()
        {
            if (Shop?.Id != 0)
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

                var response = await ShopService!.DeleteAsync(Shop!.Id);
                if (response != null && !response.Succeeded)
                {
                    MessageComponent?.ShowError(response.Message!);
                }
                else
                {
                    MessageComponent?.ShowInfo("Data has been deleted successfully");
                    NavigateToOverview();
                }
            }
        }

        private bool CanMoveUp(ShopDisplaySequenceEditModel item)
        {
            return Shop?.DisplaySequence?.IndexOf(item) - 1 >= 0;
        }

        private void MoveUp(ShopDisplaySequenceEditModel item)
        {
            int index = Shop!.DisplaySequence!.IndexOf(item);
            Shop.DisplaySequence.RemoveAt(index);
            if (index - 1 >= 0)
            {
                Shop.DisplaySequence.Insert(index - 1, item);
            }
            Shop!.DisplaySequence.SetIndexes();
        }

        private bool CanMoveDown(ShopDisplaySequenceEditModel item)
        {
            return Shop?.DisplaySequence?.IndexOf(item) + 2 <= Shop?.DisplaySequence?.Count;
        }

        private void MoveDown(ShopDisplaySequenceEditModel item)
        {
            int index = Shop!.DisplaySequence!.IndexOf(item);
            Shop.DisplaySequence.RemoveAt(index);
            if (index + 1 <= Shop?.DisplaySequence.Count)
            {
                Shop.DisplaySequence.Insert(index + 1, item);
            }
            Shop!.DisplaySequence.SetIndexes();
        }

        private void NavigateToOverview()
        {
            NavigationManager?.NavigateTo("/shopsoverview");
        }
    }
}
