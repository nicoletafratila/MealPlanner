using Microsoft.Extensions.Configuration;

namespace Common.Api.Tests
{
    [TestFixture]
    public class ApiConfigTests
    {
        [Test]
        public void Ctor_NullConfiguration_ThrowsArgumentNullException()
        {
            IConfiguration? configuration = null;

            Assert.That(
                () => new ApiConfig(configuration!),
                Throws.TypeOf<ArgumentNullException>()
                      .With.Property(nameof(ArgumentNullException.ParamName))
                      .EqualTo("configuration"));
        }

        [Test]
        public void Ctor_WhenNameIsEmpty_BindsFromRoot()
        {
            var settings = new Dictionary<string, string?>
            {
                ["BaseUrl"] = "https://example.com",
                ["Timeout"] = "30",
                ["Controllers:Products"] = "products",
                ["Controllers:Orders"] = "orders"
            };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(settings!)
                .Build();

            var config = new ApiConfig(configuration);

            Assert.Multiple(() =>
            {
                Assert.That(config.BaseUrl, Is.EqualTo(new Uri("https://example.com")));
                Assert.That(config.Timeout, Is.EqualTo(30));
                Assert.That(config.Controllers, Is.Not.Null);
            });
            Assert.That(config.Controllers, Has.Count.EqualTo(2));
            Assert.Multiple(() =>
            {
                Assert.That(config.Controllers["Products"], Is.EqualTo("products"));
                Assert.That(config.Controllers["Orders"], Is.EqualTo("orders"));
            });
        }

        [Test]
        public void Ctor_WhenNameProvided_BindsFromNamedSection()
        {
            var settings = new Dictionary<string, string?>
            {
                ["MyApi:BaseUrl"] = "https://api.example.com",
                ["MyApi:Timeout"] = "10",
                ["MyApi:Controllers:Users"] = "users",
                ["MyApi:Controllers:Reports"] = "reports",
                ["OtherApi:Timeout"] = "999"
            };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(settings!)
                .Build();

            var config = new TestApiConfig(configuration);

            Assert.Multiple(() =>
            {
                Assert.That(config.BaseUrl, Is.EqualTo(new Uri("https://api.example.com")));
                Assert.That(config.Timeout, Is.EqualTo(10));
                Assert.That(config.Controllers, Is.Not.Null);
            });
            Assert.That(config.Controllers, Has.Count.EqualTo(2));
            Assert.Multiple(() =>
            {
                Assert.That(config.Controllers["Users"], Is.EqualTo("users"));
                Assert.That(config.Controllers["Reports"], Is.EqualTo("reports"));
            });
        }

        [Test]
        public void Controllers_IsInitializedToEmptyDictionary()
        {
            var configuration = new ConfigurationBuilder().Build();

            var config = new ApiConfig(configuration);

            Assert.That(config.Controllers, Is.Not.Null);
            Assert.That(config.Controllers, Is.Empty);
        }
    }
}