# Cogito.AspNetCore.ServiceModel
WCF support for ASP.Net Core

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
