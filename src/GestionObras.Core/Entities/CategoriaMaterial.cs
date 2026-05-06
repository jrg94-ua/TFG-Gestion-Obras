namespace GestionObras.Core.Entities;

/// <summary>
/// Categoría configurable para agrupar materiales del catálogo.
/// </summary>
public class CategoriaMaterial
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public bool Activa { get; set; } = true;
}