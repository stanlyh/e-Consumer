# PersonDataApp

Aplicación full-stack para consulta de datos de personas por número de documento. El frontend consulta una API REST que aplica una estrategia de caché con base de datos SQL Server para minimizar llamadas al servicio externo.

---

## Tabla de contenidos

- [Estructura del proyecto](#estructura-del-proyecto)
- [Arquitectura backend](#arquitectura-backend)
- [Arquitectura frontend](#arquitectura-frontend)
- [Flujo de una consulta de punta a punta](#flujo-de-una-consulta-de-punta-a-punta)
- [Estrategia de caché](#estrategia-de-caché)
- [Base de datos](#base-de-datos)
- [API Reference](#api-reference)
- [Cómo levantar el proyecto](#cómo-levantar-el-proyecto)

---

## Estructura del proyecto

```
e-Consumer/
├── PersonDataApp.slnx
├── src/
│   ├── PersonDataApp.Domain/          # Entidades y puertos (interfaces)
│   ├── PersonDataApp.Application/     # Casos de uso y DTOs
│   ├── PersonDataApp.Infrastructure/  # Persistencia y servicios externos
│   └── PersonDataApp.API/             # Controladores y configuración ASP.NET Core
└── person-app-frontend/               # SPA estática con Astro + Tailwind CSS
```

---

## Arquitectura backend

El backend sigue **arquitectura hexagonal (ports & adapters)** organizada en cuatro capas con dependencias unidireccionales hacia el dominio.

```
┌──────────────────────────────────────────────────────────────────┐
│  PersonDataApp.API  (ASP.NET Core 10)                            │
│  PersonController → IGetPersonUseCase                            │
└──────────────────────────┬───────────────────────────────────────┘
                           │ depende de
┌──────────────────────────▼───────────────────────────────────────┐
│  PersonDataApp.Application                                        │
│  GetPersonUseCase  ←→  IPersonRepository  IExternalPersonService │
│  PersonDto (mapeo + campo Age calculado + flag fromCache)        │
└──────────────────────────┬───────────────────────────────────────┘
                           │ depende de
┌──────────────────────────▼───────────────────────────────────────┐
│  PersonDataApp.Domain                                             │
│  Person (entidad)  IPersonRepository  IExternalPersonService     │
└──────────────────────────────────────────────────────────────────┘
                           ▲
                           │ implementa
┌──────────────────────────┴───────────────────────────────────────┐
│  PersonDataApp.Infrastructure                                     │
│  PersonRepository (Dapper + SQL Server)                          │
│  ExternalPersonServiceAdapter (HttpClient → API externa)         │
│  FakePersonServiceAdapter (datos sintéticos para desarrollo)     │
└──────────────────────────────────────────────────────────────────┘
```

### Capa Domain

Núcleo sin dependencias externas.

| Artefacto | Rol |
|---|---|
| `Person` | Entidad principal con todos los campos de la persona y el método `IsCacheStale(maxAgeDays)` |
| `IPersonRepository` | Puerto de salida para persistencia (buscar y hacer upsert) |
| `IExternalPersonService` | Puerto de salida hacia el servicio externo de datos |

El método `IsCacheStale` compara `LastQueriedAt` contra la fecha actual y retorna `true` si el dato tiene más de 20 días.

### Capa Application

Orquesta la lógica de negocio sin saber cómo se implementan los adaptadores.

| Artefacto | Rol |
|---|---|
| `IGetPersonUseCase` | Contrato del caso de uso |
| `GetPersonUseCase` | Implementa la estrategia de caché (ver sección más abajo) |
| `PersonDto` | DTO de respuesta con campo `Age` calculado y flag `FromCache` |

### Capa Infrastructure

Adaptadores concretos que implementan los puertos del dominio.

| Artefacto | Rol |
|---|---|
| `PersonRepository` | Consulta y upsert con **Dapper** sobre SQL Server |
| `ExternalPersonServiceAdapter` | Llama a la API externa y mapea la respuesta a `Person` |
| `FakePersonServiceAdapter` | Genera datos deterministas basados en el hash del DNI (para desarrollo) |
| `InfrastructureServiceExtensions` | Registra todos los servicios en el contenedor DI. Si `UseFakeExternalService: true` usa el adaptador falso |

### Capa API

| Artefacto | Rol |
|---|---|
| `PersonController` | Único controlador, ruta `GET /api/persons/{documentNumber}` |
| `Program.cs` | Configura CORS, DI, OpenAPI y Scalar |

La documentación interactiva de la API está disponible en `/scalar/v1` cuando la API está corriendo.

---

## Arquitectura frontend

Aplicación estática construida con **Astro 6** y **Tailwind CSS 4**. No usa ningún framework JavaScript reactivo: toda la interactividad es vanilla JS con la Fetch API.

```
person-app-frontend/src/
├── layouts/
│   └── BaseLayout.astro      # HTML base, fuentes, blobs decorativos animados
├── pages/
│   └── index.astro           # Única página: lógica de búsqueda y renderizado
├── components/
│   ├── SearchForm.astro       # Formulario de ingreso de DNI
│   ├── PersonCard.astro       # Tarjeta con todos los datos de la persona
│   ├── LoadingSpinner.astro   # Indicador de carga durante el fetch
│   └── NotFoundAlert.astro    # Alerta cuando no se encuentran resultados
└── styles/
    └── global.css             # Variables CSS (tema Darcula), clases .glass y .blob
```

### Páginas y componentes

**`index.astro`** — página principal y única. Importa `SearchForm` en el markup estático y utiliza un `<script>` client-side para:

1. Escuchar el submit del formulario.
2. Limpiar el input (elimina caracteres no numéricos).
3. Mostrar `LoadingSpinner` mientras espera la respuesta.
4. Inyectar dinámicamente `PersonCard` o `NotFoundAlert` según el resultado.

**`BaseLayout.astro`** — envuelve todas las páginas. Provee el `<head>` con metadatos, la fuente JetBrains Mono, y los blobs de fondo animados (Darcula purple, cyan y green).

**`SearchForm.astro`** — card glassmorphism con el campo "DNI / Cédula" y el botón de búsqueda.

**`PersonCard.astro`** — recibe el objeto persona y lo renderiza en una grilla de dos columnas. Incluye:
- Encabezado con número de documento, nombre completo y badge de origen del dato (verde = caché, naranja = servicio externo).
- Campos: fecha de nacimiento (formato `es-AR`), edad, dirección, localidad, teléfono, email.
- Pie con la fecha de última consulta.

**`NotFoundAlert.astro`** — alerta roja que muestra el DNI buscado cuando la API retorna 404.

**`LoadingSpinner.astro`** — spinner animado con el texto "Consultando datos...".

---

## Flujo de una consulta de punta a punta

```
Usuario ingresa DNI → [SearchForm]
        │
        ▼
[index.astro script]
  fetch GET /api/persons/{dni}
        │
        ▼
[PersonController]
  valida documentNumber no vacío
        │
        ▼
[GetPersonUseCase.ExecuteAsync(documentNumber)]
        │
        ├── PersonRepository.FindByDocumentNumberAsync()
        │       └── SELECT en PersonCache (Dapper)
        │
        ├─► Si existe y no es stale (< 20 días)
        │       └── retorna PersonDto con FromCache = true
        │
        └─► Si no existe o es stale
                │
                ├── ExternalPersonServiceAdapter.GetByDocumentNumberAsync()
                │       └── GET {BaseUrl}/persons/{documentNumber}
                │
                ├─► Si la API externa retorna datos
                │       ├── PersonRepository.UpsertAsync()  ← actualiza caché
                │       └── retorna PersonDto con FromCache = false
                │
                └─► Si la API externa retorna 404
                        └── retorna null → 404 Not Found
        │
        ▼
[index.astro script]
  200 → renderiza PersonCard
  404 → renderiza NotFoundAlert
  error → muestra mensaje de conexión
```

---

## Estrategia de caché

La caché es una tabla SQL (`PersonCache`) que persiste los datos de cada persona consultada.

| Condición | Acción |
|---|---|
| Registro en BD y `LastQueriedAt` < 20 días | Se devuelve desde caché (`fromCache: true`) sin llamar al servicio externo |
| Registro en BD pero `LastQueriedAt` ≥ 20 días | Se llama al servicio externo, se actualiza el registro y se devuelve (`fromCache: false`) |
| Sin registro en BD | Se llama al servicio externo, se inserta el registro y se devuelve (`fromCache: false`) |
| Servicio externo retorna 404 | Se devuelve 404 al cliente (sin tocar la BD) |

El badge en `PersonCard` muestra visualmente si el dato provino de caché (verde) o fue consultado al servicio externo (naranja).

---

## Base de datos

### Tabla `PersonCache`

```sql
CREATE TABLE PersonCache (
    Id             INT            IDENTITY(1,1) PRIMARY KEY,
    DocumentNumber NVARCHAR(20)   NOT NULL UNIQUE,
    FirstName      NVARCHAR(100)  NOT NULL,
    LastName       NVARCHAR(100)  NOT NULL,
    BirthDate      DATE           NULL,
    Address        NVARCHAR(255)  NULL,
    Locality       NVARCHAR(100)  NULL,
    Phone          NVARCHAR(50)   NULL,
    Email          NVARCHAR(255)  NULL,
    LastQueriedAt  DATETIME2      NOT NULL,
    CreatedAt      DATETIME2      NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt      DATETIME2      NOT NULL DEFAULT GETUTCDATE()
);
```

El script de creación está en `src/PersonDataApp.Infrastructure/Persistence/Scripts/001_CreatePersonCache.sql`.

Las operaciones de escritura usan un `MERGE` (upsert): si ya existe el `DocumentNumber` actualiza el registro, si no lo inserta.

---

## API Reference

### `GET /api/persons/{documentNumber}`

Retorna los datos de una persona dado su número de documento.

**Parámetros**

| Nombre | Tipo | Descripción |
|---|---|---|
| `documentNumber` | `string` (path) | Número de documento (DNI / cédula) |

**Respuestas**

**200 OK**
```json
{
  "documentNumber": "12345678",
  "firstName": "Carlos",
  "lastName": "González",
  "birthDate": "1985-03-15T00:00:00",
  "age": 41,
  "address": "Av. González 1250",
  "locality": "Buenos Aires",
  "phone": "+54 11 2345-6789",
  "email": "carlos.gonzalez@example.com",
  "lastQueriedAt": "2026-05-09T16:30:45.123Z",
  "fromCache": false
}
```

**404 Not Found** — no se encontraron datos para el documento indicado.

**400 Bad Request** — el número de documento está vacío o contiene solo espacios.

La documentación interactiva (Scalar) está disponible en `http://localhost:5152/scalar/v1`.

---

## Cómo levantar el proyecto

### Requisitos

- .NET 10 SDK
- SQL Server (por defecto en el puerto 1436) o Docker
- Node.js >= 22.12.0

### Backend

```bash
# Crear la base de datos ejecutando el script SQL
# src/PersonDataApp.Infrastructure/Persistence/Scripts/001_CreatePersonCache.sql

# Configurar la cadena de conexión en appsettings.json (o variable de entorno)

# Ejecutar la API
cd src/PersonDataApp.API
dotnet run
# API disponible en http://localhost:5152
```

Para desarrollo sin base de datos ni API externa, setear `UseFakeExternalService: true` en `appsettings.Development.json` (ya viene configurado así).

### Frontend

```bash
cd person-app-frontend
npm install
npm run dev
# Frontend disponible en http://localhost:4321
```

Para producción:

```bash
npm run build   # genera ./dist/
npm run preview # previsualiza el build estático
```
