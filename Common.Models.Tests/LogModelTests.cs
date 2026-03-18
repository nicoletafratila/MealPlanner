namespace Common.Models.Tests
{
    [TestFixture]
    public class LogModelTests
    {
        [Test]
        public void DefaultCtor_Sets_Empty_Strings_And_HasException_False()
        {
            // Act
            var log = new LogModel();

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(log.Id, Is.EqualTo(0));
                Assert.That(log.Message, Is.EqualTo(string.Empty));
                Assert.That(log.MessageTemplate, Is.EqualTo(string.Empty));
                Assert.That(log.Level, Is.EqualTo(string.Empty));
                Assert.That(log.Exception, Is.EqualTo(string.Empty));
                Assert.That(log.Properties, Is.EqualTo(string.Empty));
                Assert.That(log.HasException, Is.False);
            }
        }

        [Test]
        public void Property_Setters_Work_As_Expected()
        {
            // Arrange
            var timestamp = new DateTime(2025, 1, 2, 3, 4, 5, DateTimeKind.Utc);

            // Act
            var log = new LogModel
            {
                Id = 42,
                Message = "Message",
                MessageTemplate = "Template {Value}",
                Level = "Information",
                Timestamp = timestamp,
                Exception = "System.Exception: boom",
                Properties = "{\"Value\": 1}"
            };

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(log.Id, Is.EqualTo(42));
                Assert.That(log.Message, Is.EqualTo("Message"));
                Assert.That(log.MessageTemplate, Is.EqualTo("Template {Value}"));
                Assert.That(log.Level, Is.EqualTo("Information"));
                Assert.That(log.Timestamp, Is.EqualTo(timestamp));
                Assert.That(log.Exception, Is.EqualTo("System.Exception: boom"));
                Assert.That(log.Properties, Is.EqualTo("{\"Value\": 1}"));
                Assert.That(log.HasException, Is.True);
            }
        }

        [Test]
        public void HasException_Is_False_When_Exception_Is_Null_Or_Whitespace()
        {
            var withNull = new LogModel { Exception = null! };
            var withEmpty = new LogModel { Exception = "" };
            var withWhitespace = new LogModel { Exception = "   " };

            using (Assert.EnterMultipleScope())
            {
                Assert.That(withNull.HasException, Is.False);
                Assert.That(withEmpty.HasException, Is.False);
                Assert.That(withWhitespace.HasException, Is.False);
            }
        }

        [Test]
        public void ToString_Contains_Timestamp_Level_And_Message()
        {
            // Arrange
            var timestamp = new DateTime(2025, 1, 2, 3, 4, 5, DateTimeKind.Utc);
            var log = new LogModel
            {
                Level = "Warning",
                Message = "Something happened",
                Timestamp = timestamp
            };

            // Act
            var text = log.ToString();

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(text, Does.Contain("Warning"));
                Assert.That(text, Does.Contain("Something happened"));
                Assert.That(text, Does.Contain(timestamp.ToString("O")));
            }
        }
    }
}