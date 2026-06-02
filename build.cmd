@echo [93m Publishing .NET 8 projects to the 'deploy' folder tree[0m

@echo [93m Building MVC project...[0m
dotnet publish eShopModernizedMVCSolution/src/eShopModernizedMVC/eShopModernizedMVC.csproj -c Release -o deploy/mvc
@echo [93m Building Webforms project...[0m
dotnet publish eShopModernizedWebFormsSolution/src/eShopModernizedWebForms/eShopModernizedWebForms.csproj -c Release -o deploy/webforms
@echo [93m Building WCF project...[0m
dotnet publish eShopModernizedNTier/src/eShopWCFService/eShopWCFService.csproj -c Release -o deploy/wcf

@echo [93m Building docker images... [0m
docker-compose -f docker-compose.yml -f docker-compose.override.yml build
