# Scripts de Despliegue

Esta carpeta contendrá scripts para:

- Despliegue en Azure/AWS
- Configuración de Docker
- CI/CD con GitHub Actions
- Scripts de respaldo

## Ejemplo de Docker Compose (futuro)

```yaml
version: '3.8'
services:
  api:
    build: ./src/GestionObras.API
    ports:
      - "5000:80"
    environment:
      - ConnectionStrings__DefaultConnection=Server=db;Database=GestionObras;User=sa;Password=YourPassword123
  
  db:
    image: mcr.microsoft.com/mssql/server:2019-latest
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=YourPassword123
    volumes:
      - sqldata:/var/opt/mssql
volumes:
  sqldata:
```
