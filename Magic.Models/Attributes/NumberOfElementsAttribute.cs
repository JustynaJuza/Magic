using System;
using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace Penna.Gateway.Modules.CTSWeb.Models
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class NumberOfElementsAttribute : ValidationAttribute
    {
        private readonly string _validationMinErrorMessageStringFormat =
            "The {0}list is invalid. The minimum elements requirement is {1} but {2} element(s) exist.";

        private readonly string _validationMaxErrorMessageStringFormat =
            "The {0} list is invalid. The maximum elements requirement is {1} but {2} element(s) exist.";

        private readonly string _nullErrorMessageStringFormat =
            "The {0} list marked with the NumberOfElements attribute in class {1} is null.";

        private readonly int _minElements;
        private readonly int? _maxElements;

        public NumberOfElementsAttribute(int minElements)
        {
            _minElements = minElements;
        }

        public NumberOfElementsAttribute(int minElements, int maxElements)
        {
            _minElements = minElements;
            _maxElements = maxElements;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var list = value as IList;
            if (list == null)
                return new ValidationResult(
                    string.Format(_nullErrorMessageStringFormat, 
                        validationContext.DisplayName, validationContext.ObjectInstance.GetType()));

            if (!_maxElements.HasValue || list.Count <= _maxElements)
            {
                return list.Count >= _minElements
                    ? ValidationResult.Success
                    : new ValidationResult(string.Format(_validationMinErrorMessageStringFormat,
                        validationContext.DisplayName, _minElements, list.Count));
            }

            return new ValidationResult(string.Format(_validationMaxErrorMessageStringFormat,
                validationContext.DisplayName, _maxElements, list.Count));
        }
    }
}