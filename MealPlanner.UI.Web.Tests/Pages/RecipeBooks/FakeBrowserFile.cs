using Microsoft.AspNetCore.Components.Forms;

namespace MealPlanner.UI.Web.Tests.Pages.RecipeBooks
{
    public class FakeBrowserFile(byte[] data, string name, string contentType, bool throwOnOpen = false) : IBrowserFile
    {
        private readonly long _size = data.LongLength;

        public string Name { get; } = name;
        public DateTimeOffset LastModified => DateTimeOffset.Now;
        public long Size => _size;
        public string ContentType { get; } = contentType;

        public Stream OpenReadStream(long maxAllowedSize = 512000, CancellationToken cancellationToken = default)
        {
            if (throwOnOpen)
            {
                throw new InvalidOperationException("Too big");
            }

            if (_size > maxAllowedSize)
            {
                throw new IOException("File too large");
            }

            return new MemoryStream(data);
        }
    }
}
