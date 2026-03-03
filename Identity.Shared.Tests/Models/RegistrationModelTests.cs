using System.ComponentModel.DataAnnotations;
using Identity.Shared.Models;

namespace Identity.Shared.Tests.Models
{
    [TestFixture]
    public class RegistrationModelTests
    {
        private static bool TryValidateConfirmPassword(
            RegistrationModel model,
            out IList<ValidationResult> results)
        {
            var context = new ValidationContext(model)
            {
                MemberName = nameof(RegistrationModel.ConfirmPassword)
            };

            results = [];

            return Validator.TryValidateProperty(
                model.ConfirmPassword,
                context,
                results);
        }

        [Test]
        public void ConfirmPassword_Required_ErrorWhenNullOrEmpty()
        {
            // Arrange
            var model = new RegistrationModel
            {
                Password = "P@ssw0rd",
                ConfirmPassword = null! // simulate missing value
            };

            // Act
            var isValid = TryValidateConfirmPassword(model, out var results);

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(isValid, Is.False);
                Assert.That(results.Any(), Is.True);
                Assert.That(
                    results.First().ErrorMessage,
                    Is.EqualTo("Confirm password is required."));
            }
        }

        [Test]
        public void ConfirmPassword_MustMatchPassword_WhenSet()
        {
            // Arrange
            var model = new RegistrationModel
            {
                Password = "P@ssw0rd",
                ConfirmPassword = "Different"
            };

            // Act
            var isValid = TryValidateConfirmPassword(model, out var results);

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(isValid, Is.False);
                Assert.That(results.Any(), Is.True);
                Assert.That(
                    results.First().ErrorMessage,
                    Is.EqualTo("Password and confirm password do not match."));
            }
        }

        [Test]
        public void ConfirmPassword_Valid_WhenMatchesPassword()
        {
            // Arrange
            var model = new RegistrationModel
            {
                Password = "P@ssw0rd",
                ConfirmPassword = "P@ssw0rd"
            };

            // Act
            var isValid = TryValidateConfirmPassword(model, out var results);

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(isValid, Is.True);
                Assert.That(results, Is.Empty);
            }
        }
    }
}