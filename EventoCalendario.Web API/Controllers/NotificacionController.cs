using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventoCalendario.Models;

namespace EventoCalendario.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificacionController : ControllerBase
    {
        private readonly EventoCalendarioDbContext _context;

        public NotificacionController(EventoCalendarioDbContext context)
        {
            _context = context;
        }

        // GET: api/Notificacion
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Notificacion>>> GetNotificaciones()
        {
            return await _context.Notificacion
                .Include(n => n.Usuario) // Incluir usuario
                .Include(n => n.Evento)  // Incluir evento
                .ToListAsync();
        }

        // GET: api/Notificacion/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Notificacion>> GetNotificacion(int id)
        {
            var notificacion = await _context.Notificacion
                .Include(n => n.Usuario)
                .Include(n => n.Evento)
                .FirstOrDefaultAsync(n => n.Id == id);

            if (notificacion == null)
            {
                return NotFound();
            }
            return notificacion;
        }

        // ✅ Nuevo Endpoint: Obtener notificaciones por usuario
        [HttpGet("Usuario/{usuarioId}")]
        public async Task<ActionResult<IEnumerable<Notificacion>>> GetNotificacionesPorUsuario(int usuarioId)
        {
            var notificaciones = await _context.Notificacion
                .Include(n => n.Evento)
                .Where(n => n.UsuarioId == usuarioId)
                .ToListAsync();

            if (!notificaciones.Any())
            {
                return NotFound($"No hay notificaciones para el usuario con ID {usuarioId}");
            }

            return notificaciones;
        }

        // POST: api/Notificacion
        [HttpPost]
        public async Task<ActionResult<Notificacion>> PostNotificacion([FromBody] Notificacion notificacion)
        {
            if (notificacion == null)
            {
                return BadRequest("La notificación no puede ser nula.");
            }

            if (notificacion.UsuarioId == 0 || notificacion.EventoId == 0 || string.IsNullOrEmpty(notificacion.Contenido))
            {
                return BadRequest(new { Message = "UsuarioId, EventoId y Contenido son obligatorios." });
            }

            // Validar si el usuario y el evento existen en la base de datos
            var usuarioExiste = await _context.Usuario.AnyAsync(u => u.Id == notificacion.UsuarioId);
            var eventoExiste = await _context.Evento.AnyAsync(e => e.Id == notificacion.EventoId);

            if (!usuarioExiste || !eventoExiste)
            {
                return BadRequest(new { Message = "El usuario o el evento no existen en la base de datos." });
            }

            // Asegurar que Usuario y Evento no sean requeridos en la inserción
            notificacion.Usuario = null;
            notificacion.Evento = null;

            _context.Notificacion.Add(notificacion);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetNotificacion), new { id = notificacion.Id }, notificacion);
        }


        // PUT: api/Notificacion/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutNotificacion(int id, Notificacion notificacion)
        {
            if (id != notificacion.Id)
            {
                return BadRequest("El ID de la notificación no coincide.");
            }

            // Validar existencia de usuario y evento
            var usuarioExiste = await _context.Usuario.AnyAsync(u => u.Id == notificacion.UsuarioId);
            var eventoExiste = await _context.Evento.AnyAsync(e => e.Id == notificacion.EventoId);

            if (!usuarioExiste || !eventoExiste)
            {
                return BadRequest("Usuario o Evento no válido.");
            }

            _context.Entry(notificacion).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Notificacion.Any(e => e.Id == id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/Notificacion/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNotificacion(int id)
        {
            var notificacion = await _context.Notificacion.FindAsync(id);
            if (notificacion == null)
            {
                return NotFound();
            }

            _context.Notificacion.Remove(notificacion);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
