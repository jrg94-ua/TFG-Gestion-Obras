using GestionObras.Core.Entities;
using GestionObras.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GestionObras.Web.Services;

public static class DemoDataSeeder
{
    public static async Task SeedAsync(
        GestionObrasDbContext dbContext,
        UserManager<UsuarioObra> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        var yaExisteDemo = await dbContext.Proyectos.AnyAsync(p => p.Nombre.StartsWith("DEMO - "));
        if (yaExisteDemo)
        {
            return;
        }

        var admin = await EnsureUserAsync(userManager, roleManager,
            "admin@gestionobras.com", "Dirección General", "00000000A", TipoUsuario.Administrador, "Admin123!", "Administrador");

        var jefeNorte = await EnsureUserAsync(userManager, roleManager,
            "jefe.norte@gestionobras.com", "Álvaro Molina Serrano", "81000001J", TipoUsuario.JefeObra, "Demo123!", "JefeObra");
        var jefeSur = await EnsureUserAsync(userManager, roleManager,
            "jefe.sur@gestionobras.com", "Cristina Llorens Vidal", "81000002J", TipoUsuario.JefeObra, "Demo123!", "JefeObra");

        var oficinaSenior = await EnsureUserAsync(userManager, roleManager,
            "oficina.senior@gestionobras.com", "Lucía Sanchis Ferrer", "82000001K", TipoUsuario.OficinaTecnica, "Demo123!", "OficinaTecnica");
        var oficinaPlanificacion = await EnsureUserAsync(userManager, roleManager,
            "oficina.planificacion@gestionobras.com", "Daniel Cervera Pons", "82000002K", TipoUsuario.OficinaTecnica, "Demo123!", "OficinaTecnica");

        var operarioObra1 = await EnsureUserAsync(userManager, roleManager,
            "obra.01@gestionobras.com", "Iván Quintana Beltrán", "83000001L", TipoUsuario.Operario, "Demo123!", "OperarioObra");
        var operarioObra2 = await EnsureUserAsync(userManager, roleManager,
            "obra.02@gestionobras.com", "Sergio Vidal Moreno", "83000002L", TipoUsuario.Operario, "Demo123!", "OperarioObra");
        var operarioObra3 = await EnsureUserAsync(userManager, roleManager,
            "obra.03@gestionobras.com", "Nuria Pastor Simó", "83000003L", TipoUsuario.Operario, "Demo123!", "OperarioObra");
        var operarioObra4 = await EnsureUserAsync(userManager, roleManager,
            "obra.04@gestionobras.com", "Mario Crespo Tomás", "83000004L", TipoUsuario.Operario, "Demo123!", "OperarioObra");

        var operarioTec1 = await EnsureUserAsync(userManager, roleManager,
            "ot.01@gestionobras.com", "Paula Cuesta León", "84000001M", TipoUsuario.Operario, "Demo123!", "OperarioOficinaT");
        var operarioTec2 = await EnsureUserAsync(userManager, roleManager,
            "ot.02@gestionobras.com", "Javier Soler Prats", "84000002M", TipoUsuario.Operario, "Demo123!", "OperarioOficinaT");

        var empleados = new List<Empleado>
        {
            new() { Nombre = "Álvaro", Apellidos = "Molina Serrano", DNI = "81000001J", Email = "jefe.norte@gestionobras.com", Telefono = "600101001", Categoria = CategoriaLaboral.JefeObra, FechaContratacion = DateTime.Today.AddYears(-8), Departamento = "Obra", Cargo = "Jefe de Obra Zona Norte", Activo = true, UsuarioId = jefeNorte.Id },
            new() { Nombre = "Cristina", Apellidos = "Llorens Vidal", DNI = "81000002J", Email = "jefe.sur@gestionobras.com", Telefono = "600101002", Categoria = CategoriaLaboral.JefeObra, FechaContratacion = DateTime.Today.AddYears(-7), Departamento = "Obra", Cargo = "Jefa de Obra Zona Sur", Activo = true, UsuarioId = jefeSur.Id },
            new() { Nombre = "Lucía", Apellidos = "Sanchis Ferrer", DNI = "82000001K", Email = "oficina.senior@gestionobras.com", Telefono = "600101003", Categoria = CategoriaLaboral.Encargado, FechaContratacion = DateTime.Today.AddYears(-6), Departamento = "Oficina Técnica", Cargo = "Coordinadora Técnica", Activo = true, UsuarioId = oficinaSenior.Id },
            new() { Nombre = "Daniel", Apellidos = "Cervera Pons", DNI = "82000002K", Email = "oficina.planificacion@gestionobras.com", Telefono = "600101004", Categoria = CategoriaLaboral.Encargado, FechaContratacion = DateTime.Today.AddYears(-5), Departamento = "Oficina Técnica", Cargo = "Planificador de Producción", Activo = true, UsuarioId = oficinaPlanificacion.Id },
            new() { Nombre = "Iván", Apellidos = "Quintana Beltrán", DNI = "83000001L", Email = "obra.01@gestionobras.com", Telefono = "600101005", Categoria = CategoriaLaboral.OficialPrimera, FechaContratacion = DateTime.Today.AddYears(-4), Departamento = "Cuadrilla Obra", Cargo = "Oficial 1ª Estructura", Activo = true, UsuarioId = operarioObra1.Id },
            new() { Nombre = "Sergio", Apellidos = "Vidal Moreno", DNI = "83000002L", Email = "obra.02@gestionobras.com", Telefono = "600101006", Categoria = CategoriaLaboral.OficialPrimera, FechaContratacion = DateTime.Today.AddYears(-3), Departamento = "Cuadrilla Obra", Cargo = "Oficial 1ª Encofrado", Activo = true, UsuarioId = operarioObra2.Id },
            new() { Nombre = "Nuria", Apellidos = "Pastor Simó", DNI = "83000003L", Email = "obra.03@gestionobras.com", Telefono = "600101007", Categoria = CategoriaLaboral.OficialSegunda, FechaContratacion = DateTime.Today.AddYears(-2), Departamento = "Cuadrilla Obra", Cargo = "Oficial 2ª Albañilería", Activo = true, UsuarioId = operarioObra3.Id },
            new() { Nombre = "Mario", Apellidos = "Crespo Tomás", DNI = "83000004L", Email = "obra.04@gestionobras.com", Telefono = "600101008", Categoria = CategoriaLaboral.OficialSegunda, FechaContratacion = DateTime.Today.AddYears(-2), Departamento = "Cuadrilla Obra", Cargo = "Oficial 2ª Instalaciones", Activo = true, UsuarioId = operarioObra4.Id },
            new() { Nombre = "Paula", Apellidos = "Cuesta León", DNI = "84000001M", Email = "ot.01@gestionobras.com", Telefono = "600101009", Categoria = CategoriaLaboral.OficialTercera, FechaContratacion = DateTime.Today.AddYears(-2), Departamento = "Oficina Técnica", Cargo = "Asistente Técnica", Activo = true, UsuarioId = operarioTec1.Id },
            new() { Nombre = "Javier", Apellidos = "Soler Prats", DNI = "84000002M", Email = "ot.02@gestionobras.com", Telefono = "600101010", Categoria = CategoriaLaboral.OficialTercera, FechaContratacion = DateTime.Today.AddYears(-1), Departamento = "Oficina Técnica", Cargo = "Control Documental", Activo = true, UsuarioId = operarioTec2.Id }
        };

        foreach (var empleado in empleados)
        {
            await EnsureEmpleadoAsync(dbContext, empleado);
        }

        var proveedores = new List<Proveedor>
        {
            new() { Nombre = "Hormigones Levante S.L.", CIF = "B12345671", Direccion = "Pol. Industrial Norte, Valencia", Telefono = "961000001", Email = "ventas@hormigoneslevante.es" },
            new() { Nombre = "Aceros del Turia", CIF = "B12345672", Direccion = "Av. Metalurgia 14, Valencia", Telefono = "961000002", Email = "compras@acerosdelturia.es" },
            new() { Nombre = "Instalaciones Técnicas Mediterráneo", CIF = "B12345673", Direccion = "C/ Electricidad 8, Castellón", Telefono = "961000003", Email = "contacto@itm.es" },
            new() { Nombre = "Cerámicas Costa", CIF = "B12345674", Direccion = "Ctra. Nacional km 22, Alicante", Telefono = "961000004", Email = "pedidos@ceramicascosta.es" },
            new() { Nombre = "Aislamientos Peninsulares", CIF = "B12345675", Direccion = "Parque Logístico 4, Sagunto", Telefono = "961000005", Email = "ofertas@aislapen.es" },
            new() { Nombre = "Prefabricados Delta", CIF = "B12345676", Direccion = "C/ Industria 90, Albacete", Telefono = "961000006", Email = "comercial@prefadelta.es" }
        };

        var proveedoresDb = new List<Proveedor>();
        foreach (var proveedor in proveedores)
        {
            proveedoresDb.Add(await EnsureProveedorAsync(dbContext, proveedor));
        }

        var materiales = new List<Material>
        {
            new() { Codigo = "MAT-HOR-001", Nombre = "Hormigón HA-25", Descripcion = "Hormigón armado para estructura", PrecioUnitario = 95m, UnidadMedida = "m3", StockDisponible = 220, StockMinimo = 60, Categoria = "Estructura", ProveedorId = proveedoresDb[0].Id, ResistenciaCompresion = 25m },
            new() { Codigo = "MAT-HOR-002", Nombre = "Hormigón HA-30", Descripcion = "Hormigón alta resistencia", PrecioUnitario = 108m, UnidadMedida = "m3", StockDisponible = 140, StockMinimo = 45, Categoria = "Estructura", ProveedorId = proveedoresDb[0].Id, ResistenciaCompresion = 30m },
            new() { Codigo = "MAT-ACE-001", Nombre = "Acero corrugado B500S", Descripcion = "Acero para armado", PrecioUnitario = 1.45m, UnidadMedida = "kg", StockDisponible = 18000, StockMinimo = 4500, Categoria = "Estructura", ProveedorId = proveedoresDb[1].Id, ResistenciaCompresion = 500m },
            new() { Codigo = "MAT-ACE-002", Nombre = "Mallazo electrosoldado", Descripcion = "Malla para losas y soleras", PrecioUnitario = 5.8m, UnidadMedida = "m2", StockDisponible = 2400, StockMinimo = 500, Categoria = "Estructura", ProveedorId = proveedoresDb[1].Id },
            new() { Codigo = "MAT-ALB-001", Nombre = "Ladrillo hueco doble", Descripcion = "Cerramientos interiores", PrecioUnitario = 0.42m, UnidadMedida = "ud", StockDisponible = 35000, StockMinimo = 6000, Categoria = "Albañilería", ProveedorId = proveedoresDb[3].Id },
            new() { Codigo = "MAT-ALB-002", Nombre = "Bloque termoarcilla", Descripcion = "Fábrica exterior de alta eficiencia", PrecioUnitario = 2.95m, UnidadMedida = "ud", StockDisponible = 7200, StockMinimo = 1800, Categoria = "Albañilería", ProveedorId = proveedoresDb[3].Id, TransmitanciaTermica = 0.34m },
            new() { Codigo = "MAT-INS-001", Nombre = "Tubo PPR", Descripcion = "Instalación fontanería", PrecioUnitario = 3.9m, UnidadMedida = "m", StockDisponible = 1200, StockMinimo = 300, Categoria = "Instalaciones", ProveedorId = proveedoresDb[2].Id },
            new() { Codigo = "MAT-INS-002", Nombre = "Cable 3x2.5", Descripcion = "Instalación eléctrica", PrecioUnitario = 2.3m, UnidadMedida = "m", StockDisponible = 2600, StockMinimo = 800, Categoria = "Instalaciones", ProveedorId = proveedoresDb[2].Id },
            new() { Codigo = "MAT-INS-003", Nombre = "Cuadro eléctrico modular", Descripcion = "Distribución principal", PrecioUnitario = 420m, UnidadMedida = "ud", StockDisponible = 18, StockMinimo = 6, Categoria = "Instalaciones", ProveedorId = proveedoresDb[2].Id },
            new() { Codigo = "MAT-TER-001", Nombre = "Panel lana mineral", Descripcion = "Aislamiento térmico fachada", PrecioUnitario = 15.8m, UnidadMedida = "m2", StockDisponible = 980, StockMinimo = 250, Categoria = "Aislamientos", ProveedorId = proveedoresDb[4].Id, TransmitanciaTermica = 0.031m, ClasificacionFuego = "A1" },
            new() { Codigo = "MAT-TER-002", Nombre = "Panel sándwich cubierta", Descripcion = "Cerramiento superior industrial", PrecioUnitario = 28.4m, UnidadMedida = "m2", StockDisponible = 620, StockMinimo = 140, Categoria = "Aislamientos", ProveedorId = proveedoresDb[4].Id, TransmitanciaTermica = 0.30m, ClasificacionFuego = "B" },
            new() { Codigo = "MAT-ACA-001", Nombre = "Azulejo porcelánico", Descripcion = "Revestimiento interior", PrecioUnitario = 18.5m, UnidadMedida = "m2", StockDisponible = 480, StockMinimo = 120, Categoria = "Acabados", ProveedorId = proveedoresDb[3].Id },
            new() { Codigo = "MAT-ACA-002", Nombre = "Pintura plástica interior", Descripcion = "Pintura lavable techos y paramentos", PrecioUnitario = 48m, UnidadMedida = "ud", StockDisponible = 95, StockMinimo = 30, Categoria = "Acabados", ProveedorId = proveedoresDb[3].Id },
            new() { Codigo = "MAT-PRE-001", Nombre = "Viga prefabricada pretensada", Descripcion = "Elemento estructural prefabricado", PrecioUnitario = 365m, UnidadMedida = "ud", StockDisponible = 64, StockMinimo = 16, Categoria = "Prefabricados", ProveedorId = proveedoresDb[5].Id }
        };

        foreach (var material in materiales)
        {
            await EnsureMaterialAsync(dbContext, material);
        }

        var proyectos = new List<Proyecto>
        {
            new()
            {
                Nombre = "DEMO - Residencial Las Encinas",
                FechaInicio = DateTime.Today.AddMonths(-10),
                FechaFin = DateTime.Today.AddMonths(6),
                Estado = EstadoProyecto.EnCurso,
                Provincia = "Valencia",
                Municipio = "Paterna",
                TipoSuelo = TipoSuelo.Urbano,
                ZonaClimatica = ZonaClimatica.B3,
                ResponsableId = jefeNorte.Id,
                Presupuesto = new Presupuesto { Total = 2250000m, FechaElaboracion = DateTime.Today.AddMonths(-11), Observaciones = "Promoción 62 viviendas, fase 2" },
                CarpetaLegal = new CarpetaLegal { FechaCreacion = DateTime.Today.AddMonths(-11), DocumentoCTE = "CTE-2026", DocumentoLOTUP = "LOTUP-CV", DocumentoPGOU = "PGOU-Paterna" }
            },
            new()
            {
                Nombre = "DEMO - Rehabilitación Centro Cívico",
                FechaInicio = DateTime.Today.AddMonths(-6),
                FechaFin = DateTime.Today.AddMonths(5),
                Estado = EstadoProyecto.EnCurso,
                Provincia = "Castellón",
                Municipio = "Vila-real",
                TipoSuelo = TipoSuelo.Urbano,
                ZonaClimatica = ZonaClimatica.C3,
                ResponsableId = oficinaSenior.Id,
                Presupuesto = new Presupuesto { Total = 980000m, FechaElaboracion = DateTime.Today.AddMonths(-7), Observaciones = "Rehabilitación energética + accesibilidad" },
                CarpetaLegal = new CarpetaLegal { FechaCreacion = DateTime.Today.AddMonths(-7), DocumentoCTE = "CTE-2026", DocumentoLOTUP = "LOTUP-CV", DocumentoPGOU = "PGOU-Vila-real" }
            },
            new()
            {
                Nombre = "DEMO - Nave Logística Sector 3",
                FechaInicio = DateTime.Today.AddMonths(-3),
                FechaFin = DateTime.Today.AddMonths(13),
                Estado = EstadoProyecto.Planificacion,
                Provincia = "Alicante",
                Municipio = "Elche",
                TipoSuelo = TipoSuelo.Urbano,
                ZonaClimatica = ZonaClimatica.B3,
                ResponsableId = jefeSur.Id,
                Presupuesto = new Presupuesto { Total = 3150000m, FechaElaboracion = DateTime.Today.AddMonths(-4), Observaciones = "Nave logística 22.000 m²" },
                CarpetaLegal = new CarpetaLegal { FechaCreacion = DateTime.Today.AddMonths(-4), DocumentoCTE = "CTE-2026", DocumentoLOTUP = "LOTUP-CV", DocumentoPGOU = "PGOU-Elche" }
            },
            new()
            {
                Nombre = "DEMO - Urbanización Parque Empresarial Oeste",
                FechaInicio = DateTime.Today.AddMonths(-1),
                FechaFin = DateTime.Today.AddMonths(16),
                Estado = EstadoProyecto.Planificacion,
                Provincia = "Valencia",
                Municipio = "Riba-roja",
                TipoSuelo = TipoSuelo.Urbano,
                ZonaClimatica = ZonaClimatica.B3,
                ResponsableId = oficinaPlanificacion.Id,
                Presupuesto = new Presupuesto { Total = 4720000m, FechaElaboracion = DateTime.Today.AddMonths(-2), Observaciones = "Urbanización, viales y redes" },
                CarpetaLegal = new CarpetaLegal { FechaCreacion = DateTime.Today.AddMonths(-2), DocumentoCTE = "CTE-2026", DocumentoLOTUP = "LOTUP-CV", DocumentoPGOU = "PGOU-Riba-roja" }
            }
        };

        dbContext.Proyectos.AddRange(proyectos);
        await dbContext.SaveChangesAsync();

        var tareas = new List<Tarea>();

        tareas.AddRange(CrearPlanProyecto(
            proyectos[0], jefeNorte, oficinaSenior,
            new[] { operarioObra1, operarioObra2, operarioObra3 },
            new[] { operarioTec1, operarioTec2 },
            progresoAlto: true));

        tareas.AddRange(CrearPlanProyecto(
            proyectos[1], jefeNorte, oficinaSenior,
            new[] { operarioObra2, operarioObra3, operarioObra4 },
            new[] { operarioTec1, operarioTec2 },
            progresoAlto: false));

        tareas.AddRange(CrearPlanProyecto(
            proyectos[2], jefeSur, oficinaPlanificacion,
            new[] { operarioObra1, operarioObra4 },
            new[] { operarioTec1 },
            progresoAlto: false,
            forzarBloqueoInstalaciones: true));

        tareas.AddRange(CrearPlanProyecto(
            proyectos[3], jefeSur, oficinaPlanificacion,
            new[] { operarioObra2, operarioObra4 },
            new[] { operarioTec2 },
            progresoAlto: false));

        dbContext.Tareas.AddRange(tareas);
        await dbContext.SaveChangesAsync();

        await SeedFirmasYBloqueosAsync(dbContext, tareas, jefeNorte, jefeSur, oficinaSenior, oficinaPlanificacion);
        await SeedFacturasAsync(dbContext, proyectos, proveedoresDb);
        await SeedSolicitudesMaterialAsync(dbContext, materiales, proyectos, jefeNorte, jefeSur, admin);

        await dbContext.SaveChangesAsync();
    }

    private static List<Tarea> CrearPlanProyecto(
        Proyecto proyecto,
        UsuarioObra jefe,
        UsuarioObra oficina,
        IEnumerable<UsuarioObra> cuadrillaObra,
        IEnumerable<UsuarioObra> cuadrillaTecnica,
        bool progresoAlto,
        bool forzarBloqueoInstalaciones = false)
    {
        var inicio = proyecto.FechaInicio;

        var planificacion = new Tarea
        {
            ProyectoId = proyecto.Id,
            Nombre = "Planificación y replanteo",
            Descripcion = "Actas de replanteo, implantación de obra y validación técnica inicial",
            Estado = EstadoTarea.Finalizado,
            FechaInicio = inicio,
            FechaFin = inicio.AddDays(25),
            FechaFinalizacion = inicio.AddDays(23),
            Prioridad = PrioridadTarea.Critica,
            PresupuestoEstimado = 38000m,
            CostesReales = 36500m,
            Nivel = 0,
            RequiereFirmaConjunta = true,
            UsuariosAsignados = new List<UsuarioObra> { jefe, oficina }
        };

        var estructura = new Tarea
        {
            ProyectoId = proyecto.Id,
            Nombre = "Estructura y cimentación",
            Descripcion = "Cimentación, armado, hormigonado y control estructural",
            Estado = progresoAlto ? EstadoTarea.Finalizado : EstadoTarea.EnCurso,
            FechaInicio = inicio.AddDays(20),
            FechaFin = inicio.AddMonths(4),
            FechaFinalizacion = progresoAlto ? inicio.AddMonths(4).AddDays(-4) : null,
            Prioridad = PrioridadTarea.Critica,
            PresupuestoEstimado = 520000m,
            CostesReales = progresoAlto ? 515000m : 298000m,
            Nivel = 0,
            RequiereFirmaConjunta = true,
            UsuariosAsignados = new List<UsuarioObra> { jefe, oficina }
        };

        var cerramientos = new Tarea
        {
            ProyectoId = proyecto.Id,
            Nombre = "Cerramientos y envolvente",
            Descripcion = "Fachada, cubierta y aislamiento térmico-acústico",
            Estado = progresoAlto ? EstadoTarea.EnCurso : EstadoTarea.Pendiente,
            FechaInicio = inicio.AddMonths(3),
            FechaFin = inicio.AddMonths(7),
            Prioridad = PrioridadTarea.Alta,
            PresupuestoEstimado = 330000m,
            CostesReales = progresoAlto ? 124000m : 18000m,
            Nivel = 0,
            RequiereFirmaConjunta = true,
            UsuariosAsignados = new List<UsuarioObra> { jefe, oficina }
        };

        var instalaciones = new Tarea
        {
            ProyectoId = proyecto.Id,
            Nombre = "Instalaciones técnicas",
            Descripcion = "Electricidad, fontanería, PCI, climatización y telecomunicaciones",
            Estado = forzarBloqueoInstalaciones ? EstadoTarea.Bloqueado : EstadoTarea.Pendiente,
            FechaInicio = inicio.AddMonths(5),
            FechaFin = inicio.AddMonths(9),
            Prioridad = PrioridadTarea.Alta,
            PresupuestoEstimado = 295000m,
            CostesReales = forzarBloqueoInstalaciones ? 32500m : 0m,
            Nivel = 0,
            RequiereFirmaConjunta = true,
            UsuariosAsignados = new List<UsuarioObra> { jefe, oficina }
        };

        var acabados = new Tarea
        {
            ProyectoId = proyecto.Id,
            Nombre = "Acabados y legalización",
            Descripcion = "Acabados finales, remates, pruebas y tramitación de cierre",
            Estado = EstadoTarea.Pendiente,
            FechaInicio = inicio.AddMonths(8),
            FechaFin = inicio.AddMonths(11),
            Prioridad = PrioridadTarea.Media,
            PresupuestoEstimado = 240000m,
            CostesReales = 0m,
            Nivel = 0,
            RequiereFirmaConjunta = true,
            UsuariosAsignados = new List<UsuarioObra> { jefe, oficina }
        };

        var entrega = new Tarea
        {
            ProyectoId = proyecto.Id,
            Nombre = "Entrega y cierre de obra",
            Descripcion = "As-built, certificación final y acta de entrega",
            Estado = EstadoTarea.Pendiente,
            FechaInicio = inicio.AddMonths(11),
            FechaFin = inicio.AddMonths(12),
            Prioridad = PrioridadTarea.Media,
            PresupuestoEstimado = 78000m,
            CostesReales = 0m,
            Nivel = 0,
            RequiereFirmaConjunta = true,
            UsuariosAsignados = new List<UsuarioObra> { jefe, oficina }
        };

        var subCimentacion = new Tarea
        {
            ProyectoId = proyecto.Id,
            TareaPadre = estructura,
            Nombre = "Cimentación profunda",
            Descripcion = "Pilotes, encepados y losa de cimentación",
            Estado = progresoAlto ? EstadoTarea.Finalizado : EstadoTarea.EnCurso,
            FechaInicio = inicio.AddDays(20),
            FechaFin = inicio.AddMonths(2),
            FechaFinalizacion = progresoAlto ? inicio.AddMonths(2).AddDays(-3) : null,
            Prioridad = PrioridadTarea.Alta,
            PresupuestoEstimado = 165000m,
            CostesReales = progresoAlto ? 162000m : 118000m,
            Nivel = 1,
            UsuariosAsignados = cuadrillaObra.Take(2).Append(jefe).ToList()
        };

        var subEstructuraVertical = new Tarea
        {
            ProyectoId = proyecto.Id,
            TareaPadre = estructura,
            Nombre = "Estructura vertical y forjados",
            Descripcion = "Pilares, vigas y forjados de plantas tipo",
            Estado = progresoAlto ? EstadoTarea.Finalizado : EstadoTarea.Pendiente,
            FechaInicio = inicio.AddMonths(1),
            FechaFin = inicio.AddMonths(4),
            FechaFinalizacion = progresoAlto ? inicio.AddMonths(4).AddDays(-7) : null,
            Prioridad = PrioridadTarea.Alta,
            PresupuestoEstimado = 245000m,
            CostesReales = progresoAlto ? 242000m : 36000m,
            Nivel = 1,
            UsuariosAsignados = cuadrillaObra.Take(2).Append(jefe).ToList()
        };

        var subInstalacionElectrica = new Tarea
        {
            ProyectoId = proyecto.Id,
            TareaPadre = instalaciones,
            Nombre = "Instalación eléctrica BT",
            Descripcion = "Bandejas, cableado y cuadros de planta",
            Estado = forzarBloqueoInstalaciones ? EstadoTarea.Bloqueado : EstadoTarea.Pendiente,
            FechaInicio = inicio.AddMonths(5),
            FechaFin = inicio.AddMonths(7),
            Prioridad = PrioridadTarea.Alta,
            PresupuestoEstimado = 92000m,
            CostesReales = forzarBloqueoInstalaciones ? 15800m : 0m,
            Nivel = 1,
            UsuariosAsignados = cuadrillaObra.Skip(1).Take(2).Concat(cuadrillaTecnica.Take(1)).Append(oficina).ToList()
        };

        var subInstalacionHidraulica = new Tarea
        {
            ProyectoId = proyecto.Id,
            TareaPadre = instalaciones,
            Nombre = "Fontanería y saneamiento",
            Descripcion = "Redes verticales/horizontales y pruebas de estanqueidad",
            Estado = EstadoTarea.Pendiente,
            FechaInicio = inicio.AddMonths(6),
            FechaFin = inicio.AddMonths(8),
            Prioridad = PrioridadTarea.Media,
            PresupuestoEstimado = 76000m,
            CostesReales = 0m,
            Nivel = 1,
            UsuariosAsignados = cuadrillaObra.Skip(1).Take(1).Concat(cuadrillaTecnica).Append(oficina).ToList()
        };

        var todas = new List<Tarea>
        {
            planificacion,
            estructura,
            cerramientos,
            instalaciones,
            acabados,
            entrega,
            subCimentacion,
            subEstructuraVertical,
            subInstalacionElectrica,
            subInstalacionHidraulica
        };

        // Dependencias de primer nivel
        estructura.Predecesoras.Add(planificacion);
        cerramientos.Predecesoras.Add(estructura);
        instalaciones.Predecesoras.Add(cerramientos);
        acabados.Predecesoras.Add(instalaciones);
        entrega.Predecesoras.Add(acabados);

        // Dependencias de subtareas
        subEstructuraVertical.Predecesoras.Add(subCimentacion);
        subInstalacionElectrica.Predecesoras.Add(cerramientos);
        subInstalacionHidraulica.Predecesoras.Add(subInstalacionElectrica);

        return todas;
    }

    private static async Task SeedFirmasYBloqueosAsync(
        GestionObrasDbContext dbContext,
        List<Tarea> tareas,
        UsuarioObra jefeNorte,
        UsuarioObra jefeSur,
        UsuarioObra oficinaSenior,
        UsuarioObra oficinaPlanificacion)
    {
        var firmas = new List<FirmaTarea>();

        foreach (var tarea in tareas.Where(t => t.RequiereFirmaConjunta))
        {
            var asignados = tarea.UsuariosAsignados.Where(u => !string.IsNullOrWhiteSpace(u.Id)).ToList();
            if (!asignados.Any())
            {
                continue;
            }

            if (tarea.Estado == EstadoTarea.Finalizado)
            {
                foreach (var usuario in asignados)
                {
                    firmas.Add(new FirmaTarea
                    {
                        TareaId = tarea.Id,
                        UsuarioId = usuario.Id,
                        FechaFirma = tarea.FechaFinalizacion?.AddDays(-1) ?? DateTime.Now.AddDays(-7),
                        Observaciones = "Conforme a planificación y control de calidad",
                        Aprobada = true
                    });
                }
            }
            else if (tarea.Estado == EstadoTarea.EnCurso)
            {
                var primero = asignados.First();
                firmas.Add(new FirmaTarea
                {
                    TareaId = tarea.Id,
                    UsuarioId = primero.Id,
                    FechaFirma = DateTime.Now.AddDays(-5),
                    Observaciones = "Avance validado en comité semanal",
                    Aprobada = true
                });
            }
        }

        // Caso realista de rechazo previo en un paquete de instalaciones
        var tareaBloqueada = tareas.FirstOrDefault(t => t.Estado == EstadoTarea.Bloqueado && t.RequiereFirmaConjunta);
        if (tareaBloqueada != null)
        {
            var firmante = tareaBloqueada.UsuariosAsignados.FirstOrDefault(u => u.Id == oficinaPlanificacion.Id)
                ?? tareaBloqueada.UsuariosAsignados.FirstOrDefault(u => u.Id == oficinaSenior.Id)
                ?? tareaBloqueada.UsuariosAsignados.FirstOrDefault();

            if (firmante != null)
            {
                firmas.Add(new FirmaTarea
                {
                    TareaId = tareaBloqueada.Id,
                    UsuarioId = firmante.Id,
                    FechaFirma = DateTime.Now.AddDays(-3),
                    Observaciones = "Se detectan incompatibilidades con trazado de instalaciones",
                    Aprobada = false,
                    MotivoRechazo = "Pendiente replanteo de canalizaciones y coordinación con estructura existente"
                });
            }

            dbContext.BloqueosTareas.Add(new BloqueoTarea
            {
                TareaId = tareaBloqueada.Id,
                Tipo = TipoBloqueo.ErrorEjecucion,
                JustificacionTecnica = "Incompatibilidad detectada entre bandejas eléctricas y pasos de instalaciones hidráulicas. Requiere nueva propuesta técnica.",
                FechaBloqueo = DateTime.Now.AddDays(-2)
            });
        }

        dbContext.FirmasTareas.AddRange(firmas);
        await dbContext.SaveChangesAsync();
    }

    private static async Task SeedFacturasAsync(
        GestionObrasDbContext dbContext,
        List<Proyecto> proyectos,
        List<Proveedor> proveedores)
    {
        var hoy = DateTime.Today;

        var facturas = new List<Factura>
        {
            CrearFactura("FAC-2026-101", "Suministro hormigón y bombeo", hoy.AddDays(-85), hoy.AddDays(-55), EstadoFactura.Pagada, 62400m, proyectos[0], proveedores[0], "Transferencia", hoy.AddDays(-50), "Certificación 03"),
            CrearFactura("FAC-2026-102", "Acero corrugado B500S", hoy.AddDays(-63), hoy.AddDays(-33), EstadoFactura.Pagada, 48200m, proyectos[0], proveedores[1], "Transferencia", hoy.AddDays(-30), "Pedido marco trimestral"),
            CrearFactura("FAC-2026-103", "Aislamiento envolvente", hoy.AddDays(-31), hoy.AddDays(-1), EstadoFactura.Pendiente, 29750m, proyectos[0], proveedores[4], "Confirming", null, "Recepción parcial de material"),
            CrearFactura("FAC-2026-104", "Reforma instalaciones existentes", hoy.AddDays(-44), hoy.AddDays(-14), EstadoFactura.Pagada, 21890m, proyectos[1], proveedores[2], "Transferencia", hoy.AddDays(-10), "Incluye mano de obra especializada"),
            CrearFactura("FAC-2026-105", "Reposición cerámica y alicatado", hoy.AddDays(-20), hoy.AddDays(10), EstadoFactura.Pendiente, 14120m, proyectos[1], proveedores[3], "Transferencia", null, "Pendiente conformidad final"),
            CrearFactura("FAC-2026-106", "Prefabricados de cubierta", hoy.AddDays(-12), hoy.AddDays(18), EstadoFactura.Pendiente, 56250m, proyectos[2], proveedores[5], "Confirming", null, "Fase inicial de acopio"),
            CrearFactura("FAC-2026-107", "Canalizaciones eléctricas", hoy.AddDays(-9), hoy.AddDays(21), EstadoFactura.Pendiente, 18940m, proyectos[2], proveedores[2], "Transferencia", null, "En revisión de medición"),
            CrearFactura("FAC-2026-108", "Viales y urbanización - lote 1", hoy.AddDays(-6), hoy.AddDays(24), EstadoFactura.Pendiente, 77300m, proyectos[3], proveedores[0], "Transferencia", null, "Certificación inicial"),
            CrearFactura("FAC-2026-109", "Ferralla para drenajes", hoy.AddDays(-52), hoy.AddDays(-22), EstadoFactura.Vencida, 16480m, proyectos[3], proveedores[1], "Transferencia", null, "Pendiente validación de albaranes")
        };

        dbContext.Facturas.AddRange(facturas);
        await dbContext.SaveChangesAsync();
    }

    private static async Task SeedSolicitudesMaterialAsync(
        GestionObrasDbContext dbContext,
        List<Material> materiales,
        List<Proyecto> proyectos,
        UsuarioObra jefeNorte,
        UsuarioObra jefeSur,
        UsuarioObra admin)
    {
        var matHormigon = await dbContext.Materiales.FirstAsync(m => m.Codigo == "MAT-HOR-002");
        var matCable = await dbContext.Materiales.FirstAsync(m => m.Codigo == "MAT-INS-002");
        var matPanel = await dbContext.Materiales.FirstAsync(m => m.Codigo == "MAT-TER-001");
        var matPref = await dbContext.Materiales.FirstAsync(m => m.Codigo == "MAT-PRE-001");

        var solicitudes = new List<SolicitudMaterial>
        {
            new()
            {
                MaterialId = matHormigon.Id,
                ProyectoId = proyectos[0].Id,
                CantidadSolicitada = 120m,
                Justificacion = "Hormigonado de forjados planta 3 y 4, según planificación semanal aprobada",
                SolicitadoPorId = jefeNorte.Id,
                FechaSolicitud = DateTime.Now.AddDays(-9),
                Estado = EstadoSolicitudMaterial.Aprobada,
                RevisadoPorId = admin.Id,
                FechaRespuesta = DateTime.Now.AddDays(-8),
                ObservacionesAdmin = "Aprobada por consumo real y curva S de obra",
                Prioridad = PrioridadSolicitud.Alta,
                FechaNecesaria = DateTime.Today.AddDays(-2)
            },
            new()
            {
                MaterialId = matCable.Id,
                ProyectoId = proyectos[2].Id,
                CantidadSolicitada = 1500m,
                Justificacion = "Canalización de cuadros secundarios en nave logística",
                SolicitadoPorId = jefeSur.Id,
                FechaSolicitud = DateTime.Now.AddDays(-5),
                Estado = EstadoSolicitudMaterial.Pendiente,
                Prioridad = PrioridadSolicitud.Urgente,
                FechaNecesaria = DateTime.Today.AddDays(4)
            },
            new()
            {
                MaterialId = matPanel.Id,
                ProyectoId = proyectos[1].Id,
                CantidadSolicitada = 420m,
                Justificacion = "Mejora energética de envolvente en rehabilitación",
                SolicitadoPorId = jefeNorte.Id,
                FechaSolicitud = DateTime.Now.AddDays(-14),
                Estado = EstadoSolicitudMaterial.Rechazada,
                RevisadoPorId = admin.Id,
                FechaRespuesta = DateTime.Now.AddDays(-12),
                ObservacionesAdmin = "Rechazada por desviación de presupuesto. Se aprueba lote parcial de 180 m2",
                Prioridad = PrioridadSolicitud.Media,
                FechaNecesaria = DateTime.Today.AddDays(-6)
            },
            new()
            {
                MaterialId = matPref.Id,
                ProyectoId = proyectos[3].Id,
                CantidadSolicitada = 28m,
                Justificacion = "Inicio de montaje en zona de drenaje y pasos técnicos",
                SolicitadoPorId = jefeSur.Id,
                FechaSolicitud = DateTime.Now.AddDays(-2),
                Estado = EstadoSolicitudMaterial.Pendiente,
                Prioridad = PrioridadSolicitud.Alta,
                FechaNecesaria = DateTime.Today.AddDays(9)
            }
        };

        dbContext.SolicitudesMateriales.AddRange(solicitudes);
        await dbContext.SaveChangesAsync();
    }

    private static Factura CrearFactura(
        string numero,
        string concepto,
        DateTime emision,
        DateTime vencimiento,
        EstadoFactura estado,
        decimal baseImponible,
        Proyecto proyecto,
        Proveedor proveedor,
        string metodoPago,
        DateTime? fechaPago,
        string observaciones)
    {
        var porcentajeIva = 21m;
        var iva = Math.Round(baseImponible * (porcentajeIva / 100m), 2);
        var total = baseImponible + iva;

        return new Factura
        {
            NumeroFactura = numero,
            Concepto = concepto,
            FechaEmision = emision,
            FechaVencimiento = vencimiento,
            Estado = estado,
            FechaPago = fechaPago,
            BaseImponible = baseImponible,
            PorcentajeIVA = porcentajeIva,
            IVA = iva,
            ImporteTotal = total,
            Importe = total,
            ProyectoId = proyecto.Id,
            ProveedorId = proveedor.Id,
            NombreProyecto = proyecto.Nombre,
            MetodoPago = metodoPago,
            Observaciones = observaciones
        };
    }

    private static async Task EnsureEmpleadoAsync(GestionObrasDbContext dbContext, Empleado empleado)
    {
        var existente = await dbContext.Empleados.FirstOrDefaultAsync(e => e.DNI == empleado.DNI);
        if (existente == null)
        {
            dbContext.Empleados.Add(empleado);
            await dbContext.SaveChangesAsync();
            return;
        }

        existente.Nombre = empleado.Nombre;
        existente.Apellidos = empleado.Apellidos;
        existente.Email = empleado.Email;
        existente.Telefono = empleado.Telefono;
        existente.Categoria = empleado.Categoria;
        existente.FechaContratacion = empleado.FechaContratacion;
        existente.Departamento = empleado.Departamento;
        existente.Cargo = empleado.Cargo;
        existente.Activo = empleado.Activo;
        existente.UsuarioId = empleado.UsuarioId;

        await dbContext.SaveChangesAsync();
    }

    private static async Task<Proveedor> EnsureProveedorAsync(GestionObrasDbContext dbContext, Proveedor proveedor)
    {
        var existente = await dbContext.Proveedores.FirstOrDefaultAsync(p => p.CIF == proveedor.CIF);
        if (existente == null)
        {
            dbContext.Proveedores.Add(proveedor);
            await dbContext.SaveChangesAsync();
            return proveedor;
        }

        existente.Nombre = proveedor.Nombre;
        existente.Direccion = proveedor.Direccion;
        existente.Telefono = proveedor.Telefono;
        existente.Email = proveedor.Email;
        await dbContext.SaveChangesAsync();

        return existente;
    }

    private static async Task EnsureMaterialAsync(GestionObrasDbContext dbContext, Material material)
    {
        var existente = await dbContext.Materiales.FirstOrDefaultAsync(m => m.Codigo == material.Codigo);
        if (existente == null)
        {
            dbContext.Materiales.Add(material);
            await dbContext.SaveChangesAsync();
            return;
        }

        existente.Nombre = material.Nombre;
        existente.Descripcion = material.Descripcion;
        existente.PrecioUnitario = material.PrecioUnitario;
        existente.UnidadMedida = material.UnidadMedida;
        existente.StockDisponible = material.StockDisponible;
        existente.StockMinimo = material.StockMinimo;
        existente.Categoria = material.Categoria;
        existente.ProveedorId = material.ProveedorId;
        existente.TransmitanciaTermica = material.TransmitanciaTermica;
        existente.ClasificacionFuego = material.ClasificacionFuego;
        existente.Densidad = material.Densidad;
        existente.ResistenciaCompresion = material.ResistenciaCompresion;

        await dbContext.SaveChangesAsync();
    }

    private static async Task<UsuarioObra> EnsureUserAsync(
        UserManager<UsuarioObra> userManager,
        RoleManager<IdentityRole> roleManager,
        string email,
        string nombreCompleto,
        string dni,
        TipoUsuario tipoUsuario,
        string password,
        params string[] roles)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
        {
            user = new UsuarioObra
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                NombreCompleto = nombreCompleto,
                DNI = dni,
                TipoUsuario = tipoUsuario,
                Activo = true,
                FechaCreacion = DateTime.Now,
                TelefonoMovil = "600000000",
                Cargo = roles.FirstOrDefault() ?? "Usuario"
            };

            var createResult = await userManager.CreateAsync(user, password);
            if (!createResult.Succeeded)
            {
                throw new InvalidOperationException($"No se pudo crear usuario demo {email}: {string.Join("; ", createResult.Errors.Select(e => e.Description))}");
            }
        }
        else
        {
            user.NombreCompleto = nombreCompleto;
            user.DNI = dni;
            user.TipoUsuario = tipoUsuario;
            user.Activo = true;
            user.Cargo = roles.FirstOrDefault() ?? user.Cargo;
            await userManager.UpdateAsync(user);
        }

        foreach (var rol in roles.Distinct())
        {
            if (!await roleManager.RoleExistsAsync(rol))
            {
                await roleManager.CreateAsync(new IdentityRole(rol));
            }

            if (!await userManager.IsInRoleAsync(user, rol))
            {
                await userManager.AddToRoleAsync(user, rol);
            }
        }

        return user;
    }
}
