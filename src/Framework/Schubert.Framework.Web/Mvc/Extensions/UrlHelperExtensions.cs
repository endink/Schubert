using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Schubert.Framework.Web.Mvc
{
    public static class UrlHelperExtensions
    {
        /// <summary>
        /// 生成其他模块内的路由 Url。
        /// </summary>
        /// <param name="url"></param>
        /// <param name="action">action 名称。</param>
        /// <param name="controller">controller 名称（Controller 的类名移除 Controller 后缀）。</param>
        /// <param name="moduleName">模块名称（配置在 module.json 中的模块名）。</param>
        /// <param name="routeValues">路由参数。</param>
        public static string ModuleAction(this IUrlHelper url, string action, string controller, string moduleName, object routeValues = null)
        {
            return url.ModuleAction(
               action: action,
               controller: controller,
               moduleName: moduleName,
               routeValues: routeValues,
               protocol: null,
               host: null,
               fragment: null
               );
        }

        /// <summary>
        /// 生成其他模块内的路由 Url。
        /// </summary>
        /// <param name="url"></param>
        /// <param name="action">action 名称。</param>
        /// <param name="controller">controller 名称（Controller 的类名移除 Controller 后缀）。</param>
        /// <param name="moduleName">模块名称（配置在 module.json 中的模块名）。</param>
        /// <param name="routeValues">路由参数。</param>
        /// <param name="protocol">协议类型（例如：http，https）</param>
        public static string ModuleAction(this IUrlHelper url, string action, string controller, string moduleName, object routeValues, string protocol)
        {
            return url.ModuleAction(
                action: action,
                controller: controller,
                moduleName: moduleName,
                routeValues: routeValues,
                protocol: protocol,
                host: null,
                fragment: null
                );
        }

        /// <summary>
        /// 生成其他模块内的路由 Url。
        /// </summary>
        /// <param name="url"></param>
        /// <param name="action">action 名称。</param>
        /// <param name="controller">controller 名称（Controller 的类名移除 Controller 后缀）。</param>
        /// <param name="moduleName">模块名称（配置在 module.json 中的模块名）。</param>
        /// <param name="routeValues">路由参数。</param>
        /// <param name="protocol">协议类型（例如：http，https）</param>
        /// <param name="host">主机名。</param>
        public static string ModuleAction(this IUrlHelper url, string action, string controller, string moduleName, object routeValues, string protocol, string host)
        {
            return url.ModuleAction(
                action:action,
                controller:controller,
                moduleName:moduleName,
                routeValues:routeValues,
                protocol:protocol,
                host:host,
                fragment:null
                );
        }

        /// <summary>
        /// 生成其他模块内的路由 Url。
        /// </summary>
        /// <param name="url"></param>
        /// <param name="action">action 名称。</param>
        /// <param name="controller">controller 名称（Controller 的类名移除 Controller 后缀）。</param>
        /// <param name="moduleName">模块名称（配置在 module.json 中的模块名）。</param>
        /// <param name="routeValues">路由参数。</param>
        /// <param name="protocol">协议类型（例如：http，https）</param>
        /// <param name="host">主机名。</param>
        /// <param name="fragment">URL 锚点（#后面不的部分）。</param>
        /// <returns></returns>
        public static string ModuleAction(this IUrlHelper url, string action, string controller, string moduleName, object routeValues, string protocol, string host, string fragment)
        {
            RouteValueDictionary values = routeValues is RouteValueDictionary ? (RouteValueDictionary)routeValues : new RouteValueDictionary(routeValues);
            
            if (!moduleName.IsNullOrWhiteSpace())
            {
                values.Set(SchubertApplicationModeProvider.ModuleRouteKeyName, moduleName);
            }
            UrlActionContext actionContext = new UrlActionContext
            {
                Action = action?.Trim(),
                Controller = controller?.Trim(),
                Values = values,
                Protocol = protocol?.Trim(),
                Host = host?.Trim(),
                Fragment = fragment?.Trim()
            };
            return url.Action(actionContext); 
        }

        //public static string ModuleContent(this IUrlHelper url, string moduleName, string fileRelativePath)
        //{
        //    Guard.ArgumentIsRelativePath(fileRelativePath, nameof(fileRelativePath));
        //    return url.Content($"~/modulefiles/{moduleName.Replace(".", "/")}/{fileRelativePath}");
        //}
    }
}
