using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Schubert.Framework.Web.Mvc.Conventions
{
    public class ApiActionModelConvention : IActionModelConvention
    {
        private bool IsHttpGetMethod(ActionModel model)
        {
            var selectors = model.Selectors ?? Enumerable.Empty<SelectorModel>();
            foreach (var selector in selectors)
            {
                if (selector.ActionConstraints.OfType<HttpMethodActionConstraint>().SelectMany(c => c.HttpMethods).Contains("GET", StringComparer.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        public void Apply(ActionModel action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }
            
            if (IsConventionApplicable(action.Controller))
            {
                foreach (var parameter in action.Parameters)
                {
                    // Some IBindingSourceMetadata attributes like ModelBinder attribute return null 
                    // as their binding source. Special case to ensure we do not ignore them.
                    if (parameter.BindingInfo?.BindingSource != null ||
                        parameter.Attributes.OfType<IBindingSourceMetadata>().Any())
                    {
                        // This has a binding behavior configured, just leave it alone.
                    }
                    else if (IsHttpGetMethod(action) || CanConvertFromString(parameter.ParameterInfo.ParameterType))
                    {
                        // Simple types are by-default from the URI.
                        parameter.BindingInfo = parameter.BindingInfo ?? new BindingInfo();
                        parameter.BindingInfo.BindingSource = CompositeBindingSource.Create(new BindingSource[]
                        {
                            BindingSource.Query,
                            BindingSource.Path
                        }, "Uri");
                    }
                    else
                    {
                        // Complex types are by-default from the body.
                        parameter.BindingInfo = parameter.BindingInfo ?? new BindingInfo();
                        parameter.BindingInfo.BindingSource = BindingSource.Body;
                    }

                    //var optionalParameters = new HashSet<string>();
                    //// For all non IOptionalBinderMetadata, which are not URL source (like FromQuery etc.) do not
                    //// participate in overload selection and hence are added to the hashset so that they can be
                    //// ignored in OverloadActionConstraint.
                    //var optionalMetadata = parameter.Attributes.OfType<IBindingSourceMetadata>().SingleOrDefault();
                    //if ((parameter.ParameterInfo.HasDefaultValue && parameter.BindingInfo.BindingSource == uriBindingSource) ||
                    //    optionalMetadata == null && parameter.BindingInfo.BindingSource != uriBindingSource)
                    //{
                    //    String name = (optionalMetadata as IModelNameProvider)?.Name;
                    //    parameter.ParameterName = name.IfNullOrWhiteSpace(parameter.ParameterName);
                    //    optionalParameters.Add(name.IfNullOrWhiteSpace(parameter.ParameterName));
                    //}
                }

                //action.Properties.Add("OptionalParameters", optionalParameters);
            }
        }

        private bool IsConventionApplicable(ControllerModel controller)
        {
            return controller.Attributes.OfType<WebApiAttribute>().Any();
        }

        private static bool CanConvertFromString(Type destinationType)
        {
            destinationType = Nullable.GetUnderlyingType(destinationType) ?? destinationType;
            return IsSimpleType(destinationType) ||
                   TypeDescriptor.GetConverter(destinationType).CanConvertFrom(typeof(string));
        }

        private static bool IsSimpleType(Type type)
        {
            return type.GetTypeInfo().IsPrimitive ||
                type.Equals(typeof(decimal)) ||
                type.Equals(typeof(string)) ||
                type.Equals(typeof(DateTime)) ||
                type.Equals(typeof(Guid)) ||
                type.Equals(typeof(DateTimeOffset)) ||
                type.Equals(typeof(TimeSpan)) ||
                type.Equals(typeof(Uri));
        }
    }
}

