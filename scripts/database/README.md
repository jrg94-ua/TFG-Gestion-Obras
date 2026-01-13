# Scripts de Base de Datos

Esta carpeta contendrá los scripts SQL para:

- Migraciones iniciales de Entity Framework
- Seeders de datos de prueba
- Scripts de mantenimiento
- Backups automatizados

## Uso

Los scripts se ejecutarán mediante Entity Framework Core:

```powershell
# Crear una nueva migración
dotnet ef migrations add NombreMigracion --project ../GestionObras.Infrastructure

# Aplicar migraciones a la base de datos
dotnet ef database update --project ../GestionObras.Infrastructure
```
