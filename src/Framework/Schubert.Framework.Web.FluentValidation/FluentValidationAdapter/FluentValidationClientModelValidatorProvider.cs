using FluentValidation;
using FluentValidation.Internal;
using FluentValidation.Validators;
using Microsoft.AspNetCore.Mvc.DataAnnotations.Internal;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Schubert.Framework.Web.Validation
{
    public class FluentValidationClientModelValidatorProvider : IClientModelValidatorProvider
    {
        // A factory for validators based on ValidationAttribute.
        internal delegate IClientModelValidator
             FluentValidationClientModelValidationFactory(IPropertyValidator validator);

        private readonly Dictionary<Type, FluentValidationClientModelValidationFactory> _attributeFactories =
            BuildAttributeFactoriesDictionary();

        internal Dictionary<Type, FluentValidationClientModelValidationFactory> RuleFactories
        {
            get { return _attributeFactories; }
        }

        /// <inheritdoc />
        public void CreateValidators(ClientValidatorProviderContext context)
        {
            var hasRequiredAttribute = false;
            foreach (var item in context.Results)
            {
                if (item.Validator == null)
                {
                    var validateRule = item.ValidatorMetadata as IValidationRule;
                    if (validateRule == null)
                    {
                        continue;
                    }
                    foreach (var val in validateRule.Validators)
                    {
                        if (!hasRequiredAttribute)
                        {
                            hasRequiredAttribute |= (val is NotNullValidator);
                            hasRequiredAttribute |= (val is NotEmptyValidator);
                        }

                        FluentValidationClientModelValidationFactory factory;
                        if (_attributeFactories.TryGetValue(val.GetType(), out factory))
                        {
                            item.Validator = factory(val);
                            item.IsReusable = true;
                        }
                    }
                }
            }
            if (hasRequiredAttribute && context.ModelMetadata.IsRequired)
            {
                var item = context.Results.Where(i=>i.Validator is RequiredAttributeAdapter).FirstOrDefault();
                //移除 Mvc 自动添加的 Required 验证器。
                if (item != null)
                {
                    context.Results.Remove(item);
                }
            }
        }

        private static Dictionary<Type, FluentValidationClientModelValidationFactory> BuildAttributeFactoriesDictionary()
        {
            return new Dictionary<Type, FluentValidationClientModelValidationFactory>()
            {
                {
                    typeof(EqualValidator),
                    (validator) => new EqualFluentAdapter((EqualValidator)validator)
                },
                {
                    typeof(LessThanOrEqualValidator),
                    (validator) => new MaxFluentAdapter((LessThanOrEqualValidator)validator)
                },
                {
                    typeof(GreaterThanOrEqualValidator),
                    (validator) => new MinFluentAdapter((GreaterThanOrEqualValidator)validator)
                },
                {
                    typeof(InclusiveBetweenValidator),
                    (validator) => new RangeFluentAdapter((InclusiveBetweenValidator)validator)
                },
                {
                    typeof(NotNullValidator),
                    (validator) => new RequiredFluentAdapter((NotNullValidator)validator)
                },
                {
                    typeof(NotEmptyValidator),
                    (validator) => new NotEmptyFluentAdapter((NotEmptyValidator)validator)
                },
                {
                    typeof(LengthValidator),
                    (validator) => new StringLengthFluentAdapter((ILengthValidator)validator)
                },
                {
                    typeof(RegularExpressionValidator),
                    (validator) => new RegexFluentAdapter((IRegularExpressionValidator)validator)
                },
                //{
                //    typeof(CreditCardAttribute),
                //    (attribute) => new DataTypeAttributeAdapter((DataTypeAttribute)attribute, "creditcard")
                //},
                //{
                //    typeof(EmailAddressAttribute),
                //    (attribute) => new DataTypeAttributeAdapter((DataTypeAttribute)attribute, "email")
                //},
                //{
                //    typeof(PhoneAttribute),
                //    (attribute) => new DataTypeAttributeAdapter((DataTypeAttribute)attribute, "phone")
                //},
                //{
                //    typeof(UrlAttribute),
                //    (attribute) => new DataTypeAttributeAdapter((DataTypeAttribute)attribute, "url")
                //}
            };
        }

    }
}
