FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Services/EmployeeService/EmployeeService.Common/EmployeeService.Common.csproj", "Services/EmployeeService/EmployeeService.Common/"]
COPY ["Services/EmployeeService/EmployeeService.API/EmployeeService.API.csproj", "Services/EmployeeService/EmployeeService.API/"]
RUN dotnet restore "Services/EmployeeService/EmployeeService.API/EmployeeService.API.csproj"
COPY . .
WORKDIR "/src/Services/EmployeeService/EmployeeService.API"
RUN dotnet build "EmployeeService.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EmployeeService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EmployeeService.API.dll"]