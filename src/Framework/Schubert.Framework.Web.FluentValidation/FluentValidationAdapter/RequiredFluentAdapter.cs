using FluentValidation.Internal;
using FluentValidation.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Schubert.Framework.Web.Validation
{
    public class RequiredFluentAdapter : FluentValidationClientModelValidator<NotNullValidator>
    {
        public RequiredFluentAdapter(NotNullValidator validator) : 
            base(validator)
        {
           
        }

        public override void AddValidation(ClientModelValidationContext context)
        {
            var formatter = new MessageFormatter().AppendPropertyName(context.ModelMetadata.PropertyName);
            var message = formatter.BuildMessage(Validator.Options.ErrorMessageSource.GetString(null));

            context.Attributes.AddIfNotExisting("data-val", "true");
            context.Attributes.AddIfNotExisting("data-val-required", message);

        }
    }

    public class NotEmptyFluentAdapter : FluentValidationClientModelValidator<NotEmptyValidator>
    {
        public NotEmptyFluentAdapter(NotEmptyValidator validator) :
            base(validator)
        {

        }

        public override void AddValidation(ClientModelValidationContext context)
        {
            var formatter = new MessageFormatter().AppendPropertyName(context.ModelMetadata.PropertyName);
            var message = formatter.BuildMessage(Validator.Options.ErrorMessageSource.GetString(null));

            context.Attributes.AddIfNotExisting("data-val", "true");
            context.Attributes.AddIfNotExisting("data-val-required", message);

        }
    }
}
