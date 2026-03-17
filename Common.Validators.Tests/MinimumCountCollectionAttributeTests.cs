namespace Common.Validators.Tests
{
    [TestFixture]
    public class MinimumCountCollectionAttributeTests
    {
        [Test]
        public void Ctor_NegativeMinCount_Throws()
        {
            Assert.That(
                () => new MinimumCountCollectionAttribute(-1),
                Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void NullValue_IsInvalid()
        {
            var attr = new MinimumCountCollectionAttribute(1);

            var isValid = attr.IsValid(null);

            Assert.That(isValid, Is.False);
        }

        [Test]
        public void NonEnumerableValue_IsInvalid()
        {
            var attr = new MinimumCountCollectionAttribute(1);
            var value = 42; 

            var isValid = attr.IsValid(value);

            Assert.That(isValid, Is.False);
        }

        [Test]
        public void StringValue_IsInvalid()
        {
            var attr = new MinimumCountCollectionAttribute(1);

            var isValid = attr.IsValid("abc");

            Assert.That(isValid, Is.False);
        }

        [Test]
        public void ICollection_WithLessThanMinimum_IsInvalid()
        {
            var attr = new MinimumCountCollectionAttribute(2);
            var list = new List<int> { 1 };

            var isValid = attr.IsValid(list);

            Assert.That(isValid, Is.False);
        }

        [Test]
        public void ICollection_WithExactlyMinimum_IsValid()
        {
            var attr = new MinimumCountCollectionAttribute(2);
            var list = new List<int> { 1, 2 };

            var isValid = attr.IsValid(list);

            Assert.That(isValid, Is.True);
        }

        [Test]
        public void ICollection_WithMoreThanMinimum_IsValid()
        {
            var attr = new MinimumCountCollectionAttribute(2);
            var list = new List<int> { 1, 2, 3 };

            var isValid = attr.IsValid(list);

            Assert.That(isValid, Is.True);
        }

        [Test]
        public void Array_WithLessThanMinimum_IsInvalid()
        {
            var attr = new MinimumCountCollectionAttribute(3);
            var array = new[] { 1, 2 };

            var isValid = attr.IsValid(array);

            Assert.That(isValid, Is.False);
        }

        [Test]
        public void Array_WithAtLeastMinimum_IsValid()
        {
            var attr = new MinimumCountCollectionAttribute(2);
            var array = new[] { 1, 2, 3 };

            var isValid = attr.IsValid(array);

            Assert.That(isValid, Is.True);
        }

        [Test]
        public void IEnumerableOnly_WithAtLeastMinimum_IsValid()
        {
            var attr = new MinimumCountCollectionAttribute(3);
            IEnumerable<int> enumerable = Enumerable.Range(1, 3); 

            var isValid = attr.IsValid(enumerable);

            Assert.That(isValid, Is.True);
        }

        [Test]
        public void IEnumerableOnly_WithLessThanMinimum_IsInvalid()
        {
            var attr = new MinimumCountCollectionAttribute(3);
            IEnumerable<int> enumerable = Enumerable.Range(1, 2);

            var isValid = attr.IsValid(enumerable);

            Assert.That(isValid, Is.False);
        }

        [Test]
        public void FormatErrorMessage_UsesDefaultMessage_WhenErrorMessageNotSet()
        {
            var attr = new MinimumCountCollectionAttribute(2);

            var message = attr.FormatErrorMessage("Items");

            Assert.That(message, Is.EqualTo("Items must contain at least 2 item(s)."));
        }

        [Test]
        public void FormatErrorMessage_UsesCustomErrorMessage_WhenErrorMessageIsSet()
        {
            var attr = new MinimumCountCollectionAttribute(2)
            {
                ErrorMessage = "Custom error"
            };

            var message = attr.FormatErrorMessage("Items");

            Assert.That(message, Is.EqualTo("Custom error"));
        }
    }
}