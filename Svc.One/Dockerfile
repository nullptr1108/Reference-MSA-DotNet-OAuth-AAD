FROM microsoft/dotnet:2.2-aspnetcore-runtime AS base
#Install the cert utils
WORKDIR /usr/local/share/ca-certificates
COPY ./Certificates/*.crt .
RUN update-ca-certificates -v
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM microsoft/dotnet:2.2-sdk AS build
WORKDIR /src
COPY ["Svc.One/Svc.One.csproj", "Svc.One/"]
COPY ["Lib.Service/Lib.Service.csproj", "Lib.Service/"]
COPY ["Lib.HealthChecks/Lib.HealthChecks.csproj", "Lib.HealthChecks/"]
RUN dotnet restore "Svc.One/Svc.One.csproj"
COPY . .
WORKDIR "/src/Svc.One"
RUN dotnet build "Svc.One.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "Svc.One.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Svc.One.dll"]