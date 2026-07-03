using System.Reflection;

namespace Common.Data.DataContext.Tests
{
    [TestFixture]
    public class TableConfigurationAssembliesTests
    {
        [Test]
        public void Ctor_StoresAssemblies_AsReadOnlyList()
        {
            var a1 = typeof(object).Assembly;
            var a2 = typeof(TableConfigurationAssemblies).Assembly;

            var sut = new TableConfigurationAssemblies([a1, a2]);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(sut.Assemblies, Has.Count.EqualTo(2));
                Assert.That(sut.Assemblies, Contains.Item(a1));
                Assert.That(sut.Assemblies, Contains.Item(a2));
            }
        }

        [Test]
        public void Ctor_WithEmptyList_ProducesEmptyAssemblies()
        {
            var sut = new TableConfigurationAssemblies([]);

            Assert.That(sut.Assemblies, Is.Empty);
        }

        [Test]
        public void Assemblies_IsReadOnly()
        {
            var sut = new TableConfigurationAssemblies([typeof(object).Assembly]);

            Assert.That(sut.Assemblies, Is.InstanceOf<IReadOnlyList<Assembly>>());
        }

        [Test]
        public void Ctor_WithSingleAssembly_StoresIt()
        {
            var assembly = typeof(TableConfigurationAssemblies).Assembly;

            var sut = new TableConfigurationAssemblies([assembly]);

            Assert.That(sut.Assemblies.Single(), Is.EqualTo(assembly));
        }
    }
}
