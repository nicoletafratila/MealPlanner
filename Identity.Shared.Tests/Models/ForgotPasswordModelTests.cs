using System.ComponentModel.DataAnnotations;using Identity.Shared.Models;

namespace Identity.Shared.Tests.Models
{
    [TestFixture]
    public class ForgotPasswordModelTests
    {
        private static bool TryValidate(object model, out IList<ValidationResult> results)
        {
            var context = new ValidationContext(model);
            results = [];
            return Validator.TryValidateObject(model, context, results, validateAllProperties: true);
        }

        [Test]
        public void DefaultCtor_InitializesEmailToEmpty_AndIsInvalid()
        {
            var model = new ForgotPasswordModel();
            var isValid = TryValidate(model, out var results);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(model.EmailAddress, Is.EqualTo(string.Empty));
                Assert.That(isValid, Is.False);
                Assert.That(results.Any(r => r.MemberNames.Contains(nameof(ForgotPasswordModel.EmailAddress))), Is.True);
            }
        }

        [Test]
        public void ValidModel_PassesValidation()
        {
            var model = new ForgotPasswordModel { EmailAddress = "user@example.com" };
            var isValid = TryValidate(model, out var results);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(isValid, Is.True);
                Assert.That(results, Is.Empty);
            }
        }

        [Test]
        public void EmailAddress_Required_ErrorWhenEmpty()
        {
            var model = new ForgotPasswordModel { EmailAddress = "" };
            var isValid = TryValidate(model, out var results);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(isValid, Is.False);
                Assert.That(results.Any(r => r.MemberNames.Contains(nameof(ForgotPasswordModel.EmailAddress))), Is.True);
            }
        }

        [Test]
        public void EmailAddress_InvalidFormat_FailsValidation()
        {
            var model = new ForgotPasswordModel { EmailAddress = "not-an-email" };
            var isValid = TryValidate(model, out var results);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(isValid, Is.False);
                Assert.That(results.Any(r => r.MemberNames.Contains(nameof(ForgotPasswordModel.EmailAddress))), Is.True);
            }
        }
    }
}
