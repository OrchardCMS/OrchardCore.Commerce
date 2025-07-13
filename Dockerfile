# TARGETARCH and TARGETOS are set automatically when --platform is provided.
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0@sha256:e6748a3b3d8cea753f643cacac0e577d8c409285b3924232db1354693bdbd3d7 AS build-env
ARG TARGETOS
LABEL stage=build-env
WORKDIR /source

# copy required files for building
# .dockerignore excludes App_Data and binaries from these
COPY ./src ./src
COPY Directory.Build.props .
COPY Directory.Packages.props .

# build, results are placed in /app
RUN dotnet publish src/OrchardCore.Commerce.Web/OrchardCore.Commerce.Web.csproj -c Release -o /app --framework net8.0 /p:RunAnalyzers=false

# build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0-nanoserver-1809@sha256:e266a84d377e5f2081e1178f3b085520833f6ba3fce45127bc348a1bbfb135a3 AS build_windows
FROM mcr.microsoft.com/dotnet/aspnet:8.0@sha256:35095f3e2bf5ab1f0c6953ed1364028343b5aef029932454cf2ce9e9680827d8 AS build_linux
FROM build_${TARGETOS} AS aspnet

EXPOSE 80
ENV ASPNETCORE_URLS=http://+:80
WORKDIR /app
COPY --from=build-env /app/ .
ENTRYPOINT ["dotnet", "OrchardCore.Commerce.Web.dll"]
