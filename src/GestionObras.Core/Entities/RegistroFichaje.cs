using System;

namespace GestionObras.Core.Entities;

/// <summary>
/// Registro de fichaje (entrada / salida) de un usuario en un proyecto.
/// Requisito funcional RF-11: Fichaje geolocalizado.
/// </summary>
public class RegistroFichaje
{
    public int Id { get; set; }

    // Usuario que ficha (UsuarioObra = cuenta con la que se autentica)
    public string UsuarioId { get; set; } = string.Empty;
    public UsuarioObra Usuario { get; set; } = null!;

    // Proyecto en el que ficha (opcional si trabaja en varios)
    public int? ProyectoId { get; set; }
    public Proyecto? Proyecto { get; set; }

    // Fecha del día de la jornada
    public DateOnly Fecha { get; set; }

    // Hora de entrada
    public DateTime HoraEntrada { get; set; }

    // Hora de salida (null = todavía en jornada)
    public DateTime? HoraSalida { get; set; }

    // Geolocalización de la entrada
    public double? LatitudEntrada { get; set; }
    public double? LongitudEntrada { get; set; }

    // Geolocalización de la salida
    public double? LatitudSalida { get; set; }
    public double? LongitudSalida { get; set; }

    // Notas o incidencias
    public string? Notas { get; set; }

    // Estado de validación por el jefe de obra
    public EstadoFichaje Estado { get; set; } = EstadoFichaje.Pendiente;

    /// <summary>
    /// Calcula el total de horas trabajadas. Devuelve null si no se ha fichado la salida.
    /// </summary>
    public double? TotalHoras =>
        HoraSalida.HasValue
            ? (HoraSalida.Value - HoraEntrada).TotalHours
            : null;
}

public enum EstadoFichaje
{
    Pendiente = 0,
    Validado = 1,
    Rechazado = 2
}
