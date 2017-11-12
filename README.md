# Cogito.AspNetCore.ServiceModel
WCF support for ASP.Net Core

Provides middleware for ASP.Net Core for wiring up a WCF `ServiceHost` instance and directing requests to it. This is a bit more complicated than just establishing a binding and dispatcher.

```

public void ConfigureServices(IServiceCollection services)
{
    services.AddServiceModel();
}
        
public void Configure(IApplicationBuilder app, IHostingEnvironment env)
{
    app.UseServiceHost<MathService>("/math", configure =>
    {
        configure.AddServiceEndpoint<IMathService>("");
        configure.AddServiceEndpoint<IMathService>("/2");
    });
}
```

I've done a few things internally that are sort of weird.

A custom baseUri is established with a custom scheme: 'aspnetcore'. Service endpoints register and listen against these addresses: 'aspnetcore:/GUID/path'. Paths are rewritten into these internal paths as they enter the middleware, and dispatched to a router. The router routes to a set of queues. The queues exist because `ChannelListeners` start up and register queues to listen to.

This lets us use the existing `ServiceHost` class. And thus eventually piggyback on existing `ServiceHost` extensions: like Autofac integration. And it keeps the dispatch/contract discovery stuff in WCF. The only unfortunate part about this is ServiceHost is kind of heavy. Seems to spawn a lot of channel listeners, and invoke `OnBeginAccept` a lot. So, we're using semaphores to lease those out.

Also, I'm using completely custom bindings. `BasicHttpBinding` is not used, of course.

MTOM encoding needs to be fixed. It looks like `MtomEncoder` from WCF is pretty much completely internaly. No good way to grab the boundary stuff.
