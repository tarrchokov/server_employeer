FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY Backend/*.csproj ./Backend/
RUN dotnet restore Backend/EmployeeDirectory.Api.csproj
COPY Backend/Program.cs ./Backend/
COPY Backend/appsettings.json ./Backend/
COPY Backend/Migrations/ ./Backend/Migrations/
WORKDIR /src/Backend
RUN dotnet publish EmployeeDirectory.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "EmployeeDirectory.Api.dll"]

