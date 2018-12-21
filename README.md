# Reference MSA DotNet AAD-B2C
This repository shows a basic micro service application with 3 service end points. Each of the end points have been configured to use Swagger for documentation and identification of services. Additionally there is a standard Health Monitor application that has been included in the solution. This application will be secured via Azure AD B2C and will have configuration details later in the readme

## Kestrel Applications
The standard .Net Core 2.2 approach for web service end points have been extended in this reference and are accomplished through a Kestrel served end point that runs in a Docker container.

## Swagger Implmentation
ToDo - Add Swagger Details

## Health Monitor Application
The health monitor application is an implmentation of the AspNetCore.HealthChecks project. The monitor application currently extends a heartbeat monitor in each of the service end points which will return healthy. This can be extended in multiple ways and there will be additional examples of this throughout the reference guides.

## Self Signed Certificates
Since this project is meant to run outside of the Visual Studio environment a solution needed to be found that would allow for the creation of self signed certificates that could be used by the containers. This is to allow for the SSL protected communication within the container orchestration. See the Reference-SelfSignedCerts repo for more details around how self signed developer certificates are being used in this reference project

## Azure AD B2C
This section will walk through all the steps that were needed to enable Azure AD B2C in this application

### Azure Set Up
The first step is to set everything up in Azure. This borrows heavily from a guide published at [ASP.NET Core, Swagger and seamless integration with Azure B2C](http://blog.codenova.pl/post/asp-net-core-swagger-and-seamless-integration-with-azure-b2c). This setup assumes there has been nothing added into Azure and will give a complete start to finish view of the steps to setup a B2C tenant, link it to an existing subscription and add an application profile

So the first step is to add in the Azure B2C Tenant.
![AzurePic1](/dev_images/2018-12-21_14-35-52.png)

Once we have selected to add an Azure B2C tenant, we are presented with the options to Create a new tenant or link an existing tenant to our subscription. We first must create the tenant, then the final step will be to link this tenant back into our subscription. 
![AzurePic2](/dev_images/2018-12-21_14-49-39.png)

For this example the tenant will be named for the purpose that it serves. In a production environment this would likely be an organization level. But in this example we are creating the following tenant
![AzurePic3](/dev_images/2018-12-21_14-53-02.png)

Now that the tenant has been created follow the link shown below to go into the tenant management blade to continue configuring our tenant. 
![AzurePic4](/dev_images/2018-12-21_14-58-10.png)

The first thing to note here is that there is a warning message that the newly created template needs to be linked to a subscription. This was referenced in the first step and will be addressed once everything else is configured.
![AzurePic5](/dev_images/2018-12-21_15-00-37.png)

Since this is a brand new tenant there will be no applications linked in so the first one needs to be added
![AzurePic6)(/dev_images/2018-12-21_15-03-44.png)

There are several things to note while creating the initial web application within the AAD-B2C tenant
* The application needs to be given a name. This typically would be a solution level name and in this case ReferenceAppAADB2C has been chosen
* This application is a microservice application so we select the Web Application / Web API switch
* This sample is using the Swagger OAUTH functionality on the client so there is no need to build a client to display functionality. Since this is the case you will not the various swagger OAuth endpoints that have been listed. Once the authorization flow has been completed in B2C this is where the tokens will be sent
* For simplicity sake the AppId URI reuses the application name but this does not have to be synced. 
![AzurePic7](/dev_images/2018-12-21_15-09-12.png)

The application has been created and an application ID has been specified. The next step will be to generate an application key. You can do this by going into the Key section and following the directions on the screen. For our purposes the interesting stuff is in the published scopes portion.
!{AzurePic8](/dev_images/2018-12-21_15-16-29.png)

By default the application comes with the scope for User_Impersonation. For this demo one for Read and one for Write need to be added. These will be used for granular control within the end points that are defined in the application. This is a simplistic example, read and write, but the concepts will carry over into a larger solution if needed
![AzurePic9](/dev_images/2018-12-21_15-22-34.png)

After the published scopes have been added they will need to be collected into an API access profile. For this profile name we will simply use the API Id URI name that was established before to build a collection of allowed scopes.
![AzurePic10](/dev_images/2018-12-21_15-28-00.png)

Once the published scopes have been made available through the API Access section we have completed the configuration for the Application. We can now go back to link the tenant that has been added into the active subscription. To do this, you will need to be in the directory for the subscription that you want the tenant added to. To accomplish this, select the change directory icon in the header and select the directory desired.
![AzurePic11](/dev_images/2018-12-21_15-32-00.png)

From inside the desired subscription select create resource > Identity > Azure AD B2C in the same way that was done to create the tenant. But rather than creating a new tenant we will be linked existing tenant. Once that is selected you will pick the B2C tenant that you want to link, identify the subscription that you want it linked to. This will be created inside a resource group, and you are given the option to create a new RG, which we have done in this example. 
![AzurePic12](/dev_images/2018-12-21_15-35-46.png)

At this point all the work that needs to be done in Azure is completed. 
