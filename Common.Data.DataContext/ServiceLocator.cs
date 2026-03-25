using Microsoft.Extensions.DependencyInjection;

namespace Common.Data.DataContext
{
    public static class ServiceLocator
    {
        private static IServiceProvider? _currentServiceProvider;

        public static bool IsInitialized => _currentServiceProvider != null;

        public static void SetLocatorProvider(IServiceProvider serviceProvider)
        {
            _currentServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public static IServiceProvider Current => _currentServiceProvider ?? throw new InvalidOperationException("ServiceLocator is not initialized. Call SetLocatorProvider(...) first.");

        public static object GetInstance(Type serviceType)
        {
            ArgumentNullException.ThrowIfNull(serviceType);
            return Current.GetRequiredService(serviceType);
        }

        public static TService GetInstance<TService>()
            where TService : notnull
        {
            return Current.GetRequiredService<TService>();
        }

        public static TService? TryGetInstance<TService>()
        {
            var provider = _currentServiceProvider;
            if (provider is null)
            {
                return default;
            }

            return provider.GetService<TService>();
        }

        public static void Reset()
        {
            _currentServiceProvider = null;
        }
    }
}
