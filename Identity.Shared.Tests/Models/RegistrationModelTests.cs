using System.ComponentModel.DataAnnotations;using Identity.Shared.Models;

namespace Identity.Shared.Tests.Models
{
    [TestFixture]
    public class RegistrationModelTests
    {
        private static bool TryValidate(object model, out IList<ValidationResult> results)
        {
            var context = new ValidationContext(model);
            results = [];
            return Validator.TryValidateObject(model, context, results, validateAllProperties: true);
        }

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
        public void DefaultCtor_InitializesDefaults_ButIsInvalid()
        {
            var model = new RegistrationModel();
            var isValid = TryValidate(model, out var results);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(model.Username, Is.EqualTo(string.Empty));
                Assert.That(model.EmailAddress, Is.EqualTo(string.Empty));
                Assert.That(model.ConfirmPassword, Is.EqualTo(string.Empty));

                Assert.That(isValid, Is.False);
                Assert.That(results.Any(r => r.MemberNames.Contains(nameof(RegistrationModel.Username))), Is.True);
                Assert.That(results.Any(r => r.MemberNames.Contains(nameof(RegistrationModel.EmailAddress))), Is.True);
            }
        }

        [Test]
        public void ValidModel_PassesValidation()
        {
            var model = new RegistrationModel
            {
                Username = "newuser",
                Password = "Test123!",
                ConfirmPassword = "Test123!",
                EmailAddress = "newuser@example.com",
                FirstName = "John",
                LastName = "Doe",
                PhoneNumber = "+123456789"
            };

            var isValid = TryValidate(model, out var results);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(isValid, Is.True);
                Assert.That(results, Is.Empty);
            }
        }

        [Test]
        public void Username_Required_ErrorWhenEmpty()
        {
            var model = new RegistrationModel
            {
                Username = "",
                Password = "Test123!",
                ConfirmPassword = "Test123!",
                EmailAddress = "user@example.com"
            };

            var isValid = TryValidate(model, out var results);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(isValid, Is.False);
                Assert.That(results.Any(r => r.MemberNames.Contains(nameof(RegistrationModel.Username))), Is.True);
            }
        }

        [Test]
        public void EmailAddress_Required_And_MustBeValidFormat()
        {
            // Missing email
            var model = new RegistrationModel
            {
                Username = "user",
                Password = "Test123!",
                ConfirmPassword = "Test123!",
                EmailAddress = ""
            };

            var isValid = TryValidate(model, out var results);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(isValid, Is.False);
                Assert.That(results.Any(r => r.MemberNames.Contains(nameof(RegistrationModel.EmailAddress))), Is.True);
            }

            // Invalid format
            model.EmailAddress = "not-an-email";
            isValid = TryValidate(model, out results);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(isValid, Is.False);
                Assert.That(results.Any(r => r.MemberNames.Contains(nameof(RegistrationModel.EmailAddress))), Is.True);
            }

            // Valid email
            model.EmailAddress = "valid@example.com";
            isValid = TryValidate(model, out results);

            Assert.That(isValid, Is.True);
        }

        [Test]
        public void FirstName_AllowsLettersAndSpacesOnly()
        {
            var model = new RegistrationModel
            {
                Username = "user",
                Password = "Test123!",
                ConfirmPassword = "Test123!",
                EmailAddress = "user@example.com",
                FirstName = "John Paul"
            };

            var isValid = TryValidate(model, out var results);

            Assert.That(isValid, Is.True);

            model.FirstName = "John123";
            isValid = TryValidate(model, out results);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(isValid, Is.False);
                Assert.That(results.Any(r => r.MemberNames.Contains(nameof(RegistrationModel.FirstName))), Is.True);
            }
        }

        [Test]
        public void LastName_AllowsLettersAndSpacesOnly()
        {
            var model = new RegistrationModel
            {
                Username = "user",
                Password = "Test123!",
                ConfirmPassword = "Test123!",
                EmailAddress = "user@example.com",
                LastName = "Smith Jr"
            };

            var isValid = TryValidate(model, out var results);

            Assert.That(isValid, Is.True);

            model.LastName = "Smith123";
            isValid = TryValidate(model, out results);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(isValid, Is.False);
                Assert.That(results.Any(r => r.MemberNames.Contains(nameof(RegistrationModel.LastName))), Is.True);
            }
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