using System.Text.Json;
using Common.Models;
using NUnit.Framework;

namespace Common.Models.Tests
{
    [TestFixture]
    public class CommandResponseTests
    {
        [Test]
        public void DefaultCtor_Sets_Default_Values()
        {
            // Act
            var response = new CommandResponse();

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.Multiple(() =>
                {
                    Assert.That(response.Succeeded, Is.False);
                    Assert.That(response.IsSuccess, Is.False);
                    Assert.That(response.Message, Is.Null);
                    Assert.That(response.ErrorCode, Is.Null);
                });
            }
        }

        [Test]
        public void Ctor_Sets_Properties()
        {
            // Act
            var response = new CommandResponse(
                succeeded: true,
                message: "OK",
                errorCode: "0");

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.Multiple(() =>
                {
                    Assert.That(response.Succeeded, Is.True);
                    Assert.That(response.IsSuccess, Is.True);
                    Assert.That(response.Message, Is.EqualTo("OK"));
                    Assert.That(response.ErrorCode, Is.EqualTo("0"));
                });
            }
        }

        [Test]
        public void Success_Factory_Sets_Succeeded_True_And_Message()
        {
            // Act
            var response = CommandResponse.Success("All good");

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.Multiple(() =>
                {
                    Assert.That(response.Succeeded, Is.True);
                    Assert.That(response.IsSuccess, Is.True);
                    Assert.That(response.Message, Is.EqualTo("All good"));
                    Assert.That(response.ErrorCode, Is.Null);
                });
            }
        }

        [Test]
        public void Failed_Factory_Sets_Succeeded_False_Message_And_ErrorCode()
        {
            // Act
            var response = CommandResponse.Failed("Something went wrong", "ERR001");

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.Multiple(() =>
                {
                    Assert.That(response.Succeeded, Is.False);
                    Assert.That(response.IsSuccess, Is.False);
                    Assert.That(response.Message, Is.EqualTo("Something went wrong"));
                    Assert.That(response.ErrorCode, Is.EqualTo("ERR001"));
                });
            }
        }

        [Test]
        public void IsSuccess_Is_Not_Serialized()
        {
            // Arrange
            var response = CommandResponse.Success("OK");

            // Act
            var json = JsonSerializer.Serialize(response);

            // Assert
            Assert.That(json, Does.Contain("\"Succeeded\""));
            Assert.That(json, Does.Not.Contain("IsSuccess"));
        }
    }
}