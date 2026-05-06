using GestionObras.Core.Entities;
using GestionObras.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestionObras.Web.Services;

public class PlanificacionHorarioService
{
    private const decimal MaxHorasDiarias = 8m;
    private const int DiasLaborablesSemana = 5;
    private const decimal MaxHorasSemanalesPorJornada = MaxHorasDiarias * DiasLaborablesSemana;

    private readonly GestionObrasDbContext _db;

    public PlanificacionHorarioService(GestionObrasDbContext db)
    {
        _db = db;
    }

    public async Task<decimal> CalcularHorasSemanalesProyectoAsync(int proyectoId)
    {
        return await _db.Tareas
            .Where(t => t.ProyectoId == proyectoId)
            .SumAsync(t => (decimal?)t.HorasSemanalesEstimadas) ?? 0m;
    }

    public async Task<GeneracionHorariosResultado> GenerarHorariosAutomaticosAsync(int proyectoId, DateOnly? vigenciaDesde = null)
    {
        var fechaInicio = vigenciaDesde ?? DateOnly.FromDateTime(DateTime.Today);
        var proyecto = await _db.Proyectos
            .Include(p => p.Tareas)
                .ThenInclude(t => t.ResponsableFinal)
            .FirstOrDefaultAsync(p => p.Id == proyectoId);

        if (proyecto == null)
        {
            return GeneracionHorariosResultado.Error("No se encontro el proyecto seleccionado.");
        }

        var tareasActivas = proyecto.Tareas
            .Where(t => t.Estado != EstadoTarea.Finalizado && !string.IsNullOrWhiteSpace(t.ResponsableFinalId))
            .ToList();

        if (!tareasActivas.Any())
        {
            return GeneracionHorariosResultado.Error("El proyecto no tiene tareas activas con trabajador final asignado.");
        }

        var resultado = new GeneracionHorariosResultado
        {
            HorasSemanalesProyecto = tareasActivas.Sum(t => t.HorasSemanalesEstimadas)
        };

        var tareasNoOperativas = tareasActivas
            .Where(t => t.ResponsableFinal?.TipoUsuario != TipoUsuario.Operario)
            .ToList();

        if (tareasNoOperativas.Any())
        {
            var horasNoOperativas = tareasNoOperativas.Sum(t => t.HorasSemanalesEstimadas);
            resultado.HorasSemanalesDeficit += horasNoOperativas;

            foreach (var tarea in tareasNoOperativas)
            {
                resultado.Alertas.Add(
                    $"La tarea '{tarea.Nombre}' tiene como responsable final a '{tarea.ResponsableFinal?.NombreCompleto ?? tarea.ResponsableFinalId}', " +
                    "que no es un perfil operativo de ejecucion. Debe reasignarse a un operario.");
            }
        }

        var tareasOperativas = tareasActivas
            .Where(t => t.ResponsableFinal?.TipoUsuario == TipoUsuario.Operario)
            .ToList();

        if (!tareasOperativas.Any())
        {
            resultado.Alertas.Add("No hay tareas con responsables finales operativos para generar horarios automaticos.");
            return resultado;
        }

        var cargasPorUsuario = tareasOperativas
            .GroupBy(t => t.ResponsableFinalId!)
            .ToDictionary(g => g.Key, g => g.Sum(t => t.HorasSemanalesEstimadas));

        var contratosActivos = await _db.Contratos
            .Include(c => c.Usuario)
            .Where(c => c.Activo && cargasPorUsuario.Keys.Contains(c.UsuarioId))
            .ToListAsync();

        var usuariosSinContrato = cargasPorUsuario.Keys
            .Where(usuarioId => contratosActivos.All(c => c.UsuarioId != usuarioId))
            .ToList();

        foreach (var usuarioId in usuariosSinContrato)
        {
            var usuario = tareasOperativas.Select(t => t.ResponsableFinal).FirstOrDefault(u => u?.Id == usuarioId);
            var horasPendientes = cargasPorUsuario.TryGetValue(usuarioId, out var horas) ? horas : 0m;
            resultado.HorasSemanalesDeficit += horasPendientes;
            resultado.Alertas.Add($"{usuario?.NombreCompleto ?? usuarioId} no tiene contrato activo para generar horario automatico.");
        }

        contratosActivos = contratosActivos
            .Where(c => c.Usuario?.TipoUsuario == TipoUsuario.Operario)
            .ToList();

        if (!contratosActivos.Any())
        {
            resultado.Alertas.Add("No hay contratos activos de perfiles operativos para cubrir el proyecto.");
            CalcularEstimacionAdicional(resultado);
            return resultado;
        }

        resultado.TrabajadoresConsiderados = contratosActivos.Count;

        foreach (var contrato in contratosActivos)
        {
            var horasProyecto = cargasPorUsuario.TryGetValue(contrato.UsuarioId, out var horas) ? horas : 0m;
            if (horasProyecto <= 0)
            {
                continue;
            }

            var capacidadSemanalContrato = CalcularCapacidadSemanalPlanificable(contrato);
            var capacidadDiariaContrato = CalcularCapacidadDiariaPlanificable(contrato);

            resultado.HorasSemanalesCapacidadActual += capacidadSemanalContrato;

            if ((decimal)contrato.HorasSemanales > MaxHorasSemanalesPorJornada)
            {
                resultado.Alertas.Add(
                    $"{contrato.Usuario?.NombreCompleto ?? contrato.UsuarioId} tiene {contrato.HorasSemanales:F1} h/sem en contrato, " +
                    $"pero la generacion automatica solo puede repartir {MaxHorasSemanalesPorJornada:F0} h/sem con un maximo de {MaxHorasDiarias:F0} h/dia.");
            }

            var horasObjetivo = Math.Min(horasProyecto, capacidadSemanalContrato);
            resultado.HorasSemanalesAsignadas += horasObjetivo;

            if (horasProyecto > capacidadSemanalContrato)
            {
                var exceso = horasProyecto - capacidadSemanalContrato;
                resultado.HorasSemanalesDeficit += exceso;
                resultado.Alertas.Add(
                    $"{contrato.Usuario?.NombreCompleto ?? contrato.UsuarioId} requiere {horasProyecto:F1} h/sem, " +
                    $"pero su capacidad planificable es {capacidadSemanalContrato:F1} h/sem segun contrato y limite diario.");
            }

            if (horasObjetivo <= 0 || capacidadDiariaContrato <= 0)
            {
                continue;
            }

            var horasDiarias = Math.Min(capacidadDiariaContrato, horasObjetivo / DiasLaborablesSemana);
            var horaEntrada = new TimeOnly(8, 0);
            var horaSalida = horaEntrada.Add(TimeSpan.FromHours((double)horasDiarias));

            foreach (var dia in ObtenerDiasLaborables())
            {
                var horariosMismaVigencia = await _db.HorariosAsignados
                    .Where(h =>
                        h.UsuarioId == contrato.UsuarioId &&
                        h.DiaSemana == dia &&
                        h.VigenteDesde == fechaInicio &&
                        h.Activo)
                    .OrderByDescending(h => h.Id)
                    .ToListAsync();

                var existente = horariosMismaVigencia.FirstOrDefault(h => h.ProyectoId == proyectoId)
                    ?? horariosMismaVigencia.FirstOrDefault();

                await CerrarHorariosConflictivosAsync(contrato.UsuarioId, dia, fechaInicio, existente?.Id);

                if (existente == null)
                {
                    _db.HorariosAsignados.Add(new HorarioAsignado
                    {
                        UsuarioId = contrato.UsuarioId,
                        ProyectoId = proyectoId,
                        DiaSemana = dia,
                        HoraEntrada = horaEntrada,
                        HoraSalida = horaSalida,
                        TipoTurno = horasDiarias >= 6m ? TipoTurno.Partido : (TipoTurno)0,
                        VigenteDesde = fechaInicio,
                        Activo = true
                    });
                    resultado.HorariosCreados++;
                }
                else
                {
                    existente.HoraEntrada = horaEntrada;
                    existente.HoraSalida = horaSalida;
                    existente.TipoTurno = horasDiarias >= 6m ? TipoTurno.Partido : (TipoTurno)0;
                    existente.ProyectoId = proyectoId;
                    existente.Activo = true;
                    resultado.HorariosActualizados++;
                }
            }
        }

        resultado.HorasSemanalesCapacidadCubierta = Math.Min(resultado.HorasSemanalesProyecto, resultado.HorasSemanalesCapacidadActual);

        if (resultado.HorasSemanalesProyecto > resultado.HorasSemanalesAsignadas)
        {
            var deficitRestante = resultado.HorasSemanalesProyecto - resultado.HorasSemanalesAsignadas;
            resultado.HorasSemanalesDeficit = Math.Max(resultado.HorasSemanalesDeficit, deficitRestante);
        }

        CalcularEstimacionAdicional(resultado);

        await _db.SaveChangesAsync();
        return resultado;
    }

    private async Task CerrarHorariosConflictivosAsync(string usuarioId, DiaSemana dia, DateOnly fechaReferencia, int? horarioAConservarId = null)
    {
        var conflictivos = await _db.HorariosAsignados
            .Where(h =>
                h.UsuarioId == usuarioId &&
                h.DiaSemana == dia &&
                h.Activo &&
                h.Id != horarioAConservarId &&
                h.VigenteDesde <= fechaReferencia &&
                (h.VigenteHasta == null || h.VigenteHasta >= fechaReferencia))
            .ToListAsync();

        foreach (var conflicto in conflictivos)
        {
            conflicto.Activo = false;
            conflicto.VigenteHasta = fechaReferencia.AddDays(-1);
        }
    }

    private static decimal CalcularCapacidadSemanalPlanificable(Contrato contrato)
    {
        return Math.Min((decimal)contrato.HorasSemanales, MaxHorasSemanalesPorJornada);
    }

    private static decimal CalcularCapacidadDiariaPlanificable(Contrato contrato)
    {
        var capacidadSemanal = CalcularCapacidadSemanalPlanificable(contrato);
        return Math.Min(MaxHorasDiarias, capacidadSemanal / DiasLaborablesSemana);
    }

    private static DiaSemana[] ObtenerDiasLaborables()
    {
        return
        [
            DiaSemana.Lunes,
            DiaSemana.Martes,
            DiaSemana.Miercoles,
            DiaSemana.Jueves,
            DiaSemana.Viernes
        ];
    }

    private static void CalcularEstimacionAdicional(GeneracionHorariosResultado resultado)
    {
        if (resultado.HorasSemanalesDeficit <= 0)
        {
            return;
        }

        var capacidadReferencia = resultado.TrabajadoresConsiderados > 0 && resultado.HorasSemanalesCapacidadActual > 0
            ? resultado.HorasSemanalesCapacidadActual / resultado.TrabajadoresConsiderados
            : MaxHorasSemanalesPorJornada;

        capacidadReferencia = Math.Max(1m, capacidadReferencia);

        resultado.CapacidadMediaSemanalTrabajador = capacidadReferencia;
        resultado.TrabajadoresAdicionalesEstimados = (int)Math.Ceiling(resultado.HorasSemanalesDeficit / capacidadReferencia);

        resultado.Alertas.Add(
            $"Capacidad actual del equipo operativo: {resultado.HorasSemanalesCapacidadActual:F1} h/sem. " +
            $"Carga del proyecto: {resultado.HorasSemanalesProyecto:F1} h/sem. " +
            $"Deficit estimado: {resultado.HorasSemanalesDeficit:F1} h/sem. " +
            $"Trabajadores adicionales estimados: {resultado.TrabajadoresAdicionalesEstimados} " +
            $"(referencia media actual: {capacidadReferencia:F1} h/sem por trabajador).");
    }
}

public class GeneracionHorariosResultado
{
    public int HorariosCreados { get; set; }
    public int HorariosActualizados { get; set; }
    public decimal HorasSemanalesProyecto { get; set; }
    public decimal HorasSemanalesCapacidadActual { get; set; }
    public decimal HorasSemanalesCapacidadCubierta { get; set; }
    public decimal HorasSemanalesAsignadas { get; set; }
    public decimal HorasSemanalesDeficit { get; set; }
    public decimal CapacidadMediaSemanalTrabajador { get; set; }
    public int TrabajadoresConsiderados { get; set; }
    public int TrabajadoresAdicionalesEstimados { get; set; }
    public List<string> Alertas { get; } = new();

    public static GeneracionHorariosResultado Error(string alerta)
    {
        var resultado = new GeneracionHorariosResultado();
        resultado.Alertas.Add(alerta);
        return resultado;
    }

    public string ObtenerMensajeResumen()
    {
        var baseMensaje =
            $"Horario automatico generado: {HorariosCreados} creado(s), {HorariosActualizados} actualizado(s). " +
            $"Proyecto: {HorasSemanalesProyecto:F1} h/sem. " +
            $"Capacidad operativa actual: {HorasSemanalesCapacidadActual:F1} h/sem. " +
            $"Asignadas: {HorasSemanalesAsignadas:F1} h/sem.";

        if (HorasSemanalesDeficit > 0)
        {
            baseMensaje +=
                $" Deficit: {HorasSemanalesDeficit:F1} h/sem. " +
                $"Trabajadores adicionales estimados: {TrabajadoresAdicionalesEstimados}.";
        }

        if (!Alertas.Any())
        {
            return baseMensaje;
        }

        return baseMensaje + " Avisos: " + string.Join(" | ", Alertas);
    }
}
