using System.Reflection;
using Identity.Shared.Models;
using MealPlanner.UI.Mobile.Pages.MealPlans;

namespace MealPlanner.UI.Mobile.Tests.Pages.MealPlans
{
    [TestFixture]
    public class ShopsOverviewPageTests
    {
        private static string DisplayName(ApplicationUserListModel user)
        {
            var method = typeof(ShopsOverviewPage).GetMethod("DisplayName", BindingFlags.NonPublic | BindingFlags.Static)!;
            return (string)method.Invoke(null, [user])!;
        }

        [Test]
        public void DisplayName_FirstAndLastNamePresent_ReturnsFullNameWithUsername()
        {
            var user = new ApplicationUserListModel { UserId = "1", Username = "bob", FirstName = "Bob", LastName = "Smith" };

            Assert.That(DisplayName(user), Is.EqualTo("Bob Smith (bob)"));
        }

        [Test]
        public void DisplayName_OnlyFirstName_ReturnsFirstNameWithUsername()
        {
            var user = new ApplicationUserListModel { UserId = "1", Username = "bob", FirstName = "Bob", LastName = null };

            Assert.That(DisplayName(user), Is.EqualTo("Bob (bob)"));
        }

        [Test]
        public void DisplayName_OnlyLastName_ReturnsLastNameWithUsername()
        {
            var user = new ApplicationUserListModel { UserId = "1", Username = "bob", FirstName = null, LastName = "Smith" };

            Assert.That(DisplayName(user), Is.EqualTo("Smith (bob)"));
        }

        [Test]
        public void DisplayName_NoFirstOrLastName_ReturnsUsernameOnly()
        {
            var user = new ApplicationUserListModel { UserId = "1", Username = "bob", FirstName = null, LastName = null };

            Assert.That(DisplayName(user), Is.EqualTo("bob"));
        }

        [Test]
        public void DisplayName_WhitespaceOnlyFirstAndLastName_ReturnsUsernameOnly()
        {
            var user = new ApplicationUserListModel { UserId = "1", Username = "bob", FirstName = "   ", LastName = "  " };

            Assert.That(DisplayName(user), Is.EqualTo("bob"));
        }
    }
}
