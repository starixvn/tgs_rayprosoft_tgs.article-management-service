#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
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

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Tgs.ArticleManagementService.dll"]