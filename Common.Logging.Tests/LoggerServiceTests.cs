using AutoMapper;
using Common.Data.Entities;
using Common.Data.Repository;
using Common.Models;
using Moq;

namespace Common.Logging.Tests
{
    [TestFixture]
    public class LoggerServiceTests
    {
        private Mock<IMapper> _mapperMock = null!;
        private Mock<ILoggerRepository> _repositoryMock = null!;
        private LoggerService _service = null!;

        [SetUp]
        public void SetUp()
        {
            _mapperMock = new Mock<IMapper>(MockBehavior.Strict);
            _repositoryMock = new Mock<ILoggerRepository>(MockBehavior.Strict);
            _service = new LoggerService(_mapperMock.Object, _repositoryMock.Object);
        }

        [Test]
        public void Ctor_Throws_When_Mapper_Is_Null()
        {
            Assert.That(
                () => new LoggerService(null!, _repositoryMock.Object),
                Throws.TypeOf<ArgumentNullException>()
                      .With.Property(nameof(ArgumentNullException.ParamName)).EqualTo("mapper"));
        }

        [Test]
        public void Ctor_Throws_When_Repository_Is_Null()
        {
            Assert.That(
                () => new LoggerService(_mapperMock.Object, null!),
                Throws.TypeOf<ArgumentNullException>()
                      .With.Property(nameof(ArgumentNullException.ParamName)).EqualTo("repository"));
        }

        [Test]
        public async Task GetLogsAsync_Returns_Empty_When_Repository_Returns_Null()
        {
            _repositoryMock
                .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))!
                .ReturnsAsync((IReadOnlyList<Log>?)null);

            var result = await _service.GetLogsAsync();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.Empty);
                _mapperMock.Verify(m => m.Map<LogModel>(It.IsAny<object>()), Times.Never);
            }
        }

        [Test]
        public async Task GetLogsAsync_Maps_And_Orders_Descending_By_Timestamp()
        {
            var entity1 = new Log();
            var entity2 = new Log();
            var logs = new[] { entity1, entity2 };

            _repositoryMock
                .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(logs);

            var model1 = new LogModel { Timestamp = new DateTime(2025, 1, 1) };
            var model2 = new LogModel { Timestamp = new DateTime(2025, 1, 2) };

            _mapperMock
                .Setup(m => m.Map<LogModel>(entity1))
                .Returns(model1);
            _mapperMock
                .Setup(m => m.Map<LogModel>(entity2))
                .Returns(model2);

            var result = (await _service.GetLogsAsync()).ToArray();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Has.Length.EqualTo(2));
                Assert.Multiple(() =>
                {
                    Assert.That(result[0], Is.EqualTo(model2)); 
                    Assert.That(result[1], Is.EqualTo(model1));
                });
            }

            _repositoryMock.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
            _mapperMock.Verify(m => m.Map<LogModel>(entity1), Times.Once);
            _mapperMock.Verify(m => m.Map<LogModel>(entity2), Times.Once);
        }

        [Test]
        public async Task GetLogAsync_Returns_Mapped_Model_When_Found()
        {
            const int id = 5;
            var entity = new Log();
            var mapped = new LogModel { Id = 5 };

            _repositoryMock
                .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(entity);

            _mapperMock
                .Setup(m => m.Map<LogModel>(entity))
                .Returns(mapped);

            var result = await _service.GetLogAsync(id);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result, Is.EqualTo(mapped));
            }

            _repositoryMock.Verify(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()), Times.Once);
            _mapperMock.Verify(m => m.Map<LogModel>(entity), Times.Once);
        }

        [Test]
        public async Task GetLogAsync_Returns_Null_When_Not_Found()
        {
            const int id = 5;

            _repositoryMock
                .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Log?)null);

            var result = await _service.GetLogAsync(id);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.Null);
            }

            _repositoryMock.Verify(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()), Times.Once);
            _mapperMock.Verify(m => m.Map<LogModel>(It.IsAny<object>()), Times.Never);
        }

        [Test]
        public async Task DeleteLogsAsync_Does_Nothing_When_Repository_Returns_Null()
        {
            _repositoryMock
                .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))!
                .ReturnsAsync((IReadOnlyList<Log>?)null);
            await _service.DeleteLogsAsync();

            _repositoryMock.Verify(r => r.DeleteAsync(It.IsAny<Log>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task DeleteLogsAsync_Deletes_All_Logs()
        {
            var entity1 = new Log();
            var entity2 = new Log();
            var logs = new[] { entity1, entity2 };

            _repositoryMock
                .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(logs);

            _repositoryMock
                .Setup(r => r.DeleteAsync(entity1, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            _repositoryMock
                .Setup(r => r.DeleteAsync(entity2, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await _service.DeleteLogsAsync();

            _repositoryMock.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
            _repositoryMock.Verify(r => r.DeleteAsync(entity1, It.IsAny<CancellationToken>()), Times.Once);
            _repositoryMock.Verify(r => r.DeleteAsync(entity2, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}