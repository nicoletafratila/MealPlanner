using Microsoft.AspNetCore.Components.Forms;

namespace MealPlanner.UI.Web.Tests.Pages.RecipeBooks
{
    public class FakeBrowserFile : IBrowserFile
    {
        private readonly byte[] _data;
        private readonly long _size;
        private readonly bool _throwOnOpen;

        public FakeBrowserFile(byte[] data, string name, string contentType, bool throwOnOpen = false)
        {
            _data = data;
            Name = name;
            ContentType = contentType;
            _size = data.LongLength;
            _throwOnOpen = throwOnOpen;
        }

        public string Name { get; }
        public DateTimeOffset LastModified => DateTimeOffset.Now;
        public long Size => _size;
        public string ContentType { get; }

        public Stream OpenReadStream(long maxAllowedSize = 512000, CancellationToken cancellationToken = default)
        {
            if (_throwOnOpen)
            {
                throw new InvalidOperationException("Too big");
            }

            if (_size > maxAllowedSize)
            {
                throw new IOException("File too large");
            }

            return new MemoryStream(_data);
        }
    }
}
