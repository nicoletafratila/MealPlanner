using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Data.DataContext.Tests
{
    public class MigrationBuilderExtensionsTests
    {
        [TearDown]
        public void TearDown()
        {
            ServiceLocator.Reset();
        }

        private static void SetupServiceLocatorWithDb(bool canConnect)
        {
            var baseContext = new DbContext(new DbContextOptionsBuilder().Options);
            var fakeDatabase = new FakeDatabaseFacade(baseContext, canConnect);
            var options = new DbContextOptionsBuilder<TestDbContext>().Options;
            var dbContext = new TestDbContext(fakeDatabase, options);

            var provider = new ServiceCollection()
                .AddSingleton(dbContext)
                .BuildServiceProvider();

            ServiceLocator.SetLocatorProvider(provider);
        }

        [Test]
        public void TableExists_WhenMigrationBuilderNull_Throws()
        {
            SetupServiceLocatorWithDb(canConnect: false);

            Assert.That(
                () => MigrationBuilderExtensions.TableExists<TestDbContext>(null!, "Table"),
                Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void TableExists_WhenTableNameInvalid_Throws()
        {
            SetupServiceLocatorWithDb(canConnect: false);
            var mb = new MigrationBuilder(activeProvider: "SqlServer");

            Assert.That(
                () => mb.TableExists<TestDbContext>(""),
                Throws.TypeOf<ArgumentException>()
                      .With.Property(nameof(ArgumentException.ParamName)).EqualTo("tableName"));
        }

        [Test]
        public void TableExists_WhenCannotConnect_ReturnsFalse()
        {
            SetupServiceLocatorWithDb(canConnect: false);
            var mb = new MigrationBuilder(activeProvider: "SqlServer");

            var result = mb.TableExists<TestDbContext>("MyTable");

            Assert.That(result, Is.False);
        }

        [Test]
        public void ColumnExists_WhenColumnNameInvalid_Throws()
        {
            SetupServiceLocatorWithDb(canConnect: false);
            var mb = new MigrationBuilder("SqlServer");

            Assert.That(
                () => mb.ColumnExists<TestDbContext>("", "MyTable"),
                Throws.TypeOf<ArgumentException>()
                      .With.Property(nameof(ArgumentException.ParamName)).EqualTo("columnName"));
        }

        [Test]
        public void ColumnExists_WhenTableNameInvalid_Throws()
        {
            SetupServiceLocatorWithDb(canConnect: false);
            var mb = new MigrationBuilder("SqlServer");

            Assert.That(
                () => mb.ColumnExists<TestDbContext>("Col", ""),
                Throws.TypeOf<ArgumentException>()
                      .With.Property(nameof(ArgumentException.ParamName)).EqualTo("tableName"));
        }

        [Test]
        public void ColumnExists_WhenCannotConnect_ReturnsFalse()
        {
            SetupServiceLocatorWithDb(canConnect: false);
            var mb = new MigrationBuilder("SqlServer");

            var result = mb.ColumnExists<TestDbContext>("Col", "MyTable");

            Assert.That(result, Is.False);
        }

        [Test]
        public void IndexExists_WhenIndexNameInvalid_Throws()
        {
            SetupServiceLocatorWithDb(canConnect: false);
            var mb = new MigrationBuilder("SqlServer");

            Assert.That(
                () => mb.IndexExists<TestDbContext>("", "MyTable"),
                Throws.TypeOf<ArgumentException>()
                      .With.Property(nameof(ArgumentException.ParamName)).EqualTo("indexName"));
        }

        [Test]
        public void IndexExists_WhenTableNameInvalid_Throws()
        {
            SetupServiceLocatorWithDb(canConnect: false);
            var mb = new MigrationBuilder("SqlServer");

            Assert.That(
                () => mb.IndexExists<TestDbContext>("Idx", ""),
                Throws.TypeOf<ArgumentException>()
                      .With.Property(nameof(ArgumentException.ParamName)).EqualTo("tableName"));
        }

        [Test]
        public void IndexExists_WhenCannotConnect_ReturnsFalse()
        {
            SetupServiceLocatorWithDb(canConnect: false);
            var mb = new MigrationBuilder("SqlServer");

            var result = mb.IndexExists<TestDbContext>("Idx", "MyTable");

            Assert.That(result, Is.False);
        }

        [Test]
        public void ForeignKeyExists_WhenKeyNameInvalid_Throws()
        {
            SetupServiceLocatorWithDb(canConnect: false);
            var mb = new MigrationBuilder("SqlServer");

            Assert.That(
                () => mb.ForeignKeyExists<TestDbContext>("", "MyTable"),
                Throws.TypeOf<ArgumentException>()
                      .With.Property(nameof(ArgumentException.ParamName)).EqualTo("keyName"));
        }

        [Test]
        public void ForeignKeyExists_WhenTableNameInvalid_Throws()
        {
            SetupServiceLocatorWithDb(canConnect: false);
            var mb = new MigrationBuilder("SqlServer");

            Assert.That(
                () => mb.ForeignKeyExists<TestDbContext>("FK_Name", ""),
                Throws.TypeOf<ArgumentException>()
                      .With.Property(nameof(ArgumentException.ParamName)).EqualTo("tableName"));
        }

        [Test]
        public void ForeignKeyExists_WhenCannotConnect_ReturnsFalse()
        {
            SetupServiceLocatorWithDb(canConnect: false);
            var mb = new MigrationBuilder("SqlServer");

            var result = mb.ForeignKeyExists<TestDbContext>("FK_Name", "MyTable");

            Assert.That(result, Is.False);
        }
    }
}