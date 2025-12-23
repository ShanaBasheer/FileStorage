src-inside API
Backend – File Storage Service (ASP.NET Core)

A secure, streaming‑based file storage backend built using ASP.NET Core, implementing all mandatory requirements from the assessment:
 JWT authentication
 File upload (streaming)
 File preview
 File download (Range supported)
 Soft delete
 Hard delete (admin only)
 Pagination, search, filtering
 SHA‑256 checksum
 Metadata stored in database
 Files stored in local filesystem
 Health checks (live + ready)
 Structured logging

1. Overview
This backend exposes a REST API for securely uploading, listing, previewing, downloading, and deleting files.
All file metadata is stored in the database, while actual file content is stored in the local filesystem using a safe, GUID‑based naming strategy.
The API is designed for:
- High‑volume file operations
- Large file streaming
- Safe filesystem writes
- Clean architecture separation
- Production‑ready health checks

2. Tech Stack
 
 Component                Technology
 Backend Framework        ASP.NET Core 8
 Database                 SQL Server (configurable
 ORM                      EF Core
 Auth                     JWT
 Logging                  Structured logging
 Storage               Local filesystem (storage/ folder)
 Architecture          Clean layered architecture



3. Project Structure
FileStorage.Api/
│
├── Controllers/          # API endpoints
├── Services/             # File storage, hashing, business logic
├── DTOs/                 # Request/response models
├── Models/               # Domain models
├── storage/              # Uploaded files stored here
├── appsettings.json
└── Program.cs


Database migrations are located in:
FileStorage.Infrastructure/Migrations/



4. Authentication
Login
POST /api/auth/login


Body
{
  "username": "admin",
  "password": "admin123"
}


Response includes:
- JWT token
- Role
- User info
Use the token in all protected endpoints:
Authorization: Bearer <token>



5. File Operations


1. Upload File -- POST /api/files
Headers
Authorization: Bearer <token>
Content-Type: multipart/form-data


Body
file: <choose file>


Response metadata
- id
- key
- originalName
- sizeBytes
- contentType
- checksum (SHA‑256)
- createdAtUtc
- version
- path

5.2 List Files (Pagination + Search + Filters)
GET /api/files


Query Parameters
Name          Description

page         Page number
pageSize     Items per page
search       Search by filename

contentType  Filter by MIME type
fromDate     Filter start date
toDate       Filter end date     


Example
GET /api/files?page=1&pageSize=10&search=doc

5.3 Preview File
GET /api/files/{id}/preview

Streams file content directly (PDF, images, text, etc.)

5.4 Download File (Range Supported)
    GET /api/files/{id}/download

 Headers
 Authorization: Bearer <token>
 Range: bytes=0-1000

 Response
- 206 Partial Content (if range requested)
- Streaming file conten


5.5 Soft Delete
DELETE /api/files/{id}

 Effect
- Marks file as deleted in DB
- File remains in storage
- Hidden from list


5.6 Hard Delete (Admin Only)
DELETE /api/files/{id}/hard
Effect
- Removes file from filesystem
- Removes metadata from DB


6. Health Checks
 Live
 GET /health/live
Checks if API is running.

Ready
GET /health/ready

Checks:
- Database connectivity
- Filesystem read/write
  Expected : Healthy
 

7. Database Schema (StoredObjects Table)

Column             Type     Description
Id                 GUID     Primary key
Key                string   Internal GUID filenam
OriginalName       string   User‑uploaded filename
SizeBytes          long     File size
ContentType        string   MIME type
Checksum           string   SHA‑256 hash
CreatedAtUtc       string   Upload timestamp
DeletedAtUtc       datetime? Soft delete timestamp
Version             int       Versioning support
Path               string           Storage path


8. Postman Testing Checklist
-   Login
-   Upload
-   List
-   Pagination
-   Search
-   Preview
-   Download (206 Partial Content)
-   Soft Delete
-   Hard Delete
-   Health Checks


 Run with Docker

1. Build the API Image
From the project root (where your Dockerfile and docker-compose.yml are located):
docker compose build --no-cache


2. Start Containers
docker compose up -d


3. Access Services
- Backend API → http://localhost:5015
- Health Checks →
- Live: http://localhost:5015/health/live
- Ready: http://localhost:5015/health/ready
4. Logs
View API logs:
docker logs filestorage_api


5. Notes
- SQL Server container must be running (configured in docker-compose.yml).
- API connects to SQL Server using connection string in appsettings.json.
- Health checks validate both database connectivity and filesystem read/write.

 Backend Status

 
Completed by: Shana Basheer PV
