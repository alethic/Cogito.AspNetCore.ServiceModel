# Cogito.AspNetCore.ServiceModel

Host WCF SOAP services inside an ASP.NET Core pipeline.

## Why

WCF server-side hosting never came to .NET Core, so a SOAP endpoint normally keeps you on .NET
Framework and IIS. This middleware routes SOAP requests to a real `ServiceHost`, which means existing
contracts and behaviours keep working while the process around them is ASP.NET Core.

## Install

```shell
dotnet add package Cogito.AspNetCore.ServiceModel
```

## Use

```csharp
services.AddServiceModel();
```

```csharp
app.UseServiceHost<OrderService>("/services/orders");
```

The transport is an ASP.NET Core binding element, so the request arrives at the service with the
HTTP context intact — reachable from inside an operation through
`OperationContext.Current.GetAspNetCoreContext()`.

`AspNetCoreBasicBinding` is the basicHttp-equivalent binding, with `AspNetCoreBasicSecurityMode` for
transport security; `TextOrMtomEncodingBindingElement` accepts either text or MTOM on the same
endpoint. `AspNetCoreServiceHostOptions` and `AspNetCoreServiceHostConfigurator` are the seams for
configuring the host that gets created.

## License

MIT.
