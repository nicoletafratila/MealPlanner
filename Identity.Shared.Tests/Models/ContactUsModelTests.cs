using System.ComponentModel.DataAnnotations;using Identity.Shared.Models;

namespace Identity.Shared.Tests.Models
{
    [TestFixture]
    public class ContactUsModelTests
    {
        private static bool TryValidate(object model, out IList<ValidationResult> results)
        {
            var context = new ValidationContext(model);
            results = [];
            return Validator.TryValidateObject(model, context, results, validateAllProperties: true);
        }

        [Test]
        public void DefaultCtor_InitializesAllStringsToEmpty_AndIsInvalid()
        {
            var model = new ContactUsModel();
            var isValid = TryValidate(model, out var results);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(model.Name, Is.EqualTo(string.Empty));
                Assert.That(model.EmailAddress, Is.EqualTo(string.Empty));
                Assert.That(model.Subject, Is.EqualTo(string.Empty));
                Assert.That(model.Message, Is.EqualTo(string.Empty));
                Assert.That(isValid, Is.False);
                Assert.That(results, Is.Not.Empty);
            }
        }

        [Test]
        public void ValidModel_PassesValidation()
        {
            var model = new ContactUsModel
            {
                Name = "Jane Doe",
                EmailAddress = "jane@example.com",
                Subject = "Hello",
                Message = "This is a test message."
            };

            var isValid = TryValidate(model, out var results);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(isValid, Is.True);
                Assert.That(results, Is.Empty);
            }
        }

        [Test]
        public void Name_Required_ErrorWhenEmpty()
        {
            var model = new ContactUsModel { Name = "", EmailAddress = "a@b.com", Subject = "s", Message = "m" };
            var isValid = TryValidate(model, out var results);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(isValid, Is.False);
                Assert.That(results.Any(r => r.MemberNames.Contains(nameof(ContactUsModel.Name))), Is.True);
            }
        }

        [Test]
        public void Name_ExceedsMaxLength_FailsValidation()
        {
            var model = new ContactUsModel { Name = new string('x', 101), EmailAddress = "a@b.com", Subject = "s", Message = "m" };
            var isValid = TryValidate(model, out var results);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(isValid, Is.False);
                Assert.That(results.Any(r => r.MemberNames.Contains(nameof(ContactUsModel.Name))), Is.True);
            }
        }

        [Test]
        public void EmailAddress_Required_ErrorWhenEmpty()
        {
            var model = new ContactUsModel { Name = "Jane", EmailAddress = "", Subject = "s", Message = "m" };
            var isValid = TryValidate(model, out var results);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(isValid, Is.False);
                Assert.That(results.Any(r => r.MemberNames.Contains(nameof(ContactUsModel.EmailAddress))), Is.True);
            }
        }

        [Test]
        public void EmailAddress_InvalidFormat_FailsValidation()
        {
            var model = new ContactUsModel { Name = "Jane", EmailAddress = "not-an-email", Subject = "s", Message = "m" };
            var isValid = TryValidate(model, out var results);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(isValid, Is.False);
                Assert.That(results.Any(r => r.MemberNames.Contains(nameof(ContactUsModel.EmailAddress))), Is.True);
            }
        }

        [Test]
        public void Subject_Required_ErrorWhenEmpty()
        {
            var model = new ContactUsModel { Name = "Jane", EmailAddress = "a@b.com", Subject = "", Message = "m" };
            var isValid = TryValidate(model, out var results);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(isValid, Is.False);
                Assert.That(results.Any(r => r.MemberNames.Contains(nameof(ContactUsModel.Subject))), Is.True);
            }
        }

        [Test]
        public void Message_Required_ErrorWhenEmpty()
        {
            var model = new ContactUsModel { Name = "Jane", EmailAddress = "a@b.com", Subject = "s", Message = "" };
            var isValid = TryValidate(model, out var results);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(isValid, Is.False);
                Assert.That(results.Any(r => r.MemberNames.Contains(nameof(ContactUsModel.Message))), Is.True);
            }
        }

        [Test]
        public void Message_ExceedsMaxLength_FailsValidation()
        {
            var model = new ContactUsModel { Name = "Jane", EmailAddress = "a@b.com", Subject = "s", Message = new string('x', 2001) };
            var isValid = TryValidate(model, out var results);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(isValid, Is.False);
                Assert.That(results.Any(r => r.MemberNames.Contains(nameof(ContactUsModel.Message))), Is.True);
            }
        }
    }
}
