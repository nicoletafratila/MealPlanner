using Microsoft.Extensions.DependencyInjection;

namespace Common.Data.DataContext
{
    public class ServiceLocator
    {
        private static ServiceProvider? _currentServiceProvider;

        public ServiceLocator(ServiceProvider currentServiceProvider)
        {
            _currentServiceProvider = currentServiceProvider;
        }

        public static ServiceLocator Current
        {
            get
            {
                return new ServiceLocator(_currentServiceProvider!);
            }
        }

        public static void SetLocatorProvider(ServiceProvider serviceProvider)
        {
            _currentServiceProvider = serviceProvider;
        }

        public object GetInstance(Type serviceType)
        {
            return _currentServiceProvider!.GetService(serviceType)!;
        }

        public TService GetInstance<TService>()
        {
            return _currentServiceProvider!.GetService<TService>()!;
        }
    }
}
