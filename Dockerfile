FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY Veterinaria.Api/Veterinaria.Api.csproj Veterinaria.Api/
RUN dotnet restore Veterinaria.Api/Veterinaria.Api.csproj
COPY Veterinaria.Api/ Veterinaria.Api/
RUN dotnet publish Veterinaria.Api/Veterinaria.Api.csproj -c Release -o /app/publish --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
COPY --from=build /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "Veterinaria.Api.dll"]
