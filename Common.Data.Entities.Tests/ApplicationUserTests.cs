using System.ComponentModel.DataAnnotations;

namespace Common.Data.Entities.Tests
{
    [TestFixture]
    public class ApplicationUserTests
    {
        [Test]
        public void DefaultCtor_Sets_Defaults()
        {
            // Act
            var user = new ApplicationUser();

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(user.FirstName, Is.Null);
                Assert.That(user.LastName, Is.Null);
                Assert.That(user.ProfilePicture, Is.Null);
                Assert.That(user.IsActive, Is.False);
                Assert.That(user.FullName, Is.EqualTo(string.Empty));
            }
        }

        [Test]
        public void FullName_Composes_First_And_Last_Name()
        {
            var user = new ApplicationUser
            {
                FirstName = "John",
                LastName = "Doe"
            };

            Assert.That(user.FullName, Is.EqualTo("John Doe"));
        }

        [Test]
        public void FullName_Handles_Missing_First_Or_Last_Name()
        {
            var onlyFirst = new ApplicationUser { FirstName = "John", LastName = null };
            var onlyLast = new ApplicationUser { FirstName = null, LastName = "Doe" };
            var neither = new ApplicationUser { FirstName = "  ", LastName = "  " };

            using (Assert.EnterMultipleScope())
            {
                Assert.That(onlyFirst.FullName, Is.EqualTo("John"));
                Assert.That(onlyLast.FullName, Is.EqualTo("Doe"));
                Assert.That(neither.FullName, Is.EqualTo(string.Empty));
            }
        }

        [Test]
        public void DataAnnotations_Are_Configured_Correctly_For_FirstName()
        {
            var prop = typeof(ApplicationUser).GetProperty(nameof(ApplicationUser.FirstName))!;
            var display = prop.GetCustomAttributes(typeof(DisplayAttribute), false)
                              .OfType<DisplayAttribute>()
                              .SingleOrDefault();
            var regex = prop.GetCustomAttributes(typeof(RegularExpressionAttribute), false)
                            .OfType<RegularExpressionAttribute>()
                            .SingleOrDefault();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(display, Is.Not.Null);
                Assert.That(display!.Name, Is.EqualTo("First Name"));

                Assert.That(regex, Is.Not.Null);
                Assert.That(regex!.Pattern, Is.EqualTo(@"^[a-zA-Z\s]*$"));
                Assert.That(regex.ErrorMessage, Is.EqualTo("First Name must be alpha characters only."));
            }
        }

        [Test]
        public void DataAnnotations_Are_Configured_Correctly_For_LastName()
        {
            var prop = typeof(ApplicationUser).GetProperty(nameof(ApplicationUser.LastName))!;
            var display = prop.GetCustomAttributes(typeof(DisplayAttribute), false)
                              .OfType<DisplayAttribute>()
                              .SingleOrDefault();
            var regex = prop.GetCustomAttributes(typeof(RegularExpressionAttribute), false)
                            .OfType<RegularExpressionAttribute>()
                            .SingleOrDefault();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(display, Is.Not.Null);
                Assert.That(display!.Name, Is.EqualTo("Last Name"));

                Assert.That(regex, Is.Not.Null);
                Assert.That(regex!.Pattern, Is.EqualTo(@"^[a-zA-Z\s]*$"));
                Assert.That(regex.ErrorMessage, Is.EqualTo("Last Name must be alpha characters only."));
            }
        }

        [Test]
        public void Validation_Passes_For_Valid_First_And_Last_Name()
        {
            var user = new ApplicationUser
            {
                FirstName = "John",
                LastName = "Doe"
            };

            var context = new ValidationContext(user);
            var results = new List<ValidationResult>();

            var isValid = Validator.TryValidateObject(user, context, results, validateAllProperties: true);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(isValid, Is.True);
                Assert.That(results, Is.Empty);
            }
        }

        [Test]
        public void Validation_Fails_For_Invalid_FirstName()
        {
            var user = new ApplicationUser
            {
                FirstName = "John123", // invalid: digits not allowed
                LastName = "Doe"
            };

            var context = new ValidationContext(user);
            var results = new List<ValidationResult>();

            var isValid = Validator.TryValidateObject(user, context, results, validateAllProperties: true);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(isValid, Is.False);
                Assert.That(results.Any(r => r.MemberNames.Contains(nameof(ApplicationUser.FirstName))), Is.True);
            }
        }

        [Test]
        public void Validation_Fails_For_Invalid_LastName()
        {
            var user = new ApplicationUser
            {
                FirstName = "John",
                LastName = "Doe123" // invalid: digits not allowed
            };

            var context = new ValidationContext(user);
            var results = new List<ValidationResult>();

            var isValid = Validator.TryValidateObject(user, context, results, validateAllProperties: true);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(isValid, Is.False);
                Assert.That(results.Any(r => r.MemberNames.Contains(nameof(ApplicationUser.LastName))), Is.True);
            }
        }
    }
}