using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Schubert.Framework.Web.Mvc
{
    public class SchubertConstraintFactory : IActionConstraintFactory
    {
        private string _moduleName = null;
        public SchubertConstraintFactory(string moduleName)
        {
            Guard.ArgumentNullOrWhiteSpaceString(moduleName, nameof(moduleName));
            _moduleName = moduleName;
        }

        public bool IsReusable
        {
            get
            {
                return true;
            }
        }

        public IActionConstraint CreateInstance(IServiceProvider services)
        {
            return new ModuleRouteKeyRequired(_moduleName);
        }
    }

    public class ModuleRouteKeyRequired : IActionConstraint
    {
        private string _module = null;

        public ModuleRouteKeyRequired(string moduleName)
        {
            _module = moduleName;
        }

        public bool Accept(ActionConstraintContext context)
        {
            object module = null;
            if (context.RouteContext.RouteData.Values.TryGetValue(SchubertApplicationModeProvider.ModuleRouteKeyName, out module))
            {
                return _module.CaseSensitiveEquals(module as String);
            }
            return false;
        }

        public int Order
        {
            get
            {
                return int.MinValue;
            }
        }
    }
}
