FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["src/Payroll.API/Payroll.API.csproj", "src/Payroll.API/"]
COPY ["src/Payroll.Application/Payroll.Application.csproj", "src/Payroll.Application/"]
COPY ["src/Payroll.Domain/Payroll.Domain.csproj", "src/Payroll.Domain/"]
COPY ["src/Payroll.Infrastructure/Payroll.Infrastructure.csproj", "src/Payroll.Infrastructure/"]

RUN dotnet restore "src/Payroll.API/Payroll.API.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/src/Payroll.API"
RUN dotnet build "Payroll.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Payroll.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Payroll.API.dll"]
