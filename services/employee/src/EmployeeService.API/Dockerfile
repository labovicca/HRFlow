FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/EmployeeService.Common/EmployeeService.Common.csproj", "src/EmployeeService.Common/"]
COPY ["src/EmployeeService.API/EmployeeService.API.csproj", "src/EmployeeService.API/"]
RUN dotnet restore "src/EmployeeService.API/EmployeeService.API.csproj"
COPY . .
WORKDIR "/src/src/EmployeeService.API"
RUN dotnet build "EmployeeService.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EmployeeService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EmployeeService.API.dll"]
