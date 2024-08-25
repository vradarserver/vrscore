// Copyright © 2024 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.


using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace VirtualRadar.Extensions
{
    /// <summary>
    /// Extends System.IServiceProvider.
    /// </summary>
    public static class IServiceProviderExtensions
    {
        public static IServiceCollection AddKeyedLifetime
        (
            this IServiceCollection services,
            Type serviceType,
            object serviceKey,
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type implementationType
        )
        {
            switch(GetLifetime(serviceType)) {
                case Lifetime.Scoped:       services.AddKeyedScoped     (serviceType, serviceKey, implementationType); break;
                case Lifetime.Singleton:    services.AddKeyedSingleton  (serviceType, serviceKey, implementationType); break;
                case Lifetime.Transient:    services.AddKeyedTransient  (serviceType, serviceKey, implementationType); break;
                default:                    throw new NotImplementedException();
            }
            return services;
        }

        public static IServiceCollection AddKeyedLifetime
        (
            this IServiceCollection services,
            Type serviceType,
            object serviceKey,
            Func<IServiceProvider, object, object> implementationFactory
        )
        {
            switch(GetLifetime(serviceType)) {
                case Lifetime.Scoped:       services.AddKeyedScoped     (serviceType, serviceKey, implementationFactory); break;
                case Lifetime.Singleton:    services.AddKeyedSingleton  (serviceType, serviceKey, implementationFactory); break;
                case Lifetime.Transient:    services.AddKeyedTransient  (serviceType, serviceKey, implementationFactory); break;
                default:                    throw new NotImplementedException();
            }
            return services;
        }

        public static IServiceCollection AddKeyedLifetime<
            TService,
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TImplementation
        >
        (
            this IServiceCollection services,
            object serviceKey
        )
            where TService : class
            where TImplementation : class, TService
        {
            switch(GetLifetime<TService>()) {
                case Lifetime.Scoped:       services.AddKeyedScoped     <TService, TImplementation>(serviceKey); break;
                case Lifetime.Singleton:    services.AddKeyedSingleton  <TService, TImplementation>(serviceKey); break;
                case Lifetime.Transient:    services.AddKeyedTransient  <TService, TImplementation>(serviceKey); break;
                default:                    throw new NotImplementedException();
            }
            return services;
        }

        public static IServiceCollection AddKeyedLifetime
        (
            this IServiceCollection services,
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type serviceType,
            object serviceKey
        )
        {
            switch(GetLifetime(serviceType)) {
                case Lifetime.Scoped:       services.AddKeyedScoped     (serviceType, serviceKey); break;
                case Lifetime.Singleton:    services.AddKeyedSingleton  (serviceType, serviceKey); break;
                case Lifetime.Transient:    services.AddKeyedTransient  (serviceType, serviceKey); break;
                default:                    throw new NotImplementedException();
            }
            return services;
        }

        public static IServiceCollection AddKeyedLifetime<
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TService
        >
        (
            this IServiceCollection services,
            object serviceKey
        )
            where TService : class
        {
            switch(GetLifetime<TService>()) {
                case Lifetime.Scoped:       services.AddKeyedScoped     <TService>(serviceKey); break;
                case Lifetime.Singleton:    services.AddKeyedSingleton  <TService>(serviceKey); break;
                case Lifetime.Transient:    services.AddKeyedTransient  <TService>(serviceKey); break;
                default:                    throw new NotImplementedException();
            }
            return services;
        }

        public static IServiceCollection AddKeyedLifetime<TService>
        (
            this IServiceCollection services,
            object serviceKey,
            Func<IServiceProvider, object, TService> implementationFactory
        )
            where TService : class
        {
            switch(GetLifetime<TService>()) {
                case Lifetime.Scoped:       services.AddKeyedScoped     <TService>(serviceKey, implementationFactory); break;
                case Lifetime.Singleton:    services.AddKeyedSingleton  <TService>(serviceKey, implementationFactory); break;
                case Lifetime.Transient:    services.AddKeyedTransient  <TService>(serviceKey, implementationFactory); break;
                default:                    throw new NotImplementedException();
            }
            return services;
        }

        public static IServiceCollection AddKeyedLifetime<TService, TImplementation>
        (
            this IServiceCollection services,
            object serviceKey,
            Func<IServiceProvider, object, TImplementation> implementationFactory
        )
            where TService : class
            where TImplementation : class, TService
        {
            switch(GetLifetime<TService>()) {
                case Lifetime.Scoped:       services.AddKeyedScoped     <TService, TImplementation>(serviceKey, implementationFactory); break;
                case Lifetime.Singleton:    services.AddKeyedSingleton  <TService, TImplementation>(serviceKey, implementationFactory); break;
                case Lifetime.Transient:    services.AddKeyedTransient  <TService, TImplementation>(serviceKey, implementationFactory); break;
                default:                    throw new NotImplementedException();
            }
            return services;
        }

        public static IServiceCollection AddLifetime
        (
            this IServiceCollection services,
            Type serviceType,
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type implementationType
        )
        {
            switch(GetLifetime(serviceType)) {
                case Lifetime.Scoped:       services.AddScoped      (serviceType, implementationType); break;
                case Lifetime.Singleton:    services.AddSingleton   (serviceType, implementationType); break;
                case Lifetime.Transient:    services.AddTransient   (serviceType, implementationType); break;
                default:                    throw new NotImplementedException();
            }
            return services;
        }

        public static IServiceCollection AddLifetime(
            this IServiceCollection services,
            Type serviceType,
            Func<IServiceProvider, object> implementationFactory
        )
        {
            switch(GetLifetime(serviceType)) {
                case Lifetime.Scoped:       services.AddScoped      (serviceType, implementationFactory); break;
                case Lifetime.Singleton:    services.AddSingleton   (serviceType, implementationFactory); break;
                case Lifetime.Transient:    services.AddTransient   (serviceType, implementationFactory); break;
                default:                    throw new NotImplementedException();
            }
            return services;
        }

        public static IServiceCollection AddLifetime
        <
            TService,
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TImplementation
        >
        (this IServiceCollection services)
            where TService : class
            where TImplementation : class, TService
        {
            switch(GetLifetime<TService>()) {
                case Lifetime.Scoped:       services.AddScoped      <TService, TImplementation>(); break;
                case Lifetime.Singleton:    services.AddSingleton   <TService, TImplementation>(); break;
                case Lifetime.Transient:    services.AddTransient   <TService, TImplementation>(); break;
                default:                    throw new NotImplementedException();
            }
            return services;
        }

        public static IServiceCollection AddLifetime(
            this IServiceCollection services,
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type serviceType
        )
        {
            switch(GetLifetime(serviceType)) {
                case Lifetime.Scoped:       services.AddScoped      (serviceType); break;
                case Lifetime.Singleton:    services.AddSingleton   (serviceType); break;
                case Lifetime.Transient:    services.AddTransient   (serviceType); break;
                default:                    throw new NotImplementedException();
            }
            return services;
        }

        public static IServiceCollection AddLifetime<
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TService
        >
        (this IServiceCollection services)
            where TService : class
        {
            switch(GetLifetime<TService>()) {
                case Lifetime.Scoped:       services.AddScoped      <TService>(); break;
                case Lifetime.Singleton:    services.AddSingleton   <TService>(); break;
                case Lifetime.Transient:    services.AddTransient   <TService>(); break;
                default:                    throw new NotImplementedException();
            }
            return services;
        }

        public static IServiceCollection AddLifetime<TService>
        (
            this IServiceCollection services,
            Func<IServiceProvider, TService> implementationFactory
        )
            where TService : class
        {
            switch(GetLifetime<TService>()) {
                case Lifetime.Scoped:       services.AddScoped      <TService>(implementationFactory); break;
                case Lifetime.Singleton:    services.AddSingleton   <TService>(implementationFactory); break;
                case Lifetime.Transient:    services.AddTransient   <TService>(implementationFactory); break;
                default:                    throw new NotImplementedException();
            }
            return services;
        }

        public static IServiceCollection AddLifetime<TService, TImplementation>
        (
            this IServiceCollection services,
            Func<IServiceProvider, TImplementation> implementationFactory
        )
            where TService : class
            where TImplementation : class, TService
        {
            switch(GetLifetime<TService>()) {
                case Lifetime.Scoped:       services.AddScoped      <TService, TImplementation>(implementationFactory); break;
                case Lifetime.Singleton:    services.AddSingleton   <TService, TImplementation>(implementationFactory); break;
                case Lifetime.Transient:    services.AddTransient   <TService, TImplementation>(implementationFactory); break;
                default:                    throw new NotImplementedException();
            }
            return services;
        }

        public static void TryAddKeyedLifetime
        (
            this IServiceCollection collection,
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type service,
            object serviceKey
        )
        {
            switch(GetLifetime(service)) {
                case Lifetime.Scoped:       collection.TryAddKeyedScoped      (service, serviceKey); break;
                case Lifetime.Singleton:    collection.TryAddKeyedSingleton   (service, serviceKey); break;
                case Lifetime.Transient:    collection.TryAddKeyedTransient   (service, serviceKey); break;
                default:                    throw new NotImplementedException();
            }
        }

        public static void TryAddKeyedLifetime
        (
            this IServiceCollection collection,
            Type service,
            object serviceKey,
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type implementationType
        )
        {
            switch(GetLifetime(service)) {
                case Lifetime.Scoped:       collection.TryAddKeyedScoped      (service, serviceKey, implementationType); break;
                case Lifetime.Singleton:    collection.TryAddKeyedSingleton   (service, serviceKey, implementationType); break;
                case Lifetime.Transient:    collection.TryAddKeyedTransient   (service, serviceKey, implementationType); break;
                default:                    throw new NotImplementedException();
            }
        }

        public static void TryAddKeyedLifetime
        (
            this IServiceCollection collection,
            Type service,
            object serviceKey,
            Func<IServiceProvider, object, object> implementationFactory
        )
        {
            switch(GetLifetime(service)) {
                case Lifetime.Scoped:       collection.TryAddKeyedScoped      (service, serviceKey, implementationFactory); break;
                case Lifetime.Singleton:    collection.TryAddKeyedSingleton   (service, serviceKey, implementationFactory); break;
                case Lifetime.Transient:    collection.TryAddKeyedTransient   (service, serviceKey, implementationFactory); break;
                default:                    throw new NotImplementedException();
            }
        }

        public static void TryAddKeyedLifetime<
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TService
        >
        (
            this IServiceCollection collection,
            object serviceKey
        )
            where TService : class
        {
            switch(GetLifetime<TService>()) {
                case Lifetime.Scoped:       collection.TryAddKeyedScoped      <TService>(serviceKey); break;
                case Lifetime.Singleton:    collection.TryAddKeyedSingleton   <TService>(serviceKey); break;
                case Lifetime.Transient:    collection.TryAddKeyedTransient   <TService>(serviceKey); break;
                default:                    throw new NotImplementedException();
            }
        }

        public static void TryAddKeyedLifetime<
            TService,
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TImplementation
        >
        (
            this IServiceCollection collection,
            object serviceKey
        )
            where TService : class
            where TImplementation : class, TService
        {
            switch(GetLifetime<TService>()) {
                case Lifetime.Scoped:       collection.TryAddKeyedScoped      <TService, TImplementation>(serviceKey); break;
                case Lifetime.Singleton:    collection.TryAddKeyedSingleton   <TService, TImplementation>(serviceKey); break;
                case Lifetime.Transient:    collection.TryAddKeyedTransient   <TService, TImplementation>(serviceKey); break;
                default:                    throw new NotImplementedException();
            }
        }

        public static void TryAddKeyedLifetime<TService>
        (
            this IServiceCollection services,
            object serviceKey,
            Func<IServiceProvider, object, TService> implementationFactory
        )
            where TService : class
        {
            switch(GetLifetime<TService>()) {
                case Lifetime.Scoped:       services.TryAddKeyedScoped      <TService>(serviceKey, implementationFactory); break;
                case Lifetime.Singleton:    services.TryAddKeyedSingleton   <TService>(serviceKey, implementationFactory); break;
                case Lifetime.Transient:    services.TryAddKeyedTransient   <TService>(serviceKey, implementationFactory); break;
                default:                    throw new NotImplementedException();
            }
        }

        public static void TryAddLifetime
        (
            this IServiceCollection collection,
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type service
        )
        {
            switch(GetLifetime(service)) {
                case Lifetime.Scoped:       collection.TryAddScoped     (service); break;
                case Lifetime.Singleton:    collection.TryAddSingleton  (service); break;
                case Lifetime.Transient:    collection.TryAddTransient  (service); break;
                default:                    throw new NotImplementedException();
            }
        }

        public static void TryAddLifetime
        (
            this IServiceCollection collection,
            Type service,
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type implementationType
        )
        {
            switch(GetLifetime(service)) {
                case Lifetime.Scoped:       collection.TryAddScoped     (service, implementationType); break;
                case Lifetime.Singleton:    collection.TryAddSingleton  (service, implementationType); break;
                case Lifetime.Transient:    collection.TryAddTransient  (service, implementationType); break;
                default:                    throw new NotImplementedException();
            }
        }

        public static void TryAddLifetime
        (
            this IServiceCollection collection,
            Type service,
            Func<IServiceProvider, object> implementationFactory
        )
        {
            switch(GetLifetime(service)) {
                case Lifetime.Scoped:       collection.TryAddScoped     (service, implementationFactory); break;
                case Lifetime.Singleton:    collection.TryAddSingleton  (service, implementationFactory); break;
                case Lifetime.Transient:    collection.TryAddTransient  (service, implementationFactory); break;
                default:                    throw new NotImplementedException();
            }
        }

        public static void TryAddLifetime<
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TService
        >
        (this IServiceCollection collection)
            where TService : class
        {
            switch(GetLifetime<TService>()) {
                case Lifetime.Scoped:       collection.TryAddScoped     <TService>(); break;
                case Lifetime.Singleton:    collection.TryAddSingleton  <TService>(); break;
                case Lifetime.Transient:    collection.TryAddTransient  <TService>(); break;
                default:                    throw new NotImplementedException();
            }
        }

        public static void TryAddLifetime<
            TService,
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TImplementation
        >
        (this IServiceCollection collection)
            where TService : class
            where TImplementation : class, TService
        {
            switch(GetLifetime<TService>()) {
                case Lifetime.Scoped:       collection.TryAddScoped     <TService, TImplementation>(); break;
                case Lifetime.Singleton:    collection.TryAddSingleton  <TService, TImplementation>(); break;
                case Lifetime.Transient:    collection.TryAddTransient  <TService, TImplementation>(); break;
                default:                    throw new NotImplementedException();
            }
        }

        public static void TryAddLifetime<TService>
        (
            this IServiceCollection services,
            Func<IServiceProvider, TService> implementationFactory
        )
            where TService : class
        {
            switch(GetLifetime<TService>()) {
                case Lifetime.Scoped:       services.TryAddScoped     <TService>(implementationFactory); break;
                case Lifetime.Singleton:    services.TryAddSingleton  <TService>(implementationFactory); break;
                case Lifetime.Transient:    services.TryAddTransient  <TService>(implementationFactory); break;
                default:                    throw new NotImplementedException();
            }
        }

        private static Lifetime GetLifetime(Type serviceType)
        {
            var attribute = serviceType.GetCustomAttribute<LifetimeAttribute>();
            return attribute != null
                ? attribute.Lifetime
                : throw new InvalidOperationException($"{serviceType.Name} is not decorated with a {nameof(LifetimeAttribute)}");
        }

        private static Lifetime GetLifetime<T>() => GetLifetime(typeof(T));

        /// <summary>
        /// Shorthand for creating a new scope and then running some code within it.
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="action"></param>
        public static void WithinNewScope(this IServiceProvider serviceProvider, Action<IServiceScope> action)
        {
            using(var scope = serviceProvider.CreateScope()) {
                action(scope);
            }
        }

        /// <summary>
        /// Injects services into an object that was not instantiated via dependency injection. Be careful
        /// about lifetimes! Use the <see cref="InjectedServiceAttribute"/> to control which public
        /// properties, fields and methods are injected.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serviceProvider"></param>
        /// <param name="target"></param>
        public static void InjectServices(this IServiceProvider serviceProvider, object target)
        {
            Type type = target?.GetType();

            if(serviceProvider != null && target != null) {
                const BindingFlags publicInstanceOnly = BindingFlags.Public | BindingFlags.Instance;

                object getService(Type serviceType, InjectedServiceAttribute controlAttr)
                {
                    return controlAttr.IsOptional
                        ? serviceProvider.GetService(serviceType)
                        : serviceProvider.GetRequiredService(serviceType);
                }

                foreach(var fieldInfo in type.GetFields(publicInstanceOnly)) {
                    var injectedService = fieldInfo.GetCustomAttribute<InjectedServiceAttribute>();
                    if(injectedService != null) {
                        var currentValue = fieldInfo.GetValue(target);
                        if(currentValue == null) {
                            var service = getService(fieldInfo.FieldType, injectedService);
                            if(service != null) {
                                fieldInfo.SetValue(target, service);
                            }
                        }
                    }
                }

                foreach(var propertyInfo in type.GetProperties(publicInstanceOnly).Where(r => r.CanRead && r.CanWrite)) {
                    var injectedService = propertyInfo.GetCustomAttribute<InjectedServiceAttribute>();
                    if(injectedService != null) {
                        var currentValue = propertyInfo.GetValue(target);
                        if(currentValue == null) {
                            var service = getService(propertyInfo.PropertyType, injectedService);
                            if(service != null) {
                                propertyInfo.SetValue(target, service);
                            }
                        }
                    }
                }

                foreach(var methodInfo in type.GetMethods(publicInstanceOnly)) {
                    var methodInjectedService = methodInfo.GetCustomAttribute<InjectedServiceAttribute>();
                    if(methodInjectedService != null) {
                        var paramInfos = methodInfo.GetParameters();
                        var parameters = new object[paramInfos.Length];
                        for(var idx = 0;idx < paramInfos.Length;++idx) {
                            var paramInfo = paramInfos[idx];
                            var parameterInjectedService = paramInfo.GetCustomAttribute<InjectedServiceAttribute>();
                            parameters[idx] = getService(
                                paramInfo.ParameterType,
                                parameterInjectedService ?? methodInjectedService
                            );
                        }
                        methodInfo.Invoke(target, parameters);
                    }
                }
            }
        }
    }
}
