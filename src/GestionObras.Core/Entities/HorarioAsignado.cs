using System;

namespace GestionObras.Core.Entities;

/// <summary>
/// Horario semanal asignado a un usuario en un proyecto.
/// Requisito funcional RF-12: Control de horarios y turnos.
/// </summary>
public class HorarioAsignado
{
    public int Id { get; set; }

    // Usuario al que se asigna el horario
    public string UsuarioId { get; set; } = string.Empty;
    public UsuarioObra Usuario { get; set; } = null!;

    // Proyecto (opcional; permite horarios globales)
    public int? ProyectoId { get; set; }
    public Proyecto? Proyecto { get; set; }

    // Día de la semana (lunes = 1, domingo = 7)
    public DiaSemana DiaSemana { get; set; }

    // Franja horaria
    public TimeOnly HoraEntrada { get; set; }
    public TimeOnly HoraSalida { get; set; }

    // Tipo de turno
    public TipoTurno TipoTurno { get; set; } = TipoTurno.Mañana;

    // Vigencia
    public DateOnly VigenteDesde { get; set; }
    public DateOnly? VigenteHasta { get; set; }

    public bool Activo { get; set; } = true;

    /// <summary>
    /// Horas previstas en este slot.
    /// </summary>
    public double HorasPrevistas =>
        (HoraSalida.ToTimeSpan() - HoraEntrada.ToTimeSpan()).TotalHours;
}

public enum DiaSemana
{
    Lunes = 1,
    Martes = 2,
    Miercoles = 3,
    Jueves = 4,
    Viernes = 5,
    Sabado = 6,
    Domingo = 7
}

public enum TipoTurno
{
    Mañana = 0,
    Tarde = 1,
    Noche = 2,
    Partido = 3
}
