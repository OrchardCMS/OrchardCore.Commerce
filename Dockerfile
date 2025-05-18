# TARGETARCH and TARGETOS are set automatically when --platform is provided.
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0@sha256:8ab06772f296ed5f541350334f15d9e2ce84ad4b3ce70c90f2e43db2752c30f6 AS build-env
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
FROM mcr.microsoft.com/dotnet/aspnet:8.0-nanoserver-1809@sha256:3e02871ca45856ebfc43876a706952cdd755b795d0bc8790f317f192dd770843 AS build_windows
FROM mcr.microsoft.com/dotnet/aspnet:8.0@sha256:6159cf66274cf52730d7a0c7bb05cf0af94b79370176886ac58286ab6cbb7faf AS build_linux
FROM build_${TARGETOS} AS aspnet

EXPOSE 80
ENV ASPNETCORE_URLS=http://+:80
WORKDIR /app
COPY --from=build-env /app/ .
ENTRYPOINT ["dotnet", "OrchardCore.Commerce.Web.dll"]
