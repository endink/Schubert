using FluentValidation;
using FluentValidation.Internal;
using FluentValidation.Validators;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Schubert.Framework.Web.Validation
{
    public class FluentValidationModelValidator : IModelValidator
    {
        private bool? _required = null;
        public FluentValidationModelValidator(IValidationRule rule)
        {
            this.Rule = rule;
        }

        public IValidationRule Rule { get; private set; }

        public bool IsRequired
        {
            get { return (bool)(_required ?? (_required = this.Rule.Validators.Any(v => v is NotNullValidator))); }
        }

        public IEnumerable<ModelValidationResult> Validate(ModelValidationContext validationContext)
        {
            var modelExplorer = validationContext.Model;
            var metadata = validationContext.ModelMetadata;

            var memberName = metadata.PropertyName;

            var container = validationContext.Container;

            var fakeParentContext = new ValidationContext(container ?? validationContext.Model);

            var result = this.Rule.Validate(fakeParentContext);
            foreach (var failure in result.Where(r=>r != null))
            {
                /* 
                 * 微软 1.0.1 改动规则（2016-12-06 by Sharping）
                  ModelValidationResult.MemberName is used by invoking validators (such as ModelValidator) to
                  append construct the ModelKey for ModelStateDictionary. When validating at type level we
                  want the returned MemberNames if specified (e.g. "person.Address.FirstName"). For property
                  validation, the ModelKey can be constructed using the ModelMetadata and we should ignore
                  MemberName (we don't want "person.Name.Name"). However the invoking validator does not have
                  a way to distinguish between these two cases. Consequently we'll only set MemberName if this
                  validation returns a MemberName that is different from the property being validated.
                */
                var newMemberName = String.Equals(failure.PropertyName, memberName, StringComparison.Ordinal) ?
                            null : failure.PropertyName;
                yield return new ModelValidationResult(newMemberName, failure.ErrorMessage);
            }
        }
    }
}
