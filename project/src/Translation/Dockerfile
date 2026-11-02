FROM mcr.microsoft.com/dotnet/sdk:8.0-bookworm-slim AS build
WORKDIR /src
COPY Backend/Translation.Backend.csproj Backend/
COPY Host/Translation.Host.csproj Host/
RUN dotnet restore Host/Translation.Host.csproj
COPY Backend/ Backend/
COPY Host/ Host/
RUN dotnet publish Host/Translation.Host.csproj -c Release --no-restore -o /out /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0-bookworm-slim AS runtime
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080 DOTNET_EnableDiagnostics=0
COPY --from=build /out/ ./
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "Translation.Host.dll"]
