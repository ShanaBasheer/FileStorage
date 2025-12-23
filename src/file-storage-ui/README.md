# FileStorageUi



# Angular Frontend — File Storage UI

Angular Frontend — File Storage UI
This is the Angular frontend for the File Storage Service.
It provides a clean UI for authentication, file upload, listing, preview, download, and delete operations.
The UI is built using Angular and follows a modular structure with dedicated components for each feature.



## 1\. Running the Frontend

### Install dependencies

npm install

### Start development server

ng serve --port 4200

The app runs at:
http://localhost:4200



# 2\. Configure API URL

API URL is configured in:
src/app/app.config.ts



Example:
export const appConfig = {
apiUrl: 'http://localhost:5015'
};



Or if provided via DI:
providers: \[
{ provide: 'API\_URL', useValue: 'http://localhost:5015' }
]
my services (auth, file operations) read the API URL from this config



## 3\. Features Implemented

# Authentication

* Login with username/password
* JWT stored in localStorage
* AuthGuard protects routes

# File Upload

* Drag \& drop upload
* Sucess message ater upload
* Validation
* Error handling

# File List

* Pagination
* Search - name
* Filters (content type, date)
* Auto-refresh after upload/delete

# File Preview

* PDF preview
* Image preview
* Inline streaming

# File Download

* Range-supported download
* Browser download dialog

# Delete

* Soft delete (admin only)
* Hard delete (admin only)
* UI refresh after delete

# UI/UX

* Responsive layout
* Toast notifications
* Clean component structure

# 4\. Folder Structure

file-storage-ui/
│
├── cypress/                     # Cypress E2E tests
│
├── public/                      # Static assets
│
├── src/
│   ├── index.html
│   ├── main.ts
│   ├── server.ts
│   ├── styles.css
│   │
│   └── app/
│       ├── assets/              # Images, icons, shared UI assets
│       │
│       ├── core/                # Core utilities (shared logic)
│       │
│       ├── login/               # Login page
│       │    ├── login.component.ts
│       │    ├── login.component.html
│       │    └── login.component.css
│       │
│       ├── home/                # Home/dashboard page
│       │
│       ├── file-list/           # File listing page
│       │
│       ├── upload/              # File upload page
│       │
│       ├── storage/             # Storage-related UI components
│       │
│       ├── auth.guard.ts        # Route protection
│       ├── auth.service.ts      # Login + token handling
│       ├── token-interceptor.ts # Attaches JWT to requests
│       │
│       ├── app.routes.ts        # Routing configuration
│       ├── app.component.ts
│       ├── app.component.html
│       └── app.component.css
│
├── angular.json
├── package.json
└── README.md                    # This file





 **API IntegrationAll API calls use the base URL from app.config.ts.Example usage inside a service:constructor(@Inject('API\_URL') private apiUrl: string) {}**

**Or:private baseUrl = appConfig.apiUrl;**

**🏗️ Build for Productionng build --configuration production**

**Output will be generated in:**

**dist/file-storage-ui/🐳 Run with Docker- Build image**

**docker compose build --no-cache**





**- Run containers**

**docker compose up -d**





**- Access UI**

**- http://localhost:4200 → Angular UI**

**- http://localhost:5015 → Backend API**

**📌 Notes for Evaluators- Global styles.css is intentionally empty.**

**- All design is implemented in component‑level CSS (e.g., login.component.css, file-list.component.css).**

**- UI loads correctly and is fully functional for testing authentication, upload, list, preview, download, and delete flows.**

**✅ Frontend StatusCompleted by: Shana Basheer PV**





