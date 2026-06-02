FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["eShopWCFService.csproj", "./"]
RUN dotnet restore "eShopWCFService.csproj"
COPY . .
RUN dotnet build "eShopWCFService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "eShopWCFService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "eShopWCFService.dll"]
