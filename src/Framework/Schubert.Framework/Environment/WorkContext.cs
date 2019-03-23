using Schubert.Framework.Domain;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Schubert.Framework.Environment
{
    [DebuggerDisplay("WorkContext: U = {CurrentUser.UserName}; L = {CurrentLanguage}; T = {CurrentTimeZone.Id}")]
    public abstract class WorkContext : IDisposable
    {
        public const string CurrentUserStateName = "CurrentUser";
        public const string CurrentLanguageStateName = "CurrentLanguage";
        public const string CurrentTimeZoneState = "CurrentTimeZone";
        private IEnumerable<IWorkContextStateProvider> _workContextStateProviders;
        private readonly ConcurrentDictionary<string, Object> _stateResolvers;
        private bool _disposed;

        public WorkContext()
        {
            _stateResolvers = new ConcurrentDictionary<string, Object>();
        }

        protected abstract IEnumerable<IWorkContextStateProvider> GetStateProviders();

        private IEnumerable<IWorkContextStateProvider> Providers
        {
            get { return _workContextStateProviders ?? (_workContextStateProviders = this.GetStateProviders() ?? Enumerable.Empty<IWorkContextStateProvider>()); }
        }

        /// <summary>
        /// 获取一个已经注册的依赖项。
        /// </summary>
        /// <typeparam name="T">依赖项的类型。</typeparam>
        /// <returns>如果获取成功，返回依赖项实例；否则，返回 default(T)。</returns>
        public T Resolve<T>()
        {
            return (T)this.Resolve(typeof(T));
        }
        /// <summary>
        /// 获取一个已经注册的依赖项，如果获取失败，抛出异常。
        /// </summary>
        /// <typeparam name="T">依赖项的类型。</typeparam>
        /// <returns>返回依赖项实例。</returns>
        public T ResolveRequired<T>()
        {
            return (T)this.ResolveRequired(typeof(T));
        }

        /// <summary>
        /// 获取一个已经注册的依赖项。
        /// </summary>
        /// <param name="type">依赖项的类型。</param>
        /// <returns>如果获取成功，返回依赖项实例；否则，返回 null。</returns>
        public abstract object Resolve(Type type);

        /// <summary>
        /// 获取一个已经注册的依赖项。
        /// </summary>
        /// <param name="type">依赖项的类型。</param>
        /// <returns>如果获取成功，返回依赖项实例；否则抛出异常。</returns>
        public abstract object ResolveRequired(Type type);

        public virtual T GetState<T>(string name)
        {
            var factory = this.FindResolverForState<T>(name);
            var state = _stateResolvers.GetOrAdd(name, k=>factory());
            return (T)state;
        }

        private Func<object> FindResolverForState<T>(string name)
        {
            var resolver = this.Providers.Select(wcsp => wcsp.Get(name)).FirstOrDefault(value => value != null);

            if (resolver == null)
            {
                return () => default(T);
            }
            return () => resolver(this);
        }


        public virtual void SetState<T>(string name, T value)
        {
            _stateResolvers[name] = value;
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        ///<summary>
        /// 必须，以备程序员忘记了显式调用Dispose方法
        ///</summary>
        ~WorkContext()
        {
            //必须为false
            Dispose(false);
        }


        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }
            _disposed = true;
            if (disposing)
            {
                var providers = _stateResolvers.Values.OfType<IDisposable>().ToArray();
                foreach (var p in providers)
                {
                    p.Dispose();
                }
            }
        }


        /// <summary>
        /// 获取或设置当前用户。
        /// </summary>
        public UserBase CurrentUser
        {
            get { return GetState<UserBase>(CurrentUserStateName); }
            set { SetState(CurrentUserStateName, value); }
        }

        /// <summary>
        /// 获取或设置当前上下文的语言。
        /// </summary>
        public Language CurrentLanguage
        {
            get { return GetState<Language>(CurrentLanguageStateName); }
            set { SetState(CurrentLanguageStateName, value); }
        }

        ///// <summary>
        ///// 获取或设置当前上下文中启用的日历。
        ///// </summary>
        //public Calendar CurrentCalendar
        //{
        //    get { return GetState<Calendar>("CurrentCalendar"); }
        //    set { SetState("CurrentCalendar", value); }
        //}

        /// <summary>
        /// 获取或设置当前上下文中的时区。
        /// </summary>
        public TimeZoneInfo CurrentTimeZone
        {
            get { return GetState<TimeZoneInfo>(CurrentTimeZoneState); }
            set { SetState(CurrentTimeZoneState, value); }
        }
    }
}