using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventoCalendario.Models;

namespace EventoCalendario.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AsistenciaController : ControllerBase
    {
        private readonly EventoCalendarioDbContext _context;

        public AsistenciaController(EventoCalendarioDbContext context)
        {
            _context = context;
        }

        // 📌 GET: api/Asistencia - Obtener todas las asistencias
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Asistencia>>> GetAsistencias()
        {
            var asistencias = await _context.Asistencia
                .Include(a => a.Usuario)  // 🔹 Incluir Usuario
                .Include(a => a.Evento)   // 🔹 Incluir Evento
                .ToListAsync();

            if (!asistencias.Any())
            {
                return NotFound(new { message = "No hay asistencias registradas." });
            }

            // 🔹 Verifica si los datos se están obteniendo correctamente
            Console.WriteLine("Asistencias obtenidas:");
            foreach (var asistencia in asistencias)
            {
                Console.WriteLine($"Asistencia ID: {asistencia.Id}, Usuario: {asistencia.Usuario?.Nombre}, Evento: {asistencia.Evento?.Nombre}");
            }

            return Ok(asistencias);
        }



        // 📌 GET: api/Asistencia/{id} - Obtener asistencia por ID
        [HttpGet("{id}")]
        public async Task<ActionResult<Asistencia>> GetAsistencia(int id)
        {
            var asistencia = await _context.Asistencia
                .Include(a => a.Evento)
                .Include(a => a.Usuario)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (asistencia == null)
            {
                return NotFound(new { error = "La asistencia no fue encontrada." });
            }

            return asistencia;
        }

        // 📌 POST: api/Asistencia - Registrar asistencia (Validado)
        [HttpPost]
        public async Task<ActionResult<Asistencia>> PostAsistencia(Asistencia asistencia)
        {
            if (asistencia.UsuarioId == 0 || asistencia.EventoId == 0)
            {
                return BadRequest(new { error = "UsuarioId y EventoId son obligatorios." });
            }

            try
            {
                // Validar si el usuario y el evento existen en la base de datos
                var usuario = await _context.Usuario.FindAsync(asistencia.UsuarioId);
                if (usuario == null)
                {
                    return BadRequest(new { error = "El usuario no existe." });
                }

                var evento = await _context.Evento.FindAsync(asistencia.EventoId);
                if (evento == null)
                {
                    return BadRequest(new { error = "El evento no existe." });
                }

                // Verificar si la asistencia ya existe
                var existeAsistencia = await _context.Asistencia
                    .AnyAsync(a => a.EventoId == asistencia.EventoId && a.UsuarioId == asistencia.UsuarioId);

                if (existeAsistencia)
                {
                    return Conflict(new { error = "El usuario ya está registrado en este evento." });
                }

                asistencia.Usuario = null; // Evita que se envíe un objeto completo
                asistencia.Evento = null;  // Evita que se envíe un objeto completo
                asistencia.Estado = "Pendiente"; // Estado inicial

                _context.Asistencia.Add(asistencia);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetAsistencia), new { id = asistencia.Id }, asistencia);
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error al registrar asistencia: {ex.Message}");
                return StatusCode(500, new { error = "Ocurrió un error al registrar la asistencia." });
            }
        }
        // En tu API, asegúrate de tener el siguiente endpoint para verificar la existencia de la invitación
        [HttpGet("usuario/{usuarioId}/evento/{eventoId}")]
        public async Task<IActionResult> CheckAsistencia(int usuarioId, int eventoId)
        {
            var asistencia = await _context.Asistencia
                .FirstOrDefaultAsync(a => a.UsuarioId == usuarioId && a.EventoId == eventoId);
            if (asistencia != null)
            {
                return Ok(asistencia); // Ya existe, se devuelve la asistencia
            }

            return NotFound(); // No existe
        }


        // 📌 PUT: api/Asistencia/{id} - Actualizar asistencia (Validado)
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAsistencia(int id, Asistencia asistencia)
        {
            if (id != asistencia.Id)
            {
                return BadRequest(new { error = "El ID de la asistencia no coincide." });
            }

            // 🔹 Validar si el usuario y el evento existen
            var usuario = await _context.Usuario.FindAsync(asistencia.UsuarioId);
            if (usuario == null)
            {
                return BadRequest(new { error = "El usuario no existe." });
            }

            var evento = await _context.Evento.FindAsync(asistencia.EventoId);
            if (evento == null)
            {
                return BadRequest(new { error = "El evento no existe." });
            }

            _context.Entry(asistencia).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Asistencia.AnyAsync(e => e.Id == id))
                {
                    return NotFound(new { error = "La asistencia no fue encontrada." });
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // 📌 GET: api/Asistencia/usuario/{usuarioId} - Obtener asistencias de un usuario
        [HttpGet("usuario/{usuarioId}")]
        public async Task<ActionResult<IEnumerable<Asistencia>>> GetAsistenciasPorUsuario(int usuarioId)
        {
            var asistencias = await _context.Asistencia
                .Where(a => a.UsuarioId == usuarioId)
                .Include(a => a.Evento)
                .ToListAsync();

            if (!asistencias.Any())
            {
                return NotFound(new { error = "No hay asistencias registradas para este usuario." });
            }

            return asistencias;
        }

        // 📌 DELETE: api/Asistencia/{id} - Cancelar asistencia en lugar de eliminarla (Soft Delete)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsistencia(int id)
        {
            var asistencia = await _context.Asistencia.FindAsync(id);
            if (asistencia == null)
            {
                return NotFound(new { error = "La asistencia no fue encontrada." });
            }

            asistencia.Estado = "Cancelada"; // 🔹 Cambia el estado en lugar de eliminarlo físicamente
            _context.Entry(asistencia).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
