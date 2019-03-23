using FluentValidation.Internal;
using FluentValidation.Resources;
using FluentValidation.Validators;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Schubert.Framework.Web.Validation
{
    public class RangeFluentAdapter : FluentValidationClientModelValidator<InclusiveBetweenValidator>
    {
        public RangeFluentAdapter(InclusiveBetweenValidator validator)
            : base(validator)
        {

        }

        public override void AddValidation(ClientModelValidationContext context)
        {
            context.Attributes.AddIfNotExisting("data-val", "true");
            context.Attributes.AddIfNotExisting("data-val-range", GetErrorMessage(context));
            context.Attributes.AddIfNotExisting("data-val-range-min", this.Validator.From.ToString());
            context.Attributes.AddIfNotExisting("data-val-range-max", this.Validator.To.ToString());
        }

        private string GetErrorMessage(ClientModelValidationContext context)
        {
            var formatter = new MessageFormatter()
                .AppendPropertyName(context.ModelMetadata.PropertyName)
                .AppendArgument("From", this.Validator.From)
                .AppendArgument("To", this.Validator.To);

            string message = this.Validator.Options.ErrorMessageSource.GetString(null);

            //if (this.Validator.ErrorMessageSource.ResourceType == typeof(Messages))
            //{
            //    // If we're using the default resources then the mesage for length errors will have two parts, eg:
            //    // '{PropertyName}' must be between {From} and {To}. You entered {Value}.
            //    // We can't include the "Value" part of the message because this information isn't available at the time the message is constructed.
            //    // Instead, we'll just strip this off by finding the index of the period that separates the two parts of the message.

            //    message = message.Substring(0, message.IndexOf(".") + 1);
            //}

            message = formatter.BuildMessage(message);
            return message;
        }
    }
}
