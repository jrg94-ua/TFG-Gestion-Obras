# Análisis de Requisitos del Sistema

## Introducción

Este documento detalla los requisitos funcionales y no funcionales del **Sistema de Gestión de Obras para PYMEs Constructoras**, basado en la propuesta de TFG de Jorge Ros Gómez.

---

## 1. Actores del Sistema

### 1.1 Actores Humanos (Perfiles de Usuario)

| Actor | Descripción | Nivel de Acceso |
|-------|-------------|-----------------|
| **Administrador/Gerente** | Responsable de la supervisión estratégica y financiera de la empresa | Total |
| **Jefe de Obra** | Controla la operativa diaria a pie de obra | Operativo elevado |
| **Oficina Técnica** | Gestiona la validación técnica y cumplimiento normativo | Técnico especializado |
| **Operario** | Personal de obra con acceso básico para fichaje y consultas | Limitado |

### 1.2 Servicios Automatizados (Actores del Sistema)

#### Servicio Nacional (CTE)
- **Responsabilidad**: Vigilancia del Código Técnico de la Edificación
- **Fuente de datos**: BOE (Boletín Oficial del Estado) mediante RSS
- **Alcance**: Aplicación transversal a todos los proyectos en España

#### Servicio Autonómico (LOTUP - Comunidad Valenciana)
- **Responsabilidad**: Control de normativa autonómica de urbanismo
- **Fuente de datos**: DOCV (Diari Oficial de la Comunitat Valenciana)
- **Alcance**: Proyectos en suelo rústico de la C.V. (distancias, ocupación, paisaje)

#### Servicio Local (IA Urbanística)
- **Responsabilidad**: Localización y análisis de Planes Generales (PGOU)
- **Tecnología**: IA híbrida (OpenAI/Tavily + Llama 3 con RAG)
- **Alcance**: Extracción de parámetros municipales (alturas, retranqueos, estética)

---

## 2. Lógica de Decisión del Sistema

### Flujo de Validación Normativa

```
ENTRADA DE DATOS
│
├─ Paso 1: Ubicación
│  └─ Selección: Comunidad Valenciana
│
├─ Paso 2: Naturaleza del suelo
│  ├─ Suelo Urbano → Activa: CTE + PGOU Municipal
│  └─ Suelo Rústico → Activa: CTE + LOTUP + PGOU Municipal
│
└─ Paso 3: Validación Técnica
   ├─ Servicio Nacional: Verifica cumplimiento CTE
   ├─ Servicio Autonómico: Aplica restricciones LOTUP (si rústico)
   └─ Servicio Local: Extrae parámetros del PGOU
```

---

## 3. Requisitos Funcionales (RF)

### Bloque 1: Gestión de Proyectos e Inteligencia Normativa

| ID | Requisito | Descripción | Prioridad |
|----|-----------|-------------|-----------|
| **RF-01** | Alta y Parametrización de Obra | Crear proyectos definiendo ubicación (Provincia/Municipio) y tipo de suelo (Urbano/Rústico) | Alta |
| **RF-02** | Sincronización con el BOE | Consulta automática de índices de códigos electrónicos para asegurar vigencia normativa estatal | Alta |
| **RF-03** | Integración con Catálogo del CTE | Vinculación de materiales con exigencias de Documentos Básicos (DB) mediante archivos XML | Alta |
| **RF-04** | Agente de Inteligencia Territorial | Localización automática del PGOU en sedes electrónicas municipales | Media |
| **RF-05** | Extracción de Parámetros Urbanísticos | IA identifica altura máxima, retranqueos, edificabilidad del PDF municipal | Media |

### Bloque 2: Gestión de Operaciones y Producción

| ID | Requisito | Descripción | Prioridad |
|----|-----------|-------------|-----------|
| **RF-06** | Tablero Kanban de Obra | Organización visual de fases (Cimentación, Estructura, etc.) con estados: Pendiente, En curso, Bloqueado, Finalizado | Alta |
| **RF-07** | Gestión de Bloqueos | Obligatoriedad de justificar técnicamente tareas paradas (falta material, error ejecución, incidencia normativa) | Alta |
| **RF-08** | Registro de Mediciones Reales | Introducción diaria de unidades ejecutadas para comparar con presupuesto inicial | Alta |
| **RF-09** | Diario de Obra Digital | Muro cronológico con fotos de incidencias y comentarios técnicos con sello de tiempo | Media |

### Bloque 3: Gestión de Recursos Humanos y Seguridad

| ID | Requisito | Descripción | Prioridad |
|----|-----------|-------------|-----------|
| **RF-10** | Asignación de Personal | Destinar operarios y subcontratas a proyectos según categoría profesional | Alta |
| **RF-11** | Registro de Jornada Geolocalizado | Fichaje entrada/salida validando ubicación del empleado | Alta |
| **RF-12** | Control de Horarios y Turnos | Gestión de calendarios laborales y control de horas extra por convenio | Media |
| **RF-13** | Validación de Seguridad (PRL) | Bloqueo de acceso a tareas si el empleado no tiene cursos de prevención vigentes | Alta |

### Bloque 4: Gestión Económica y Compras

| ID | Requisito | Descripción | Prioridad |
|----|-----------|-------------|-----------|
| **RF-14** | Gestión de Facturas y Gastos | Subida y archivo de facturas vinculadas a partidas específicas | Alta |
| **RF-15** | Control de Estados de Pago | Seguimiento de facturas pendientes, vencidas y pagadas con alertas de tesorería | Alta |
| **RF-16** | Contabilidad de Materiales (Stock) | Descuento automático del inventario al registrar uso en tareas del tablero | Media |
| **RF-17** | Comparativa de Presupuestos | Herramienta de licitación interna para elegir oferta más económica por material | Media |
| **RF-18** | Cálculo de ROI en Tiempo Real | Dashboard mostrando rentabilidad esperada vs real basada en costes actuales | Alta |

### Bloque 5: Comunicación y Notificaciones

| ID | Requisito | Descripción | Prioridad |
|----|-----------|-------------|-----------|
| **RF-19** | Centro de Notificaciones Push | Avisos al móvil sobre cambios importantes en el proyecto | Media |
| **RF-20** | Carpeta Documental Inteligente | Repositorio centralizado por obra con normativa (PGOU, LOTUP, CTE) y licencias | Alta |

---

## 4. Requisitos No Funcionales (RNF)

### Bloque 1: Seguridad y Protección de Datos

| ID | Requisito | Descripción | Estándar |
|----|-----------|-------------|----------|
| **RNF-01** | Autenticación y Autorización | Control de acceso basado en roles (RBAC): operario no accede a facturas/ROI | OWASP |
| **RNF-02** | Integridad de los Datos | Integridad referencial mediante BD centralizada: eliminación de material no borra historial facturas | ACID |
| **RNF-03** | Cifrado de Información | Datos entre dispositivos de obra y servidor bajo protocolo HTTPS | TLS 1.3 |

### Bloque 2: Disponibilidad y Rendimiento

| ID | Requisito | Descripción | Métrica |
|----|-----------|-------------|---------|
| **RNF-04** | Ubicuidad (Acceso 24/7) | Arquitectura Cloud: disponible para jefe de obra en cualquier momento/lugar con internet | 99.9% uptime |
| **RNF-05** | Tiempo de Respuesta | Consultas normativa BOE o carga Kanban no deben superar 2 segundos | < 2s |
| **RNF-06** | Diseño Responsive | Interfaz adaptada a smartphones/tablets (medio principal de uso en obra) | Mobile-first |

### Bloque 3: Escalabilidad y Mantenibilidad

| ID | Requisito | Descripción | Patrón |
|----|-----------|-------------|--------|
| **RNF-07** | Arquitectura Evolutiva | Diseño para integrar APIs de administración pública (Open Data) sin reescribir núcleo | Microservicios |
| **RNF-08** | Capacidad de Integración | Soporte de formatos estándar: XML (CTE), RSS (BOE) | Interoperabilidad |
| **RNF-09** | Robustez frente a Fallos | Ante pérdida de conexión en obra, no corromper datos del diario de obra | Eventual consistency |

---

## 5. Casos de Uso Detallados

### CU-01: Inicio de Nuevo Proyecto

**Actor Principal**: Jefe de Obra  
**Precondiciones**: Usuario autenticado con rol "Jefe de Obra"

**Flujo Principal**:
1. Usuario selecciona "Nuevo Proyecto"
2. Sistema solicita: Nombre, Ubicación (Provincia/Municipio), Tipo de suelo
3. Usuario introduce: "Vivienda Rural" - Comunidad Valenciana - Valencia - Suelo Rústico
4. Sistema activa Servicios de Inteligencia:
   - 4.1. Consulta RSS del BOE → Descarga CTE vigente
   - 4.2. Activa Servicio LOTUP (por ser suelo rústico en C.V.)
   - 4.3. IA busca PGOU de Valencia en sede electrónica
   - 4.4. IA extrae parámetros: distancia mínima núcleo urbano, ocupación máxima
5. Sistema crea "Carpeta Legal" del proyecto con todos los documentos
6. Sistema genera Tablero Kanban con fases estándar
7. Sistema notifica a Oficina Técnica para validación técnica

**Postcondiciones**: Proyecto creado con normativa aplicable prevalidada

---

### CU-02: Validación de Material con CTE

**Actor Principal**: Oficina Técnica  
**Precondiciones**: Proyecto existente, usuario con rol "Oficina Técnica"

**Flujo Principal**:
1. Usuario accede a "Presupuestos" del proyecto
2. Usuario añade material: "Panel aislante XPS 80mm"
3. Sistema busca en Catálogo del CTE (archivo XML del Ministerio)
4. Sistema extrae propiedades técnicas del material:
   - Transmitancia térmica: 0.35 W/m²K
   - Clasificación fuego: E
5. Sistema valida contra requisitos DB-HE (Ahorro de Energía):
   - Para zona climática B3 (Valencia): Límite 0.38 W/m²K
6. Sistema muestra: ✅ "Material CUMPLE requisitos energéticos"
7. Sistema vincula material a partida del presupuesto

**Flujo Alternativo 4A**: Material NO cumple requisitos
- 4A.1. Sistema muestra: ❌ "Material NO CUMPLE. Transmitancia máxima: 0.38 W/m²K"
- 4A.2. Sistema sugiere materiales alternativos del catálogo
- 4A.3. Vuelve al paso 2

**Postcondiciones**: Material validado y presupuesto técnicamente correcto

---

### CU-03: Monitorización de ROI en Tiempo Real

**Actor Principal**: Administrador/Gerente  
**Precondiciones**: Proyecto con presupuesto inicial cargado y mediciones en curso

**Flujo Principal**:
1. Usuario accede al Dashboard Financiero
2. Sistema calcula métricas en tiempo real:
   - **Presupuesto inicial**: 250.000 €
   - **Costes reales acumulados**: 180.000 €
   - **% Ejecución física**: 65%
   - **Proyección a cierre**: 276.923 € (180k / 0.65)
   - **Desviación prevista**: +26.923 € (+10.7%)
3. Sistema muestra gráfico comparativo por partidas:
   - Cimentación: -2% (ahorrado)
   - Estructura: +15% (sobrecostado)
   - Instalaciones: 0% (ajustado)
4. Sistema identifica causa de desviación:
   - "Estructura: 3 días extra de grúa (2.500 €/día) = +7.500 €"
5. Sistema calcula nuevo ROI proyectado:
   - Beneficio inicial esperado: 15% (37.500 €)
   - Beneficio proyectado: 9.8% (24.577 €)
6. Sistema genera alerta: ⚠️ "Margen por debajo del 10% configurado"

**Postcondiciones**: Gerente identifica partida problemática y toma decisiones correctivas

---

## 6. Matriz de Trazabilidad

| Requisito Funcional | Objetivo del TFG | Actor Principal | Servicio Relacionado |
|---------------------|------------------|-----------------|----------------------|
| RF-01, RF-02, RF-03, RF-04, RF-05 | Automatizar cumplimiento normativo | Sistema + Oficina Técnica | BOE, CTE, IA Local |
| RF-06, RF-07, RF-08, RF-09 | Optimizar operativa de obra | Jefe de Obra | Kanban |
| RF-10, RF-11, RF-12, RF-13 | Gestión RRHH y PRL | Jefe de Obra + Operario | Geolocalización, PRL |
| RF-14, RF-15, RF-16, RF-17, RF-18 | Optimizar ROI | Gerente + Jefe de Obra | Contabilidad, ROI |
| RF-19, RF-20 | Centralizar documentación | Todos | Notificaciones, Docs |

---

## 7. Criterios de Aceptación

### Para Requisitos de Inteligencia Normativa
- ✅ El sistema debe detectar cambios en el BOE en un plazo máximo de 24 horas
- ✅ La IA debe localizar el PGOU en un 90% de los municipios españoles > 5.000 habitantes
- ✅ La extracción de parámetros urbanísticos debe tener precisión > 85%

### Para Requisitos de Operativa
- ✅ El tablero Kanban debe cargar en < 1 segundo para proyectos con < 100 tareas
- ✅ El fichaje geolocalizado debe validar la ubicación en un radio de 100 metros de la obra

### Para Requisitos de ROI
- ✅ El cálculo del ROI debe actualizarse en < 5 segundos tras registrar una nueva factura
- ✅ Las desviaciones > 5% deben generar notificación automática al gerente

---

## 8. Riesgos Identificados

| Riesgo | Probabilidad | Impacto | Mitigación |
|--------|--------------|---------|------------|
| Estructura de PGOU municipal heterogénea | Alta | Alto | Implementar múltiples parsers + validación manual |
| Cambios frecuentes en API del BOE | Media | Medio | Diseño con patrón Adapter para fácil sustitución |
| Latencia en consultas de IA | Media | Bajo | Implementar caché local de resultados previos |
| Pérdida de conexión en obra | Alta | Bajo | Modo offline con sincronización diferida |

---

## 9. Próximos Pasos

1. **Fase de Diseño**: Crear diagramas UML (casos de uso, secuencia, clases)
2. **Prototipado**: Mockups de interfaz de usuario para cada perfil
3. **Desarrollo Iterativo**: Sprints de 2 semanas priorizando MVP
4. **Testing**: Pruebas con datos reales de PGOU y CTE
5. **Validación**: Sesiones con PYMEs constructoras reales

---

**Documento elaborado**: Enero 2026  
**Versión**: 1.0  
**Autor**: Jorge Ros Gómez
