using GestionObras.Core.Entities;
using GestionObras.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestionObras.Web.Services
{
    public class DocumentoService
    {
        private readonly GestionObrasDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<DocumentoService> _logger;

        public DocumentoService(
            GestionObrasDbContext context, 
            IWebHostEnvironment env,
            ILogger<DocumentoService> logger)
        {
            _context = context;
            _env = env;
            _logger = logger;
        }

        public async Task<DocumentoTarea> SubirDocumentoAsync(
            int tareaId,
            string usuarioId,
            Stream archivoStream,
            string nombreArchivo,
            string tipoMime,
            long tamañoBytes,
            TipoDocumento tipoDocumento,
            string? descripcion = null)
        {
            try
            {
                // Crear directorio si no existe
                var uploadsPath = Path.Combine(_env.WebRootPath, "uploads", "tareas", tareaId.ToString());
                Directory.CreateDirectory(uploadsPath);

                // Generar nombre único para el archivo
                var extension = Path.GetExtension(nombreArchivo);
                var nombreUnico = $"{Guid.NewGuid()}{extension}";
                var rutaCompleta = Path.Combine(uploadsPath, nombreUnico);

                // Guardar archivo
                using (var fileStream = new FileStream(rutaCompleta, FileMode.Create))
                {
                    await archivoStream.CopyToAsync(fileStream);
                }

                // Crear registro en base de datos
                var documento = new DocumentoTarea
                {
                    TareaId = tareaId,
                    NombreArchivo = nombreArchivo,
                    RutaArchivo = Path.Combine("uploads", "tareas", tareaId.ToString(), nombreUnico),
                    TipoDocumento = tipoDocumento,
                    TamañoBytes = tamañoBytes,
                    TipoMime = tipoMime,
                    FechaSubida = DateTime.Now,
                    UsuarioSubidaId = usuarioId,
                    Descripcion = descripcion
                };

                _context.DocumentosTareas.Add(documento);
                await _context.SaveChangesAsync();

                return documento;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al subir documento para tarea {TareaId}", tareaId);
                throw;
            }
        }

        public async Task<List<DocumentoTarea>> ObtenerDocumentosPorTareaAsync(int tareaId)
        {
            return await _context.DocumentosTareas
                .Include(d => d.UsuarioSubida)
                .Where(d => d.TareaId == tareaId)
                .OrderByDescending(d => d.FechaSubida)
                .ToListAsync();
        }

        public async Task<DocumentoTarea?> ObtenerDocumentoPorIdAsync(int documentoId)
        {
            return await _context.DocumentosTareas
                .Include(d => d.UsuarioSubida)
                .Include(d => d.Tarea)
                .FirstOrDefaultAsync(d => d.Id == documentoId);
        }

        public async Task<bool> EliminarDocumentoAsync(int documentoId)
        {
            try
            {
                var documento = await _context.DocumentosTareas.FindAsync(documentoId);
                if (documento == null)
                    return false;

                // Eliminar archivo físico
                var rutaCompleta = Path.Combine(_env.WebRootPath, documento.RutaArchivo);
                if (File.Exists(rutaCompleta))
                {
                    File.Delete(rutaCompleta);
                }

                // Eliminar registro de base de datos
                _context.DocumentosTareas.Remove(documento);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar documento {DocumentoId}", documentoId);
                return false;
            }
        }

        public string ObtenerRutaCompleta(string rutaRelativa)
        {
            return Path.Combine(_env.WebRootPath, rutaRelativa);
        }
    }
}
