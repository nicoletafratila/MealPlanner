namespace Common.Models.Tests
{
    [TestFixture]
    public class BaseModelTests
    {
        [Test]
        public void DefaultCtor_Sets_Default_Values()
        {
            // Act
            var model = new BaseModel();

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(model.Index, Is.EqualTo(0));
                Assert.That(model.IsSelected, Is.False);
            }
        }

        [Test]
        public void Properties_Can_Be_Set_And_Read()
        {
            // Act
            var model = new BaseModel
            {
                Index = 5,
                IsSelected = true
            };

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(model.Index, Is.EqualTo(5));
                Assert.That(model.IsSelected, Is.True);
            }
        }

        [Test]
        public void IsSelected_Can_Be_Toggled()
        {
            var model = new BaseModel { IsSelected = false };

            model.IsSelected = !model.IsSelected;

            Assert.That(model.IsSelected, Is.True);
        }
    }
}