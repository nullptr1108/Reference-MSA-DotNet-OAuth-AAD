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
COPY ["Svc.B/Svc.B.csproj", "Svc.B/"]
COPY ["Lib.Service/Lib.Service.csproj", "Lib.Service/"]
COPY ["Lib.HealthChecks/Lib.HealthChecks.csproj", "Lib.HealthChecks/"]
RUN dotnet restore "Svc.B/Svc.B.csproj"
COPY . .
WORKDIR "/src/Svc.B"
RUN dotnet build "Svc.B.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "Svc.B.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Svc.B.dll"]
