
File Storage Service — Full Project Documentation

A secure, streaming‑based file storage system built with ASP.NET Core and Angular, implementing all mandatory requirements from the assessment:
- JWT authentication
- File upload (streaming)
- File preview
- File download (Range support)
- Soft delete
- Hard delete (admin only)
- Pagination, search, filtering
- SHA‑256 checksum
- Metadata stored in database
- Files stored in local filesystem
- Health checks
- Structured logging
- E2E test (Cypress)
This README explains how to run the entire project, the architecture, design decisions, limitations, and future enhancements.

1. Project Structure
1. 
FileStorage/

src/
 ├── Api/             # ASP.NET Core API layer --- ← BACKEND API (Dockerfile here)

 │    ├── README.md               # Backend-specific documentation
 │    ├── Controllers/            # REST endpoints
 │    ├── DTOs/                   # Request/response models
 │    └── Program.cs
 │
 ├── Application/     # Application layer (use cases, interfaces)
 │    ├── Services/
 │    └── Interfaces/
 │
 ├── Domain/          # Domain layer (entities, rules)
 │    ├── Entities/
 │    ├── ValueObjects/
 │    └── Enums/
 │
 └── .Infrastructure/  # Infrastructure layer (EF Core, storage)
      ├── Migrations/
      ├── DbContext/
      └── FileSystemStorage/
      ├── file-storage-ui/         ← Angular frontend
 │
 ├── docker-compose.yml       ← ROOT LEVEL
 └── README.md


    2. Running the Project

2.1 Prerequisites
- .NET 8 SDK
- Node.js + npm
- Angular CLI
- Docker & Docker Compose
- sqlserver (only if running without Docker)

2. Running the Project with Docker (Recommended)

From the project root:

```bash
docker-compose up -d --build
```
This builds and starts the full solution:
- **Backend API**: http://localhost:5015
  - Swagger UI: http://localhost:5015/swagger
- **Frontend UI**: http://localhost:4200
- **SQL Server**: localhost:1433  (User: sa / Pass: YourStrong@Password123)

Wait a few seconds for SQL Server to initialize. The API will automatically apply database migrations on startup.


2.4 Run Frontend Locally
cd src/file-storage-ui  ....
npm install
ng serve --port 4200

Frontend runs at: http://localhost:4200

Configure API URL in:
src/environments/environment.ts

3. Architecture Overview
This solution follows a clean, layered architecture.

3.1 Domain Layer
- Entities
- Value objects
- Business rules
- No external dependencies


3.2 Infrastructure Layer
- EF Core DbContext
- Migrations
- Repositories
- Local file storage implementation
- 
- SQL migrations are located in src/Infrastructure/Migrations
- cd src/Api
dotnet ef database update

3.3 API Layer

- Controllers
- DTOs
- Validation
- Health checks
- Logging
- Dependency injection
- Readme


3.4 Frontend (Angular)
- Login
- Upload
- File list
- Preview
- Download
- Delete
- Filters + pagination


      4. Design Decisions

4.1 Storage Strategy
- Files stored using GUID‑based internal names
- Prevents collisions
- Prevents directory traversal
- Original filename stored in DB

4.2 Streaming

- Uploads use streaming (CopyToAsync)
- Downloads use streaming (FileStream)
- Supports large files (100 MB+)

4.3 Security
- JWT authentication
- Role‑based delete (admin only)
- Filename sanitization
- Directory traversal protection
- Max upload size enforced

4.5 Health Checks
- /health/live → app running
- /health/ready → DB + filesystem
-
4.6 Error Handling
- Consistent error responses
- Validation errors return structured messages

5. Features Implemented
    JWT Authentication
    Upload (streaming)
    Preview
   Download (Range)
   Soft Delete
   Hard Delete
   Pagination
   Search
   Filters
SHA‑256 checksum
Health checks
Local filesystem storage
E2E test (Cypress)


6. Known Limitations
- Upload E2E test skipped (manual OK)
- Local filesystem only (no cloud storage)
- No advanced RBAC
- Limited unit tests
- Angular not containerized  

7. Future Enhancements
- Add S3 / Azure Blob storage provider
- Add Playwright upload test
- Add audit logging
- Add background cleanup for soft deletes
- Add Angular container to docker-compose
- Add correlation ID middleware

8. End‑to‑End Test Summary
Cypress test covers:
- Login
- File list
- Download (206 Partial Content)
  Upload tested manually  
 

 Docker Setup (Recommended)
1. Install Docker
- Install Docker Desktop (Windows 11 or later).
- Ensure both Docker and Docker Compose are available in your terminal.
2. Run Docker Compose
From the project root (where docker-compose.yml is located):
docker compose up -d --build


This builds and starts the full solution:
- Backend API → http://localhost:5015
- Swagger UI → http://localhost:5015/swagger
- Frontend UI → http://localhost:4200
- SQL Server → localhost:1433
- User: sa
- Password: YourStrong@Password123
👉 Wait a few seconds for SQL Server to initialize. The API will automatically apply database migrations on startup.
3. Logs & Health Checks
- View API logs:
docker logs filestorage_api
- Health endpoints:
- Live → http://localhost:5015/health/live
- Ready → http://localhost:5015/health/ready
4. Connection Strings
- Local development → Windows Authentication (configured in appsettings.Development.json).
- Docker environment → SQL Authentication (sa + password above).
- If connection fails, check that SQL Server container is running and credentials match docker-compose.yml.



## Final Notes

This project was designed with a layered architecture to simulate real-world production systems.  
The following points summarize the key technical decisions and considerations:

# Architecture
- **Api layer**: Handles HTTP requests, controllers, Swagger/OpenAPI, health checks.
- **Application layer**: Contains business logic, use cases, and orchestrates domain + infrastructure.
- **Domain layer**: Defines core entities, aggregates, and contracts independent of frameworks.
- **Infrastructure layer**: Implements persistence (EF Core, SQL Server), file storage, and external integrations.
- Clear separation of concerns ensures maintainability, testability, and scalability.

# Code Structure
- Organized into four projects (`Api`, `Application`, `Domain`, `Infrastructure`) with strict boundaries.
- Dependency flow: Api → Application → Domain; Infrastructure implements interfaces from Domain.
- Consistent naming conventions, comments, and modular design.

# Security
- JWT authentication for API endpoints.
- File upload logic validates file types and blocks dangerous executables.
- SHA-256 checksum ensures file integrity.
- HTTPS ready for production deployment.

# API Design
- RESTful endpoints with predictable routes (`/api/files`, `/api/auth`, `/api/health`).
- Pagination, filtering, and search supported in file listing.


# Complexity Handling
- Large features broken into smaller modules (upload, preview, delete, authentication).
- Structured logging and error handling for easier debugging.
- Docker setup provided for containerized deployment, with fallback to local run.

# Communication
- README and inline documentation explain setup, architecture, and design choices.
- Technical decisions narrated with clarity, simulating real-world engineering communication.



  Author
     Developed by:
     Shana Basheer PV
 























- 

