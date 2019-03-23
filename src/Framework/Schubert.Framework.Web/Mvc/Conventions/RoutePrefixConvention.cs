using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Schubert.Framework.Web.Mvc.Conventions
{
    public class RoutePrefixConvention : IApplicationModelConvention
    {
        private readonly AttributeRouteModel _routePrefix;

        public RoutePrefixConvention(IRouteTemplateProvider routeTemplateProvider)
        {
            _routePrefix = new AttributeRouteModel(routeTemplateProvider);
        }

        // Interface Apply Method 
        public void Apply(ApplicationModel application)
        {
            // Traverse all  Controller
            foreach (var controller in application.Controllers)
            {
                //  Already marked  RouteAttribute  The  Controller
                var matchedSelectors = controller.Selectors.Where(x => x.AttributeRouteModel != null).ToList();
                if (matchedSelectors.Any())
                {
                    foreach (var selectorModel in matchedSelectors)
                    {
                        //  stay   Current routing   again   Add one   Routing prefix 
                        selectorModel.AttributeRouteModel = AttributeRouteModel.CombineAttributeRouteModel(_routePrefix,
                         selectorModel.AttributeRouteModel);
                    }
                }

                //  No mark  RouteAttribute  The  Controller
                var unmatchedSelectors = controller.Selectors.Where(x => x.AttributeRouteModel == null).ToList();
                if (unmatchedSelectors.Any())
                {
                    foreach (var selectorModel in unmatchedSelectors)
                    {
                        //  Add one   Routing prefix 
                        selectorModel.AttributeRouteModel = _routePrefix;
                    }
                }
            }
        }
    }
}
