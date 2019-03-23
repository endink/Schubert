using FluentValidation;
using FluentValidation.Internal;
using FluentValidation.Validators;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Schubert.Framework.Web.Validation
{
    public class EqualFluentAdapter : FluentValidationClientModelValidator<EqualValidator>
    {
        public EqualFluentAdapter(EqualValidator validator)
            : base(validator)
        {
        }

        protected override string GetErrorMessage(ModelMetadata modelMetadata)
        {
            return base.GetErrorMessage(modelMetadata);
        }

        public override void AddValidation(ClientModelValidationContext context)
        {
            var propertyToCompare = this.Validator.MemberToCompare as PropertyInfo;
            if (propertyToCompare != null)
            {
                // If propertyToCompare is not null then we're comparing to another property.
                // If propertyToCompare is null then we're either comparing against a literal value, a field or a method call.
                // We only care about property comparisons in this case.

                var comparisonDisplayName =
                    ValidatorOptions.DisplayNameResolver(this.Validator.MemberToCompare.DeclaringType, propertyToCompare, null)
                    ?? propertyToCompare.Name.SplitPascalCase();

                var formatter = new MessageFormatter()
                    .AppendPropertyName(context.ModelMetadata.PropertyName)
                    .AppendArgument("ComparisonValue", comparisonDisplayName);


                string message = formatter.BuildMessage(this.Validator.Options.ErrorMessageSource.GetString(null));
                context.Attributes.AddIfNotExisting("data-val", "true");
                context.Attributes.AddIfNotExisting("data-val-equalto", message);
                context.Attributes.AddIfNotExisting("data-val-equalto-other", $"*.{propertyToCompare.Name}");

            }
        }
    }
}
