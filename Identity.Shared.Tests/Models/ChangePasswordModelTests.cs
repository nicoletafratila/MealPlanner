using System.ComponentModel.DataAnnotations;using Identity.Shared.Models;

namespace Identity.Shared.Tests.Models
{
    [TestFixture]
    public class ChangePasswordModelTests
    {
        private static bool TryValidate(object model, out IList<ValidationResult> results)
        {
            var context = new ValidationContext(model);
            results = [];
            return Validator.TryValidateObject(model, context, results, validateAllProperties: true);
        }

        private static bool TryValidateProperty(
            ChangePasswordModel model,
            string propertyName,
            object? value,
            out IList<ValidationResult> results)
        {
            var context = new ValidationContext(model) { MemberName = propertyName };
            results = [];
            return Validator.TryValidateProperty(value, context, results);
        }

        [Test]
        public void DefaultCtor_InitializesDefaults_AndIsInvalid()
        {
            var model = new ChangePasswordModel();
            var isValid = TryValidate(model, out var results);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(model.UserId, Is.EqualTo(string.Empty));
                Assert.That(model.CurrentPassword, Is.EqualTo(string.Empty));
                Assert.That(model.NewPassword, Is.EqualTo(string.Empty));
                Assert.That(model.ConfirmPassword, Is.EqualTo(string.Empty));

                Assert.That(isValid, Is.False);
                Assert.That(results.Any(r => r.MemberNames.Contains(nameof(ChangePasswordModel.CurrentPassword))), Is.True);
            }
        }

        [Test]
        public void ValidModel_PassesValidation()
        {
            var model = new ChangePasswordModel
            {
                UserId = "user-id",
                CurrentPassword = "OldPass123!",
                NewPassword = "NewPass123!",
                ConfirmPassword = "NewPass123!"
            };

            var isValid = TryValidate(model, out var results);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(isValid, Is.True);
                Assert.That(results, Is.Empty);
            }
        }

        [Test]
        public void CurrentPassword_Required_ErrorWhenEmpty()
        {
            var model = new ChangePasswordModel
            {
                UserId = "user-id",
                CurrentPassword = "",
                NewPassword = "NewPass123!",
                ConfirmPassword = "NewPass123!"
            };

            var isValid = TryValidate(model, out var results);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(isValid, Is.False);
                Assert.That(results.Any(r => r.MemberNames.Contains(nameof(ChangePasswordModel.CurrentPassword))), Is.True);
            }
        }

        [Test]
        public void NewPassword_Required_ErrorWhenEmpty()
        {
            var model = new ChangePasswordModel
            {
                UserId = "user-id",
                CurrentPassword = "OldPass123!",
                NewPassword = "",
                ConfirmPassword = "NewPass123!"
            };

            var isValid = TryValidate(model, out var results);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(isValid, Is.False);
                Assert.That(results.Any(r => r.MemberNames.Contains(nameof(ChangePasswordModel.NewPassword))), Is.True);
            }
        }

        [Test]
        public void ConfirmPassword_Required_ErrorWhenNull()
        {
            var model = new ChangePasswordModel
            {
                UserId = "user-id",
                CurrentPassword = "OldPass123!",
                NewPassword = "NewPass123!",
                ConfirmPassword = null!
            };

            var isValid = TryValidateProperty(model, nameof(ChangePasswordModel.ConfirmPassword), null, out var results);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(isValid, Is.False);
                Assert.That(results.Any(), Is.True);
            }
        }

        [Test]
        public void ConfirmPassword_MustMatchNewPassword()
        {
            var model = new ChangePasswordModel
            {
                NewPassword = "NewPass123!",
                ConfirmPassword = "DifferentPass!"
            };

            var isValid = TryValidateProperty(model, nameof(ChangePasswordModel.ConfirmPassword), model.ConfirmPassword, out var results);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(isValid, Is.False);
                Assert.That(results.Any(), Is.True);
            }
        }

        [Test]
        public void ConfirmPassword_Valid_WhenMatchesNewPassword()
        {
            var model = new ChangePasswordModel
            {
                NewPassword = "NewPass123!",
                ConfirmPassword = "NewPass123!"
            };

            var isValid = TryValidateProperty(model, nameof(ChangePasswordModel.ConfirmPassword), model.ConfirmPassword, out var results);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(isValid, Is.True);
                Assert.That(results, Is.Empty);
            }
        }

        [Test]
        public void UserId_HasNoValidationConstraints()
        {
            var model = new ChangePasswordModel
            {
                UserId = "",
                CurrentPassword = "OldPass123!",
                NewPassword = "NewPass123!",
                ConfirmPassword = "NewPass123!"
            };

            var isValid = TryValidate(model, out var results);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(isValid, Is.True);
                Assert.That(results.Any(r => r.MemberNames.Contains(nameof(ChangePasswordModel.UserId))), Is.False);
            }
        }
    }
}
