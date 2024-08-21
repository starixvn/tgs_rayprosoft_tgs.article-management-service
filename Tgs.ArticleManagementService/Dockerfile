FROM hub.lumigame.com/base/dotnet/core/sdk:8.0 AS build
ARG BuildConfiguration=Release
WORKDIR /src
COPY ["nuget.config", "."]
COPY ["Tgs.ArticleManagementService/Tgs.ArticleManagementService.csproj", "Tgs.ArticleManagementService/"]
COPY ["ArticleManagement.Core/ArticleManagement.Core.csproj", "ArticleManagement.Core/"]
COPY ["ArticleManagement.Common/ArticleManagement.Common.csproj", "ArticleManagement.Common/"]
COPY ["ArticleManagement.Repositories/ArticleManagement.Repositories.csproj", "ArticleManagement.Repositories/"]
RUN dotnet restore "Tgs.ArticleManagementService/Tgs.ArticleManagementService.csproj"
COPY . .
WORKDIR "/src/Tgs.ArticleManagementService"
RUN dotnet build "Tgs.ArticleManagementService.csproj" -c $BuildConfiguration -o /app/build

FROM build AS publish
ARG BuildConfiguration
RUN dotnet publish "Tgs.ArticleManagementService.csproj" -c $BuildConfiguration -o /app/publish /p:UseAppHost=false

FROM hub.lumigame.com/base/dotnet/core/aspnet:8.0-alpine-24h1.1 AS base
USER app
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Tgs.ArticleManagementService.dll"]