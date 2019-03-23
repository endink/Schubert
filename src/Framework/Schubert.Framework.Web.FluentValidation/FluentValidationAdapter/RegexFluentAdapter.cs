using FluentValidation.Validators;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;

namespace Schubert.Framework.Web.Validation
{
    public class RegexFluentAdapter  : FluentValidationClientModelValidator<IRegularExpressionValidator>
    {
        public RegexFluentAdapter(IRegularExpressionValidator validator)
            :base(validator)
        {
            
        }
        public override void AddValidation(ClientModelValidationContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            context.Attributes.AddIfNotExisting("data-val", "true");
            context.Attributes.AddIfNotExisting("data-val-regex", GetErrorMessage(context.ModelMetadata));
            context.Attributes.AddIfNotExisting("data-val-regex-pattern", this.Validator.Expression);
        }

    }
}
