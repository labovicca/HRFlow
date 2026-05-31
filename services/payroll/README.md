# Payroll Service

Mikroservis za obračun plata zaposlenih - deo HRFlow sistema.

## Tehnologije

- .NET 8
- Entity Framework Core 8
- PostgreSQL 15
- MediatR (CQRS)
- AutoMapper
- FluentValidation
- Docker & Docker Compose

## Arhitektura

Projekat koristi Clean Architecture sa CQRS pattern-om:

```
services/payroll/
├── src/
│   ├── Payroll.API/              # Web API layer (Controllers)
│   ├── Payroll.Application/      # Application layer (CQRS, DTOs, Interfaces)
│   │   ├── Commands/             # Create, Update, Delete, Calculate, Approve
│   │   ├── Queries/              # Get by ID, Get all with filters
│   │   ├── DTOs/                 # Data Transfer Objects
│   │   ├── Interfaces/           # Repository & Service interfaces
│   │   └── Mappings/             # AutoMapper profiles
│   ├── Payroll.Domain/           # Domain layer (Entities, Enums)
│   └── Payroll.Infrastructure/   # Infrastructure layer
│       ├── Persistence/          # EF Core DbContext, Configurations
│       ├── Repositories/         # Repository implementations
│       └── Services/             # PayrollCalculator
├── tests/
│   └── Payroll.UnitTests/        # Unit testovi
├── docker/
│   ├── Dockerfile
│   └── docker-compose.yml
└── docs/
```

## Pokretanje

### Sa Docker Compose (preporučeno)

```bash
cd docker
sudo docker-compose up -d
```

API: http://localhost:5003
Swagger: http://localhost:5003/swagger

### Lokalno

```bash
# Pokreni samo bazu
cd docker
sudo docker-compose up -d postgres-payroll

# Pokreni API
cd ../src/Payroll.API
dotnet run
```

## API Endpoints

### PayrollRuns

| Endpoint | Metoda | Opis |
|----------|--------|------|
| `/api/payroll/runs` | GET | Lista svih payroll run-ova (sa filterima) |
| `/api/payroll/runs/{id}` | GET | Detalji payroll run-a |
| `/api/payroll/runs` | POST | Kreiranje novog payroll run-a |
| `/api/payroll/runs/{id}/calculate` | POST | Obračun plate |
| `/api/payroll/runs/{id}/approve` | POST | Odobravanje payroll run-a |
| `/api/payroll/runs/{id}` | DELETE | Brisanje payroll run-a |

### PayrollConfigurations

| Endpoint | Metoda | Opis |
|----------|--------|------|
| `/api/payroll/configurations` | GET | Lista svih konfiguracija |
| `/api/payroll/configurations/{id}` | GET | Detalji konfiguracije |
| `/api/payroll/configurations` | POST | Kreiranje nove konfiguracije |
| `/api/payroll/configurations/{id}` | PUT | Ažuriranje konfiguracije |
| `/api/payroll/configurations/{id}` | DELETE | Brisanje konfiguracije |

### Health Check

| Endpoint | Metoda | Opis |
|----------|--------|------|
| `/api/health` | GET | Health check |

## Entiteti

- **PayrollRun** - Obračun plate za zaposlenog po mesecu
- **SalaryComponent** - Dodaci i odbici (bonus, porez, doprinosi)
- **PayrollConfiguration** - Konfiguracija plate (osnovica, stope)
- **Payslip** - Generisani payslip dokument

## Statusni dijagram PayrollRun

```
Draft → Calculated → Approved → Paid
          ↘            ↗
           Cancelled
```

## Obračun plate

Formula:
```
Bruto = Osnovna plata + Dodaci
Neto = Bruto - Porez - PIO - Zdravstveno - Nezaposlenost - Ostali odbici
```

Stope (konfigurisano u PayrollConfiguration):
- Porez na dohodak: TaxRate
- PIO doprinos: PensionContributionRate
- Zdravstveno: HealthInsuranceRate
- Nezaposlenost: UnemploymentInsuranceRate

## Environment varijable

| Varijabla | Opis |
|-----------|------|
| `ConnectionStrings__PayrollDb` | PostgreSQL connection string |
| `ASPNETCORE_ENVIRONMENT` | Development/Production |

## Autor

Luka - Payroll Service

## Status

U razvoju - Sprint 2 (CQRS implementiran)
