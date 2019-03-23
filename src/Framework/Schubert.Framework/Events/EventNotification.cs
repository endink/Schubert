using Schubert.Framework.Environment.ShellBuilders;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Schubert.Framework.Environment;

namespace Schubert.Framework.Events
{
    public sealed class EventNotification : IEventNotification
    {
        private readonly ConcurrentDictionary<String, ObservableEvent> _events = null;
        private IServiceProvider _serviceProvider;
        public EventNotification(IServiceProvider serviceProvider)
        {
            Guard.ArgumentNotNull(serviceProvider, nameof(serviceProvider));

            _events = new ConcurrentDictionary<string, ObservableEvent>();
            _serviceProvider = serviceProvider;
            BuildEventSubscriptions(serviceProvider);
        }

        private void BuildEventSubscriptions(IServiceProvider serviceProvider)
        {
            ShellContext context = serviceProvider.GetRequiredService<ShellContext>();
            if (context != null)
            {
                foreach (var sub in context.EventSubscriptors)
                {
                    var @event = _events.GetOrAdd(sub.EventName, k => new ObservableEvent(serviceProvider));
                    @event.SubscribeCore(sub);
                }
            }
        }

        public void Notify(string eventName, object sender, object args)
        {
            ObservableEvent @event = null;
            if (_events.TryGetValue(eventName, out @event))
            {
                @event.Notify(sender, args);
            }
        }
    }
}
