# SQL Server Master Data Services (MDS) Web API Sample

## Summary

This is a sample implementation of an API endpoint for SQL Server Master Data Services (MDS). 

It wraps certain operations exposed by [Master Data Manager web service](https://docs.microsoft.com/en-us/sql/master-data-services/develop/create-master-data-manager-web-service-proxy-classes?view=sql-server-ver15). The Master Data Manager web service is a Windows Communication Foundation (WCF) service that developers use to control Master Data Services features through code. 

By using or customizing this API you can automate MDS operations using tools - like Azure Logic Apps or Power Automate - which are more suited to working with OpenAPI / Swagger compliant APIs, as opposed to the older WCF / SOAP supported by MDS.

## Prerequisites

1. A working SQL Server Master Data Services (MDS) instance.
2. [.NET Core v3.1.404](https://dotnet.microsoft.com/download/dotnet-core/3.1) or newer.
3. A server or service to host this API on. If you're using Microsoft Azure, the [Web App Service](https://azure.microsoft.com/en-us/services/app-service/web/) is a great option. 
4. (Optional) [Visual Studio Code](https://code.visualstudio.com/) or [Microsoft Visual Studio](https://visualstudio.microsoft.com/).


## Setup

1. Clone this repo on your machine.
1. Open and edit the `appsettings.Development.json` file to configure the MDS WCF endpoint URL and Windows credentials:

    ```json
    "Mds": {
        "ServiceUrl": "<your MDS WCF Service endpoint> - for example https://<server>/MDS/Service/Service.svc",
        "Credentials": {
            "Username": "<username>",
            "Password": "<password>",
            "Domain": "<domain>" 
        }
    }
    ```

1. Build and run the API locally:

    ```bash
    dotnet restore
    dotnet build
    dotnet run
    ```

1. Test that the service is configured correctly. 

    Open the `https://localhost:5001/MDS/SayHello` URL in a browser. You should receive a *Hello MDS!* message if your MDS configuration is correct.

1. (Optional) Download the OpenAPI specification file.

    Open the `https://localhost:5001/swagger/` URL and browse the Swagger page. You'll also have a link to download the `swagger.json` spec file.

## Hosting the API in Azure App Service

1. [Create a Web App](https://docs.microsoft.com/en-us/azure/app-service/quickstart-dotnetcore?tabs=netcore31&pivots=platform-linux) and deploy this API in Azure.

1. Configure the MDS WCF settings for the API in Azure.

    Open a cloud shell and run the following command (be sure to replace with your own values):

    ```sh
    az webapp config appsettings set --name <web app name> --resource-group <resource group> --settings Mds__ServiceUrl="<your MDS WCF Service endpoint>" Mds__Credentials__Username="<username>" Mds__Credentials__Password="<password>" Mds__Credentials__Domain="<domain>"
    ```

    If you prefer, you can use the [Azure portal](https://docs.microsoft.com/en-us/azure/app-service/configure-common) instead to add your settings. The correct names for the settings are:

    - `Mds__ServiceUrl`
    - `Mds__Credentials__Username`
    - `Mds__Credentials__Password`
    - `Mds__Credentials__Domain`

1. [Secure access to your API](https://docs.microsoft.com/en-us/azure/app-service/overview-authentication-authorization). This step is dependent on your organization's cloud security policies.

1. (Optional) If your MDS instance is hosted on-premises and/or not exposed via a public endpoint, you'll need to either configure [Vnet integration](https://docs.microsoft.com/en-us/azure/app-service/web-sites-integrate-with-vnet) or [Hybrid connections](https://docs.microsoft.com/en-us/azure/app-service/app-service-hybrid-connections) for your App Service instance, to allow the API to reach MDS.