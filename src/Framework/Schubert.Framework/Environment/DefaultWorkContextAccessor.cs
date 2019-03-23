using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace Schubert.Framework.Environment
{
    public class DefaultWorkContextAccessor : IWorkContextAccessor
    {
        private IEnumerable<IWorkContextProvider> _contextProviders = null;
        private readonly object SyncRoot = new object();

        private IWorkContextProvider _lastProvider = null;

        public DefaultWorkContextAccessor(IEnumerable<IWorkContextProvider> contextProviders)
        {
            contextProviders = contextProviders ?? Enumerable.Empty<IWorkContextProvider>();
            _contextProviders = contextProviders.OrderByDescending(cp => cp.Priority).ToArray();
        }

        public WorkContext GetContext()
        {
            //基于这样一个假设：
            //上次访问的 ContextProvider 会有更大的几率被下一次调用访问，冷热数据原理，热的更热，所以缓存一下。
            WorkContext context = null;
            if ((context = _lastProvider?.GetWorkContext()) == null)
            {
                lock(SyncRoot)
                {
                    if ((context = _lastProvider?.GetWorkContext()) == null)
                    {
                        //热数据转冷数据，可以切换 ContextProvider 了
                        _lastProvider = null;
                        foreach (var p in _contextProviders)
                        {
                            context = p.GetWorkContext();
                            if (context != null)
                            {
                                _lastProvider = p;
                                break;
                            }
                        }
                    }
                }
            }
            return context;
        }
    }
}
