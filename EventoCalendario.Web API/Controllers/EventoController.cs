using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventoCalendario.Models;
using Newtonsoft.Json;

namespace EventoCalendario.Web_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventoController : ControllerBase
    {
        private readonly EventoCalendarioDbContext _context;

        public EventoController(EventoCalendarioDbContext context)
        {
            _context = context;
        }

        // GET: api/Evento
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Evento>>> GetEventos()
        {
            return await _context.Evento
                .Where(e => !e.EsEliminado) // Filtrar eventos activos
                .ToListAsync();
        }

        // GET: api/Evento/5
        // GET: api/Evento/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Evento>> GetEvento(int id)
        {
            var evento = await _context.Evento
                .Include(e => e.Asistencias) // Asegúrate de incluir las asistencias también, si las necesitas
                .ThenInclude(a => a.Usuario)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (evento == null || evento.EsEliminado)
            {
                return NotFound("Evento no encontrado o eliminado.");
            }

            return evento;
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEvento(int id, [FromBody] Evento evento)
        {
            if (evento == null || id != evento.Id)
            {
                return BadRequest("El ID del evento no coincide o el cuerpo de la solicitud está vacío.");
            }

            Console.WriteLine($"Evento recibido: {JsonConvert.SerializeObject(evento)}");

            if (evento.Fecha < DateTime.Now)
            {
                return BadRequest("No puedes asignar una fecha pasada a un evento.");
            }

            var eventoExistente = await _context.Evento.FindAsync(id);
            if (eventoExistente == null)
            {
                return NotFound("Evento no encontrado.");
            }

            // Actualiza solo los campos permitidos
            eventoExistente.Nombre = evento.Nombre;
            eventoExistente.Fecha = evento.Fecha;
            eventoExistente.Ubicacion = evento.Ubicacion;
            eventoExistente.Estado = evento.Estado;
            eventoExistente.Tipo = evento.Tipo;
            eventoExistente.UsuarioId = evento.UsuarioId;

            try
            {
                await _context.SaveChangesAsync();
                Console.WriteLine("Evento actualizado correctamente.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al actualizar el evento: {ex.Message}");
                return StatusCode(500, "Error interno del servidor.");
            }

            return NoContent();
        }



        // ✅ GET: api/Evento/{id}/asistencias
        // ✅ GET: api/Evento/{id}/asistencias
        [HttpGet("{id}/asistencias")]
        public async Task<ActionResult<IEnumerable<Asistencia>>> GetAsistenciasDelEvento(int id)
        {
            var evento = await _context.Evento
                .Include(e => e.Asistencias)
                .ThenInclude(a => a.Usuario)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (evento == null)
            {
                return NotFound("Evento no encontrado.");
            }

            // 🔹 Conversión explícita de ICollection a List para evitar el error
            return Ok(evento.Asistencias.ToList());
        }



        // POST: api/Evento
        [HttpPost]
        public async Task<ActionResult<Evento>> PostEvento(Evento evento)
        {
            if (evento.Fecha < DateTime.Now)
            {
                return BadRequest("No se pueden crear eventos en fechas pasadas.");
            }

            var usuarioExistente = await _context.Usuario.FindAsync(evento.UsuarioId);
            if (usuarioExistente == null)
            {
                return BadRequest("El usuario especificado no existe.");
            }

            evento.Creador = usuarioExistente;
            evento.EsEliminado = false; // Asegurar que el evento inicia activo

            _context.Evento.Add(evento);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetEvento", new { id = evento.Id }, evento);
        }

        // DELETE: api/Evento/5 (Soft Delete)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvento(int id)
        {
            var evento = await _context.Evento.FindAsync(id);
            if (evento == null)
            {
                return NotFound("Evento no encontrado.");
            }

            evento.EsEliminado = true; // 🔹 Marcar como eliminado en lugar de borrarlo físicamente
            _context.Entry(evento).State = EntityState.Modified; // 🔹 Importante para forzar la actualización

            try
            {
                await _context.SaveChangesAsync();
                Console.WriteLine($"Evento {id} marcado como eliminado.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al eliminar el evento: {ex.Message}");
                return StatusCode(500, "Error interno del servidor.");
            }

            return NoContent();
        }

        private bool EventoExists(int id)
        {
            return _context.Evento.Any(e => e.Id == id);
        }
    }
}
