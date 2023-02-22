using System.ComponentModel.DataAnnotations;

namespace Common.Validators
{
    public class MinimumCountCollectionAttribute : ValidationAttribute
    {
        readonly int _count;

        public MinimumCountCollectionAttribute(int count)
        {
            _count = count;
        }

        public override bool IsValid(object value)
        {
            if (value == null)
                return false;

            var collection = value as IEnumerable<object>;
            if (collection != null && collection.Count() < _count)
                return false;

            return true;
        }
    }
}