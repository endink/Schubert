using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Schubert.Framework.Web
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple =false, Inherited = true)]
    public class WebApiAttribute : Attribute
    {
    }
}
