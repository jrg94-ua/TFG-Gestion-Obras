namespace GestionObras.Core.Entities;

public static class UsuarioPerfilRules
{
    public static readonly TipoUsuario[] TiposOperativos =
    [
        TipoUsuario.Operario,
        TipoUsuario.OperarioObra,
        TipoUsuario.OperarioOficinaT
    ];

    public static bool EsPerfilOperativo(this TipoUsuario tipoUsuario)
    {
        return TiposOperativos.Contains(tipoUsuario);
    }

    public static bool EsPerfilOperativo(this UsuarioObra? usuario)
    {
        return usuario != null && usuario.TipoUsuario.EsPerfilOperativo();
    }

    public static TipoUsuario MapRolPrincipalATipoUsuario(string rol)
    {
        return rol switch
        {
            "Administrador" => TipoUsuario.Administrador,
            "JefeObra" => TipoUsuario.JefeObra,
            "OficinaTecnica" => TipoUsuario.OficinaTecnica,
            "RecursosHumanos" => TipoUsuario.RecursosHumanos,
            "OperarioObra" => TipoUsuario.OperarioObra,
            "OperarioOficinaT" => TipoUsuario.OperarioOficinaT,
            "Operario" => TipoUsuario.Operario,
            _ => TipoUsuario.Operario
        };
    }

    public static string MapTipoUsuarioARolPrincipal(TipoUsuario tipoUsuario)
    {
        return tipoUsuario switch
        {
            TipoUsuario.Administrador => "Administrador",
            TipoUsuario.JefeObra => "JefeObra",
            TipoUsuario.OficinaTecnica => "OficinaTecnica",
            TipoUsuario.RecursosHumanos => "RecursosHumanos",
            TipoUsuario.OperarioObra => "OperarioObra",
            TipoUsuario.OperarioOficinaT => "OperarioOficinaT",
            TipoUsuario.Operario => "Operario",
            _ => "Operario"
        };
    }
}
