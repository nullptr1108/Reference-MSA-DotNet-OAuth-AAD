FROM microsoft/dotnet:2.2-aspnetcore-runtime AS base
# Copy self signed development certificates
WORKDIR /usr/local/share/ca-certificates
COPY ./dev_certs/*.crt .
RUN update-ca-certificates -v
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM microsoft/dotnet:2.2-sdk AS build
WORKDIR /src
COPY ["Svc.C/Svc.C.csproj", "Svc.C/"]
COPY ["Lib.Service/Lib.Service.csproj", "Lib.Service/"]
COPY ["Lib.HealthChecks/Lib.HealthChecks.csproj", "Lib.HealthChecks/"]
RUN dotnet restore "Svc.C/Svc.C.csproj"
COPY . .
WORKDIR "/src/Svc.C"
RUN dotnet build "Svc.C.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "Svc.C.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Svc.C.dll"]
