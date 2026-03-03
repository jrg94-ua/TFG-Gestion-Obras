using GestionObras.Core.Entities;
using GestionObras.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestionObras.Infrastructure.Repositories;

public interface IFichajeRepository
{
    // --- Fichajes ---
    Task<RegistroFichaje?> GetFichajeAbiertoAsync(string usuarioId, DateOnly fecha);
    Task<List<RegistroFichaje>> GetFichajesByUsuarioAsync(string usuarioId, DateOnly desde, DateOnly hasta);
    Task<List<RegistroFichaje>> GetFichajesByProyectoAsync(int proyectoId, DateOnly desde, DateOnly hasta);
    Task<RegistroFichaje> AddFichajeAsync(RegistroFichaje fichaje);
    Task UpdateFichajeAsync(RegistroFichaje fichaje);

    // --- Horarios ---
    Task<List<HorarioAsignado>> GetHorariosByUsuarioAsync(string usuarioId);
    Task<List<HorarioAsignado>> GetHorariosActivosByProyectoAsync(int proyectoId);
    Task<HorarioAsignado> AddHorarioAsync(HorarioAsignado horario);
    Task UpdateHorarioAsync(HorarioAsignado horario);
    Task DeleteHorarioAsync(int id);
}

public class FichajeRepository : IFichajeRepository
{
    private readonly GestionObrasDbContext _context;

    public FichajeRepository(GestionObrasDbContext context)
    {
        _context = context;
    }

    // ────────── Fichajes ──────────

    public async Task<RegistroFichaje?> GetFichajeAbiertoAsync(string usuarioId, DateOnly fecha)
    {
        return await _context.RegistrosFichaje
            .Include(f => f.Proyecto)
            .FirstOrDefaultAsync(f =>
                f.UsuarioId == usuarioId &&
                f.Fecha == fecha &&
                f.HoraSalida == null);
    }

    public async Task<List<RegistroFichaje>> GetFichajesByUsuarioAsync(string usuarioId, DateOnly desde, DateOnly hasta)
    {
        return await _context.RegistrosFichaje
            .Include(f => f.Proyecto)
            .Where(f => f.UsuarioId == usuarioId && f.Fecha >= desde && f.Fecha <= hasta)
            .OrderByDescending(f => f.Fecha)
            .ThenByDescending(f => f.HoraEntrada)
            .ToListAsync();
    }

    public async Task<List<RegistroFichaje>> GetFichajesByProyectoAsync(int proyectoId, DateOnly desde, DateOnly hasta)
    {
        return await _context.RegistrosFichaje
            .Include(f => f.Usuario)
            .Where(f => f.ProyectoId == proyectoId && f.Fecha >= desde && f.Fecha <= hasta)
            .OrderByDescending(f => f.Fecha)
            .ThenBy(f => f.Usuario!.NombreCompleto)
            .ToListAsync();
    }

    public async Task<RegistroFichaje> AddFichajeAsync(RegistroFichaje fichaje)
    {
        _context.RegistrosFichaje.Add(fichaje);
        await _context.SaveChangesAsync();
        return fichaje;
    }

    public async Task UpdateFichajeAsync(RegistroFichaje fichaje)
    {
        _context.RegistrosFichaje.Update(fichaje);
        await _context.SaveChangesAsync();
    }

    // ────────── Horarios ──────────

    public async Task<List<HorarioAsignado>> GetHorariosByUsuarioAsync(string usuarioId)
    {
        return await _context.HorariosAsignados
            .Include(h => h.Proyecto)
            .Where(h => h.UsuarioId == usuarioId && h.Activo)
            .OrderBy(h => h.DiaSemana)
            .ThenBy(h => h.HoraEntrada)
            .ToListAsync();
    }

    public async Task<List<HorarioAsignado>> GetHorariosActivosByProyectoAsync(int proyectoId)
    {
        return await _context.HorariosAsignados
            .Include(h => h.Usuario)
            .Where(h => h.ProyectoId == proyectoId && h.Activo)
            .OrderBy(h => h.Usuario!.NombreCompleto)
            .ThenBy(h => h.DiaSemana)
            .ToListAsync();
    }

    public async Task<HorarioAsignado> AddHorarioAsync(HorarioAsignado horario)
    {
        _context.HorariosAsignados.Add(horario);
        await _context.SaveChangesAsync();
        return horario;
    }

    public async Task UpdateHorarioAsync(HorarioAsignado horario)
    {
        _context.HorariosAsignados.Update(horario);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteHorarioAsync(int id)
    {
        var horario = await _context.HorariosAsignados.FindAsync(id);
        if (horario != null)
        {
            _context.HorariosAsignados.Remove(horario);
            await _context.SaveChangesAsync();
        }
    }
}
