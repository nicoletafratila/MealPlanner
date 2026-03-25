using Microsoft.Extensions.DependencyInjection;

namespace Common.Data.DataContext.Tests
{
    [TestFixture]
    public class ServiceLocatorTests
    {
        private class DummyService { }

        [TearDown]
        public void TearDown()
        {
            ServiceLocator.Reset();
        }

        [Test]
        public void Current_WhenNotInitialized_ThrowsInvalidOperationException()
        {
            ServiceLocator.Reset();

            Assert.That(
                () => { var _ = ServiceLocator.Current; },
                Throws.TypeOf<InvalidOperationException>()
                      .With.Message.Contains("not initialized"));
        }

        [Test]
        public void SetLocatorProvider_Null_ThrowsArgumentNullException()
        {
            Assert.That(
                () => ServiceLocator.SetLocatorProvider(null!),
                Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void IsInitialized_ReflectsInitializationState()
        {
            Assert.That(ServiceLocator.IsInitialized, Is.False);

            var provider = new ServiceCollection()
                .AddSingleton<DummyService>()
                .BuildServiceProvider();

            ServiceLocator.SetLocatorProvider(provider);

            Assert.That(ServiceLocator.IsInitialized, Is.True);
        }

        [Test]
        public void GetInstance_Generic_ReturnsRegisteredService()
        {
            var provider = new ServiceCollection()
                .AddSingleton<DummyService>()
                .BuildServiceProvider();

            ServiceLocator.SetLocatorProvider(provider);

            var instance = ServiceLocator.GetInstance<DummyService>();

            Assert.That(instance, Is.Not.Null);
            Assert.That(instance, Is.InstanceOf<DummyService>());
        }

        [Test]
        public void GetInstance_ByType_ReturnsRegisteredService()
        {
            var provider = new ServiceCollection()
                .AddSingleton<DummyService>()
                .BuildServiceProvider();

            ServiceLocator.SetLocatorProvider(provider);

            var instance = (DummyService)ServiceLocator.GetInstance(typeof(DummyService));

            Assert.That(instance, Is.Not.Null);
            Assert.That(instance, Is.InstanceOf<DummyService>());
        }

        [Test]
        public void GetInstance_WhenServiceNotRegistered_ThrowsInvalidOperationException()
        {
            var provider = new ServiceCollection()
                .BuildServiceProvider();

            ServiceLocator.SetLocatorProvider(provider);

            Assert.That(
                () => ServiceLocator.GetInstance<DummyService>(),
                Throws.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void TryGetInstance_WhenServiceRegistered_ReturnsInstance()
        {
            var provider = new ServiceCollection()
                .AddSingleton<DummyService>()
                .BuildServiceProvider();

            ServiceLocator.SetLocatorProvider(provider);

            var instance = ServiceLocator.TryGetInstance<DummyService>();

            Assert.That(instance, Is.Not.Null);
            Assert.That(instance, Is.InstanceOf<DummyService>());
        }

        [Test]
        public void TryGetInstance_WhenServiceNotRegistered_ReturnsNull()
        {
            var provider = new ServiceCollection()
                .BuildServiceProvider();

            ServiceLocator.SetLocatorProvider(provider);

            var instance = ServiceLocator.TryGetInstance<DummyService>();

            Assert.That(instance, Is.Null);
        }

        [Test]
        public void TryGetInstance_WhenNotInitialized_ReturnsNull()
        {
            ServiceLocator.Reset();

            var instance = ServiceLocator.TryGetInstance<DummyService>();

            Assert.That(instance, Is.Null);
        }
    }
}