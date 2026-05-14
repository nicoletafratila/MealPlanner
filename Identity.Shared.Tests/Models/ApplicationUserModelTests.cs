using Identity.Shared.Models;

namespace Identity.Shared.Tests.Models
{
    [TestFixture]
    public class ApplicationUserModelTests
    {
        [Test]
        public void DefaultCtor_InitializesDefaults()
        {
            var model = new ApplicationUserModel();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(model.UserId, Is.EqualTo(string.Empty));
                Assert.That(model.Username, Is.EqualTo(string.Empty));
                Assert.That(model.FirstName, Is.Null);
                Assert.That(model.LastName, Is.Null);
                Assert.That(model.EmailAddress, Is.Null);
                Assert.That(model.IsActive, Is.False);
                Assert.That(model.Index, Is.Zero);
                Assert.That(model.IsSelected, Is.False);
            }
        }

        [Test]
        public void Properties_CanBeSetAndRead()
        {
            var model = new ApplicationUserModel
            {
                UserId = "u1",
                Username = "alice",
                FirstName = "Alice",
                LastName = "Smith",
                EmailAddress = "alice@example.com",
                IsActive = true,
                Index = 3
            };

            using (Assert.EnterMultipleScope())
            {
                Assert.That(model.UserId, Is.EqualTo("u1"));
                Assert.That(model.Username, Is.EqualTo("alice"));
                Assert.That(model.FirstName, Is.EqualTo("Alice"));
                Assert.That(model.LastName, Is.EqualTo("Smith"));
                Assert.That(model.EmailAddress, Is.EqualTo("alice@example.com"));
                Assert.That(model.IsActive, Is.True);
                Assert.That(model.Index, Is.EqualTo(3));
            }
        }
    }
}
