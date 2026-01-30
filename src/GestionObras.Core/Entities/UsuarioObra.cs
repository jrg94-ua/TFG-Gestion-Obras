using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace GestionObras.Core.Entities;

/// <summary>
/// Usuario del sistema extendiendo IdentityUser para autenticación
/// RF-13: Gestión de usuarios con roles diferenciados
/// </summary>
public class UsuarioObra : IdentityUser
{
    public string DNI { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string? TelefonoMovil { get; set; }
    public string? Cargo { get; set; }
    public TipoUsuario TipoUsuario { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; } = DateTime.Now;
    public DateTime? UltimoLogin { get; set; }
    
    // Relaciones
    public List<Proyecto> ProyectosAsignados { get; set; } = new();
}

/// <summary>
/// Tipos de usuario del sistema según roles de la empresa constructora
/// </summary>
public enum TipoUsuario
{
    Administrador = 1,
    JefeObra = 2,
    OficinaTecnica = 3,
    Operario = 4
}
