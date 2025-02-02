using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventoCalendario.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventoCalendario.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CalendarioController : ControllerBase
    {
        private readonly EventoCalendarioDbContext _context;

        public CalendarioController(EventoCalendarioDbContext context)
        {
            _context = context;
        }

        // Obtener el único calendario con sus eventos
        [HttpGet]
        public async Task<ActionResult<Calendario>> GetCalendario()
        {
            var calendario = await _context.Calendario
                .Include(c => c.Eventos) // Incluir los eventos relacionados
                .FirstOrDefaultAsync(c => c.Id == 1); // ID fijo para el único calendario

            if (calendario == null)
            {
                return NotFound("No se encontró el calendario.");
            }

            return Ok(calendario);
        }
        [HttpGet("eventos")]
        public async Task<IActionResult> GetEventos()
        {
            try
            {
                var eventos = await _context.Evento
                    .Where(e => e.Fecha != null
                                && e.Fecha > DateTime.MinValue
                                && !e.EsEliminado) // 🔹 Excluir eventos eliminados
                    .ToListAsync();

                if (!eventos.Any())
                {
                    Console.WriteLine("📌 No hay eventos válidos en la base de datos.");
                    return Ok(new List<object>());
                }

                // 🔹 Imprimir eventos en consola para depuración
                foreach (var evento in eventos)
                {
                    Console.WriteLine($"📌 Evento: {evento.Nombre}, Fecha: {evento.Fecha}, Eliminado: {evento.EsEliminado}");
                }

                return Ok(eventos);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERROR en API: {ex.Message}");
                return StatusCode(500, "Error al obtener los eventos.");
            }
        }

        // Obtener lista de usuarios
        [HttpGet("usuarios")]
        public async Task<IActionResult> GetUsuarios()
        {
            try
            {
                var usuarios = await _context.Usuario
                    .Select(u => new
                    {
                        u.Id,
                        u.Nombre
                    })
                    .ToListAsync();

                if (!usuarios.Any())
                {
                    Console.WriteLine("📌 No hay usuarios registrados en la base de datos.");
                    return Ok(new List<object>());
                }

                // 🔹 Depuración en consola
                Console.WriteLine($"📌 Usuarios obtenidos: {usuarios.Count}");
                foreach (var usuario in usuarios)
                {
                    Console.WriteLine($"ID: {usuario.Id}, Nombre: {usuario.Nombre}");
                }

                return Ok(usuarios);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERROR en API al obtener usuarios: {ex.Message}");
                return StatusCode(500, "Error al obtener los usuarios.");
            }
        }



        // Obtener eventos del calendario por fecha
        [HttpGet("eventos/porFecha")]
        public async Task<ActionResult<IEnumerable<Evento>>> GetEventosPorFecha([FromQuery] DateTime fecha)
        {
            var calendario = await _context.Calendario
                .Include(c => c.Eventos)
                .FirstOrDefaultAsync(c => c.Id == 1);

            if (calendario == null)
            {
                return NotFound("No se encontró el calendario.");
            }

            var eventos = calendario.Eventos
                .Where(e => e.Fecha.Date == fecha.Date)
                .ToList();

            return Ok(eventos);
        }

        // Crear un nuevo evento en el calendario
        [HttpPost("eventos")]
        public async Task<ActionResult<Evento>> PostEvento([FromBody] Evento evento)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var calendario = await _context.Calendario
                .Include(c => c.Eventos)
                .FirstOrDefaultAsync(c => c.Id == 1);

            if (calendario == null)
            {
                return NotFound("No se encontró el calendario.");
            }

            // Asignar el evento al calendario
            calendario.Eventos.Add(evento);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetEventosPorFecha", new { fecha = evento.Fecha }, evento);
        }

        // Actualizar un evento existente
        [HttpPut("eventos/{id}")]
        public async Task<IActionResult> PutEvento(int id, [FromBody] Evento evento)
        {
            if (id != evento.Id || !ModelState.IsValid)
            {
                return BadRequest();
            }

            var calendario = await _context.Calendario
                .Include(c => c.Eventos)
                .FirstOrDefaultAsync(c => c.Id == 1);

            if (calendario == null)
            {
                return NotFound("No se encontró el calendario.");
            }

            var eventoExistente = calendario.Eventos.FirstOrDefault(e => e.Id == id);
            if (eventoExistente == null)
            {
                return NotFound("No se encontró el evento.");
            }

            // Actualizar el evento
            eventoExistente.Nombre = evento.Nombre;
            eventoExistente.Fecha = evento.Fecha;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Eliminar un evento
        [HttpDelete("eventos/{id}")]
        public async Task<IActionResult> DeleteEvento(int id)
        {
            var calendario = await _context.Calendario
                .Include(c => c.Eventos)
                .FirstOrDefaultAsync(c => c.Id == 1);

            if (calendario == null)
            {
                return NotFound("No se encontró el calendario.");
            }

            var evento = calendario.Eventos.FirstOrDefault(e => e.Id == id);
            if (evento == null)
            {
                return NotFound("No se encontró el evento.");
            }

            calendario.Eventos.Remove(evento);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}