﻿using BlazorBootstrap;
using Common.Models;
using Common.Pagination;
using MealPlanner.UI.Web.Services;
using MealPlanner.UI.Web.Shared;
using Microsoft.AspNetCore.Components;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Pages
{
    public partial class RecipeCategoriesOverview
    {
        private List<BreadcrumbItem>? NavItems { get; set; }

        public PagedList<RecipeCategoryModel>? Categories { get; set; }

        [Inject]
        public IRecipeCategoryService? RecipeCategoriesService { get; set; }

        [Inject]
        public NavigationManager? NavigationManager { get; set; }

        [CascadingParameter(Name = "MessageComponent")]
        protected IMessageComponent? MessageComponent { get; set; }

        protected ConfirmDialog dialog = default!;
        protected GridTemplate<RecipeCategoryModel>? recipeCategoryGrid;

        protected override async Task OnInitializedAsync()
        {
            NavItems = new List<BreadcrumbItem>
            {
               new BreadcrumbItem{ Text = "Home", Href ="/recipesoverview" }
            };
            await RefreshAsync();
        }

        private void New()
        {
            NavigationManager?.NavigateTo($"recipecategoryedit/");
        }

        private void Update(RecipeCategoryModel item)
        {
            NavigationManager?.NavigateTo($"recipecategoryedit/{item.Id}");
        }

        private async void DeleteAsync(RecipeCategoryModel item)
        {
            if (item != null)
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

                var response = await RecipeCategoriesService!.DeleteAsync(item.Id);
                if (!string.IsNullOrWhiteSpace(response))
                {
                    MessageComponent?.ShowError(response);
                }
                else
                {
                    MessageComponent?.ShowInfo("Data has been deleted successfully");
                    await recipeCategoryGrid!.RefreshDataAsync();
                }
            }
        }

        private async void SaveAsync()
        {
            var response = await RecipeCategoriesService!.UpdateAsync(Categories!.Items!);
            if (!string.IsNullOrWhiteSpace(response))
            {
                MessageComponent?.ShowError(response);
            }
            else
            {
                MessageComponent?.ShowInfo("Data has been saved successfully");
                await RefreshAsync();
            }
        }

        private bool CanMoveUp(RecipeCategoryModel item)
        {
            return Categories!.Items!.IndexOf(item) - 1 >= 0;
        }

        private void MoveUp(RecipeCategoryModel item)
        {
            int index = Categories!.Items!.IndexOf(item);
            Categories!.Items!.RemoveAt(index);
            if (index - 1 >= 0)
            {
                Categories!.Items!.Insert(index - 1, item);
            }
            Categories!.Items!.SetIndexes();
        }

        private bool CanMoveDown(RecipeCategoryModel item)
        {
            return Categories!.Items!.IndexOf(item) + 2 <= Categories!.Items!.Count;
        }

        private void MoveDown(RecipeCategoryModel item)
        {
            int index = Categories!.Items!.IndexOf(item);
            Categories!.Items!.RemoveAt(index);
            if (index + 1 <= Categories!.Items!.Count)
            {
                Categories!.Items!.Insert(index + 1, item);
            }
            Categories!.Items!.SetIndexes();
        }

        private async Task RefreshAsync()
        {
            var request = new GridDataProviderRequest<RecipeCategoryModel>
            {
                Filters = new List<FilterItem>() { },
                Sorting = new List<SortingItem<RecipeCategoryModel>>
                        {
                            new SortingItem<RecipeCategoryModel>("Name", item => item.Name!, SortDirection.Ascending),
                        },
                PageNumber = 1,
                PageSize = 10
            };
            await CategoriesDataProvider(request);
        }

        private async Task<GridDataProviderResult<RecipeCategoryModel>> CategoriesDataProvider(GridDataProviderRequest<RecipeCategoryModel> request)
        {
            string sortString = "";
            SortDirection sortDirection = SortDirection.None;

            if (request.Sorting is not null && request.Sorting.Any())
            {
                sortString = request.Sorting.FirstOrDefault()!.SortString;
                sortDirection = request.Sorting.FirstOrDefault()!.SortDirection;
            }
            var queryParameters = new QueryParameters()
            {
                Filters = request.Filters,
                SortString = sortString,
                SortDirection = sortDirection,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
            };

            Categories = await RecipeCategoriesService!.SearchAsync(queryParameters);
            return await Task.FromResult(new GridDataProviderResult<RecipeCategoryModel> { Data = Categories!.Items, TotalCount = Categories.Metadata!.TotalCount });
        }
    }
}
