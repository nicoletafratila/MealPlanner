using System.Globalization;
using MealPlanner.UI.Mobile.Converters;

namespace MealPlanner.UI.Mobile.Tests.Converters
{
    [TestFixture]
    public class StringNotNullConverterTests
    {
        private readonly StringNotNullConverter _converter = new();

        [Test]
        public void Convert_NonEmptyString_ReturnsTrue()
        {
            var result = _converter.Convert("hello", typeof(bool), null, CultureInfo.InvariantCulture);

            Assert.That(result, Is.EqualTo(true));
        }

        [Test]
        public void Convert_Null_ReturnsFalse()
        {
            var result = _converter.Convert(null, typeof(bool), null, CultureInfo.InvariantCulture);

            Assert.That(result, Is.EqualTo(false));
        }

        [Test]
        public void Convert_WhitespaceString_ReturnsFalse()
        {
            var result = _converter.Convert("   ", typeof(bool), null, CultureInfo.InvariantCulture);

            Assert.That(result, Is.EqualTo(false));
        }

        [Test]
        public void Convert_EmptyString_ReturnsFalse()
        {
            var result = _converter.Convert(string.Empty, typeof(bool), null, CultureInfo.InvariantCulture);

            Assert.That(result, Is.EqualTo(false));
        }

        [Test]
        public void Convert_NonStringValue_ReturnsFalse()
        {
            var result = _converter.Convert(42, typeof(bool), null, CultureInfo.InvariantCulture);

            Assert.That(result, Is.EqualTo(false));
        }

        [Test]
        public void ConvertBack_Always_ThrowsNotImplementedException()
        {
            Assert.Throws<NotImplementedException>(() =>
                _converter.ConvertBack(true, typeof(string), null, CultureInfo.InvariantCulture));
        }
    }

    [TestFixture]
    public class NotNullConverterTests
    {
        private readonly NotNullConverter _converter = new();

        [Test]
        public void Convert_NonNullValue_ReturnsTrue()
        {
            var result = _converter.Convert("anything", typeof(bool), null, CultureInfo.InvariantCulture);

            Assert.That(result, Is.EqualTo(true));
        }

        [Test]
        public void Convert_Null_ReturnsFalse()
        {
            var result = _converter.Convert(null, typeof(bool), null, CultureInfo.InvariantCulture);

            Assert.That(result, Is.EqualTo(false));
        }

        [Test]
        public void Convert_BoxedDefaultStruct_ReturnsTrue()
        {
            // A boxed value type (e.g. 0) is still "not null" - only an actual null reference is false.
            var result = _converter.Convert(0, typeof(bool), null, CultureInfo.InvariantCulture);

            Assert.That(result, Is.EqualTo(true));
        }

        [Test]
        public void ConvertBack_Always_ThrowsNotImplementedException()
        {
            Assert.Throws<NotImplementedException>(() =>
                _converter.ConvertBack("value", typeof(object), null, CultureInfo.InvariantCulture));
        }
    }

    [TestFixture]
    public class InvertBoolConverterTests
    {
        private readonly InvertBoolConverter _converter = new();

        [Test]
        public void Convert_True_ReturnsFalse()
        {
            var result = _converter.Convert(true, typeof(bool), null, CultureInfo.InvariantCulture);

            Assert.That(result, Is.EqualTo(false));
        }

        [Test]
        public void Convert_False_ReturnsTrue()
        {
            var result = _converter.Convert(false, typeof(bool), null, CultureInfo.InvariantCulture);

            Assert.That(result, Is.EqualTo(true));
        }

        [Test]
        public void Convert_NonBoolValue_ReturnsFalse()
        {
            var result = _converter.Convert("not-a-bool", typeof(bool), null, CultureInfo.InvariantCulture);

            Assert.That(result, Is.EqualTo(false));
        }

        [Test]
        public void Convert_Null_ReturnsFalse()
        {
            var result = _converter.Convert(null, typeof(bool), null, CultureInfo.InvariantCulture);

            Assert.That(result, Is.EqualTo(false));
        }

        [Test]
        public void ConvertBack_True_ReturnsFalse()
        {
            var result = _converter.ConvertBack(true, typeof(bool), null, CultureInfo.InvariantCulture);

            Assert.That(result, Is.EqualTo(false));
        }

        [Test]
        public void ConvertBack_False_ReturnsTrue()
        {
            var result = _converter.ConvertBack(false, typeof(bool), null, CultureInfo.InvariantCulture);

            Assert.That(result, Is.EqualTo(true));
        }

        [Test]
        public void ConvertBack_NonBoolValue_ReturnsFalse()
        {
            var result = _converter.ConvertBack(123, typeof(bool), null, CultureInfo.InvariantCulture);

            Assert.That(result, Is.EqualTo(false));
        }
    }

    [TestFixture]
    public class BoolToStringConverterTests
    {
        private readonly BoolToStringConverter _converter = new();

        [Test]
        public void Convert_TrueWithBothParameterParts_ReturnsFirstPart()
        {
            var result = _converter.Convert(true, typeof(string), "Yep|Nope", CultureInfo.InvariantCulture);

            Assert.That(result, Is.EqualTo("Yep"));
        }

        [Test]
        public void Convert_FalseWithBothParameterParts_ReturnsSecondPart()
        {
            var result = _converter.Convert(false, typeof(string), "Yep|Nope", CultureInfo.InvariantCulture);

            Assert.That(result, Is.EqualTo("Nope"));
        }

        [Test]
        public void Convert_TrueWithNullParameter_ReturnsDefaultYes()
        {
            var result = _converter.Convert(true, typeof(string), null, CultureInfo.InvariantCulture);

            Assert.That(result, Is.EqualTo("Yes"));
        }

        [Test]
        public void Convert_FalseWithNullParameter_ReturnsDefaultNo()
        {
            var result = _converter.Convert(false, typeof(string), null, CultureInfo.InvariantCulture);

            Assert.That(result, Is.EqualTo("No"));
        }

        [Test]
        public void Convert_FalseWithParameterMissingSecondPart_ReturnsDefaultNo()
        {
            var result = _converter.Convert(false, typeof(string), "OnlyOnePart", CultureInfo.InvariantCulture);

            Assert.That(result, Is.EqualTo("No"));
        }

        [Test]
        public void Convert_NonBoolValue_ReturnsFalseBranch()
        {
            var result = _converter.Convert("not-a-bool", typeof(string), "Yep|Nope", CultureInfo.InvariantCulture);

            Assert.That(result, Is.EqualTo("Nope"));
        }

        [Test]
        public void Convert_NullValue_ReturnsFalseBranchDefault()
        {
            var result = _converter.Convert(null, typeof(string), null, CultureInfo.InvariantCulture);

            Assert.That(result, Is.EqualTo("No"));
        }

        [Test]
        public void ConvertBack_Always_ThrowsNotImplementedException()
        {
            Assert.Throws<NotImplementedException>(() =>
                _converter.ConvertBack("Yes", typeof(bool), null, CultureInfo.InvariantCulture));
        }
    }

    [TestFixture]
    public class CollectedToDecorConverterTests
    {
        private readonly CollectedToDecorConverter _converter = new();

        [Test]
        public void Convert_True_ReturnsStrikethrough()
        {
            var result = _converter.Convert(true, typeof(TextDecorations), null, CultureInfo.InvariantCulture);

            Assert.That(result, Is.EqualTo(TextDecorations.Strikethrough));
        }

        [Test]
        public void Convert_False_ReturnsNone()
        {
            var result = _converter.Convert(false, typeof(TextDecorations), null, CultureInfo.InvariantCulture);

            Assert.That(result, Is.EqualTo(TextDecorations.None));
        }

        [Test]
        public void Convert_NonBoolValue_ReturnsNone()
        {
            var result = _converter.Convert("not-a-bool", typeof(TextDecorations), null, CultureInfo.InvariantCulture);

            Assert.That(result, Is.EqualTo(TextDecorations.None));
        }

        [Test]
        public void Convert_Null_ReturnsNone()
        {
            var result = _converter.Convert(null, typeof(TextDecorations), null, CultureInfo.InvariantCulture);

            Assert.That(result, Is.EqualTo(TextDecorations.None));
        }

        [Test]
        public void ConvertBack_Always_ThrowsNotImplementedException()
        {
            Assert.Throws<NotImplementedException>(() =>
                _converter.ConvertBack(TextDecorations.Strikethrough, typeof(bool), null, CultureInfo.InvariantCulture));
        }
    }

    [TestFixture]
    public class ZeroToBoolConverterTests
    {
        private readonly ZeroToBoolConverter _converter = new();

        [Test]
        public void Convert_Zero_ReturnsTrue()
        {
            var result = _converter.Convert(0, typeof(bool), null, CultureInfo.InvariantCulture);

            Assert.That(result, Is.EqualTo(true));
        }

        [Test]
        public void Convert_NonZeroInt_ReturnsFalse()
        {
            var result = _converter.Convert(5, typeof(bool), null, CultureInfo.InvariantCulture);

            Assert.That(result, Is.EqualTo(false));
        }

        [Test]
        public void Convert_NegativeInt_ReturnsFalse()
        {
            var result = _converter.Convert(-1, typeof(bool), null, CultureInfo.InvariantCulture);

            Assert.That(result, Is.EqualTo(false));
        }

        [Test]
        public void Convert_NonIntValue_ReturnsFalse()
        {
            var result = _converter.Convert("0", typeof(bool), null, CultureInfo.InvariantCulture);

            Assert.That(result, Is.EqualTo(false));
        }

        [Test]
        public void Convert_Null_ReturnsFalse()
        {
            var result = _converter.Convert(null, typeof(bool), null, CultureInfo.InvariantCulture);

            Assert.That(result, Is.EqualTo(false));
        }

        [Test]
        public void ConvertBack_Always_ThrowsNotImplementedException()
        {
            Assert.Throws<NotImplementedException>(() =>
                _converter.ConvertBack(true, typeof(int), null, CultureInfo.InvariantCulture));
        }
    }

    [TestFixture]
    public class FractionToStarConverterTests
    {
        private readonly FractionToStarConverter _converter = new();

        [Test]
        public void Convert_PositiveDouble_ReturnsStarGridLengthWithThatValue()
        {
            var result = _converter.Convert(2.5, typeof(GridLength), null, CultureInfo.InvariantCulture);

            var gridLength = (GridLength)result;
            using (Assert.EnterMultipleScope())
            {
                Assert.That(gridLength.Value, Is.EqualTo(2.5));
                Assert.That(gridLength.GridUnitType, Is.EqualTo(GridUnitType.Star));
            }
        }

        [Test]
        public void Convert_ZeroDouble_ReturnsMinimumValue()
        {
            var result = _converter.Convert(0d, typeof(GridLength), null, CultureInfo.InvariantCulture);

            var gridLength = (GridLength)result;
            Assert.That(gridLength.Value, Is.EqualTo(0.01));
        }

        [Test]
        public void Convert_NegativeDouble_ReturnsMinimumValue()
        {
            var result = _converter.Convert(-5d, typeof(GridLength), null, CultureInfo.InvariantCulture);

            var gridLength = (GridLength)result;
            Assert.That(gridLength.Value, Is.EqualTo(0.01));
        }

        [Test]
        public void Convert_NonDoubleValue_ReturnsMinimumValue()
        {
            var result = _converter.Convert("not-a-double", typeof(GridLength), null, CultureInfo.InvariantCulture);

            var gridLength = (GridLength)result;
            using (Assert.EnterMultipleScope())
            {
                Assert.That(gridLength.Value, Is.EqualTo(0.01));
                Assert.That(gridLength.GridUnitType, Is.EqualTo(GridUnitType.Star));
            }
        }

        [Test]
        public void Convert_Null_ReturnsMinimumValue()
        {
            var result = _converter.Convert(null, typeof(GridLength), null, CultureInfo.InvariantCulture);

            var gridLength = (GridLength)result;
            Assert.That(gridLength.Value, Is.EqualTo(0.01));
        }

        [Test]
        public void ConvertBack_Always_ThrowsNotImplementedException()
        {
            Assert.Throws<NotImplementedException>(() =>
                _converter.ConvertBack(new GridLength(1, GridUnitType.Star), typeof(double), null, CultureInfo.InvariantCulture));
        }
    }
}
