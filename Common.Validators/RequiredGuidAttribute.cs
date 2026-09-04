using System.ComponentModel.DataAnnotations;
using Common.Validators.Resources;

namespace Common.Validators
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
    public class RequiredGuidAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
            => value is Guid guid && guid != Guid.Empty;

        public override string FormatErrorMessage(string name)
        {
            if (ErrorMessage is null && ErrorMessageResourceName is null && ErrorMessageResourceType is null)
                return string.Format(ValidatorMessages.RequiredGuidError, name);

            return base.FormatErrorMessage(name);
        }
    }
}
