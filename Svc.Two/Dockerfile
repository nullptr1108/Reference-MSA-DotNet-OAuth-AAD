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
COPY ["Svc.Two/Svc.Two.csproj", "Svc.Two/"]
COPY ["Lib.Service/Lib.Service.csproj", "Lib.Service/"]
COPY ["Lib.HealthChecks/Lib.HealthChecks.csproj", "Lib.HealthChecks/"]
RUN dotnet restore "Svc.Two/Svc.Two.csproj"
COPY . .
WORKDIR "/src/Svc.Two"
RUN dotnet build "Svc.Two.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "Svc.Two.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Svc.Two.dll"]