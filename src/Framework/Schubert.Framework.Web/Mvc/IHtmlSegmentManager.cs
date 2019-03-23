using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Schubert.Framework.Web.Mvc
{
    internal interface IHtmlSegmentManager : IDependency
    {
        void AddScript(string scriptContent, string category = null);
        string GetAllScripts();

        String GetScriptCategory(string category = null);
    }
}
