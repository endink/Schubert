using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Schubert.Framework.DependencyInjection
{
   public static class ServiceDescriber
	{
		public static SmartServiceDescriptor Transient<TService, TImplementation>(SmartOptions options = SmartOptions.TryAdd) where TImplementation : TService
		{
			return Describe<TService, TImplementation>(ServiceLifetime.Transient, options);
		}
		public static SmartServiceDescriptor Transient(Type service, Type implementationType, SmartOptions options = SmartOptions.TryAdd)
		{
            Guard.ArgumentNotNull(service, nameof(service));
            Guard.ArgumentNotNull(implementationType, nameof(implementationType));

			return Describe(service, implementationType, ServiceLifetime.Transient, options);
		}
		public static SmartServiceDescriptor Transient<TService>(Func<IServiceProvider, TService> implementationFactory, SmartOptions options = SmartOptions.TryAdd) 
            where TService : class
		{
            Guard.ArgumentNotNull(implementationFactory, nameof(implementationFactory));

			return Describe(typeof(TService), implementationFactory, ServiceLifetime.Transient);
		}
		public static SmartServiceDescriptor Transient(Type service, Func<IServiceProvider, object> implementationFactory, SmartOptions options = SmartOptions.TryAdd)
		{
            Guard.ArgumentNotNull(service, nameof(service));
            Guard.ArgumentNotNull(implementationFactory, nameof(implementationFactory));

            return Describe(service, implementationFactory, ServiceLifetime.Transient, options);
		}
		public static SmartServiceDescriptor Scoped<TService, TImplementation>(SmartOptions options = SmartOptions.TryAdd) where TImplementation : TService
		{
			return Describe<TService, TImplementation>(ServiceLifetime.Scoped, options);
		}
		public static SmartServiceDescriptor Scoped(Type service, Type implementationType, SmartOptions options = SmartOptions.TryAdd)
		{
			return Describe(service, implementationType, ServiceLifetime.Scoped, options);
		}
		public static SmartServiceDescriptor Scoped<TService>(Func<IServiceProvider, TService> implementationFactory, SmartOptions options = SmartOptions.TryAdd) where TService : class
		{
            Guard.ArgumentNotNull(implementationFactory, nameof(implementationFactory));

            return Describe(typeof(TService), implementationFactory, ServiceLifetime.Scoped, options);
		}
		public static SmartServiceDescriptor Scoped(Type service,  Func<IServiceProvider, object> implementationFactory, SmartOptions options = SmartOptions.TryAdd)
		{
            Guard.ArgumentNotNull(service, nameof(service));
            Guard.ArgumentNotNull(implementationFactory, nameof(implementationFactory));

            return Describe(service, implementationFactory, ServiceLifetime.Scoped, options);
		}
		public static SmartServiceDescriptor Singleton<TService, TImplementation>(SmartOptions options = SmartOptions.TryAdd) 
            where TImplementation : TService
		{
			return Describe<TService, TImplementation>(ServiceLifetime.Singleton, options);
		}
		public static SmartServiceDescriptor Singleton(Type service, Type implementationType, SmartOptions options = SmartOptions.TryAdd)
		{
			return Describe(service, implementationType, ServiceLifetime.Singleton, options);
		}
		public static SmartServiceDescriptor Singleton<TService>(Func<IServiceProvider, TService> implementationFactory, SmartOptions options = SmartOptions.TryAdd) where TService : class
		{
            Guard.ArgumentNotNull(implementationFactory, nameof(implementationFactory));

            return Describe(typeof(TService), implementationFactory, ServiceLifetime.Singleton, options);
		}
		public static SmartServiceDescriptor Singleton(Type serviceType, Func<IServiceProvider, object> implementationFactory, SmartOptions options)
		{
            Guard.ArgumentNotNull(serviceType, nameof(serviceType));
            Guard.ArgumentNotNull(implementationFactory, nameof(implementationFactory));

            return Describe(serviceType, implementationFactory, ServiceLifetime.Singleton, options);
		}
		public static SmartServiceDescriptor Instance<TService>(TService implementationInstance, SmartOptions options = SmartOptions.TryAdd)
		{
            Guard.ArgumentNotNull(implementationInstance, nameof(implementationInstance));

            return Instance(typeof(TService), implementationInstance, options);
		}
		public static SmartServiceDescriptor Instance(Type serviceType, object implementationInstance, SmartOptions options = SmartOptions.TryAdd)
		{
            Guard.ArgumentNotNull(implementationInstance, nameof(implementationInstance));
            
			return new SmartServiceDescriptor(serviceType, implementationInstance) { Options = options };
		}
		private static SmartServiceDescriptor Describe<TService, TImplementation>(ServiceLifetime lifetime, SmartOptions options = SmartOptions.TryAdd)
		{
			return Describe(typeof(TService), typeof(TImplementation), lifetime, options);
		}
		public static SmartServiceDescriptor Describe(Type serviceType, Type implementationType, 
            ServiceLifetime lifecycle, SmartOptions options = SmartOptions.TryAdd)
		{
			return new SmartServiceDescriptor(serviceType, implementationType, lifecycle) { Options = options };
		}

        public static SmartServiceDescriptor Describe(Type serviceType, Func<IServiceProvider, object> implementationFactory, 
            ServiceLifetime lifecycle, SmartOptions options = SmartOptions.TryAdd)
        {
            return new SmartServiceDescriptor(serviceType, implementationFactory, lifecycle) { Options = options };
        }
    }
}
