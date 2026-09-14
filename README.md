# Cogito.AspNetCore.ServiceModel

[![Build](https://github.com/alethic/Cogito.AspNetCore.ServiceModel/actions/workflows/Cogito.AspNetCore.ServiceModel.yml/badge.svg)](https://github.com/alethic/Cogito.AspNetCore.ServiceModel/actions/workflows/Cogito.AspNetCore.ServiceModel.yml)

Hosts WCF SOAP services inside an ASP.NET Core pipeline, so existing contracts keep working on a modern host.

## Packages

**[Cogito.AspNetCore.ServiceModel](https://www.nuget.org/packages/Cogito.AspNetCore.ServiceModel)** — Host WCF SOAP services inside an ASP.NET Core pipeline.

Each package carries its own README with the detail; the links above go to nuget.org.

## Building

```shell
dotnet restore Cogito.AspNetCore.ServiceModel.slnx
dotnet msbuild -p:Configuration=Release Cogito.AspNetCore.ServiceModel.dist.msbuildproj
```

Packages are staged into `dist/nuget` and test suites into `dist/tests`; run a suite with
`dotnet test -f <tfm> <path to its assembly>`.

## License

MIT — see [LICENSE](LICENSE).
