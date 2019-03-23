using FluentValidation.Internal;
using FluentValidation.Validators;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Schubert.Framework.Web.Validation
{
    public class MaxFluentAdapter : FluentValidationClientModelValidator<LessThanOrEqualValidator>
    {
        public MaxFluentAdapter(LessThanOrEqualValidator validator)
            :base(validator)
        {

        }

        public override void AddValidation(ClientModelValidationContext context)
        {
            context.Attributes.AddIfNotExisting("data-val", "true");
            context.Attributes.AddIfNotExisting("data-val-range", GetErrorMessage(context.ModelMetadata));
            context.Attributes.AddIfNotExisting("data-val-range-max", this.Validator.ValueToCompare.ToString());
        }
    }
}
