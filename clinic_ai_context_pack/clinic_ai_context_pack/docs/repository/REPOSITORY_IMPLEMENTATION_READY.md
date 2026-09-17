# Repository / Solution Structure — Implementation Ready

## Status
Repository / Solution Structure is COMPLETE / IMPLEMENTATION-READY.

## Required Tree
```text
ClinicManagementSystem/
├── README.md
├── LICENSE
├── .gitignore
├── .editorconfig
├── Directory.Build.props
├── Directory.Build.targets
├── Directory.Packages.props
├── global.json
├── docs/
│   ├── architecture/
│   ├── requirements/
│   ├── api/
│   ├── synchronization/
│   ├── security/
│   └── deployment/
├── src/
│   ├── Client/
│   │   ├── Clinic.Client.Wpf/
│   │   ├── Clinic.Client.Application/
│   │   ├── Clinic.Client.Domain/
│   │   ├── Clinic.Client.Infrastructure/
│   │   └── Clinic.Client.Sync/
│   ├── Authority/
│   │   ├── Clinic.Authority.Api/
│   │   ├── Clinic.Authority.Application/
│   │   ├── Clinic.Authority.Domain/
│   │   ├── Clinic.Authority.Infrastructure/
│   │   └── Clinic.Authority.Worker/
│   └── Contracts/
│       └── Clinic.Contracts/
├── tests/
│   ├── Unit/
│   │   ├── Clinic.Client.Domain.Tests/
│   │   ├── Clinic.Client.Application.Tests/
│   │   ├── Clinic.Authority.Domain.Tests/
│   │   └── Clinic.Authority.Application.Tests/
│   ├── Integration/
│   │   ├── Clinic.Client.Infrastructure.Tests/
│   │   ├── Clinic.Authority.Infrastructure.Tests/
│   │   ├── Clinic.Sync.Integration.Tests/
│   │   └── Clinic.Persistence.Integration.Tests/
│   ├── Contract/
│   │   └── Clinic.Contracts.Tests/
│   └── EndToEnd/
│       └── Clinic.EndToEnd.Tests/
├── database/
│   ├── authority/
│   │   └── migrations/
│   └── workstation/
│       └── migrations/
├── deployment/
│   ├── authority/
│   ├── workstation/
│   ├── packages/
│   ├── scripts/
│   └── configuration/
└── tools/
    ├── development/
    ├── migration/
    └── diagnostics/
```

## Layer Responsibilities
- WPF: presentation only; no direct PostgreSQL or raw sync-table access.
- Client.Application: local use-case orchestration/application services.
- Client.Domain: workstation-local domain concepts/rules required by client; no WPF/HTTP/EF/SQLite implementation dependency.
- Client.Infrastructure: SQLite/local infrastructure and persistence implementation.
- Client.Sync: business-aware synchronization engine and durable sync state handling.
- Authority.Api: external boundary / transport adapter.
- Authority.Application: Authority use-case orchestration.
- Authority.Domain: authoritative business rules and invariants.
- Authority.Infrastructure: PostgreSQL/EF Core/persistence implementation.
- Authority.Worker: background processing inside Authority boundary.
- Contracts: actual boundary contracts only; not EF entities or domain entities.

## Dependency Direction
`UI → Application → Domain`

Infrastructure implements outward-facing needs; Domain does not depend on UI, HTTP, SQLite or EF implementation details.

`Authority transport/adapters → Authority Application → Authority Domain`

Authority Infrastructure implements persistence/outbound needs.

## Repository Invariants
- WPF never talks directly to PostgreSQL.
- WPF never mutates raw synchronization tables as substitute for business operations.
- Domain projects do not depend on UI/HTTP/EF/SQLite implementation specifics.
- Infrastructure does not redefine Domain/Application business invariants.
- Reporting/read models do not mutate authoritative business state.
- Technical logs are not business audit.
- Business audit is not generic logging.
- Sync does not bypass Authority authorization/domain/audit boundaries.
- Authority PostgreSQL migrations and workstation SQLite migrations are separate.
- Tests are separated into Unit / Integration / Contract / EndToEnd.
- Deployment/package structure remains outside application source projects.
- Namespace root is an implementation convention and can evolve if frozen boundaries remain unchanged.

## Source
Master handoff pages 12–15.
