using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using Schubert.Framework.Environment;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

namespace Schubert.Framework.Web.Mvc
{
    public static class ControllerExtensions
    {
        public static string GetUrlFromExpression<TController>(this TController controller, Expression<Action<TController>> action)
            where TController : ControllerBase
        {
            RouteValueDictionary dir = GetRouteValuesFromExpression<TController>(action);
            string url = controller.Url.Action(null, dir);

            return url;
        }

        public static string GetUrlFromExpression<TController>(this TController controller, Expression<Func<TController, Task<IActionResult>>> action)
            where TController : ControllerBase
        {
            RouteValueDictionary dir = GetRouteValuesFromExpression<TController>(action);
            string url = controller.Url.Action(null, dir);

            return url;
        }

        private static RouteValueDictionary GetRouteValuesFromExpression<TController>(LambdaExpression action) where TController : ControllerBase
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }
            MethodCallExpression methodCallExpression = action.Body as MethodCallExpression;
            if (methodCallExpression == null)
            {
                throw new ArgumentException("表达式必须是一个 Action 方法调用。", nameof(action));
            }
            string text = typeof(TController).Name;
            if (!text.EndsWith("Controller", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException($"{typeof(TController).Name} 类型不符合命名约定， Controller 类型必须具有 Controller 后缀。", nameof(TController));
            }
            text = text.Substring(0, text.Length - "Controller".Length);
            if (text.Length == 0)
            {
                throw new ArgumentException($"{typeof(TController).Name} 类型不符合命名约定， Controller 类型必须具有 Controller 后缀， 切必须有前缀字符。", nameof(TController));
            }
            string targetActionName = GetTargetActionName(methodCallExpression.Method);
            RouteValueDictionary routeValueDictionary = new RouteValueDictionary();
            routeValueDictionary.Add("controller", text);
            routeValueDictionary.Add("action", targetActionName);
            //ActionLinkAreaAttribute actionLinkAreaAttribute = typeof(TController).GetCustomAttributes(typeof(ActionLinkAreaAttribute), true).FirstOrDefault<object>() as ActionLinkAreaAttribute;
            //if (actionLinkAreaAttribute != null)
            //{
            //    string area = actionLinkAreaAttribute.Area;
            //    routeValueDictionary.Add("area", area);
            //}
            AddParameterValuesFromExpressionToDictionary(routeValueDictionary, methodCallExpression);
            return routeValueDictionary;
        }

        private static void AddParameterValuesFromExpressionToDictionary(RouteValueDictionary rvd, MethodCallExpression call)
        {
            ParameterInfo[] parameters = call.Method.GetParameters();
            if (parameters.Length > 0)
            {
                for (int i = 0; i < parameters.Length; i++)
                {
                    Expression expression = call.Arguments[i];
                    object value = null;
                    ConstantExpression constantExpression = expression as ConstantExpression;
                    if (constantExpression != null)
                    {
                        value = constantExpression.Value;
                    }
                    else
                    {
                        Expression<Func<object, object>> lambdaExpression = Expression.Lambda<Func<object, object>>(Expression.Convert(expression, typeof(object)), new ParameterExpression[]
                        {
                            Expression.Parameter(typeof(object), "_nullParam")
                        });
                        value = lambdaExpression.Compile().Invoke(null);
                    }
                    rvd.Add(parameters[i].Name, value);
                }
            }
        }

        private static string GetTargetActionName(MethodInfo methodInfo)
        {
            string name = methodInfo.Name;
            if (methodInfo.IsDefined(typeof(NonActionAttribute), true))
            {
                throw new InvalidOperationException(String.Format("The method {0} is not a action.", methodInfo.Name));
            }
            ActionNameAttribute actionNameAttribute = methodInfo.GetCustomAttributes(typeof(ActionNameAttribute), true).OfType<ActionNameAttribute>().FirstOrDefault<ActionNameAttribute>();
            if (actionNameAttribute != null)
            {
                return actionNameAttribute.Name;
            }
            return name;
        }


        //public async static Task<String> RenderPartialViewStringAsync(this Controller controller, object model, string viewName = null)
        //{
        //    if (string.IsNullOrEmpty(viewName))
        //        viewName = controller.RouteData.Values["action"].ToString();

        //    var options = controller.HttpContext.RequestServices.GetService<IOptions<HtmlHelperOptions>>();


        //    var ve = controller.Request.HttpContext.RequestServices.GetRequiredService<IViewEngine>();
        //    ViewEngineResult result = ve.FindView(
        //        new ActionContext(controller.HttpContext, controller.RouteData, controller.ControllerContext.ActionDescriptor, controller.ModelState), 
        //        viewName, false);
        //    if (result == null)
        //    {
        //        return String.Empty;
        //    }
        //    using (StringWriter sw = new StringWriter())
        //    {
        //        controller.ViewData.Model = model;
        //        ViewContext viewContext = new ViewContext(controller.ActionContext, result.View, controller.ViewData, controller.TempData, sw, options.Value);
        //        await result.View.RenderAsync(viewContext);
        //        return sw.GetStringBuilder().ToString();
        //    }
        //}

        public static RedirectToRouteResult RedirectToAction<TController>(this TController controller, Expression<Action<TController>> action) 
            where TController : ControllerBase
        {
            if (controller == null)
            {
                throw new ArgumentNullException("controller");
            }
            RouteValueDictionary routeValuesFromExpression = GetRouteValuesFromExpression<TController>(action);
            return new RedirectToRouteResult(routeValuesFromExpression);
        }

        public static RedirectToRouteResult RedirectToAction<TController>(this TController controller, Expression<Func<TController, Task<IActionResult>>> action) 
            where TController : ControllerBase
        {
            if (controller == null)
            {
                throw new ArgumentNullException("controller");
            }
            RouteValueDictionary routeValuesFromExpression = GetRouteValuesFromExpression<TController>(action);
            return new RedirectToRouteResult(routeValuesFromExpression);
        }

        public static IActionResult HandldError(this Controller controller, string viewName = "Error", Func<Exception, Exception> handler = null)
        {
            var feature = controller.HttpContext.Features.Get<IExceptionHandlerFeature>();
            var error = feature?.Error;
            if (error == null)
            {
                return controller.NoContent();
            }
            if (handler != null)
            {
                error = handler(error);
            }
            var target = controller.HttpContext.Features.Get<IHttpRequestFeature>();
            var logger = controller.HttpContext.RequestServices.GetService<ILoggerFactory>().CreateLogger("UnhandledException");

            controller.Request.Headers.TryGetValue(HeaderNames.Referer, out StringValues referrer);

            logger.WriteError(new EventId(0), "应用程序发生未处理的异常。",
                error.GetOriginalException(), new
                {
                    RequestPath = target?.RawTarget,
                    HttpMethod = target?.Method,
                    HttpReferrer = referrer.ToString()
                });

            var message = SchubertEngine.Current.IsDevelopmentEnvironment.IfNull(false) ?
                $"服务端调用发生错误。{System.Environment.NewLine}{error.GetOriginalException().Message}" :
                 "服务器出错了。";

            if (!controller.Request.IsAjaxRequest())
            {
                return controller.View(viewName, message);
            }

            return SchubertEngine.Current.IsDevelopmentEnvironment.IfNull(false)
                ? (IActionResult)controller.Content(message)
                : controller.StatusCode(500);
        }

        public static IActionResult HandldError(this SchubertApiController controller, Func<Exception, Exception> handler = null)
        {
            var feature = controller.HttpContext.Features.Get<IExceptionHandlerFeature>();
            var error = feature?.Error;
            if (error == null) return controller.Ok();
            if (handler != null) error = handler(error);
            var target = controller.HttpContext.Features.Get<IHttpRequestFeature>();
            var logger = controller.HttpContext.RequestServices.GetService<ILoggerFactory>().CreateLogger("UnhandledException");
            //var referrer = controller.Request.Headers.Referrer;
            logger.WriteError(new EventId(0), "应用程序发生未处理的异常。",
                error.GetOriginalException(), new
                {
                    RequestPath = target?.RawTarget,
                    HttpMethod = target?.Method,
                    //HttpReferrer = referrer.ToString()
                });
            var message = SchubertEngine.Current.IsDevelopmentEnvironment.IfNull(false) ?
                $"服务端调用发生错误。{System.Environment.NewLine}{error.GetOriginalException().Message}" :
                "服务器出错了。";
            return SchubertEngine.Current.IsDevelopmentEnvironment.IfNull(false)
                ? (IActionResult)controller.Ok(message)
                : controller.StatusCode((int)HttpStatusCode.InternalServerError);
        }

    }
}
