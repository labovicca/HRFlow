# Payroll Service

Mikroservis za obračun plata zaposlenih - deo HRFlow sistema.

## Tehnologije

- .NET 8
- Entity Framework Core 8
- PostgreSQL 15
- Docker & Docker Compose

## Struktura projekta

```
services/payroll/
├── src/
│   ├── Payroll.API/           # Web API layer
│   ├── Payroll.Application/   # Application layer (CQRS)
│   ├── Payroll.Domain/        # Domain layer (entiteti, enumi)
│   └── Payroll.Infrastructure/ # Infrastructure layer (EF Core, baza)
├── tests/
│   └── Payroll.UnitTests/     # Unit testovi
├── docker/
│   ├── Dockerfile
│   └── docker-compose.yml
└── docs/
```

## Pokretanje

### Sa Docker Compose (preporučeno)

```bash
cd docker
docker-compose up --build
```

API će biti dostupan na: http://localhost:5003
Swagger: http://localhost:5003/swagger

### Lokalno (bez Docker-a)

1. Pokreni PostgreSQL:
```bash
docker run -d --name payroll-postgres \
  -e POSTGRES_DB=payroll_db \
  -e POSTGRES_USER=payroll_user \
  -e POSTGRES_PASSWORD=payroll_pass \
  -p 5433:5432 \
  postgres:15-alpine
```

2. Pokreni migracije:
```bash
cd src/Payroll.Infrastructure
dotnet ef migrations add InitialCreate --startup-project ../Payroll.API
dotnet ef database update --startup-project ../Payroll.API
```

3. Pokreni API:
```bash
cd src/Payroll.API
dotnet run
```

## API Endpoints

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
                  ↘         ↗
                   Cancelled
```

## Environment varijable

| Varijabla | Opis | Default |
|-----------|------|---------|
| `ConnectionStrings__PayrollDb` | PostgreSQL connection string | - |
| `ASPNETCORE_ENVIRONMENT` | Environment (Development/Production) | Production |

## Autor

Luka - Payroll Service

## Status

U razvoju - Sprint 1
