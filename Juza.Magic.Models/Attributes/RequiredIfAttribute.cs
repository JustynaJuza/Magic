using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

// based on http://www.devtrends.co.uk/blog/the-complete-guide-to-validation-in-asp.net-mvc-3-part-2

namespace Juza.Magic.Models.Attributes
{
    public enum Comparison
    {
        IsEqualTo,
        IsNotEqualTo
    }

    [AttributeUsage(AttributeTargets.Property)]
    public sealed class RequiredIfAttribute : ValidationAttribute, IClientValidatable
    {
        private const string DefaultErrorMessageFormatString = "The {0} field is required.";

        public string OtherProperty { get; protected set; }
        public Comparison Comparison { get; protected set; }
        public object Value { get; protected set; }

        public RequiredIfAttribute(string otherProperty, Comparison comparison, object value)
            : base(() => DefaultErrorMessageFormatString)
        {
            // TODO: Check if property exists

            OtherProperty = otherProperty;
            Comparison = comparison;
            Value = value;
        }

        private bool TriggerRequirementFromOtherProperty(object otherPropertyValue)
        {
            switch (Comparison)
            {
                case Comparison.IsNotEqualTo:
                    return !otherPropertyValue.Equals(Value);
                default:
                    return otherPropertyValue.Equals(Value);
            }
        }
        
        protected override ValidationResult IsValid(object value,
            ValidationContext validationContext)
        {
            if (value == null)
            {
                var otherProperty = validationContext.ObjectInstance.GetType()
                    .GetProperty(OtherProperty);

                var otherPropertyValue = otherProperty.GetValue(validationContext.ObjectInstance, null);

                if (TriggerRequirementFromOtherProperty(otherPropertyValue))
                {
                    return new ValidationResult(FormatErrorMessage(validationContext.DisplayName));
                }
            }
            return ValidationResult.Success;
        }

        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(
            ModelMetadata metadata,
            ControllerContext context)
        {
            return new[]
            {
                new ModelClientValidationRequiredIfRule(string.Format(ErrorMessageString,
                    metadata.GetDisplayName()), OtherProperty, Comparison, Value)
            };
        }
    }

    public class ModelClientValidationRequiredIfRule : ModelClientValidationRule
    {
        public ModelClientValidationRequiredIfRule(string errorMessage,
            string otherProperty,
            Comparison comparison,
            object value)
        {
            ErrorMessage = errorMessage;
            ValidationType = "requiredif";
            ValidationParameters.Add("other", otherProperty);
            ValidationParameters.Add("comp", comparison.ToString().ToLower());
            ValidationParameters.Add("value", value.ToString().ToLower());
        }
    }
}