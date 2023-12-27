using Microsoft.AspNetCore.Components;

namespace MealPlanner.UI.Web.Shared
{
    public partial class Pagination
    {
        [Parameter]
        [EditorRequired]
        public int CurrentPage { get; set; } = 1;

        [Parameter]
        [EditorRequired]
        public int TotalItems { get; set; }

        [Parameter]
        [EditorRequired]
        public int TotalPages { get; set; }

        [Parameter]
        public bool HasNext { get; set; }

        [Parameter]
        public bool HasPrevious { get; set; }

        [Parameter]
        public bool ShowGoToFirst { get; set; }

        [Parameter]
        public bool ShowGoToLast { get; set; }

        [Parameter]
        [EditorRequired]
        public EventCallback<int> PageChanged { get; set; }

        private async Task OnPageChangedAsync(int pageNumber)
        {
            await PageChanged.InvokeAsync(pageNumber);
        }

        public int StartPage
        {
            get
            {
                var startPage = CurrentPage - 2;
                if (startPage < 1)
                {
                    startPage = 1;
                }

                return startPage;
            }
        }

        public int MaxPage
        {
            get
            {
                var maxPage = StartPage + 4;
                if (maxPage > TotalPages)
                {
                    maxPage = TotalPages;
                }

                return maxPage;
            }
        }
    }
}
