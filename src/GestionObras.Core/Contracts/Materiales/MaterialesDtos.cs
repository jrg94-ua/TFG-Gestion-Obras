using GestionObras.Core.Contracts.JefeObra;
using GestionObras.Core.Entities;

namespace GestionObras.Core.Contracts.Materiales;

public sealed record ProveedorResumenDto(
    int Id,
    string Nombre,
    string CIF,
    string Direccion,
    string Telefono,
    string Email,
    bool Activo);

public sealed record CategoriaMaterialResumenDto(
    int Id,
    string Nombre,
    string? Descripcion,
    bool Activa);

public sealed record MaterialResumenDto(
    int Id,
    string Codigo,
    string Nombre,
    string Descripcion,
    bool Activo,
    decimal PrecioUnitario,
    string UnidadMedida,
    int StockDisponible,
    decimal StockMinimo,
    string Categoria,
    int? ProveedorId,
    ProveedorResumenDto? Proveedor,
    List<ProveedorResumenDto> Proveedores);

public sealed record SolicitudMaterialResumenDto(
    int Id,
    int MaterialId,
    MaterialResumenDto Material,
    int ProyectoId,
    ProyectoResumenDto Proyecto,
    decimal CantidadSolicitada,
    string Justificacion,
    string SolicitadoPorId,
    UsuarioResumenDto SolicitadoPor,
    DateTime FechaSolicitud,
    EstadoSolicitudMaterial Estado,
    string? RevisadoPorId,
    UsuarioResumenDto? RevisadoPor,
    DateTime? FechaRespuesta,
    string? ObservacionesAdmin,
    PrioridadSolicitud Prioridad,
    DateTime? FechaNecesaria);

public sealed class MaterialesGestionResponse
{
    public List<MaterialResumenDto> Materiales { get; init; } = new();
    public List<ProveedorResumenDto> Proveedores { get; init; } = new();
    public List<CategoriaMaterialResumenDto> Categorias { get; init; } = new();
}

public sealed class CatalogosMaterialesResponse
{
    public List<ProveedorResumenDto> Proveedores { get; init; } = new();
    public List<MaterialResumenDto> Materiales { get; init; } = new();
    public List<CategoriaMaterialResumenDto> Categorias { get; init; } = new();
}

public sealed class SolicitarMaterialesResponse
{
    public List<ProyectoResumenDto> MisProyectos { get; init; } = new();
    public List<MaterialResumenDto> Materiales { get; init; } = new();
}

public sealed class MisSolicitudesMaterialesResponse
{
    public List<SolicitudMaterialResumenDto> Solicitudes { get; init; } = new();
    public List<ProyectoResumenDto> Proyectos { get; init; } = new();
}

public sealed class GestionSolicitudesMaterialesResponse
{
    public List<SolicitudMaterialResumenDto> Solicitudes { get; init; } = new();
    public List<ProyectoResumenDto> Proyectos { get; init; } = new();
}

public sealed class GuardarMaterialRequest
{
    public int Id { get; init; }
    public string Codigo { get; init; } = string.Empty;
    public string Nombre { get; init; } = string.Empty;
    public string Descripcion { get; init; } = string.Empty;
    public bool Activo { get; init; } = true;
    public decimal PrecioUnitario { get; init; }
    public string UnidadMedida { get; init; } = string.Empty;
    public int StockDisponible { get; init; }
    public decimal StockMinimo { get; init; }
    public string Categoria { get; init; } = string.Empty;
    public List<int> ProveedorIds { get; init; } = new();
}

public sealed class GuardarProveedorRequest
{
    public int Id { get; init; }
    public string Nombre { get; init; } = string.Empty;
    public string CIF { get; init; } = string.Empty;
    public string Direccion { get; init; } = string.Empty;
    public string Telefono { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public bool Activo { get; init; } = true;
}

public sealed class GuardarCategoriaMaterialRequest
{
    public int Id { get; init; }
    public string Nombre { get; init; } = string.Empty;
    public string? Descripcion { get; init; }
    public bool Activa { get; init; } = true;
}

public sealed class CrearSolicitudMaterialRequest
{
    public int MaterialId { get; init; }
    public int ProyectoId { get; init; }
    public decimal CantidadSolicitada { get; init; }
    public string Justificacion { get; init; } = string.Empty;
    public string SolicitadoPorId { get; init; } = string.Empty;
    public PrioridadSolicitud Prioridad { get; init; }
    public DateTime? FechaNecesaria { get; init; }
}

public sealed class RevisarSolicitudMaterialRequest
{
    public string RevisadoPorId { get; init; } = string.Empty;
    public bool Aprobar { get; init; }
    public string? ObservacionesAdmin { get; init; }
}
