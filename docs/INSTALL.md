# Guía de instalación y arranque

Esta guía está actualizada para el estado actual del repositorio. El flujo recomendado de desarrollo es con contenedores mediante `docker-compose`.

---

## Opción recomendada: arranque con contenedores

### Requisitos

- Docker Desktop (o Docker Engine + Compose Plugin)
- Git

Verificar instalación:

```powershell
docker --version
docker compose version
```

### 1) Clonar el repositorio

```powershell
git clone https://github.com/jrg94-ua/tfg-gestion-obras.git
cd tfg-gestion-obras
```

### 2) Construir y levantar servicios

Desde la raíz del repositorio (donde está `docker-compose.yml`):

```powershell
docker compose up -d --build
```

Esto levanta:

- `sqlserver` (SQL Server 2022)
- `api` (ASP.NET Core API)
- `web` (Blazor)

### 3) Comprobar estado

```powershell
docker compose ps
docker compose logs -f api
docker compose logs -f web
```

Accesos por defecto:

- Web: `http://localhost:5001`
- API: `http://localhost:5000`
- SQL Server: `localhost,1433`

### 4) Parar el entorno

```powershell
docker compose down
```

Para borrar también volúmenes de base de datos:

```powershell
docker compose down -v
```

---

## Opción alternativa: arranque local con .NET (sin contenedores)

### Requisitos

- .NET SDK 8.0+
- SQL Server local (o cadena de conexión equivalente)

### Pasos

```powershell
cd src
dotnet restore ..\TFG-JORGE.sln
dotnet build ..\TFG-JORGE.sln
```

Después, ejecutar cada proyecto por separado si se necesita modo local.

---

## Configuración relevante

- El orquestador está en `docker-compose.yml`.
- Los `Dockerfile` están en:
  - `src/GestionObras.API/Dockerfile`
  - `src/GestionObras.Web/Dockerfile`
- El seeding demo está habilitado en contenedores con `SeedDemoOnStartup=true` para el servicio `web`.

---

## Resolución rápida de problemas

- Si un puerto está ocupado (5000, 5001, 1433), cambia el mapeo en `docker-compose.yml`.
- Si falla SQL Server al arrancar, revisa memoria asignada a Docker Desktop y logs de `sqlserver`.
- Si tras cambios de código no se reflejan, reconstruye imágenes:

```powershell
docker compose down
docker compose up -d --build
```
