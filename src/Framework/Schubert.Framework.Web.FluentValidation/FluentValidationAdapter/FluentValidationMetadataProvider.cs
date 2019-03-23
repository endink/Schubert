using FluentValidation;
using FluentValidation.Internal;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Schubert.Framework.Environment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Schubert.Framework.Web.Validation
{
    public class FluentValidationMetadataProvider :
        IValidationMetadataProvider
    {
        private static IDictionary<Type, IEnumerable<IValidationRule>> _validatorFactory;

        //public FluentValidationMetadataProvider(IEnumerable<IDependencyValidator> validators)
        //{
        //    _validators = validators ?? Enumerable.Empty<IDependencyValidator>();
        //}

        public void CreateValidationMetadata(ValidationMetadataProviderContext context)
        {
            if (SchubertEngine.Current.IsRunning)
            {
                if (_validatorFactory == null)
                {
                    lock (this)
                    {
                        if (_validatorFactory == null)
                        {
                            var validators = SchubertEngine.Current.GetWorkContext().ResolveRequired<IEnumerable<IDependencyValidator>>();
                            var query = from v in validators
                                        where v.GetType().GetTypeInfo().BaseType.GetGenericTypeDefinition().Equals(typeof(DependencyValidator<>))
                                        select new { Type = v.GetType().GetTypeInfo().BaseType.GetGenericArguments().First(), Validator = v };

                            _validatorFactory = query.ToDictionary(a => a.Type, a => (a.Validator as IEnumerable<IValidationRule>));
                        }
                    }
                }

                if (context.Key.MetadataKind == ModelMetadataKind.Type)
                {
                    IEnumerable<IValidationRule> rules;
                    _validatorFactory.TryGetValue(context.Key.ModelType, out rules);
                    rules = rules ?? Enumerable.Empty<IValidationRule>();
                    foreach (var rule in rules.Where(r => !(r is PropertyRule)))
                    {
                        if (!context.ValidationMetadata.ValidatorMetadata.Contains(rule))
                        {
                            context.ValidationMetadata.ValidatorMetadata.Add(rule);
                        }
                    }
                }

                if (context.Key.MetadataKind == ModelMetadataKind.Property)
                {
                    IEnumerable<IValidationRule> rules;
                    _validatorFactory.TryGetValue(context.Key.ContainerType, out rules);
                    rules = rules ?? Enumerable.Empty<IValidationRule>();
                    foreach (var rule in rules.Where(r=>(r is PropertyRule) && ((PropertyRule)r).PropertyName.Equals(context.Key.Name)))
                    {
                        if (!context.ValidationMetadata.ValidatorMetadata.Contains(rule))
                        {
                            context.ValidationMetadata.ValidatorMetadata.Add(rule);
                        }
                    }
                }
            }
        }
    }

}
