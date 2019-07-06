using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
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
        readonly AspNetCoreBindingFactory bindings;
        readonly IApplicationLifetime applicationLifetime;
        readonly Binding httpBinding;
        readonly Binding httpsBinding;
        readonly AspNetCoreRequestRouter router;
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
        /// <param name="applicationLifetime"></param>
        AspNetCoreServiceHostMiddleware(
            RequestDelegate next,
            AspNetCoreBindingFactory bindings,
            AspNetCoreRequestRouter router,
            IApplicationLifetime applicationLifetime)
        {
            this.next = next;
            this.bindings = bindings ?? throw new ArgumentNullException(nameof(bindings));
            this.applicationLifetime = applicationLifetime ?? throw new ArgumentNullException(nameof(applicationLifetime));
            this.router = router ?? throw new ArgumentNullException(nameof(router));

            // bindings for registered services
            httpBinding = bindings.CreateBinding(MessageVersion.Default, false);
            httpsBinding = bindings.CreateBinding(MessageVersion.Default, true);

            // identifies this registered middleware route
            routeId = Guid.NewGuid();

            // listen on multiple URIs
            httpBaseUri = new Uri($"http://{routeId.ToString("N")}/");
            httpsBaseUri = new Uri($"https://{routeId.ToString("N")}/");

            // register for cleanup when application is stopped
            applicationLifetime.ApplicationStopping.Register(() => host.Close());
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="next"></param>
        /// <param name="serviceType"></param>
        /// <param name="bindings"></param>
        /// <param name="router"></param>
        /// <param name="applicationLifetime"></param>
        protected AspNetCoreServiceHostMiddleware(
            RequestDelegate next,
            Type serviceType,
            AspNetCoreBindingFactory bindings,
            AspNetCoreRequestRouter router,
            IApplicationLifetime applicationLifetime) :
            this(next, bindings, router, applicationLifetime)
        {
            this.host = new ServiceHost(serviceType, new[] { httpBaseUri, httpsBaseUri });
            host.Description.Behaviors.Add(new AspNetCoreServiceBehavior(router));
            host.Description.Behaviors.Add(new UseRequestHeadersForMetadataAddressBehavior());

            var szs = host.Description.Behaviors.Find<ServiceBehaviorAttribute>();
            if (szs == null)
                host.Description.Behaviors.Add(szs = new ServiceBehaviorAttribute());

            szs.IncludeExceptionDetailInFaults = true;
            szs.UseSynchronizationContext = false;

            var sdb = host.Description.Behaviors.Find<ServiceDebugBehavior>();
            if (sdb == null)
                host.Description.Behaviors.Add(sdb = new ServiceDebugBehavior());

            sdb.HttpHelpPageEnabled = true;
            sdb.HttpHelpPageBinding = bindings.CreateBinding(MessageVersion.None, false);
            sdb.HttpHelpPageUrl = new Uri("/", UriKind.Relative);
            sdb.HttpsHelpPageEnabled = true;
            sdb.HttpsHelpPageBinding = bindings.CreateBinding(MessageVersion.None, true);
            sdb.HttpsHelpPageUrl = new Uri("/", UriKind.Relative);
            sdb.IncludeExceptionDetailInFaults = false;

            var smb = host.Description.Behaviors.Find<ServiceMetadataBehavior>();
            if (smb == null)
                host.Description.Behaviors.Add(smb = new ServiceMetadataBehavior());

            smb.HttpGetEnabled = true;
            smb.HttpGetBinding = bindings.CreateBinding(MessageVersion.None, false);
            smb.HttpsGetEnabled = true;
            smb.HttpsGetBinding = bindings.CreateBinding(MessageVersion.None, true);
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="next"></param>
        /// <param name="bindings"></param>
        /// <param name="router"></param>
        /// <param name="options"></param>
        /// <param name="applicationLifetime"></param>
        public AspNetCoreServiceHostMiddleware(
            RequestDelegate next,
            AspNetCoreBindingFactory bindings,
            AspNetCoreRequestRouter router,
            IOptions<AspNetCoreServiceHostOptions> options,
            IApplicationLifetime applicationLifetime) :
            this(next, options.Value.ServiceType, bindings, router, applicationLifetime)
        {
            options.Value.Configure?.Invoke(new AspNetCoreServiceHostConfigurator(this));
        }

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
        /// Registers a new service endpoint.
        /// </summary>
        /// <typeparam name="TContract"></typeparam>
        /// <param name="relativePath"></param>
        public void AddServiceEndpoint<TContract>(string relativePath)
        {
            host.AddServiceEndpoint(typeof(TContract), httpBinding, relativePath);
            host.AddServiceEndpoint(typeof(TContract), httpsBinding, relativePath);
        }

        /// <summary>
        /// Registers a new service endpoint.
        /// </summary>
        /// <param name="contractType"></param>
        /// <param name="relativePath"></param>
        public void AddServiceEndpoint(Type contractType, string relativePath)
        {
            host.AddServiceEndpoint(contractType, httpBinding, relativePath);
            host.AddServiceEndpoint(contractType, httpsBinding, relativePath);
        }

        /// <summary>
        /// Registers a new service endpoint.
        /// </summary>
        /// <param name="implementedContract"></param>
        /// <param name="relativePath"></param>
        public void AddServiceEndpoint(string implementedContract, string relativePath)
        {
            host.AddServiceEndpoint(implementedContract, httpBinding, relativePath);
            host.AddServiceEndpoint(implementedContract, httpsBinding, relativePath);
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
