# Multi-stage build for ASP.NET Core 8.0
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["BookStore.csproj", "./"]
RUN dotnet restore "BookStore.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "BookStore.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BookStore.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BookStore.dll"]
