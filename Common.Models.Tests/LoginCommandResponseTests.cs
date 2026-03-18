namespace Common.Models.Tests
{
    [TestFixture]
    public class LoginCommandResponseTests
    {
        [Test]
        public void DefaultCtor_Initializes_Defaults()
        {
            // Act
            var response = new LoginCommandResponse();

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.Multiple(() =>
                {
                    Assert.That(response.Username, Is.Null);
                    Assert.That(response.JwtBearer, Is.Null);
                    Assert.That(response.Claims, Is.Not.Null);
                    Assert.That(response.Claims, Is.Empty);
                });

                Assert.That(response.IsAuthenticated, Is.False);
            }
        }

        [Test]
        public void Ctor_WithoutToken_Sets_Base_Properties_And_Leaves_Token_Null()
        {
            // Act
            var response = new LoginCommandResponse(
                succeeded: true,
                message: "OK",
                errorCode: "0");

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.Multiple(() =>
                {
                    Assert.That(response.Succeeded, Is.True);
                    Assert.That(response.Message, Is.EqualTo("OK"));
                    Assert.That(response.ErrorCode, Is.EqualTo("0"));
                    Assert.That(response.JwtBearer, Is.Null);
                    Assert.That(response.Claims, Is.Empty);
                });

                Assert.That(response.IsAuthenticated, Is.False);
            }
        }

        [Test]
        public void Ctor_WithToken_Sets_Jwt_And_Authentication_Flag()
        {
            // Act
            var response = new LoginCommandResponse(
                succeeded: true,
                message: "Logged in",
                errorCode: "",
                token: "jwt-token-value",
                username: "user1");

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.Multiple(() =>
                {
                    Assert.That(response.Succeeded, Is.True);
                    Assert.That(response.Message, Is.EqualTo("Logged in"));
                    Assert.That(response.ErrorCode, Is.EqualTo(string.Empty));
                    Assert.That(response.JwtBearer, Is.EqualTo("jwt-token-value"));
                    Assert.That(response.Username, Is.EqualTo("user1"));
                });

                Assert.That(response.IsAuthenticated, Is.True);
            }
        }

        [Test]
        public void Ctor_WithClaims_Populates_Claims_Collection()
        {
            // Arrange
            var claims = new[]
            {
                new KeyValuePair<string, string>("role", "admin"),
                new KeyValuePair<string, string>("tenant", "t1")
            };

            // Act
            var response = new LoginCommandResponse(
                succeeded: true,
                message: "Logged in",
                errorCode: "",
                token: "jwt-token-value",
                username: "user1",
                claims: claims);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(response.Claims, Has.Count.EqualTo(2));
                Assert.That(response.Claims, Does.Contain(claims[0]));
                Assert.That(response.Claims, Does.Contain(claims[1]));
            }
        }

        [Test]
        public void IsAuthenticated_False_When_Succeeded_False_Even_With_Token()
        {
            // Act
            var response = new LoginCommandResponse(
                succeeded: false,
                message: "Bad credentials",
                errorCode: "AUTH_FAIL",
                token: "some-token");

            // Assert
            Assert.That(response.IsAuthenticated, Is.False);
        }

        [Test]
        public void IsAuthenticated_False_When_Token_Is_Null_Or_Whitespace()
        {
            var withNullToken = new LoginCommandResponse(true, "ok", "", token: null!);
            var withEmptyToken = new LoginCommandResponse(true, "ok", "", token: "");
            var withWhitespaceToken = new LoginCommandResponse(true, "ok", "", token: "   ");

            using (Assert.EnterMultipleScope())
            {
                Assert.That(withNullToken.IsAuthenticated, Is.False);
                Assert.That(withEmptyToken.IsAuthenticated, Is.False);
                Assert.That(withWhitespaceToken.IsAuthenticated, Is.False);
            }
        }
    }
}