ARG APP_PORT=7185
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore src/promotions_services.api/promotions_services.api.csproj
RUN dotnet publish src/promotions_services.api/promotions_services.api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
ARG APP_PORT=7185
ENV ASPNETCORE_URLS=http://*:${APP_PORT}
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE ${APP_PORT}
ENTRYPOINT ["dotnet", "promotions_services.api.dll"]