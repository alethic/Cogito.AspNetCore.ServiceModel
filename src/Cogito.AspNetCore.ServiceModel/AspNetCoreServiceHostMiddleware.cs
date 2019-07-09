using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Cogito.AspNetCore.ServiceModel
{

    /// <summary>
    /// Dispatches requests to the WCF infrastructure.
    /// </summary>
    class AspNetCoreServiceHostMiddleware
    {

        readonly RequestDelegate next;
        readonly AspNetCoreBindingFactory bindings;
        readonly AspNetCoreRequestRouter router;

        readonly Binding httpBinding;
        readonly Binding httpsBinding;
        readonly ServiceHost host;
        readonly Guid routeId;
        readonly Uri httpBaseUri;
        readonly Uri httpsBaseUri;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="next"></param>
        /// <param name="bindings"></param>
        /// <param name="router"></param>
        /// <param name="messageVersion"></param>
        /// <param name="logger"></param>
        /// <param name="applicationLifetime"></param>
        AspNetCoreServiceHostMiddleware(
            RequestDelegate next,
            AspNetCoreBindingFactory bindings,
            AspNetCoreRequestRouter router,
            MessageVersion messageVersion,
            IApplicationLifetime applicationLifetime,
            ILogger<AspNetCoreServiceHostMiddleware> logger,
            IServiceProvider serviceProvider)
        {
            this.next = next;
            this.bindings = bindings ?? throw new ArgumentNullException(nameof(bindings));
            this.router = router ?? throw new ArgumentNullException(nameof(router));

            // bindings for registered services
            httpBinding = bindings.CreateBinding(serviceProvider, messageVersion, false);
            httpsBinding = bindings.CreateBinding(serviceProvider, messageVersion, true);

            // identifies this registered middleware route
            routeId = Guid.NewGuid();

            // listen on multiple URIs
            httpBaseUri = new Uri($"http://{routeId.ToString("N")}/");
            httpsBaseUri = new Uri($"https://{routeId.ToString("N")}/");

            // register for cleanup when application is stopped
            applicationLifetime.ApplicationStopping.Register(() =>
            {
                try
                {
                    host.Close();
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Exception closing ServiceHost.");
                }
            });
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="next"></param>
        /// <param name="serviceType"></param>
        /// <param name="bindings"></param>
        /// <param name="router"></param>
        /// <param name="applicationLifetime"></param>
        /// <param name="logger"></param>
        /// <param name="serviceProvider"></param>
        protected AspNetCoreServiceHostMiddleware(
            RequestDelegate next,
            Type serviceType,
            AspNetCoreBindingFactory bindings,
            AspNetCoreRequestRouter router,
            MessageVersion messageVersion,
            IApplicationLifetime applicationLifetime,
            ILogger<AspNetCoreServiceHostMiddleware> logger,
            IServiceProvider serviceProvider) :
            this(next, bindings, router, messageVersion, applicationLifetime, logger, serviceProvider)
        {
            host = new AspNetCoreServiceHost(serviceType, AddDefaultServiceEndpoint, httpBaseUri, httpsBaseUri);
            host.Description.Behaviors.Add(new AspNetCoreRequestRouterBehavior(router));
            host.Description.Behaviors.Add(new AspNetCoreServiceMetadataBehavior());

            var sdb = host.Description.Behaviors.Find<ServiceDebugBehavior>();
            if (sdb == null)
                host.Description.Behaviors.Add(sdb = new ServiceDebugBehavior());

            var httpGetBinding = bindings.CreateBinding(serviceProvider, MessageVersion.None, false, HttpMethods.Get);
            var httpsGetBinding = bindings.CreateBinding(serviceProvider, MessageVersion.None, true, HttpMethods.Get);

            sdb.HttpHelpPageEnabled = true;
            sdb.HttpHelpPageBinding = httpGetBinding;
            sdb.HttpHelpPageUrl = new Uri("", UriKind.Relative);
            sdb.HttpsHelpPageEnabled = true;
            sdb.HttpsHelpPageBinding = httpsGetBinding;
            sdb.HttpsHelpPageUrl = new Uri("", UriKind.Relative);

            var smb = host.Description.Behaviors.Find<ServiceMetadataBehavior>();
            if (smb == null)
                host.Description.Behaviors.Add(smb = new ServiceMetadataBehavior());

            smb.HttpGetEnabled = true;
            smb.HttpGetBinding = httpGetBinding;
            smb.HttpsGetEnabled = true;
            smb.HttpsGetBinding = httpsGetBinding;
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="next"></param>
        /// <param name="router"></param>
        /// <param name="options"></param>
        /// <param name="applicationLifetime"></param>
        /// <param name="serviceProvider"></param>
        public AspNetCoreServiceHostMiddleware(
            RequestDelegate next,
            AspNetCoreRequestRouter router,
            AspNetCoreServiceHostOptions options,
            IApplicationLifetime applicationLifetime,
            ILogger<AspNetCoreServiceHostMiddleware> logger,
            IServiceProvider serviceProvider) :
            this(next, options.ServiceType, CreateBindingFactory(serviceProvider, options.BindingFactoryType), router, options.MessageVersion, applicationLifetime, logger, serviceProvider)
        {
            options.Configure?.Invoke(new AspNetCoreServiceHostConfigurator(this));
        }

        /// <summary>
        /// Obtains an instance of the binding factory based on the configured type.
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        static AspNetCoreBindingFactory CreateBindingFactory(IServiceProvider serviceProvider, Type type) =>
            (AspNetCoreBindingFactory)serviceProvider.GetRequiredService(type);

        /// <summary>
        /// Gets a reference to the service host.
        /// </summary>
        internal ServiceHost ServiceHost => host;

        /// <summary>
        /// Gets a reference to the binding.
        /// </summary>
        internal AspNetCoreBindingFactory Bindings => bindings;

        /// <summary>
        /// Binding configured for HTTP requests.
        /// </summary>
        internal Binding HttpBinding => httpBinding;

        /// <summary>
        /// Binding configured for HTTPS requests.
        /// </summary>
        internal Binding HttpsBinding => httpsBinding;

        /// <summary>
        /// Adds the default service endpoint for the given contract type.
        /// </summary>
        /// <param name="contractType"></param>
        /// <returns></returns>
        IEnumerable<ServiceEndpoint> AddDefaultServiceEndpoint(Type contractType)
        {
            yield return host.AddServiceEndpoint(contractType, httpBinding, "", httpBaseUri);
            yield return host.AddServiceEndpoint(contractType, httpsBinding, "", httpsBaseUri);
        }

        /// <summary>
        /// Registers a new service endpoint.
        /// </summary>
        /// <typeparam name="TContract"></typeparam>
        /// <param name="relativePath"></param>
        public void AddServiceEndpoint<TContract>(string relativePath)
        {
            host.AddServiceEndpoint(typeof(TContract), httpBinding, relativePath, httpBaseUri);
            host.AddServiceEndpoint(typeof(TContract), httpsBinding, relativePath, httpsBaseUri);
        }

        /// <summary>
        /// Registers a new service endpoint.
        /// </summary>
        /// <param name="contractType"></param>
        /// <param name="relativePath"></param>
        public void AddServiceEndpoint(Type contractType, string relativePath)
        {
            host.AddServiceEndpoint(contractType, httpBinding, relativePath, httpBaseUri);
            host.AddServiceEndpoint(contractType, httpsBinding, relativePath, httpsBaseUri);
        }

        /// <summary>
        /// Registers a new service endpoint.
        /// </summary>
        /// <param name="implementedContract"></param>
        /// <param name="relativePath"></param>
        public void AddServiceEndpoint(string implementedContract, string relativePath)
        {
            host.AddServiceEndpoint(implementedContract, httpBinding, relativePath, httpBaseUri);
            host.AddServiceEndpoint(implementedContract, httpsBinding, relativePath, httpsBaseUri);
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

            // build request URI
            var b = new UriBuilder(new Uri(context.Request.IsHttps ? httpsBaseUri : httpBaseUri, context.Request.Path.Value?.TrimStart('/') ?? PathString.Empty));

            // dispatch request to router, which sends to service host
            context.Items[AspNetCoreUri.UriContextItemName] = b.Uri;
            await router.RunAsync(context);
        }

    }

}
