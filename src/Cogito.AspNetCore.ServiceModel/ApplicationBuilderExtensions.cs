using System;
using System.ServiceModel;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Cogito.AspNetCore.ServiceModel
{

    public static class ApplicationBuilderExtensions
    {

        /// <summary>
        /// Creates a <see cref="ServiceHost"/> listening for requests for a given service.
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseServiceHost<TService, TContract>(this IApplicationBuilder app, PathString path)
            where TService : TContract
        {
            if (app == null)
                throw new ArgumentNullException(nameof(app));

            // start up service host
            var q = new AspNetCoreRequestQueue();
            var u = new Uri($"aspnetcore://{path}");
            var h = new ServiceHost(typeof(TService), u);
            h.AddServiceEndpoint(typeof(TContract), new AspNetCoreBasicBinding(), u);
            h.Description.Behaviors.Add(new AspNetCoreServiceBehavior(q));
            h.Open();

            // close service host on application shutdown
            var lft = app.ApplicationServices.GetRequiredService<IApplicationLifetime>();
            lft.ApplicationStopping.Register(() => h.Close());

            // map requests to service
            return app.Use(async (context, next) => { await q.SendAsync(context); });
        }

    }

}
