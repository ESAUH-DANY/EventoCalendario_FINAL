using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventoCalendario.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net.Http;

namespace EventoCalendario.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReporteController : ControllerBase
    {
        private readonly EventoCalendarioDbContext _context;

        public ReporteController(EventoCalendarioDbContext context)
        {
            _context = context;
        }

        // ✅ Obtener todos los reportes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Reporte>>> GetReportes()
        {
            var reportes = await _context.Reporte
                .Include(r => r.Evento)
                .Include(r => r.Usuario)
                .ToListAsync();

            return Ok(reportes);
        }

        // ✅ Obtener un reporte por ID con asistencias incluidas
        [HttpGet("{id}")]
        public async Task<ActionResult<Reporte>> GetReporte(int id)
        {
            var reporte = await _context.Reporte
                .Include(r => r.Evento)
                    .ThenInclude(e => e.Asistencias)
                        .ThenInclude(a => a.Usuario)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reporte == null)
            {
                return NotFound(new { mensaje = "Reporte no encontrado." });
            }

            return Ok(reporte);
        }

        // ✅ Generar un nuevo reporte
        [HttpPost("Generar")]
        public async Task<ActionResult<Reporte>> GenerarReporte([FromBody] Reporte reporteRequest)
        {
            // Validar que los datos necesarios estén presentes
            if (reporteRequest.EventoId == 0 || string.IsNullOrEmpty(reporteRequest.Tipo) || string.IsNullOrEmpty(reporteRequest.Contenido))
            {
                return BadRequest(new { mensaje = "Debe proporcionar EventoId, Tipo y Contenido." });
            }

            // Buscar el evento y validar que exista
            var evento = await _context.Evento
                .Include(e => e.Asistencias)
                .FirstOrDefaultAsync(e => e.Id == reporteRequest.EventoId);

            if (evento == null)
            {
                return NotFound(new { mensaje = "Evento no encontrado." });
            }

            // Crear el reporte
            var reporte = new Reporte
            {
                Tipo = reporteRequest.Tipo,
                FechaGeneracion = DateTime.UtcNow, // Establecer la fecha de generación del reporte
                EventoId = reporteRequest.EventoId,
                Evento = evento,
                Contenido = reporteRequest.Contenido,
                UsuarioId = reporteRequest.UsuarioId
            };

            // Agregar el reporte a la base de datos
            _context.Reporte.Add(reporte);
            await _context.SaveChangesAsync();

            // Retornar la respuesta de éxito
            return CreatedAtAction(nameof(GetReporte), new { id = reporte.Id }, reporte);
        }

        // ✅ Obtener detalles de un reporte con los asistentes
        [HttpGet("Detalles/{id}")]
        public async Task<ActionResult<object>> VerDetallesReporte(int id)
        {
            var reporte = await _context.Reporte
                .Include(r => r.Evento)
                    .ThenInclude(e => e.Asistencias)
                        .ThenInclude(a => a.Usuario)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reporte == null)
            {
                return NotFound(new { mensaje = "El reporte no fue encontrado." });
            }

            // Filtramos los asistentes confirmados
            var asistentesConfirmados = reporte.Evento.Asistencias
                .Where(a => a.Estado == "Aceptada" && a.Usuario != null)
                .Select(a => a.Usuario.Nombre)
                .ToList();

            // Aseguramos que el número de asistentes no sea nulo, asignando un valor predeterminado (0) si es necesario
            int numeroDeAsistentes = asistentesConfirmados.Count; // Este valor no debería ser null
            int totalPersonasEnEvento = reporte.Evento.Asistencias.Count();

            return Ok(new
            {
                ReporteId = reporte.Id,
                Evento = reporte.Evento.Nombre,
                NumeroDeAsistentes = numeroDeAsistentes, // Usamos el valor calculado
                TotalPersonasEnEvento = totalPersonasEnEvento, // Usamos el valor calculado
                Asistentes = asistentesConfirmados,
                FechaGeneracion = reporte.FechaGeneracion,
                Tipo = reporte.Tipo,
                Contenido = reporte.Contenido
            });
        }



    }
}
