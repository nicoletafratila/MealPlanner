using AutoMapper;
using Common.Data.Entities;
using Common.Models;
using Microsoft.Extensions.Logging.Abstractions;

namespace Common.Data.Profiles.Tests
{
    [TestFixture]
    public class LogProfileTests
    {
        private IMapper _mapper = null!;

        [SetUp]
        public void Setup()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<LogProfile>();
            }, NullLoggerFactory.Instance);

            config.AssertConfigurationIsValid();
            _mapper = config.CreateMapper();
        }

        [Test]
        public void Log_To_LogModel_Maps_Properties()
        {
            var id = Guid.NewGuid();
            var entity = new Log
            {
                Id = id,
                Message = "Error occurred",
                Level = "Warning"
            };

            var result = _mapper.Map<LogModel>(entity);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Id, Is.EqualTo(id));
                Assert.That(result.Message, Is.EqualTo("Error occurred"));
                Assert.That(result.Level, Is.EqualTo("Warning"));

                Assert.That(result.Index, Is.Zero);
                Assert.That(result.IsSelected, Is.False);
            }
        }

        [Test]
        public void LogModel_To_Log_Maps_Properties()
        {
            var id = Guid.NewGuid();
            var model = new LogModel
            {
                Id = id,
                Message = "Something happened",
                Level = "Info"
            };

            var result = _mapper.Map<Log>(model);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Id, Is.EqualTo(id));
                Assert.That(result.Message, Is.EqualTo("Something happened"));
                Assert.That(result.Level, Is.EqualTo("Info"));
            }
        }
    }
}