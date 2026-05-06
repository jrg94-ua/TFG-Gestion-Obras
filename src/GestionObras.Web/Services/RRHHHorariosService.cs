using GestionObras.Core.Entities;
using GestionObras.Infrastructure.Data;
using GestionObras.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GestionObras.Web.Services;

public sealed class RRHHHorariosService
{
    public const double MaxHorasDiarias = 8d;
    public const double MaxHorasSemanalesPorJornada = MaxHorasDiarias * 5d;

    private readonly GestionObrasDbContext _db;
    private readonly IFichajeRepository _fichajeRepository;
    private readonly PlanificacionHorarioService _planificacionHorarioService;

    public RRHHHorariosService(
        GestionObrasDbContext db,
        IFichajeRepository fichajeRepository,
        PlanificacionHorarioService planificacionHorarioService)
    {
        _db = db;
        _fichajeRepository = fichajeRepository;
        _planificacionHorarioService = planificacionHorarioService;
    }

    public async Task<RRHHHorariosContext> ObtenerContextoAsync(
        string? filtroProyectoId,
        string? filtroUsuarioId,
        DateOnly? fechaVigencia = null)
    {
        var referencia = fechaVigencia ?? DateOnly.FromDateTime(DateTime.Today);

        var proyectos = await _db.Proyectos
            .OrderBy(p => p.Nombre)
            .ToListAsync();

        var trabajadores = await _db.Users
            .Where(u => u.Activo)
            .OrderBy(u => u.NombreCompleto)
            .ToListAsync();

        IQueryable<HorarioAsignado> query = _db.HorariosAsignados
            .Include(h => h.Usuario)
            .Include(h => h.Proyecto)
            .Where(h => h.Activo &&
                        h.VigenteDesde <= referencia &&
                        (h.VigenteHasta == null || h.VigenteHasta >= referencia));

        if (!string.IsNullOrWhiteSpace(filtroProyectoId) && int.TryParse(filtroProyectoId, out var proyectoId))
        {
            query = query.Where(h => h.ProyectoId == proyectoId);
        }

        if (!string.IsNullOrWhiteSpace(filtroUsuarioId))
        {
            query = query.Where(h => h.UsuarioId == filtroUsuarioId);
        }

        var horariosVigentes = await query
            .OrderBy(h => h.Usuario!.NombreCompleto)
            .ThenBy(h => h.DiaSemana)
            .ThenByDescending(h => h.VigenteDesde)
            .ThenByDescending(h => h.Id)
            .ToListAsync();

        var horarios = horariosVigentes
            .GroupBy(h => new { h.UsuarioId, h.DiaSemana })
            .Select(g => g
                .OrderByDescending(h => h.VigenteDesde)
                .ThenByDescending(h => h.Id)
                .First())
            .OrderBy(h => h.Usuario!.NombreCompleto)
            .ThenBy(h => h.DiaSemana)
            .ThenBy(h => h.HoraEntrada)
            .ToList();

        var usuariosConHorario = horarios.Select(h => h.UsuarioId).Distinct().ToHashSet();
        var trabConHorario = trabajadores.Where(t => usuariosConHorario.Contains(t.Id)).ToList();

        var proyectoIds = horarios
            .Where(h => h.ProyectoId.HasValue)
            .Select(h => h.ProyectoId!.Value)
            .Distinct()
            .ToList();

        var horasEstimadasPorProyecto = proyectoIds.Any()
            ? await _db.Tareas
                .Where(t => proyectoIds.Contains(t.ProyectoId))
                .GroupBy(t => t.ProyectoId)
                .ToDictionaryAsync(g => g.Key, g => g.Sum(t => t.HorasSemanalesEstimadas))
            : new Dictionary<int, decimal>();

        var capacidadSemanalPorUsuario = await _db.Contratos
            .Include(c => c.Usuario)
            .Where(c => c.Activo &&
                        c.Usuario.Activo &&
                        c.Usuario.TipoUsuario == TipoUsuario.Operario &&
                        c.FechaInicio <= referencia &&
                        (c.FechaFin == null || c.FechaFin >= referencia))
            .GroupBy(c => c.UsuarioId)
            .Select(g => g
                .OrderByDescending(c => c.FechaInicio)
                .ThenByDescending(c => c.Id)
                .First())
            .ToDictionaryAsync(
                c => c.UsuarioId,
                c => Math.Min(c.HorasSemanales, MaxHorasSemanalesPorJornada));

        var capacidadMediaOperativaDisponible = capacidadSemanalPorUsuario.Any()
            ? capacidadSemanalPorUsuario.Values.Average()
            : MaxHorasSemanalesPorJornada;

        var horasEstimadasProyecto = 0m;
        if (!string.IsNullOrWhiteSpace(filtroProyectoId) && int.TryParse(filtroProyectoId, out var pid))
        {
            horasEstimadasProyecto = await _db.Tareas
                .Where(t => t.ProyectoId == pid)
                .SumAsync(t => (decimal?)t.HorasSemanalesEstimadas) ?? 0m;
        }

        return new RRHHHorariosContext
        {
            Proyectos = proyectos,
            Trabajadores = trabajadores,
            Horarios = horarios,
            TrabajadoresConHorario = trabConHorario,
            HorasEstimadasPorProyecto = horasEstimadasPorProyecto,
            CapacidadSemanalPorUsuario = capacidadSemanalPorUsuario,
            CapacidadMediaOperativaDisponible = capacidadMediaOperativaDisponible,
            HorasEstimadasProyecto = horasEstimadasProyecto
        };
    }

    public async Task<GeneracionHorariosResultado> GenerarHorarioAutomaticoAsync(int proyectoId)
    {
        return await _planificacionHorarioService.GenerarHorariosAutomaticosAsync(proyectoId);
    }

    public async Task<OperacionHorarioResultado> GuardarHorarioAsync(
        HorarioAsignado nuevo,
        string? nuevoProyId,
        IReadOnlyCollection<DiaSemana> diasSeleccionados,
        string horaEntradaTexto,
        string horaSalidaTexto,
        DateOnly? fechaVigencia = null)
    {
        if (string.IsNullOrWhiteSpace(nuevo.UsuarioId))
        {
            return OperacionHorarioResultado.Error("Selecciona un trabajador.");
        }

        if (!diasSeleccionados.Any())
        {
            return OperacionHorarioResultado.Error("Selecciona al menos un dia.");
        }

        if (!TimeOnly.TryParse(horaEntradaTexto, out var horaEntrada) ||
            !TimeOnly.TryParse(horaSalidaTexto, out var horaSalida))
        {
            return OperacionHorarioResultado.Error("Introduce horas de entrada y salida validas.");
        }

        if (horaSalida <= horaEntrada)
        {
            return OperacionHorarioResultado.Error("La hora de salida debe ser posterior a la de entrada.");
        }

        var horasDiarias = (horaSalida.ToTimeSpan() - horaEntrada.ToTimeSpan()).TotalHours;
        if (horasDiarias > MaxHorasDiarias)
        {
            return OperacionHorarioResultado.Error($"No se pueden asignar mas de {MaxHorasDiarias:F0} h en un mismo dia.");
        }

        var referencia = fechaVigencia ?? DateOnly.FromDateTime(DateTime.Today);
        int? proyectoId = null;
        if (!string.IsNullOrWhiteSpace(nuevoProyId) && int.TryParse(nuevoProyId, out var proyectoParseado))
        {
            proyectoId = proyectoParseado;
        }

        var trabajador = await _db.Users.FirstOrDefaultAsync(u => u.Id == nuevo.UsuarioId && u.Activo);
        if (trabajador == null)
        {
            return OperacionHorarioResultado.Error("El trabajador seleccionado no existe o esta inactivo.");
        }

        if (proyectoId.HasValue && trabajador.TipoUsuario != TipoUsuario.Operario)
        {
            return OperacionHorarioResultado.Error("Solo los perfiles operativos pueden recibir horarios de obra por proyecto.");
        }

        var contrato = await _db.Contratos
            .Where(c => c.UsuarioId == nuevo.UsuarioId &&
                        c.Activo &&
                        c.FechaInicio <= referencia &&
                        (c.FechaFin == null || c.FechaFin >= referencia))
            .OrderByDescending(c => c.FechaInicio)
            .FirstOrDefaultAsync();

        if (contrato == null)
        {
            return OperacionHorarioResultado.Error("El trabajador no tiene contrato activo para asignarle horario.");
        }

        var maximoSemanalContrato = Math.Min(contrato.HorasSemanales, MaxHorasSemanalesPorJornada);
        var horariosVigentes = await _db.HorariosAsignados
            .Where(h => h.UsuarioId == nuevo.UsuarioId &&
                        h.Activo &&
                        h.VigenteDesde <= referencia &&
                        (h.VigenteHasta == null || h.VigenteHasta >= referencia))
            .OrderByDescending(h => h.VigenteDesde)
            .ThenByDescending(h => h.Id)
            .ToListAsync();

        var horasSemanalesResultantes = horariosVigentes
            .GroupBy(h => h.DiaSemana)
            .Select(g => g.First())
            .Where(h => !diasSeleccionados.Contains(h.DiaSemana))
            .Sum(h => h.HorasPrevistas) + (diasSeleccionados.Count * horasDiarias);

        if (horasSemanalesResultantes > maximoSemanalContrato)
        {
            return OperacionHorarioResultado.Error(
                $"La asignacion dejaria a {trabajador.NombreCompleto} con {horasSemanalesResultantes:F1} h/sem y su maximo planificable es {maximoSemanalContrato:F1} h/sem segun contrato.");
        }

        foreach (var dia in diasSeleccionados.OrderBy(d => d))
        {
            await CerrarHorariosConflictivosAsync(nuevo.UsuarioId, dia, referencia);

            _db.HorariosAsignados.Add(new HorarioAsignado
            {
                UsuarioId = nuevo.UsuarioId,
                ProyectoId = proyectoId,
                DiaSemana = dia,
                HoraEntrada = horaEntrada,
                HoraSalida = horaSalida,
                TipoTurno = nuevo.TipoTurno,
                Activo = true,
                VigenteDesde = referencia
            });
        }

        await _db.SaveChangesAsync();
        return OperacionHorarioResultado.Ok($"Horario asignado para {diasSeleccionados.Count} dia(s).");
    }

    public async Task<OperacionHorarioResultado> EliminarHorarioAsync(int id)
    {
        try
        {
            await _fichajeRepository.DeleteHorarioAsync(id);
            return OperacionHorarioResultado.Ok("Turno eliminado.");
        }
        catch (Exception ex)
        {
            return OperacionHorarioResultado.Error($"Error: {ex.Message}");
        }
    }

    public IReadOnlyList<RRHHResumenProyectoHorario> CalcularResumenProyectos(
        IReadOnlyCollection<HorarioAsignado> horarios,
        IReadOnlyDictionary<int, decimal> horasEstimadasPorProyecto,
        IReadOnlyDictionary<string, double> capacidadSemanalPorUsuario,
        double capacidadMediaOperativaDisponible)
    {
        return horarios
            .Where(h => h.ProyectoId.HasValue && h.Proyecto != null)
            .GroupBy(h => new { h.ProyectoId, h.Proyecto!.Nombre })
            .Select(g =>
            {
                var proyectoId = g.Key.ProyectoId!.Value;
                var horasCubiertas = g.Sum(h => h.HorasPrevistas);
                var horasEstimadas = horasEstimadasPorProyecto.TryGetValue(proyectoId, out var estimadas)
                    ? estimadas
                    : 0m;
                var horasPendientes = Math.Max(0m, horasEstimadas - (decimal)horasCubiertas);

                var trabajadoresAsignados = g
                    .Select(h => h.UsuarioId)
                    .Distinct()
                    .ToList();

                var capacidadReferencia = trabajadoresAsignados
                    .Where(capacidadSemanalPorUsuario.ContainsKey)
                    .Select(uid => capacidadSemanalPorUsuario[uid])
                    .DefaultIfEmpty(capacidadMediaOperativaDisponible)
                    .Average();

                capacidadReferencia = Math.Max(1d, capacidadReferencia);

                return new RRHHResumenProyectoHorario
                {
                    NombreProyecto = g.Key.Nombre,
                    TrabajadoresAsignados = trabajadoresAsignados.Count,
                    TurnosAsignados = g.Count(),
                    HorasCubiertas = horasCubiertas,
                    HorasPendientes = horasPendientes,
                    TrabajadoresFaltantes = horasPendientes > 0
                        ? (int)Math.Ceiling((double)horasPendientes / capacidadReferencia)
                        : 0
                };
            })
            .OrderBy(r => r.NombreProyecto)
            .ToList();
    }

    private async Task CerrarHorariosConflictivosAsync(string usuarioId, DiaSemana dia, DateOnly fechaReferencia)
    {
        var conflictivos = await _db.HorariosAsignados
            .Where(h => h.UsuarioId == usuarioId &&
                        h.DiaSemana == dia &&
                        h.Activo &&
                        h.VigenteDesde <= fechaReferencia &&
                        (h.VigenteHasta == null || h.VigenteHasta >= fechaReferencia))
            .ToListAsync();

        foreach (var conflicto in conflictivos)
        {
            conflicto.Activo = false;
            conflicto.VigenteHasta = fechaReferencia.AddDays(-1);
        }
    }
}

public sealed class RRHHHorariosContext
{
    public List<Proyecto> Proyectos { get; init; } = new();
    public List<UsuarioObra> Trabajadores { get; init; } = new();
    public List<HorarioAsignado> Horarios { get; init; } = new();
    public List<UsuarioObra> TrabajadoresConHorario { get; init; } = new();
    public Dictionary<int, decimal> HorasEstimadasPorProyecto { get; init; } = new();
    public Dictionary<string, double> CapacidadSemanalPorUsuario { get; init; } = new();
    public double CapacidadMediaOperativaDisponible { get; init; }
    public decimal HorasEstimadasProyecto { get; init; }
}

public sealed class RRHHResumenProyectoHorario
{
    public string NombreProyecto { get; init; } = string.Empty;
    public int TrabajadoresAsignados { get; init; }
    public int TurnosAsignados { get; init; }
    public double HorasCubiertas { get; init; }
    public decimal HorasPendientes { get; init; }
    public int TrabajadoresFaltantes { get; init; }
}

public sealed record OperacionHorarioResultado(bool Correcto, string Mensaje, bool EsError)
{
    public static OperacionHorarioResultado Ok(string mensaje) => new(true, mensaje, false);

    public static OperacionHorarioResultado Error(string mensaje) => new(false, mensaje, true);
}
