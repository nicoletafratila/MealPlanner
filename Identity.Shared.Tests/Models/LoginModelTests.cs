using System.ComponentModel.DataAnnotations;
using Identity.Shared.Models;

namespace Identity.Shared.Tests.Models
{
    [TestFixture]
    public class LoginModelTests
    {
        private static bool TryValidate(object model, out IList<ValidationResult> results)
        {
            var context = new ValidationContext(model);
            results = [];
            return Validator.TryValidateObject(model, context, results, validateAllProperties: true);
        }

        [Test]
        public void DefaultCtor_InitializesDefaults_ButIsInvalid_BecauseUsernameRequired()
        {
            // Act
            var model = new LoginModel();
            var isValid = TryValidate(model, out var results);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(model.Username, Is.EqualTo(string.Empty));
                Assert.That(model.Password, Is.Null);
                Assert.That(model.RememberLogin, Is.False);

                Assert.That(isValid, Is.False);
                Assert.That(results.Any(r => r.MemberNames.Contains(nameof(LoginModel.Username))), Is.True);
            }
        }

        [Test]
        public void ValidModel_PassesValidation()
        {
            // Arrange
            var model = new LoginModel
            {
                Username = "user1",
                Password = "P@ssw0rd",
                RememberLogin = true
            };

            // Act
            var isValid = TryValidate(model, out var results);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(isValid, Is.True);
                Assert.That(results, Is.Empty);
            }
        }

        [Test]
        public void Username_Required_ErrorWhenEmpty()
        {
            // Arrange
            var model = new LoginModel
            {
                Username = "",
                Password = "anything"
            };

            // Act
            var isValid = TryValidate(model, out var results);

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(isValid, Is.False);
                Assert.That(results.Any(r => r.MemberNames.Contains(nameof(LoginModel.Username))), Is.True);
            }
        }

        [Test]
        public void Password_CanBeNullOrEmpty_AndStillValid()
        {
            // Arrange
            var model = new LoginModel
            {
                Username = "user1",
                Password = null
            };

            // Act
            var isValid = TryValidate(model, out var results);

            using (Assert.EnterMultipleScope())
            {
                // Assert: since Password is not [Required], this is valid
                Assert.That(isValid, Is.True);
                Assert.That(results, Is.Empty);
            }
        }
    }
}