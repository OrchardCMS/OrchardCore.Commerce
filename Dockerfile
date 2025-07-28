# TARGETARCH and TARGETOS are set automatically when --platform is provided.
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0@sha256:4763fe4aeb0c5f0dec72fe29678329113f9ee5d4f092a86a5a5044c0bb18e974 AS build-env
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
FROM mcr.microsoft.com/dotnet/aspnet:8.0@sha256:fbc9110dff9359545a6e1e67cc8295a3e001fe3c04c677abedb6e764d4277b3a AS build_linux
FROM build_${TARGETOS} AS aspnet

EXPOSE 80
ENV ASPNETCORE_URLS=http://+:80
WORKDIR /app
COPY --from=build-env /app/ .
ENTRYPOINT ["dotnet", "OrchardCore.Commerce.Web.dll"]
