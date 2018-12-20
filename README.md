# Reference MSA DotNet
This repository shows a basic micro service application with 3 service end points. Each of the end points have been configured to use Swagger for documentation and identification of services. Additionally there is a standard Health Monitor application that has been included in the solution.

## Kestrel Applications
The standard .Net Core 2.2 approach for web service end points have been extended in this reference and are accomplished through a Kestrel served end point that runs in a Docker container.

## Health Monitor Application
The health monitor application is an implmentation of the AspNetCore.HealthChecks project. The monitor application currently extends a heartbeat monitor in each of the service end points which will return healthy. This can be extended in multiple ways and there will be additional examples of this throughout the reference guides.

## Self Signed Certificates
Since this project is meant to run outside of the Visual Studio environment a solution needed to be found that would allow for the creation of self signed certificates that could be used by the containers. This is to allow for the SSL protected communication within the container orchestration. See the Reference-SelfSignedCerts repo for more details around how self signed developer certificates are being used in this reference project
