using System.ComponentModel.DataAnnotations;
using Identity.Shared.Models;

namespace Identity.Shared.Tests.Models
{
    [TestFixture]
    public class ApplicationUserEditModelTests
    {
        private static bool TryValidate(object model, out IList<ValidationResult> results)
        {
            var context = new ValidationContext(model);
            results = new List<ValidationResult>();
            return Validator.TryValidateObject(model, context, results, validateAllProperties: true);
        }

        [Test]
        public void DefaultCtor_InitializesDefaults_ButIsInvalid()
        {
            // Act
            var model = new ApplicationUserEditModel();
            var isValid = TryValidate(model, out var results);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(model.Username, Is.EqualTo(string.Empty));
                Assert.That(model.EmailAddress, Is.EqualTo(string.Empty));

                Assert.That(isValid, Is.False);
                Assert.That(results.Any(r => r.MemberNames.Contains(nameof(ApplicationUserEditModel.Username))), Is.True);
                Assert.That(results.Any(r => r.MemberNames.Contains(nameof(ApplicationUserEditModel.EmailAddress))), Is.True);
            });
        }

        [Test]
        public void ValidModel_PassesValidation()
        {
            // Arrange
            var model = new ApplicationUserEditModel
            {
                UserId = "user-1",
                Username = "user1",
                FirstName = "John",
                LastName = "Doe",
                EmailAddress = "john.doe@example.com",
                PhoneNumber = "+123456789",
                IsActive = true
            };

            // Act
            var isValid = TryValidate(model, out var results);

            // Assert
            Assert.That(isValid, Is.True);
            Assert.That(results, Is.Empty);
        }

        [Test]
        public void Username_Required_ErrorWhenEmpty()
        {
            var model = new ApplicationUserEditModel
            {
                Username = "",
                EmailAddress = "test@example.com"
            };

            var isValid = TryValidate(model, out var results);

            Assert.That(isValid, Is.False);
            Assert.That(results.Any(r => r.MemberNames.Contains(nameof(ApplicationUserEditModel.Username))), Is.True);
        }

        [Test]
        public void EmailAddress_Required_And_MustBeValidFormat()
        {
            // Missing email
            var model = new ApplicationUserEditModel
            {
                Username = "user1",
                EmailAddress = ""
            };

            var isValid = TryValidate(model, out var results);

            Assert.That(isValid, Is.False);
            Assert.That(results.Any(r => r.MemberNames.Contains(nameof(ApplicationUserEditModel.EmailAddress))), Is.True);

            // Invalid format
            model.EmailAddress = "not-an-email";
            isValid = TryValidate(model, out results);

            Assert.That(isValid, Is.False);
            Assert.That(results.Any(r => r.MemberNames.Contains(nameof(ApplicationUserEditModel.EmailAddress))), Is.True);

            // Valid email
            model.EmailAddress = "valid@example.com";
            isValid = TryValidate(model, out results);

            Assert.That(isValid, Is.True);
        }

        [Test]
        public void FirstName_AllowsLettersAndSpacesOnly()
        {
            var model = new ApplicationUserEditModel
            {
                Username = "user1",
                EmailAddress = "user1@example.com",
                FirstName = "John Doe"
            };

            var isValid = TryValidate(model, out var results);

            Assert.That(isValid, Is.True);

            model.FirstName = "John123";
            isValid = TryValidate(model, out results);

            Assert.That(isValid, Is.False);
            Assert.That(results.Any(r => r.MemberNames.Contains(nameof(ApplicationUserEditModel.FirstName))), Is.True);
        }

        [Test]
        public void LastName_AllowsLettersAndSpacesOnly()
        {
            var model = new ApplicationUserEditModel
            {
                Username = "user1",
                EmailAddress = "user1@example.com",
                LastName = "Smith Jr"
            };

            var isValid = TryValidate(model, out var results);

            Assert.That(isValid, Is.True);

            model.LastName = "Smith123";
            isValid = TryValidate(model, out results);

            Assert.That(isValid, Is.False);
            Assert.That(results.Any(r => r.MemberNames.Contains(nameof(ApplicationUserEditModel.LastName))), Is.True);
        }
    }
}