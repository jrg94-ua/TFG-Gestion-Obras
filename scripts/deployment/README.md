# Scripts de despliegue

Esta carpeta documenta la estrategia de despliegue del proyecto. Actualmente el arranque de entorno de desarrollo se realiza con el `docker-compose.yml` ubicado en la raíz del repositorio.

## Estado actual

- Orquestación: `docker-compose.yml` (raíz)
- Servicios: `sqlserver`, `api`, `web`
- Dockerfiles:
  - `src/GestionObras.API/Dockerfile`
  - `src/GestionObras.Web/Dockerfile`

## Comandos base

Desde la raíz del repositorio:

```powershell
docker compose up -d --build
docker compose ps
docker compose logs -f api
docker compose logs -f web
docker compose down
```

## Puertos por defecto

- Web: `5001`
- API: `5000`
- SQL Server: `1433`

## Próxima evolución

Pendiente de incorporar en esta carpeta:

- plantillas de despliegue cloud (Azure/AWS)
- automatización CI/CD
- scripts de backup y restore
