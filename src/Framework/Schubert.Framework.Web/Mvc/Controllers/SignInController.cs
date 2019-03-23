using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Schubert.Framework.Web.Mvc.Controllers
{
    /// <summary>
    /// 集成第三方登陆的能力的 Controller。
    /// </summary>
    /// <typeparam name="TUser"></typeparam>
    public abstract class SignInController<TUser> : SchubertController
        where TUser : class
    {
        private Lazy<SignInManager<TUser>> _signInManager = null;

        public SignInController()
        {
            this._signInManager = new Lazy<SignInManager<TUser>>(() =>
            this.HttpContext.RequestServices.GetRequiredService<SignInManager<TUser>>());
        }

        /// <summary>
        /// 获取或设置 <see cref="SignInManager"/> 实例。
        /// </summary>
        protected SignInManager<TUser> SignInManager
        {
            get { return _signInManager.Value; }
        }

        [HttpPost("[action]")]
        public IActionResult ExternalLogin(string provider, string returnUrl = null)
        {
            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), new { returnUrl = returnUrl });
            var properties = this.SignInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return new ChallengeResult(provider, properties);
        }

        private async Task<ExternalLoginInfo> GetExternalLoginInfoAsync()
        {
            var info = await this.SignInManager.GetExternalLoginInfoAsync();
            if (info != null)
            {
                if (info.ProviderDisplayName.IsNullOrWhiteSpace()) //微软BUG？没有带出 DisplayName
                {
                    var schemas = await this.SignInManager.GetExternalAuthenticationSchemesAsync();
                    var p = schemas.FirstOrDefault(s => s.Name.CaseInsensitiveEquals(info.LoginProvider));
                    info.ProviderDisplayName = p?.DisplayName;
                }
            }
            return info;
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (!returnUrl.IsNullOrWhiteSpace() && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return Redirect("~/");
            }
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null)
        {
            var info = await this.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return await this.ShowExternalSignInErrorAsync(OAuthSignInError.InvalidRequest);
            }
            if (info.ProviderDisplayName.IsNullOrWhiteSpace()) //微软BUG？没有带出 DisplayName
            {
                var schemas = await this.SignInManager.GetExternalAuthenticationSchemesAsync();
                var p = schemas.FirstOrDefault(s => s.Name.CaseInsensitiveEquals(info.LoginProvider));
                info.ProviderDisplayName = p?.DisplayName;
            }
            Microsoft.AspNetCore.Identity.SignInResult result = await ExternalLoginSignInAsync(info);
            if (result.Succeeded)
            {
                return this.RedirectToLocal(returnUrl);
            }
            if (result.IsLockedOut)
            {
                return await this.ShowExternalSignInErrorAsync(OAuthSignInError.UserLockedOut);
            }
            else if (result.IsNotAllowed)
            {
                return await this.ShowExternalSignInErrorAsync(OAuthSignInError.NotAllowed);
            }
            else
            {
                var loginModel =
                     new OAuthSignedInModel
                     {
                         LoginProvider = info.LoginProvider,
                         ProviderDisplayName = info.ProviderDisplayName,
                         ProviderKey = info.ProviderKey,
                         ThirdPartUserName = info.Principal.FindFirst(ClaimTypes.Name)?.Value
                     };
                await this.SignInManager.SignOutAsync();
                return await this.ShowBindExternalAccountAsync(loginModel);
            }
        }

        /// <summary>
        /// 派生类中重写时表示对第三方登陆进行验证。 
        /// 默认使用 <see cref="SignInManager{TUser}.ExternalLoginSignInAsync(string, string, bool)"/> 方法进行登陆。
        /// <c>当返回值为 <see cref="Microsoft.AspNetCore.Identity.SignInResult.RequiresTwoFactor"/>时表示本地用户未绑定。</c>
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        protected virtual Task<Microsoft.AspNetCore.Identity.SignInResult> ExternalLoginSignInAsync(ExternalLoginInfo info)
        {
            // Sign in the user with this external login provider if the user already has a login.
            return this.SignInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false);
        }
        
        /// <summary>
        /// 派生类中重写时表示显示第三方登陆错误的结果。
        /// </summary>
        /// <param name="error">表示第三方登陆错误的枚举。</param>
        /// <returns></returns>
        protected abstract Task<IActionResult> ShowExternalSignInErrorAsync(OAuthSignInError error);

        /// <summary>
        /// 表示返回对第三方账号进行绑定的错操的结果。
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        protected abstract Task<IActionResult> ShowBindExternalAccountAsync(OAuthSignedInModel model);
    }
}
