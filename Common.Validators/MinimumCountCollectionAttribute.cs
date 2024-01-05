using System.ComponentModel.DataAnnotations;

namespace Common.Validators
{
    public class MinimumCountCollectionAttribute(int count) : ValidationAttribute
    {
        readonly int _count = count;

        public override bool IsValid(object? value)
        {
            if (value == null)
                return false;

            if (value is IEnumerable<object> collection && collection.Count() < _count)
                return false;

            return true;
        }
    }
}