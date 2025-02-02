using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using EventoCalendario.Models;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace EventoCalendario.MVC.Controllers
{
    public class NotificacionController : Controller
    {
        private readonly HttpClient _httpClient;

        public NotificacionController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("EventoCalendarioAPI");
        }

        // GET: Notificaciones
        public async Task<IActionResult> Index()
        {
            var response = await _httpClient.GetAsync("Notificacion");
            if (!response.IsSuccessStatusCode)
            {
                return View(new List<Notificacion>());
            }

            var jsonData = await response.Content.ReadAsStringAsync();
            var notificaciones = JsonConvert.DeserializeObject<List<Notificacion>>(jsonData);

            // Obtener usuarios y eventos
            var usuariosResponse = await _httpClient.GetAsync("Usuario");
            var eventosResponse = await _httpClient.GetAsync("Evento");

            var usuarios = new List<Usuario>();
            var eventos = new List<Evento>();

            if (usuariosResponse.IsSuccessStatusCode)
            {
                var usuariosJson = await usuariosResponse.Content.ReadAsStringAsync();
                usuarios = JsonConvert.DeserializeObject<List<Usuario>>(usuariosJson);
            }

            if (eventosResponse.IsSuccessStatusCode)
            {
                var eventosJson = await eventosResponse.Content.ReadAsStringAsync();
                eventos = JsonConvert.DeserializeObject<List<Evento>>(eventosJson);
            }

            ViewData["Usuarios"] = usuarios;
            ViewData["Eventos"] = eventos;

            return View(notificaciones);
        }

        // POST: Enviar Notificación
        public async Task<IActionResult> EnviarNotificacion(int usuarioId, int eventoId, string contenido)
        {
            if (usuarioId == 0 || eventoId == 0 || string.IsNullOrEmpty(contenido))
            {
                TempData["ErrorMessage"] = "Debe seleccionar un usuario, un evento y escribir un mensaje.";
                return RedirectToAction(nameof(Index));
            }

            var notificacion = new
            {
                UsuarioId = usuarioId,
                EventoId = eventoId,
                Contenido = contenido,
                FechaEnvio = DateTime.Now
            };

            var jsonContent = new StringContent(JsonConvert.SerializeObject(notificacion), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("Notificacion", jsonContent);

            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                TempData["ErrorMessage"] = $"No se pudo enviar la notificación. Error: {errorMessage}";
                return RedirectToAction(nameof(Index));
            }

            TempData["SuccessMessage"] = "Notificación enviada exitosamente.";
            return RedirectToAction(nameof(Index));
        }


        // GET: Ver Notificaciones de un usuario
        public async Task<IActionResult> VerNotificaciones(int usuarioId)
        {
            var response = await _httpClient.GetAsync($"Notificacion/Usuario/{usuarioId}");
            if (!response.IsSuccessStatusCode)
            {
                return NotFound("Usuario no encontrado o sin notificaciones.");
            }

            var jsonData = await response.Content.ReadAsStringAsync();
            var notificaciones = JsonConvert.DeserializeObject<List<Notificacion>>(jsonData);

            var usuarioResponse = await _httpClient.GetAsync($"Usuario/{usuarioId}");
            if (!usuarioResponse.IsSuccessStatusCode)
            {
                return NotFound("Usuario no encontrado.");
            }

            var usuarioJson = await usuarioResponse.Content.ReadAsStringAsync();
            var usuario = JsonConvert.DeserializeObject<Usuario>(usuarioJson);

            ViewBag.Usuario = usuario;
            return View(notificaciones);
        }
    }
}
