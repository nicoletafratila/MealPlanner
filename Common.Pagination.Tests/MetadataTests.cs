namespace Common.Pagination.Tests
{
    [TestFixture]
    public class MetadataTests
    {
        [Test]
        public void DefaultCtor_Sets_Reasonable_Defaults()
        {
            // Act
            var metadata = new Metadata();

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.Multiple(() =>
                {
                    Assert.That(metadata.PageNumber, Is.EqualTo(1));
                    Assert.That(metadata.PageSize, Is.EqualTo(1));
                    Assert.That(metadata.TotalPages, Is.EqualTo(0));
                    Assert.That(metadata.TotalCount, Is.EqualTo(0));
                    Assert.That(metadata.HasPreviousPage, Is.False);
                    Assert.That(metadata.HasNextPage, Is.False);
                });
            }
        }

        [Test]
        public void Create_Computes_TotalPages_Correctly()
        {
            // Arrange
            const int pageNumber = 2;
            const int pageSize = 10;
            const int totalCount = 35;

            // Act
            var metadata = Metadata.Create(pageNumber, pageSize, totalCount);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.Multiple(() =>
                {
                    Assert.That(metadata.PageNumber, Is.EqualTo(pageNumber));
                    Assert.That(metadata.PageSize, Is.EqualTo(pageSize));
                    Assert.That(metadata.TotalCount, Is.EqualTo(totalCount));
                    Assert.That(metadata.TotalPages, Is.EqualTo(4)); // 35 / 10 => 3.5 => 4
                    Assert.That(metadata.HasPreviousPage, Is.True);
                    Assert.That(metadata.HasNextPage, Is.True);
                });
            }
        }

        [Test]
        public void Create_Sets_HasNextPage_False_On_Last_Page()
        {
            // Arrange
            const int pageNumber = 4;
            const int pageSize = 10;
            const int totalCount = 35; // 4 pages

            // Act
            var metadata = Metadata.Create(pageNumber, pageSize, totalCount);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.Multiple(() =>
                {
                    Assert.That(metadata.TotalPages, Is.EqualTo(4));
                    Assert.That(metadata.HasNextPage, Is.False);
                    Assert.That(metadata.HasPreviousPage, Is.True);
                });
            }
        }

        [Test]
        public void Create_Throws_When_PageNumber_Is_Invalid()
        {
            Assert.That(
                () => Metadata.Create(0, pageSize: 10, totalCount: 100),
                Throws.TypeOf<ArgumentOutOfRangeException>()
                      .With.Property(nameof(ArgumentOutOfRangeException.ParamName)).EqualTo("pageNumber"));
        }

        [Test]
        public void Create_Throws_When_PageSize_Is_Invalid()
        {
            Assert.That(
                () => Metadata.Create(pageNumber: 1, pageSize: 0, totalCount: 100),
                Throws.TypeOf<ArgumentOutOfRangeException>()
                      .With.Property(nameof(ArgumentOutOfRangeException.ParamName)).EqualTo("pageSize"));
        }

        [Test]
        public void Create_Throws_When_TotalCount_Is_Negative()
        {
            Assert.That(
                () => Metadata.Create(pageNumber: 1, pageSize: 10, totalCount: -1),
                Throws.TypeOf<ArgumentOutOfRangeException>()
                      .With.Property(nameof(ArgumentOutOfRangeException.ParamName)).EqualTo("totalCount"));
        }
    }
}