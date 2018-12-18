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
COPY ["Svc.Three/Svc.Three.csproj", "Svc.Three/"]
COPY ["Lib.Service/Lib.Service.csproj", "Lib.Service/"]
COPY ["Lib.HealthChecks/Lib.HealthChecks.csproj", "Lib.HealthChecks/"]
RUN dotnet restore "Svc.Three/Svc.Three.csproj"
COPY . .
WORKDIR "/src/Svc.Three"
RUN dotnet build "Svc.Three.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "Svc.Three.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Svc.Three.dll"]