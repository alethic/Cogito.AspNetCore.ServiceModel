using System;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Cogito.AspNetCore.ServiceModel
{

    /// <summary>
    /// Dispatches requests to the WCF infrastructure.
    /// </summary>
    public class AspNetCoreServiceHostMiddleware
    {

        readonly RequestDelegate next;
        readonly IServiceProvider services;
        readonly AspNetCoreBindingBase binding;
        readonly AspNetCoreRequestRouter router;
        readonly ServiceHost host;
        readonly Uri baseUri;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="next"></param>
        /// <param name="services"></param>
        /// <param name="binding"></param>
        AspNetCoreServiceHostMiddleware(
            RequestDelegate next,
            IServiceProvider services,
            AspNetCoreBindingBase binding)
        {
            this.next = next;
            this.services = services ?? throw new ArgumentNullException(nameof(services));
            this.binding = binding ?? throw new ArgumentNullException(nameof(binding));
            this.router = new AspNetCoreRequestRouter();
            this.baseUri = AspNetCoreUri.GetUri("/" + Guid.NewGuid().ToString("N") + "/");

            // register for cleanup when application is stopped
            services.GetRequiredService<IApplicationLifetime>().ApplicationStopping.Register(() => host.Close());
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="next"></param>
        /// <param name="services"></param>
        /// <param name="serviceType"></param>
        /// <param name="binding"></param>
        protected AspNetCoreServiceHostMiddleware(
            RequestDelegate next,
            IServiceProvider services,
            Type serviceType,
            AspNetCoreBindingBase binding) :
            this(next, services, binding)
        {
            this.host = new ServiceHost(serviceType, baseUri);
            host.Description.Behaviors.Add(new AspNetCoreServiceBehavior(router));
            host.Description.Behaviors.Add(new ServiceThrottlingBehavior() { MaxConcurrentSessions = 10 });
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="next"></param>
        /// <param name="services"></param>
        /// <param name="options"></param>
        public AspNetCoreServiceHostMiddleware(
            RequestDelegate next,
            IServiceProvider services,
            IOptions<AspNetCoreServiceHostOptions> options) :
            this(
                next,
                services,
                options.Value.ServiceType,
                (AspNetCoreBindingBase)services.GetService(options.Value.BindingType ?? typeof(AspNetCoreBasicBinding)))
        {
            options.Value.Configure?.Invoke(new AspNetCoreServiceHostConfigurator(this));
        }

        /// <summary>
        /// Registers a new service endpoint.
        /// </summary>
        /// <typeparam name="TContract"></typeparam>
        /// <param name="relativePath"></param>
        public void AddServiceEndpoint<TContract>(string relativePath)
        {
            host.AddServiceEndpoint(typeof(TContract), binding, relativePath);
        }

        /// <summary>
        /// Registers a new service endpoint.
        /// </summary>
        /// <param name="contractType"></param>
        /// <param name="relativePath"></param>
        public void AddServiceEndpoint(Type contractType, string relativePath)
        {
            host.AddServiceEndpoint(contractType, binding, relativePath);
        }

        /// <summary>
        /// Invokes the middleware.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="next"></param>
        /// <returns></returns>
        public async Task InvokeAsync(HttpContext context)
        {
            if (host.State == CommunicationState.Created)
            {
                await host.OpenAsync();
                await Task.Delay(TimeSpan.FromSeconds(1));
            }

            if (host.State != CommunicationState.Opened)
                throw new CommunicationException("ServiceHost is not open. Cannot route request.");

            context.Items[AspNetCoreUri.UriContextItemName] = new Uri(baseUri, context.Request.Path.Value?.TrimStart('/') ?? PathString.Empty);
            await router.RunAsync(context);
        }

    }

}
