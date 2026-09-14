# CLAUDE.md

## What this is

Cogito.AspNetCore.ServiceModel — ASP.NET Core WCF middleware. Includes middleware for routing SOAP requests to WCF ServiceHost instances. Allows you to host your SOAP service on ASP.Net Core.

Publishes 1 package: `Cogito.AspNetCore.ServiceModel`.

## Build and test

```shell
dotnet restore Cogito.AspNetCore.ServiceModel.slnx
dotnet msbuild -p:Configuration=Release Cogito.AspNetCore.ServiceModel.dist.msbuildproj
```

The dist project stages packages into `dist/nuget` and test suites into
`dist/tests/<suite>/<tfm>`; run a suite with `dotnet test -f <tfm> <path to its assembly>`.
A .NET Framework suite builds as an `.exe`, not a `.dll`.

## Conventions

- Packaging is the dist project's job. No project sets `GeneratePackageOnBuild`.
- Versions come from GitVersion. Release by creating a GitHub release; that tag publishes to nuget.org.
- Add package references with `dotnet add package`, no version, so the version resolved is one the project's target frameworks support. Don't hand-edit the csproj.
- Add target frameworks, never silently substitute one. Dropping a target framework is a breaking change.
- Every package carries its own `README.md`; the repository `README.md` is for GitHub.

## Attribution

Do not add AI or tool attribution anywhere — not in commit messages, pull request
descriptions, code comments, or documentation. No `Co-Authored-By` trailers for tools, no
"generated with" footers. Write commits and PRs as the author.
