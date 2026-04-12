FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Directory.Build.props", "src/Directory.Build.props"]
COPY ["Directory.Packages.props", "src/Directory.Packages.props"]
COPY ["src/BlogGateway/BlogGateway.csproj", "src/BlogGateway/"]
COPY ["src/Presentation/BlogGateway.Presentation.Rest.Posts/BlogGateway.Presentation.Rest.Posts.csproj", "src/Presentation/BlogGateway.Presentation.Rest.Posts/"]
COPY ["src/Presentation/BlogGateway.Presentation.Rest.Users/BlogGateway.Presentation.Rest.Users.csproj", "src/Presentation/BlogGateway.Presentation.Rest.Users/"]
COPY ["src/Presentation/BlogGateway.Presentation.Rest.Auth/BlogGateway.Presentation.Rest.Auth.csproj", "src/Presentation/BlogGateway.Presentation.Rest.Auth/"]
RUN dotnet restore "src/BlogGateway/BlogGateway.csproj"
COPY . .
WORKDIR "/src/src/BlogGateway"
RUN dotnet build "./BlogGateway.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./BlogGateway.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BlogGateway.dll"]
