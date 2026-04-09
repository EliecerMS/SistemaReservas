# SistemaReservas

Trabajo en progreso / Work in progress

SistemaReservas es un sistema de reserva de espacios full-stack con una arquitectura de doble interfaz. Este proyecto tiene fines demostrativos y se prevé su implementación en la nube en un futuro próximo y servicio de almacenamiento de imagenes en contenedor de Azure(blob storage) o AWS S3. 

SistemaReservas is a full-stack venue reservation system featuring a dual-frontend architecture, this project is for demonstration purposes with near future implementation in the cloud with storage image storage in Azure(blob storage) or AWS S3. 

Mira el readme en ingles o español  / Take a look at the readme file in English or Spanish

<details>
  <summary>🇺🇸 English Click to expand</summary>

> **Status:** Work in Progress  

SistemaReservas is a full-stack venue reservation system featuring a dual-frontend architecture:
- **ASP.NET Core MVC** (new) - Traditional server-rendered UI
- **React + Vite** (original) - Modern SPA UI

Both frontends consume the same **ASP.NET Core Web API**.

The API uses **Dapper** as a lightweight micro-ORM for high-performance database queries. 

At the moment the stored procedure used is:

| Procedure | Purpose |
|-----------|---------|
| `sp_Reservations_Create` | Creates a new reservation with validation |

This approach ensures:
- **Security**: Parameterized queries prevent SQL injection
- **Performance**: Stored procedures are pre-compiled
- **Maintainability**: Business logic in the database layer

- ** Stored Procedure**: sp_CreateReservation
- ** Uses UPDLOCK + ROWLOCK**: to prevent double-booking
- ** (race condition prevention for concurrent requests)**

---

## Architecture Overview

```
SistemaReservas/
├── SistemaReservas.Core/           # Domain entities & interfaces
├── SistemaReservas.Application/     # DTOs & application services
├── SistemaReservas.Infrastructure/   # Repositories, JWT, Dapper
├── SistemaReservas.Api/             # REST API (ASP.NET Core 9)
├── SistemaReservas.UI/              # React 19 SPA (Vite)
└── SistemaReservas.WebMVC/          # ASP.NET Core 9 MVC (NEW)
```

---

## Technology Stack

| Layer | Technology | Version |
|-------|------------|---------|
| Framework | ASP.NET Core | 9.0 |
| Language | C# | 13.0 |
| Database | SQL Server | 2022+ |
| ORM | Dapper | 2.1+ |
| Auth | JWT + Refresh Tokens | - |
| React | React | 19.2 |
| Build Tool | Vite | 7.3 |
| MVC | ASP.NET Core MVC | 9.0 |
| HTTP Client | Axios (React) / HttpClient (MVC) | 1.13 / - |

---

## Getting Started

### Prerequisites

- **.NET 9.0 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/9.0)
- **Node.js 20+** - [Download](https://nodejs.org/)
- **SQL Server** (LocalDB or full instance)
- **Visual Studio 2022** or **VS Code**

### Running the API

```bash
# From the solution root
cd SistemaReservas.Api
dotnet run
```

The API runs on `https://localhost:7148` with Swagger available at `/swagger`.

### Running the React UI

```bash
cd SistemaReservas.UI
npm install
npm run dev
```

The React app runs on `http://localhost:5173`.

### Running the MVC UI

```bash
cd SistemaReservas.WebMVC
dotnet run
```

The MVC app runs on `https://localhost:7001`.

---

## Environment Configuration

### Required Environment Variables

**API:**
```bash
ConnectionStrings__DefaultConnection="Server=.;Database=SistemaReservas;Trusted_Connection=True;TrustServerCertificate=True"
JwtSettings__Secret="your-256-bit-secret-key-min-32-chars"
JwtSettings__ExpiryMinutes="15"
JwtSettings__RefreshTokenExpiryDays="7"
```

**MVC:**
```bash
ApiSettings__BaseUrl="https://localhost:7148"
```

---

## API Endpoints

### Authentication

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/auth/register` | Register new user |
| POST | `/api/auth/login` | Login (sets cookies) |
| POST | `/api/auth/refresh` | Refresh access token |
| POST | `/api/auth/logout` | Logout (clears cookies) |

### Zones

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/zones` | List all zones (paginated) |
| GET | `/api/zones/{id}` | Get zone by ID |
| GET | `/api/zones/event-types` | Get all event types |

### Reservations

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/reservations` | Create reservation |
| GET | `/api/reservations/my` | Get user's reservations |
| GET | `/api/reservations` | Get all reservations (Admin) |
| PUT | `/api/reservations/{id}/confirm` | Confirm reservation (Admin) |
| DELETE | `/api/reservations/{id}` | Cancel reservation |

---

## Authentication Flow

Both frontends use **HTTP-only cookies** for authentication:

1. **Login** → POST `/api/auth/login` → API sets `auth_token` and `refresh_token` cookies
2. **Authenticated Requests** → Cookies sent automatically with each request
3. **Token Refresh** → On 401, client calls `/api/auth/refresh` to get new tokens
4. **Logout** → POST `/api/auth/logout` → Cookies deleted

### MVC Token Refresh Handler

The MVC project includes a `TokenRefreshHandler` (DelegatingHandler) that:
- Intercepts 401 responses automatically
- Calls `/api/auth/refresh` to obtain new tokens
- Updates cookies and retries the original request
- Signs out user if refresh fails

### React Axios Interceptor

The React project uses Axios interceptors for automatic token refresh:
- Queues requests while refresh is in progress
- Retries failed requests with new tokens
- Triggers `onSessionExpired` callback on refresh failure

---


### API (Clean Architecture)

| Practice | Implementation |
|----------|----------------|
| **Separation of Concerns** | Core, Application, Infrastructure, Api layers |
| **DTOs** | Request/Response objects separate from domain entities |
| **Interface-based Services** | `IAuthService`, `IZoneRepository`, etc. |
| **Dependency Injection** | All services registered in `DependencyInjection.cs` |
| **JWT with Refresh Tokens** | Secure stateless authentication with token rotation |
| **Paged Results** | `PagedResult<T>` for paginated endpoints |
| **Input Validation** | Data annotations on DTOs |

### React UI

| Practice | Implementation |
|----------|----------------|
| **Context API** | `AuthContext` for global auth state |
| **Axios Interceptors** | Automatic 401 handling and token refresh |
| **Environment Variables** | Vite environment variables for configuration |
| **Component-based** | Reusable UI components (Navbar, Carousel) |
| **Client-side Routing** | React Router with protected routes |

### MVC UI

| Practice | Implementation |
|----------|----------------|
| **Typed HttpClient** | `IHttpClientFactory` with named clients |
| **DelegatingHandler** | Centralized token refresh logic |
| **ViewModels** | Strict 1:1 mapping with Views |
| **Antiforgery Tokens** | `[ValidateAntiForgeryToken]` on all POST actions |
| **Cookie Proxy** | Replicates API Set-Cookie headers to browser |
| **Attribute Routing** | `[Route("[controller]/[action]")]` convention |

---

## Project Structure

```
SistemaReservas/
├── SistemaReservas.sln      # Solution file
├── SistemaReservas.Core/
│   ├── Entities/           # Domain models
│   ├── Interfaces/         # Repository interfaces
│   └── PagedResult.cs      # Pagination helper
├── SistemaReservas.Application/
│   ├── DTOs/               # Data transfer objects
│   ├── Services/           # Application services
│   └── Interfaces/         # Service interfaces
├── SistemaReservas.Infrastructure/
│   ├── Auth/               # JWT & password hashing
│   └── Repositories/       # Dapper implementations
├── SistemaReservas.Api/
│   ├── Controllers/        # API endpoints
│   └── Program.cs          # API configuration
├── SistemaReservas.UI/
│   ├── src/
│   │   ├── components/     # Reusable React components
│   │   ├── context/        # Auth context
│   │   ├── pages/          # Page components
│   │   └── services/       # API client
│   └── package.json
└── SistemaReservas.WebMVC/
    ├── Controllers/        # MVC controllers
    ├── Services/           # HTTP client services
    ├── Models/
    │   ├── ApiDTOs/       # API DTOs
    │   └── ViewModels/    # View models
    └── Views/              # views
```

---

This project is for demonstration purposes.
</details>



<details>
  <summary> es Español Haz clic para ampliar</summary>

> **Estado:** Trabajo en progreso 

SistemaReservas es un sistema de reservas de locales full-stack que cuenta con una arquitectura de doble frontend:
- **ASP.NET Core MVC** (nuevo) - Interfaz tradicional renderizada por servidor
- **React + Vite** (original) - Interfaz SPA moderna

Ambos frontends consumen la misma **ASP.NET API web central**.

La API utiliza **Dapper** como un micro-ORM ligero para consultas de bases de datos de alto desempeño. 

Actualmente, el procedimiento almacenado utilizado es:

| Procedimiento | Propósito |
|-----------|---------|
| 'sp_Reservations_Create' | Crea una nueva reserva con validación |

Este enfoque asegura:
- **Seguridad**: Las consultas parametrizadas impiden la inyección SQL
- **Rendimiento**: Los procedimientos almacenados están precompilados
- **Mantenibilidad**: Lógica de negocio en la capa de base de datos

- ** Procedimiento almacenado**: sp_CreateReservation
- ** Utiliza ACTUALIZACIÓN + ROWLOCK**: para evitar la doble reserva
- ** (prevención de condiciones de carrera para solicitudes concurrentes)**

---

## Resumen de la arquitectura

```
SistemaReservas/
├── SistemaReservas.Core/ # Entidades e interfaces de dominio
├── SistemaReservas.Application/     # DTOs & application services
├── SistemaReservas.Infrastructure/ # Repositories, JWT, Dapper
├── SistemaReservas.Api/             # REST API (ASP.NET Core 9)
├── SistemaReservas.UI/              # React 19 SPA (Vite)
└── SistemaReservas.WebMVC/          # ASP.NET Core 9 MVC (NEW)
```

---

## Pila de Tecnología

| Capa | Tecnología | Versión |
|-------|------------|---------|
| Marco | ASP.NET Núcleo | 9.0 |
| Lenguaje | C# | 13.0 |
| Base de datos | SQL Server | 2022+ |
| ORM | Dapper | 2.1+ |
| Auth | JWT + Tokens de actualización | - |
| React | React | 19.2 |
| Herramienta de construcción | Vite | 7.3 |
| MVC | ASP.NET MVC central | 9.0 |
| HTTP Client | Axios (React) / HttpClient (MVC) | 1.13 / - |

---

## Usar

### Requisitos previos

- **.NET 9.0 SDK** - [Descargar](https://dotnet.microsoft.com/download/dotnet/9.0)
- **Node.js 20+** - [Download](https://nodejs.org/)
- **SQL Server** (LocalDB o instancia completa)
- **Visual Studio 2022** o **VS Code**

### Ejecutar la API

```bash
# Desde la raíz de la solución
cd SistemaReservas.Api
dotnet run
```

La API funciona en 'https://localhost:7148' con Swagger disponible en '/swagger'.

### Ejecutando la interfaz de React

```bash
cd SistemaReservas.UI
npm install
npm run dev
```

La app React funciona en 'http://localhost:5173'

### Ejecutar la interfaz MVC

```bash
cd SistemaReservas.WebMVC
dotnet run
```

La app MVC funciona en 'https://localhost:7001'.

---

## Configuración del entorno

### Variables de Entorno Requeridas

**API:**
```bash
ConnectionStrings__DefaultConnection="Server=.;Database=SistemaReservas;Trusted_Connection=True;TrustServerCertificate=True"
JwtSettings__Secret="your-256-bit-secret-key-min-32-chars"
JwtSettings__ExpiryMinutes="15"
JwtSettings__RefreshTokenExpiryDays="7"
```

**MVC:**
```bash
ApiSettings__BaseUrl="https://localhost:7148"
```

---

## Endpoints API

### Autenticación

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| POST | '/api/auth/register' | Registrar nuevo usuario |
| POST | '/api/auth/login' | Iniciar sesión (sets cookies) |
| POST | '/api/auth/refresh' | Actualizar token de acceso |
| POST | '/api/auth/logout' | Cerrar sesión (borra cookies) |

### Zonas

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | '/api/zones' | Listar todas las zonas (paginadas) |
| GET | '/api/zones/{id}' | Obtener zona por ID |
| GET | '/api/zonas/tipos de evento' | Obtén todos los tipos de eventos |

### Reservas

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| POST | '/api/reservations' | Crear reserva |
| GET | '/api/reservations/my' | Obtén reservas de usuarios |
| GET | '/api/reservations' | Consigue todas las reservas (Admin) |
| PUT | '/api/reservations/{id}/confirm' | Confirmar reserva (Admin) |
| BORRAR | '/api/reservations/{id}' | Cancelar reserva |

---

## Flujo de autenticación

Ambas interfaces utilizan **cookies solo HTTP** para la autenticación:

1. **Inicio de sesión** → POST '/api/auth/login' → API establece cookies 'auth_token' y 'refresh_token'
2. **Solicitudes autenticadas** → Cookies enviadas automáticamente con cada solicitud
3. **Actualización de token** → En 401, el cliente llama a '/api/auth/refresh' para obtener nuevos tokens
4. **Cerrar sesión** → POST '/api/auth/logout' → Cookies eliminadas

### Gestor de actualización de tokens MVC

El proyecto MVC incluye un 'TokenRefreshHandler' (DelegandoHandler) que:
- Intercepta automáticamente las respuestas 401
- Llama a '/api/auth/refresh' para obtener nuevos tokens
- Actualiza las cookies y vuelve a intentar la solicitud original
- Desconecta al usuario si falla la actualización

### Interceptor Axios de React

El proyecto React utiliza interceptores Axios para la actualización automática de tokens:
- Cola las solicitudes mientras la actualización está en curso
- Reintentos de solicitudes fallidas con nuevos tokens
- Activa la llamada 'onSessionExpired' en caso de fallo de actualización

---

### API (Clean Architecture)

| Práctica | Implementación |
|----------|----------------|
| **Separación de preocupaciones** | Núcleo, Aplicación, Infraestructura, Capas API |
| **DTOs** | Objetos de Solicitud/Respuesta separados de las entidades de dominio |
| **Servicios basados en interfaz** | 'IAuthService', 'IZoneRepository', etc. |
| **Inyección de dependencia** | Todos los servicios registrados en 'DependencyInjection.cs' |
| **JWT con fichas de refresco** | Autenticación segura sin estado con rotación de token |
| **Resultados paginados** | 'PagedResult<T>' para endpoints paginados |
| **Validación de entrada** | Anotaciones de datos sobre DTOs |

### React UI

| Práctica | Implementación |
|----------|----------------|
| **API de contexto** | 'AuthContext' para el estado de autenticación global |
| **Interceptores Axios** | Gestión automática del 401 y actualización de tokens |
| **Variables de entorno** | Variables de entorno Vite para configuración |
| **Basado en componentes** | Componentes reutilizables de la interfaz (Navbar, Carusel) |
| **Enrutamiento del lado del cliente** | Router React con rutas protegidas |

### UI MVC

| Práctica | Implementación |
|----------|----------------|
| **Cliente Http Tipado** | 'IHttpClientFactory' con clientes nombrados |
| ** DelegatingHandler ** | Lógica centralizada de actualización de tokens |
| ** ViewModels ** | Mapeo estricto 1:1 con vistas |
| ** Antiforgery Tokens ** | '[ValidateAntiForgeryToken]' en todas las acciones POST |
| ** Cookie Proxy ** | Replica cabeceras API Set-Cookie al navegador |
| ** Attribute Routing ** | `[Route("[controller]/[action]")]` convention |

---

## Estructura del proyecto

```
SistemaReservas/
├── SistemaReservas.sln      # Solution file
├── SistemaReservas.Core/
│ ├── Entities / # Modelos de dominio
│ ├── Interfaces / # Interfaces de repositorio
│ └── PagedResult.cs # Ayudante de paginación
├── SistemaReservas.Application/
│ ├── DTOs/ # Objetos de transferencia de datos
│ ├── Services / # Servicios de aplicación
│ └── Interfaces / # Interfaces de servicio
├── SistemaReservas.Infrastructure/
│ ├── Auth / # JWT y hashing de contraseñas
│ └── Repositories / # Implementaciones Dapper
├── SistemaReservas.Api/
│ ├── Controllers / # API endpoints
│ └── Program.cs # API configuration
├── SistemaReservas.UI/
│   ├── src/
│   │   ├── components /     # Reusable React components
│   │   ├── context /        # Auth context
│   │   ├── pages /          # Page components
│   │   └── services /       # API client
│   └── package.json
└── SistemaReservas.WebMVC/
    ├── Controladores/ # Controladores MVC
    ├── Services/           # servicios HTTP del cliente
    ├── Modelos/
 │ ├── ApiDTOs/ # API DTOs
│ └── ViewModels/ # Modelos de vista
└── Views / # Vistas
```

---

Este proyecto es para fines demostrativos.
</details>
