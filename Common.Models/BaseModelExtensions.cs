namespace Common.Models
{
    public static class BaseModelExtensions
    {
        public static void SetIndexes<T>(this IList<T> models) where T : BaseModel
        {
            ArgumentNullException.ThrowIfNull(models);

            for (var i = 0; i < models.Count; i++)
            {
                var item = models[i];
                if (item is null)
                {
                    continue; 
                }

                item.Index = i + 1;
            }
        }
    }
}