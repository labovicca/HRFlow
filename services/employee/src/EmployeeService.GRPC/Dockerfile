FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/EmployeeService.Common/EmployeeService.Common.csproj", "src/EmployeeService.Common/"]
COPY ["src/EmployeeService.GRPC/EmployeeService.GRPC.csproj", "src/EmployeeService.GRPC/"]
RUN dotnet restore "src/EmployeeService.GRPC/EmployeeService.GRPC.csproj"
COPY . .
WORKDIR "/src/src/EmployeeService.GRPC"
RUN dotnet build "EmployeeService.GRPC.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EmployeeService.GRPC.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EmployeeService.GRPC.dll"]
