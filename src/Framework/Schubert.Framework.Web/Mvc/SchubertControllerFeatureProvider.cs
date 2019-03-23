using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Schubert.Framework.Environment;
using Schubert.Framework.Environment.ShellBuilders;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Schubert.Framework.Web.Mvc
{
    public class SchubertControllerFeatureProvider : IApplicationFeatureProvider<ControllerFeature>
    {
        public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
        {
            ShellBlueprint blue = SchubertEngine.Current.GetRequiredService<ShellBlueprint>();

            foreach (var c in blue.Controllers)
            {
                feature.Controllers.Add(c.Type.GetTypeInfo());
            }
        }
    }
}
