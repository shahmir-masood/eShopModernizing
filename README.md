
# eShopModernizing - Modernizing ASP.NET Web apps (MVC and WebForms) and N-Tier apps (WCF + WinForms) on .NET 8 with Linux Containers and Azure

This repo provides three sample hypothetical eShop web apps (ASP.NET MVC and WebForms, plus an N-Tier app based on a WCF service and a client WinForms desktop app), now upgraded to **.NET 8**. The ASP.NET MVC apps run on **ASP.NET Core MVC 8**, the WebForms apps were rewritten as **ASP.NET Core Razor Pages**, the WCF service runs on **CoreWCF**, and the WinForms client targets **net8.0-windows**. You can deploy them with **Linux containers** and Azure Cloud into the following deployment options:

- Local build and deployment with the .NET 8 SDK and Docker
- Azure Container Instances (ACI)
- Virtual Machine (Linux or Windows with Docker)
- AKS Kubernetes orchestrator cluster
- Azure Web App for Containers (Linux Containers)

All those mentioned environments can be deployed into Azure cloud (as explained in the Wiki) but you can also deploy all those environments into on-premises servers or even in other public clouds.

## Related Guide/eBook
You can download its related guidance with this free guide/eBook (2nd Edition):

<img src="https://github.com/dotnet/docs/raw/master/docs/architecture/modernize-with-azure-containers/media/index/web-application-guide-cover-image.png" width="300">

.PDF download: https://aka.ms/liftandshiftwithcontainersebook

The upgrade to .NET 8 and Linux containers significantly improves the deployments for DevOps and lets the apps run cross-platform.

The sample apps are simple web apps for the internal backoffice of an eShop so employees can update the Product Catalog. 
Both apps are therefore simple CRUD web application to update data into a SQL Server database. 

See a screenshots of both apps below.

### INITIAL VERSIONS OF EXISTING ASP.NET WEB APPS

![image](https://user-images.githubusercontent.com/1712635/30354184-db7f1098-97df-11e7-8e7b-c18c67b8ba2a.png)

### CONTAINERIZED VERSION IN DEVELOPMENT ENVIRONMENT

![image](https://user-images.githubusercontent.com/1712635/30395628-9c4bff98-987b-11e7-82ca-89a1648f3bdc.png)

### UI and business features

The WebFoms and MVC apps are pretty similiar in regards UI and business features. We just created both versions so you can compare, depending on what technology you are using for your existing apps (ASP.NET MVC or Web Forms).

![image](https://user-images.githubusercontent.com/1712635/30354210-0638f3b2-97e0-11e7-82c5-df18197ccdbd.png)

### Winforms + WCF Application

The winforms application is a catalog management, and uses a WCF as a back-end. Read more about the Winforms + WCF sample [here](./winforms-wcf.md)

### DEPLOYMENT TO AZURE CONTAINER INSTANCES
![image](https://user-images.githubusercontent.com/1712635/38395601-9258dd0e-38e8-11e8-8b42-cafff5f93c57.png)

### DEPLOYMENT TO AZURE WINDOWS SERVER 2016 VM
![image](https://user-images.githubusercontent.com/1712635/30402804-d62632a2-9893-11e7-817a-f9f616cdf380.png)

### DEPLOYMENT TO KUBERNETES CLUSTER IN AKS (Azure Kubernetes Service)
![image](https://user-images.githubusercontent.com/1899987/61177768-7526a100-a5aa-11e9-8279-bdfba19e1335.png)

### DEPLOYMENT TO AZURE WEB APP FOR CONTAINERS
![image](https://docs.microsoft.com/en-us/dotnet/architecture/modernize-with-azure-containers/media/image5-11.png)

## Quick start: Running all apps together locally with Docker

The quickest way to get started is to install the **.NET 8 SDK** (8.0.400 or later; the repo pins it via `global.json`) and Docker, go to the eShopModernizing root folder, and run the `build.cmd` script.

**Prerequisites:** [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) and Docker. The build now uses the `dotnet` CLI (`dotnet restore`/`dotnet publish`) instead of `nuget.exe` + `msbuild`.

This script will:

* Build MVC project
* Build Webforms project
* Build WCF back-end project
* Create three Docker images (Linux Container images, multi-stage .NET 8 builds):
   * `eshop/modernizedwebforms`
   * `eshop/modernizedmvc`
   * `eshop/wcfservice`

You can check the just created Docker images by running `docker images` from the command line:

![image](https://user-images.githubusercontent.com/1712635/38949583-a2c11ba2-42f7-11e8-9c10-b74f2a005186.png)

Finally just run `docker-compose up` (in the root of the repo) to start all three projects and one SQL Server container. Once the containers are started:

* MVC web app listens in: 
     - Port 5115 on the Docker Host (PC) network card IP
     - Port 8080 on the internal container's IP
* Webforms web app listens in:  
     - Port 5114 on the Docker Host (PC) network card IP
     - Port 80 on the internal container's IP
* WCF service listens in port: 
     - Port 5113 on the Docker Host (PC) network card IP
     - Port 8080 on the internal container's IP

>**Note** You should be able to use `http://localhost:<port>` to access the desired application. 

In order to test the apps/containers from within the Docker host itself (the dev Windows PC) you need to use the internal IP (container's IP) to access the application. To find the internal IP, just type  `docker ps` to find the container ids:

![docker ps output](./assets/docker-ps.png)

Then use the command `docker inspect  <CONTAINER-ID> -f {{.NetworkSettings.Networks.nat.IPAddress}}` to find the container's IP, and use that IP **and port 80** to access the container:

![accessing-container](./assets/internal-ip-access.png)

### The localhost loopback limitation in Windows Containers Docker hosts

Due to a default NAT limitation in current versions of Windows (see [https://blog.sixeyed.com/published-ports-on-windows-containers-dont-do-loopback/](https://blog.sixeyed.com/published-ports-on-windows-containers-dont-do-loopback/)) you can't access your containers using `localhost` from the host computer.
You have further information here, too: https://blogs.technet.microsoft.com/virtualization/2016/05/25/windows-nat-winnat-capabilities-and-limitations/

Although that [limitation has been removed beginning with Build 17025](https://blogs.technet.microsoft.com/networking/2017/11/06/available-to-windows-10-insiders-today-access-to-published-container-ports-via-localhost127-0-0-1/) (as of early 2018, still only available today to Windows Insiders, not public/stable release). With that version (Windows 10 Build 17025 or later), access to published container ports via “localhost”/127.0.0.1 should be available.


## Review the Wiki for detailed instructions on how to set it up and deploy to multiple environments

Wiki: https://github.com/dotnet-architecture/eShopModernizing/wiki

### Choose in-memory mock-data or real database connection to a SQL Server database

The MVC and WebForms web apps allow either to connect to the real database to get/update the product catalog or to use mock-data if, due to any reason, the database is still not available and you need to test/demo the app. 

For each application, the option to select one or the other mode can be configured in the docker-compose.override.yml file when using containers or in the `appsettings.json` file when running the apps directly (outside containers).


