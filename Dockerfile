# TARGETARCH and TARGETOS are set automatically when --platform is provided.
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0@sha256:f2f0cb3af991eb6959c8a20551b0152f10cce61354c089dd863a7b72c0f00fea AS build-env
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
FROM mcr.microsoft.com/dotnet/aspnet:8.0-nanoserver-1809@sha256:c38811f61937add522f95d1ba2f13b695a1628ec5b98010f1098dc88c9fd6aa5 AS build_windows
FROM mcr.microsoft.com/dotnet/aspnet:8.0@sha256:590a934508c35c2ef61203ded76f088f5ec6263b863b55d3deaea8fc2d6b55a6 AS build_linux
FROM build_${TARGETOS} AS aspnet

EXPOSE 80
ENV ASPNETCORE_URLS=http://+:80
WORKDIR /app
COPY --from=build-env /app/ .
ENTRYPOINT ["dotnet", "OrchardCore.Commerce.Web.dll"]
