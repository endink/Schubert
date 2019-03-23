using Schubert.Framework.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Schubert.Framework.Services
{
    /// <summary>
    /// 用户服务。
    /// </summary>
    public interface IIdentityService
    {
        /// <summary>
        /// 根据用户 Id 查找用户。
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<UserBase> GetByIdAsync(long userId);

        /// <summary>
        /// 创建匿名用户，通常情况下匿名用户应该具有统一实例或同一用户名。
        /// </summary>
        /// <returns><see cref="UserBase"/>实例， 现实此接口的类在实现此方法时不应返回 null。</returns>
        UserBase CreateAnonymous();

        /// <summary>
        /// 获取当前登录用户，对于 Web 程序通常从 Cookie 来判断。
        /// </summary>
        /// <returns></returns>
        UserBase GetAuthenticatedUser();

        /// <summary>
        /// 刷新指定用户 Id 的用户（这通常表示刷新缓存中的用户存储）。
        /// </summary>
        /// <returns></returns>
        Task RefreshIdentityAsync(long userId);
    }
}
