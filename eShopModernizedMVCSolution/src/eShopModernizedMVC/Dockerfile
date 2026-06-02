FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["eShopModernizedMVC.csproj", "./"]
RUN dotnet restore "eShopModernizedMVC.csproj"
COPY . .
RUN dotnet publish "eShopModernizedMVC.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "eShopModernizedMVC.dll"]
