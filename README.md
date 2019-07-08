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
    app.UseServiceHost<MathService>("/math");
}
```

I've done a few things internally that are sort of weird.

Each invocation of `UseServiceHost` starts up a `ServiceHost` instance. This service host is configured by default with
bindings that establish a `AspNetCoreTransport`. The transport generates `ReplyChannel`s which sets up an inbound
delivery queue based on the binding properties. As ASP.Net Core requests arrive, they are dispatched into the router
where the hunt for a queue that they should be put
into. The `ReplyChannelListener` dequeues these requests and enters them into the WCF pipeline. Reply messages likewise
write back to the originating ASP.Net core Response.

Internally a non-standard ListenURI is set up for each invocation of `UseServiceHost`. These each get a "Router ID",
which is a GUID that idenfies the target `ServiceHost` instance. This can probably go away now that I can get access to
the server addresses of Kestrel and such. The main problem here is the branching paths in ASP.Net are not known until a
request traverses them. So, I can't determine the path of the service until a request comes in. WCF makes this all very
hard.

This lets us use the existing `ServiceHost` class. And thus eventually piggyback on existing `ServiceHost` extensions:
like Autofac integration. And it keeps the dispatch/contract discovery stuff in WCF.

