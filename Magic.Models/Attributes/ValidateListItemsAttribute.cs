using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace Penna.Gateway.Modules.CTSWeb.Models
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class ValidateListItemsAttribute : ValidationAttribute
    {
        private readonly string _validationErrorMessageStringFormat = 
            "The value {0} at index {1} of list {2} is not valid. Reason for error: {3}";

        private readonly string _argumentErrorMessageStringFormat =
            "You must provide a property name for the attribute list on the ValidateListItems attribute.";

        private readonly string _memberAccessErrorMessageStringFormat =
            "The property name {0} for the attribute list on the ValidateListItems attribute does not refer to a valid member of the {1} class.";

        private readonly string _attributesCastErrorMessageStringFormat =
            "The property {0} of class {1} is not an IEnumerable<ValidationAttribute> collection that can be used for validating list items in the ValidateListItems attribute";

        private IList _list;
        private IList<ValidationAttribute> _attributes;
        private object _errorValue;
        private int _errorIndex;
        private ValidationAttribute _errorAttribute;

        private string AttributeListPropertyName { get; set; }

        public ValidateListItemsAttribute(string attributeListPropertyName)
        {
            if (string.IsNullOrWhiteSpace(attributeListPropertyName))
            {
                throw new ArgumentException(_argumentErrorMessageStringFormat);
            }
            AttributeListPropertyName = attributeListPropertyName;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            _list = value as IList;
            if (_list == null)
                return ValidationResult.Success;

            FetchAttributes(validationContext);
            var isValid = CheckAllItemsAgainstAttributes();

            return isValid
                ? ValidationResult.Success
                : new ValidationResult(
                    string.Format(_validationErrorMessageStringFormat, _errorValue, _errorIndex, validationContext.DisplayName, _errorAttribute.ErrorMessage));
        }

        private void FetchAttributes(ValidationContext validationContext)
        {
            var attributeValues = validationContext.ObjectInstance.GetPropertyValue(AttributeListPropertyName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (attributeValues == null)
            {
                throw new MissingMemberException(
                    string.Format(_memberAccessErrorMessageStringFormat, AttributeListPropertyName,
                        validationContext.ObjectInstance.GetType()));
            }

            _attributes = attributeValues as IList<ValidationAttribute>;
            if (_attributes == null)
            {
                throw new InvalidCastException(
                    string.Format(_attributesCastErrorMessageStringFormat, AttributeListPropertyName,
                        validationContext.ObjectInstance.GetType()));
            }
        }

        private void SetErrorDetailsForItem(object item)
        {
            _errorValue = item;
            _errorIndex = _list.IndexOf(item);
            _errorAttribute = _attributes.First(i => !i.IsValid(item));
        }

        private bool CheckAllItemsAgainstAttributes()
        {
            foreach (var item in _list)
            {
                var isValid = _attributes.All(i => i.IsValid(item));
                if (isValid) continue;

                SetErrorDetailsForItem(item);
                return false;
            }

            return true;
        }
    }
}