using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace Common.Validators
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
    public class MinimumCountCollectionAttribute : ValidationAttribute
    {
        private readonly int _minCount;

        public MinimumCountCollectionAttribute(int minCount)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(minCount);

            _minCount = minCount;
        }

        public override bool IsValid(object? value)
        {
            if (value == null)
                return false;

            if (value is string)
                return false;

            if (value is ICollection collection)
                return collection.Count >= _minCount;

            if (value is not IEnumerable enumerable)
                return false;

            var count = 0;
            foreach (var _ in enumerable)
            {
                count++;
                if (count >= _minCount)
                    return true;
            }

            return false;
        }

        public override string FormatErrorMessage(string name)
            => ErrorMessage ?? $"{name} must contain at least {_minCount} item(s).";
    }
}