using AutoMapper;
using Common.Data.Entities;
using Common.Models;

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
            });

            config.AssertConfigurationIsValid();
            _mapper = config.CreateMapper();
        }

        [Test]
        public void Log_To_LogModel_Maps_Properties()
        {
            var entity = new Log
            {
                Id = 1,
                Message = "Error occurred",
                Level = "Warning"
            };

            var result = _mapper.Map<LogModel>(entity);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Id, Is.EqualTo(1));
                Assert.That(result.Message, Is.EqualTo("Error occurred"));
                Assert.That(result.Level, Is.EqualTo("Warning"));

                Assert.That(result.Index, Is.Zero);
                Assert.That(result.IsSelected, Is.False);
            }
        }

        [Test]
        public void LogModel_To_Log_Maps_Properties()
        {
            var model = new LogModel
            {
                Id = 11,
                Message = "Something happened",
                Level = "Info"
            };

            var result = _mapper.Map<Log>(model);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Id, Is.EqualTo(11));
                Assert.That(result.Message, Is.EqualTo("Something happened"));
                Assert.That(result.Level, Is.EqualTo("Info"));
            }
        }
    }
}