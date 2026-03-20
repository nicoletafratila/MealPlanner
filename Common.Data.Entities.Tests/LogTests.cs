namespace Common.Data.Entities.Tests
{
    [TestFixture]
    public class LogTests
    {
        [Test]
        public void DefaultCtor_Sets_Defaults()
        {
            // Act
            var log = new Log();

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(log.Id, Is.Zero);
                Assert.That(log.Message, Is.EqualTo(string.Empty));
                Assert.That(log.MessageTemplate, Is.EqualTo(string.Empty));
                Assert.That(log.Level, Is.EqualTo(string.Empty));
                Assert.That(log.Exception, Is.EqualTo(string.Empty));
                Assert.That(log.Properties, Is.EqualTo(string.Empty));
                Assert.That(log.HasException, Is.False);
            }
        }

        [Test]
        public void Properties_Can_Be_Set_And_Read()
        {
            // Arrange
            var ts = new DateTime(2025, 1, 2, 3, 4, 5, DateTimeKind.Utc);

            // Act
            var log = new Log
            {
                Id = 1,
                Message = "Test message",
                MessageTemplate = "Template {Value}",
                Level = "Information",
                TimeStamp = ts,
                Exception = "System.Exception: boom",
                Properties = "{\"Value\":42}"
            };

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(log.Id, Is.EqualTo(1));
                Assert.That(log.Message, Is.EqualTo("Test message"));
                Assert.That(log.MessageTemplate, Is.EqualTo("Template {Value}"));
                Assert.That(log.Level, Is.EqualTo("Information"));
                Assert.That(log.TimeStamp, Is.EqualTo(ts));
                Assert.That(log.Exception, Is.EqualTo("System.Exception: boom"));
                Assert.That(log.Properties, Is.EqualTo("{\"Value\":42}"));
                Assert.That(log.HasException, Is.True);
            }
        }

        [Test]
        public void HasException_False_When_Exception_Null_Or_Whitespace()
        {
            var l1 = new Log { Exception = null! };
            var l2 = new Log { Exception = "" };
            var l3 = new Log { Exception = "   " };

            using (Assert.EnterMultipleScope())
            {
                Assert.That(l1.HasException, Is.False);
                Assert.That(l2.HasException, Is.False);
                Assert.That(l3.HasException, Is.False);
            }
        }

        [Test]
        public void ToString_Contains_Timestamp_Level_And_Message()
        {
            // Arrange
            var ts = new DateTime(2025, 1, 2, 3, 4, 5, DateTimeKind.Utc);
            var log = new Log
            {
                Level = "Warning",
                Message = "Something happened",
                TimeStamp = ts
            };

            // Act
            var text = log.ToString();

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(text, Does.Contain("Warning"));
                Assert.That(text, Does.Contain("Something happened"));
                Assert.That(text, Does.Contain(ts.ToString("O")));
            }
        }
    }
}