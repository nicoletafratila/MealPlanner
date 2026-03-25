using Blazored.SessionStorage;
using Moq;

namespace Common.Api.Tests
{
    public static class MoqExtensions
    {
        public static void VerifyNoMocks(this Mock<ISessionStorageService> mock)
        {
            mock.VerifyNoOtherCalls();
        }
    }
}
