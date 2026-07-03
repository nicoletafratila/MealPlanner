using System.ComponentModel.DataAnnotations;using Identity.Shared.Models;

namespace Identity.Shared.Tests.Models
{
    [TestFixture]
    public class ResetPasswordModelTests
    {
        private static bool TryValidate(object model, out IList<ValidationResult> results)
        {
            var context = new ValidationContext(model);
            results = [];
            return Validator.TryValidateObject(model, context, results, validateAllProperties: true);
        }

        private static bool TryValidateConfirmPassword(
            ResetPasswordModel model,
            out IList<ValidationResult> results)
        {
            var context = new ValidationContext(model)
            {
                MemberName = nameof(ResetPasswordModel.ConfirmPassword)
            };
            results = [];
            return Validator.TryValidateProperty(model.ConfirmPassword, context, results);
        }

        [Test]
        public void DefaultCtor_InitializesDefaults_AndIsInvalid()
        {
            var model = new ResetPasswordModel();
            var isValid = TryValidate(model, out var results);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(model.UserId, Is.EqualTo(string.Empty));
                Assert.That(model.Token, Is.EqualTo(string.Empty));
                Assert.That(model.NewPassword, Is.EqualTo(string.Empty));
                Assert.That(model.ConfirmPassword, Is.EqualTo(string.Empty));

                Assert.That(isValid, Is.False);
                Assert.That(results.Any(r => r.MemberNames.Contains(nameof(ResetPasswordModel.NewPassword))), Is.True);
            }
        }

        [Test]
        public void ValidModel_PassesValidation()
        {
            var model = new ResetPasswordModel
            {
                UserId = "user-id",
                Token = "reset-token",
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
        public void NewPassword_Required_ErrorWhenEmpty()
        {
            var model = new ResetPasswordModel
            {
                UserId = "user-id",
                Token = "token",
                NewPassword = "",
                ConfirmPassword = "NewPass123!"
            };

            var isValid = TryValidate(model, out var results);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(isValid, Is.False);
                Assert.That(results.Any(r => r.MemberNames.Contains(nameof(ResetPasswordModel.NewPassword))), Is.True);
            }
        }

        [Test]
        public void ConfirmPassword_Required_ErrorWhenNull()
        {
            var model = new ResetPasswordModel
            {
                NewPassword = "NewPass123!",
                ConfirmPassword = null!
            };

            var isValid = TryValidateConfirmPassword(model, out var results);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(isValid, Is.False);
                Assert.That(results.Any(), Is.True);
            }
        }

        [Test]
        public void ConfirmPassword_MustMatchNewPassword()
        {
            var model = new ResetPasswordModel
            {
                NewPassword = "NewPass123!",
                ConfirmPassword = "DifferentPass!"
            };

            var isValid = TryValidateConfirmPassword(model, out var results);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(isValid, Is.False);
                Assert.That(results.Any(), Is.True);
            }
        }

        [Test]
        public void ConfirmPassword_Valid_WhenMatchesNewPassword()
        {
            var model = new ResetPasswordModel
            {
                NewPassword = "NewPass123!",
                ConfirmPassword = "NewPass123!"
            };

            var isValid = TryValidateConfirmPassword(model, out var results);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(isValid, Is.True);
                Assert.That(results, Is.Empty);
            }
        }

        [Test]
        public void UserId_And_Token_HaveNoValidationConstraints()
        {
            var model = new ResetPasswordModel
            {
                UserId = "",
                Token = "",
                NewPassword = "NewPass123!",
                ConfirmPassword = "NewPass123!"
            };

            var isValid = TryValidate(model, out var results);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(isValid, Is.True);
                Assert.That(results.Any(r => r.MemberNames.Contains(nameof(ResetPasswordModel.UserId))), Is.False);
                Assert.That(results.Any(r => r.MemberNames.Contains(nameof(ResetPasswordModel.Token))), Is.False);
            }
        }
    }
}
