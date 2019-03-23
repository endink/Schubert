using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Schubert.Framework.Web.Mvc
{
    /// <summary>
    /// 当  <see cref="Controller"/> 设置了多个 <see cref="PermissionAttribute"/> 时，
    /// 在 Action 上应用此属性可以忽略指定名称的授权。要忽略所有授权应使用 <see cref="Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute"/>。
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class IgnorePermissionAttribute : Attribute
    {
        public IgnorePermissionAttribute(string permissionName)
        {
            Guard.ArgumentNullOrWhiteSpaceString(permissionName, nameof(permissionName));
            this.PermissionName = permissionName.Trim();
        }

        public string PermissionName { get; private set; }
    }
}
