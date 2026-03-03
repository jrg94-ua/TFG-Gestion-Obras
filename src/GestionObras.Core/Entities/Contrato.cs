using System;

namespace GestionObras.Core.Entities;

/// <summary>
/// Contrato laboral de un trabajador.
/// Gestionado por el departamento de Recursos Humanos.
/// </summary>
public class Contrato
{
    public int Id { get; set; }

    // Trabajador al que pertenece el contrato
    public string UsuarioId { get; set; } = string.Empty;
    public UsuarioObra Usuario { get; set; } = null!;

    // Tipo de contrato
    public TipoContrato TipoContrato { get; set; }

    // Fechas
    public DateOnly FechaInicio { get; set; }
    public DateOnly? FechaFin { get; set; }

    // Condiciones económicas
    public decimal SalarioBrutoAnual { get; set; }
    public JornadaLaboral Jornada { get; set; } = JornadaLaboral.Completa;
    public double HorasSemanales { get; set; } = 40;

    // Datos laborales
    public string? CategoriaConvenio { get; set; }
    public string? NumeroSeguridadSocial { get; set; }
    public string? CentroTrabajo { get; set; }

    // Observaciones
    public string? Observaciones { get; set; }

    // Estado
    public bool Activo { get; set; } = true;

    public DateTime FechaCreacion { get; set; } = DateTime.Now;
    public DateTime? FechaModificacion { get; set; }
}

/// <summary>
/// Tipos de contrato laboral según legislación española
/// </summary>
public enum TipoContrato
{
    Indefinido = 0,
    TempObraoServicio = 1,
    TempCircunstanciasProduccion = 2,
    FormacionAlternancia = 3,
    FormacionPractica = 4,
    Interinidad = 5,
    FijoDiscontinuo = 6
}

/// <summary>
/// Tipos de jornada laboral
/// </summary>
public enum JornadaLaboral
{
    Completa = 0,
    Parcial = 1,
    Reducida = 2
}
