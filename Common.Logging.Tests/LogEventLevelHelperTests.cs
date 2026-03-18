using Serilog.Events;

namespace Common.Logging.Tests
{
    [TestFixture]
    public class LogEventLevelHelperTests
    {
        [Test]
        public void StringToEnum_Parses_Valid_Values_CaseInsensitive()
        {
            using (Assert.EnterMultipleScope())
            {
                Assert.Multiple(() =>
                {
                    Assert.That(LogEventLevelHelper.StringToEnum("Verbose"), Is.EqualTo(LogEventLevel.Verbose));
                    Assert.That(LogEventLevelHelper.StringToEnum("debug"), Is.EqualTo(LogEventLevel.Debug));
                    Assert.That(LogEventLevelHelper.StringToEnum("INFORMATION"), Is.EqualTo(LogEventLevel.Information));
                    Assert.That(LogEventLevelHelper.StringToEnum("Warning"), Is.EqualTo(LogEventLevel.Warning));
                    Assert.That(LogEventLevelHelper.StringToEnum("Error"), Is.EqualTo(LogEventLevel.Error));
                    Assert.That(LogEventLevelHelper.StringToEnum("Fatal"), Is.EqualTo(LogEventLevel.Fatal));
                });
            }
        }

        [Test]
        public void StringToEnum_Throws_For_Invalid_Value()
        {
            Assert.That(
                () => LogEventLevelHelper.StringToEnum("InvalidLevel"),
                Throws.TypeOf<ArgumentException>()
                      .With.Message.Contains("is not a valid LogEventLevel"));
        }

        [Test]
        public void StringToEnum_Throws_For_Null()
        {
            Assert.That(
                () => LogEventLevelHelper.StringToEnum(null!),
                Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void GetBootstrapUIClass_String_Returns_Expected_Classes()
        {
            using (Assert.EnterMultipleScope())
            {
                Assert.Multiple(() =>
                {
                    Assert.That(LogEventLevelHelper.GetBootstrapUIClass("Verbose"), Is.EqualTo("info"));
                    Assert.That(LogEventLevelHelper.GetBootstrapUIClass("Debug"), Is.EqualTo("info"));
                    Assert.That(LogEventLevelHelper.GetBootstrapUIClass("Information"), Is.EqualTo("info"));
                    Assert.That(LogEventLevelHelper.GetBootstrapUIClass("Warning"), Is.EqualTo("warning"));
                    Assert.That(LogEventLevelHelper.GetBootstrapUIClass("Error"), Is.EqualTo("danger"));
                    Assert.That(LogEventLevelHelper.GetBootstrapUIClass("Fatal"), Is.EqualTo("danger"));
                });
            }
        }

        [Test]
        public void GetBootstrapUIClass_Enum_Returns_Expected_Classes()
        {
            using (Assert.EnterMultipleScope())
            {
                Assert.Multiple(() =>
                {
                    Assert.That(LogEventLevelHelper.GetBootstrapUIClass(LogEventLevel.Verbose), Is.EqualTo("info"));
                    Assert.That(LogEventLevelHelper.GetBootstrapUIClass(LogEventLevel.Debug), Is.EqualTo("info"));
                    Assert.That(LogEventLevelHelper.GetBootstrapUIClass(LogEventLevel.Information), Is.EqualTo("info"));
                    Assert.That(LogEventLevelHelper.GetBootstrapUIClass(LogEventLevel.Warning), Is.EqualTo("warning"));
                    Assert.That(LogEventLevelHelper.GetBootstrapUIClass(LogEventLevel.Error), Is.EqualTo("danger"));
                    Assert.That(LogEventLevelHelper.GetBootstrapUIClass(LogEventLevel.Fatal), Is.EqualTo("danger"));
                });
            }
        }

        [Test]
        public void GetBootstrapUIClass_String_Throws_For_Invalid_Value()
        {
            Assert.That(
                () => LogEventLevelHelper.GetBootstrapUIClass("InvalidLevel"),
                Throws.TypeOf<ArgumentException>()
                      .With.Message.Contains("is not a valid LogEventLevel"));
        }

        [Test]
        public void GetBootstrapUIClass_String_Throws_For_Null()
        {
            Assert.That(
                () => LogEventLevelHelper.GetBootstrapUIClass(null!),
                Throws.TypeOf<ArgumentNullException>());
        }
    }
}