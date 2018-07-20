using System;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Cogito.AspNetCore.ServiceModel
{

    public static class AspNetCoreExtensions
    {

        /// <summary>
        /// Adds ServiceModel services to the specified <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddServiceModel(
            this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services.AddTransient<AspNetCoreBasicBinding>();
            services.AddTransient<AspNetCoreBasicMtomBinding>();
            return services;
        }

        /// <summary>
        /// Wires up the AspNetCore WCF framework to the given path.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="path"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseServiceHost(
            this IApplicationBuilder app,
            PathString path,
            AspNetCoreServiceHostOptions options)
        {
            if (app == null)
                throw new ArgumentNullException(nameof(app));
            if (options == null)
                throw new ArgumentNullException(nameof(options));
            if (options.ServiceType == null)
                throw new ArgumentException("Must specify ServiceType in options.", nameof(options));

            if (path.HasValue)
                return app.Map(path, x => x.UseMiddleware<AspNetCoreServiceHostMiddleware>(Options.Create(options)));
            else
                return app.UseMiddleware<AspNetCoreServiceHostMiddleware>(Options.Create(options));
        }

        /// <summary>
        /// Wires up the AspNetCore WCF framework to the given path.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="path"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseServiceHost(
            this IApplicationBuilder app,
            AspNetCoreServiceHostOptions options)
        {
            if (app == null)
                throw new ArgumentNullException(nameof(app));
            if (options == null)
                throw new ArgumentNullException(nameof(options));
            if (options.ServiceType == null)
                throw new ArgumentException("Must specify ServiceType in options.", nameof(options));

            return app.UseServiceHost(PathString.Empty, options);
        }

        /// <summary>
        /// Wires up the AspNetCore WCF framework to the given path.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="path"></param>
        /// <param name="serviceType"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseServiceHost(
            this IApplicationBuilder app,
            PathString path,
            Type serviceType,
            Action<AspNetCoreServiceHostConfigurator> configure = null)
        {
            if (app == null)
                throw new ArgumentNullException(nameof(app));
            if (serviceType == null)
                throw new ArgumentNullException(nameof(serviceType));

            return app.UseServiceHost(path, new AspNetCoreServiceHostOptions()
            {
                ServiceType = serviceType,
                Configure = configure,
            });
        }

        /// <summary>
        /// Wires up the AspNetCore WCF framework to the given path.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="path"></param>
        /// <param name="serviceType"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseServiceHost(
            this IApplicationBuilder app,
            Type serviceType,
            Action<AspNetCoreServiceHostConfigurator> configure = null)
        {
            if (app == null)
                throw new ArgumentNullException(nameof(app));
            if (serviceType == null)
                throw new ArgumentNullException(nameof(serviceType));

            return app.UseServiceHost(PathString.Empty, serviceType, configure);
        }

        /// <summary>
        /// Wires up the AspNetCore WCF framework to the given path and hosts the specified service.
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="app"></param>
        /// <param name="path"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseServiceHost<TService>(
            this IApplicationBuilder app,
            PathString path,
            Action<AspNetCoreServiceHostConfigurator> configure = null)
        {
            if (app == null)
                throw new ArgumentNullException(nameof(app));

            return app.UseServiceHost(path, typeof(TService), configure);
        }

        /// <summary>
        /// Wires up the AspNetCore WCF framework to the given path and hosts the specified service.
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="app"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseServiceHost<TService>(
            this IApplicationBuilder app,
            Action<AspNetCoreServiceHostConfigurator> configure = null)
        {
            return app.UseServiceHost<TService>(PathString.Empty, configure);
        }

        /// <summary>
        /// Wires up the AspNetCore WCF framework to the given path and hosts the specified service and contract type.
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TContract"></typeparam>
        /// <param name="app"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseServiceHost<TService, TContract>(
            this IApplicationBuilder app,
            PathString path)
        {
            if (app == null)
                throw new ArgumentNullException(nameof(app));

            return app.UseServiceHost<TService>(path, configure => configure.AddServiceEndpoint<TContract>());
        }

        /// <summary>
        /// Wires up the AspNetCore WCF framework to the given path and hosts the specified service and contract type.
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TContract"></typeparam>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseServiceHost<TService, TContract>(
            this IApplicationBuilder app)
        {
            if (app == null)
                throw new ArgumentNullException(nameof(app));

            return app.UseServiceHost<TService, TContract>(PathString.Empty);
        }

    }

}
