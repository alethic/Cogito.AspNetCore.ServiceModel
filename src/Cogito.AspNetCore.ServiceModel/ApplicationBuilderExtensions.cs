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
        public static IApplicationBuilder UseServiceHost<TService>(this IApplicationBuilder app)
        {
            if (app == null)
                throw new ArgumentNullException(nameof(app));

            // start up service host
            var q = new AspNetCoreRequestQueue();
            var h = new ServiceHost(typeof(TService));
            h.Description.Behaviors.Add(new AspNetCoreServiceBehavior(q));
            h.Open();

            // close service host on application shutdown
            var lft = app.ApplicationServices.GetRequiredService<IApplicationLifetime>();
            lft.ApplicationStopping.Register(() => h.Close());

            // map requests to service
            return app.Use(async (context, next) => { await q.SendAsync(context); });
        }

        /// <summary>
        /// Creates a <see cref="ServiceHost"/> listening for requests for a given service on the specified path.
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="app"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseServiceHost<TService>(this IApplicationBuilder app, PathString path)
        {
            if (app == null)
                throw new ArgumentNullException(nameof(app));
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            return app.Map(path, i => i.UseServiceHost<TService>());
        }

    }

}
