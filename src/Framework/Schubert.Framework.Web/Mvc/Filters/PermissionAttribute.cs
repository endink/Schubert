using Microsoft.AspNetCore.Mvc;
using Schubert.Framework.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.DependencyInjection;
using Schubert.Framework.Environment;
using Schubert.Framework.Services;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Schubert.Framework.Web.Mvc
{
    /// <summary>
    /// 标识操作需要获得授权。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public class PermissionAttribute : ActionFilterAttribute
    {
        public PermissionAttribute(string permissionName)
        {
            Guard.ArgumentNullOrWhiteSpaceString(permissionName, nameof(permissionName));
            this.PermiasionName = permissionName.Trim();
        }

        /// <summary>
        /// 获取授权名称。
        /// </summary>
        public string PermiasionName { get; private set; }

        /// <summary>
        /// 获取或设置一值，指示在没有授权时是否显示404（页面未找到），如果为 false， 将使用默认的未授权页，出于安全考虑 Web 应用应该屏蔽授权提示，开发API时才使用未授权返回，此值默认为 true。
        /// </summary>
        public bool Use404HasNoPermission { get; set; } = true;

        private IActionResult CreateInvalidResult()
        {
            return this.Use404HasNoPermission ? (IActionResult)(new NotFoundResult()) : (IActionResult)(new UnauthorizedResult());
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            IIdentityService svc = context.HttpContext.RequestServices.GetRequiredService<IIdentityService>();
            var user = svc.GetAuthenticatedUser();
            if (user == null)
            {
                context.Result = this.CreateInvalidResult();
            }
            else
            {
                IPermissionService permissionSvc = context.HttpContext.RequestServices.GetRequiredService<IPermissionService>();
                if (!(await permissionSvc.HasPermissionAsync(PermiasionName, user.Id)))
                {
                    context.Result = this.CreateInvalidResult();
                }
            }
            await base.OnActionExecutionAsync(context, next);
        }
    }
}
