# TARGETARCH and TARGETOS are set automatically when --platform is provided.
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0@sha256:57c3bf930db9ac075c9019b83fdfc77c474b11b874120b84bc97fa65f33662dd AS build-env
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
FROM mcr.microsoft.com/dotnet/aspnet:8.0-nanoserver-1809@sha256:64ca9021761454a54d397fe681acb064af27647ba799229915e2d0f58fb47903 AS build_windows
FROM mcr.microsoft.com/dotnet/aspnet:8.0@sha256:68669af44214899b4a5ff5fa0dd5fc10e7e9d665669a44dcbc1a142a99b2ec5b AS build_linux
FROM build_${TARGETOS} AS aspnet

EXPOSE 80
ENV ASPNETCORE_URLS=http://+:80
WORKDIR /app
COPY --from=build-env /app/ .
ENTRYPOINT ["dotnet", "OrchardCore.Commerce.Web.dll"]
