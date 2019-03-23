using FluentValidation.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using FluentValidation.Internal;

namespace Schubert.Framework.Web.Validation
{
    public class StringLengthFluentAdapter : FluentValidationClientModelValidator<ILengthValidator>
    {
        public StringLengthFluentAdapter(ILengthValidator validator)
            :base(validator)
        {
           
        }

        public override void AddValidation(ClientModelValidationContext context)
        {
            context.Attributes.AddIfNotExisting("data-val", "true");
            context.Attributes.AddIfNotExisting("data-val-length", GetErrorMessage(context.ModelMetadata));

            if (this.Validator.Max != int.MaxValue)
            {
                context.Attributes.AddIfNotExisting("data-val-length-max", this.Validator.Max.ToString());
            }

            if (this.Validator.Min != 0)
            {
                context.Attributes.AddIfNotExisting("data-val-length-min", this.Validator.Min.ToString());
            }
        }
    }
}
