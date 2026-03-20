using AutoMapper;
using Common.Data.Entities;
using Identity.Shared.Models;

namespace Common.Data.Profiles.Tests
{
    [TestFixture]
    public class ApplicationUserProfileTests
    {
        private IMapper _mapper = null!;

        [SetUp]
        public void Setup()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<ApplicationUserProfile>();
            });

            config.AssertConfigurationIsValid();
            _mapper = config.CreateMapper();
        }

        [Test]
        public void ApplicationUser_To_ApplicationUserEditModel_Maps_Properties()
        {
            var entity = new ApplicationUser
            {
                Id = "abc123",
                UserName = "testuser",
                FirstName = "John",
                LastName = "Doe",
                ProfilePicture = [1, 2, 3],
                PhoneNumber = "123456",
                Email = "test@example.com",
                IsActive = true
            };

            var result = _mapper.Map<ApplicationUserEditModel>(entity);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.UserId, Is.EqualTo("abc123"));
                Assert.That(result.Username, Is.EqualTo("testuser"));
                Assert.That(result.FirstName, Is.EqualTo("John"));
                Assert.That(result.LastName, Is.EqualTo("Doe"));
                Assert.That(result.PhoneNumber, Is.EqualTo("123456"));
                Assert.That(result.EmailAddress, Is.EqualTo("test@example.com"));
                Assert.That(result.IsActive, Is.True);

                Assert.That(result.ProfilePictureUrl, Does.StartWith("data:image/jpg;base64,"));

                Assert.That(result.Index, Is.Zero);
                Assert.That(result.IsSelected, Is.False);
            }
        }

        [Test]
        public void ApplicationUser_To_ApplicationUserEditModel_Null_Picture_Generates_Empty_Base64()
        {
            var entity = new ApplicationUser
            {
                Id = "u1",
                UserName = "noimage"
            };

            var result = _mapper.Map<ApplicationUserEditModel>(entity);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.ProfilePictureUrl, Is.EqualTo("data:image/jpg;base64,"));
            }
        }

        [Test]
        public void ApplicationUserEditModel_To_ApplicationUser_Maps_Properties()
        {
            var model = new ApplicationUserEditModel
            {
                UserId = "xyz",
                Username = "alice",
                FirstName = "Alice",
                LastName = "Smith",
                PhoneNumber = "555",
                EmailAddress = "alice@mail.com",
                IsActive = false
            };

            var result = _mapper.Map<ApplicationUser>(model);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Id, Is.EqualTo("xyz"));
                Assert.That(result.UserName, Is.EqualTo("alice"));
                Assert.That(result.FirstName, Is.EqualTo("Alice"));
                Assert.That(result.LastName, Is.EqualTo("Smith"));
                Assert.That(result.PhoneNumber, Is.EqualTo("555"));
                Assert.That(result.Email, Is.EqualTo("alice@mail.com"));
                Assert.That(result.IsActive, Is.False);
            }
        }
    }
}