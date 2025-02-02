using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using EventoCalendario.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EventoCalendario.WebApp.Controllers
{
    public class EventoController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl;

        public EventoController(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiBaseUrl = configuration["ApiSettings:BaseUrl"] + "evento";
        }

        // GET: Evento (Listado de eventos)
        public async Task<IActionResult> Index()
        {
            var response = await _httpClient.GetAsync(_apiBaseUrl);
            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Error al obtener la lista de eventos.";
                return View(new List<Evento>());
            }

            var eventos = JsonConvert.DeserializeObject<List<Evento>>(await response.Content.ReadAsStringAsync());
            return View(eventos);
        }

        //  GET: Evento/Create
        // ✅ GET: Evento/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var response = await _httpClient.GetAsync($"{_apiBaseUrl}/{id}");
            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Evento no encontrado.";
                return RedirectToAction(nameof(Index));
            }

            var evento = JsonConvert.DeserializeObject<Evento>(await response.Content.ReadAsStringAsync());

            // 🔹 Obtener asistencias confirmadas del evento
            var asistenciasResponse = await _httpClient.GetAsync($"{_apiBaseUrl}/{id}/asistencias");
            List<Usuario> usuariosConfirmados = new List<Usuario>();

            if (asistenciasResponse.IsSuccessStatusCode)
            {
                var jsonAsistencias = await asistenciasResponse.Content.ReadAsStringAsync();
                var asistencias = JsonConvert.DeserializeObject<List<Asistencia>>(jsonAsistencias);

                // Filtrar solo asistentes confirmados
                usuariosConfirmados = asistencias
                    .Where(a => a.Estado == "Aceptada")
                    .Select(a => a.Usuario)
                    .ToList();
            }

            ViewData["UsuariosConfirmados"] = usuariosConfirmados;
            return View(evento);
        }



        // POST: Evento/Create
        public async Task<IActionResult> Create()
        {
            await CargarUsuariosEnViewBag(); // 🔹 Carga usuarios antes de mostrar la vista
            return View();
        }

        // 📌 POST: Evento/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Evento evento)
        {
            if (evento.Fecha < DateTime.Today)
            {
                ModelState.AddModelError("Fecha", "La fecha del evento debe ser hoy o en el futuro.");
            }

            if (!ModelState.IsValid)
            {
                await CargarUsuariosEnViewBag();
                return View(evento);
            }

            // 🔹 Asegurar que el JSON incluye los campos correctos
            var jsonPayload = JsonConvert.SerializeObject(new
            {
                evento.Nombre,
                evento.Descripcion,
                Fecha = evento.Fecha.ToString("yyyy-MM-dd"), // 🔹 Formato correcto para la API
                evento.Ubicacion,
                evento.Estado,
                evento.Tipo,
                evento.UsuarioId
            });

            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            // 🔹 Imprimir JSON en consola para depuración
            Console.WriteLine($"Enviando JSON al API: {jsonPayload}");

            var response = await _httpClient.PostAsync(_apiBaseUrl, content);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Error en la API al crear: {responseBody}");
                TempData["ErrorMessage"] = $"Error al crear el evento: {responseBody}";
                await CargarUsuariosEnViewBag();
                return View(evento);
            }

            TempData["SuccessMessage"] = "Evento creado exitosamente.";
            return RedirectToAction(nameof(Index));
        }


        // 📌 GET: Evento/Edit/5


        // 📌 Método auxiliar para cargar usuarios en ViewBag
        private async Task CargarUsuariosEnViewBag()
        {
            var response = await _httpClient.GetAsync(_apiBaseUrl.Replace("evento", "usuario")); // Obtener usuarios desde API

            if (response.IsSuccessStatusCode)
            {
                var usuarios = JsonConvert.DeserializeObject<List<Usuario>>(await response.Content.ReadAsStringAsync());
                ViewBag.UsuarioId = new SelectList(usuarios, "Id", "Nombre"); // Pasar lista a la vista
            }
            else
            {
                ViewBag.UsuarioId = new SelectList(new List<Usuario>(), "Id", "Nombre"); // Evitar error si falla la API
                TempData["ErrorMessage"] = "Error al obtener la lista de usuarios.";
            }
        }
        // GET: Evento/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var response = await _httpClient.GetAsync($"{_apiBaseUrl}/{id}");
            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Evento no encontrado.";
                return RedirectToAction(nameof(Index));
            }

            var evento = JsonConvert.DeserializeObject<Evento>(await response.Content.ReadAsStringAsync());
            await CargarUsuariosEnViewBag(); // Cargar usuarios antes de mostrar la vista
            return View(evento);
        }

        // POST: Evento/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Evento evento)
        {
            if (id != evento.Id)
            {
                return BadRequest("El ID del evento no coincide.");
            }

            if (evento.Fecha < DateTime.Today)
            {
                ModelState.AddModelError("Fecha", "No puedes asignar una fecha pasada.");
            }

            if (!ModelState.IsValid)
            {
                await CargarUsuariosEnViewBag();
                return View(evento);
            }

            // Serializar el objeto evento incluyendo el ID
            var jsonPayload = JsonConvert.SerializeObject(new
            {
                evento.Id,
                evento.Nombre,
                evento.Descripcion,
                evento.Fecha,
                evento.Ubicacion,
                evento.Estado,
                evento.Tipo,
                evento.UsuarioId
            });

            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            // Imprimir en consola la URL y el JSON enviado
            Console.WriteLine($"PUT URL: {_apiBaseUrl}/{id}");
            Console.WriteLine($"Payload: {jsonPayload}");

            var response = await _httpClient.PutAsync($"{_apiBaseUrl}/{id}", content);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Error en la respuesta del servidor: {responseBody}");
                TempData["ErrorMessage"] = $"Error al actualizar el evento: {responseBody}";
                return View(evento);
            }

            TempData["SuccessMessage"] = "Evento actualizado exitosamente.";

            // 🔹 Refrescar la lista de eventos al volver al Index
            return RedirectToAction(nameof(Index));
        }


        // 📌 GET: Evento/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _httpClient.GetAsync($"{_apiBaseUrl}/{id}");
            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Evento no encontrado.";
                return RedirectToAction(nameof(Index));
            }

            var evento = JsonConvert.DeserializeObject<Evento>(await response.Content.ReadAsStringAsync());
            return View(evento);
        }

        // 📌 POST: Evento/Delete/5 (Soft Delete)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var response = await _httpClient.DeleteAsync($"{_apiBaseUrl}/{id}");
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Error en la API al eliminar: {responseBody}");
                TempData["ErrorMessage"] = $"Error al eliminar el evento: {responseBody}";
                return RedirectToAction(nameof(Index));
            }

            Console.WriteLine($"Evento {id} eliminado correctamente en la API.");
            TempData["SuccessMessage"] = "Evento eliminado exitosamente.";

            return RedirectToAction(nameof(Index));
        }


    }
}
