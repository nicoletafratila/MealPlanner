namespace Common.Models
{
    public static class BaseModelExtensions
    {
        public static void SetIndexes<T>(this IList<T> models) where T : BaseModel
        {
            foreach (var item in models)
            {
                item.Index = models.IndexOf(item) + 1;
            }
        }
    }
}
