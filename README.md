# Repro-HeathChecksUI
Reproduction of issue with the AspNetCore.HealthChecks.UI

This application consists of 2 projects. One is the health monitor application and the second is a target API project that is meant to be monitored by the application. This solution is a docker based solution that can either be debugged through Visual Studio or through a docker-compose up command

Since both projects are using HTTPS, there is a powershell script which will generate developer test certificates and put the information in the secrets.json file for the projects. 

For simplicity I have added a heart beat validation that will return Healthy when ever called.
