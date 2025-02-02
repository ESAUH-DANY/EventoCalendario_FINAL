using EventoCalendario.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text.Json;
using System.Text;

public class AsistenciaController : Controller
{
    private readonly HttpClient _httpClient;

    public AsistenciaController(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("EventoCalendarioAPI");
    }
    private async Task<List<Usuario>> ObtenerUsuarios()
    {
        var response = await _httpClient.GetAsync("/api/usuario");
        if (response.IsSuccessStatusCode)
        {
            var jsonData = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<Usuario>>(jsonData) ?? new List<Usuario>();
        }
        else
        {
            // Manejo de error si no se pueden cargar los usuarios
            TempData["ErrorMessage"] = "No se pudieron cargar los usuarios.";
            return new List<Usuario>();
        }
    }

    // 📌 Método para obtener eventos desde la API
    private async Task<List<Evento>> ObtenerEventos()
    {
        var response = await _httpClient.GetAsync("/api/evento");
        if (response.IsSuccessStatusCode)
        {
            var jsonData = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<Evento>>(jsonData) ?? new List<Evento>();
        }
        else
        {
            // Manejo de error si no se pueden cargar los eventos
            TempData["ErrorMessage"] = "No se pudieron cargar los eventos.";
            return new List<Evento>();
        }
    }
    // 📌 GET: Cargar Asistencias
    public async Task<IActionResult> Index()
    {
        ViewData["Usuarios"] = await ObtenerUsuarios() ?? new List<Usuario>();
        ViewData["Eventos"] = await ObtenerEventos() ?? new List<Evento>();

        var responseAsistencias = await _httpClient.GetAsync("/api/asistencia");
        List<Asistencia> asistencias = new List<Asistencia>();

        if (responseAsistencias.IsSuccessStatusCode)
        {
            var jsonData = await responseAsistencias.Content.ReadAsStringAsync();

            // 🔹 Imprimir en la consola para verificar si los datos son correctos
            Console.WriteLine("JSON recibido de la API:");
            Console.WriteLine(jsonData);

            asistencias = JsonConvert.DeserializeObject<List<Asistencia>>(jsonData) ?? new List<Asistencia>();
        }
        else
        {
            TempData["ErrorMessage"] = "No se pudieron cargar las asistencias.";
        }

        return View(asistencias);
    }


    // 📌 POST: Enviar Invitación
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EnviarInvitacion(int usuarioId, int eventoId)
    {
        if (usuarioId == 0 || eventoId == 0)
        {
            TempData["ErrorMessage"] = "Debe seleccionar un usuario y un evento.";
            return RedirectToAction(nameof(Index));
        }

        // Verificar si la asistencia ya existe en la API
        var responseCheck = await _httpClient.GetAsync($"/api/asistencia/usuario/{usuarioId}/evento/{eventoId}");
        if (responseCheck.IsSuccessStatusCode)
        {
            TempData["ErrorMessage"] = "El usuario ya tiene una invitación para este evento.";
            return RedirectToAction(nameof(Index));
        }

        // Crear la nueva asistencia para enviar la invitación
        var nuevaAsistencia = new
        {
            UsuarioId = usuarioId,
            EventoId = eventoId,
            Estado = "Pendiente"
        };

        var content = new StringContent(JsonConvert.SerializeObject(nuevaAsistencia), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("/api/asistencia", content);
        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            TempData["ErrorMessage"] = $"Error al enviar la invitación: {responseBody}";
        }
        else
        {
            TempData["SuccessMessage"] = "Invitación enviada exitosamente.";
        }

        // 🔹 Redirigir al Index para recargar completamente la página y mostrar los datos correctos
        return RedirectToAction(nameof(Index));
    }



    // 📌 POST: Actualizar Estado de la Asistencia
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ActualizarEstado(int id, string estado)
    {
        // Verificar que la asistencia existe
        var asistenciaExistenteResponse = await _httpClient.GetAsync($"/api/asistencia/{id}");
        if (!asistenciaExistenteResponse.IsSuccessStatusCode)
        {
            TempData["ErrorMessage"] = "Error al encontrar la asistencia.";
            return RedirectToAction(nameof(Index));
        }

        var asistenciaExistenteJson = await asistenciaExistenteResponse.Content.ReadAsStringAsync();
        var asistenciaExistente = JsonConvert.DeserializeObject<Asistencia>(asistenciaExistenteJson);

        var asistenciaActualizada = new
        {
            Id = id,
            EventoId = asistenciaExistente.EventoId,
            UsuarioId = asistenciaExistente.UsuarioId,
            Estado = estado
        };

        var content = new StringContent(JsonConvert.SerializeObject(asistenciaActualizada), Encoding.UTF8, "application/json");
        var response = await _httpClient.PutAsync($"/api/asistencia/{id}", content);

        if (!response.IsSuccessStatusCode)
        {
            TempData["ErrorMessage"] = "Error al actualizar el estado de la asistencia.";
        }
        else
        {
            TempData["SuccessMessage"] = "Estado actualizado exitosamente.";
        }

        return RedirectToAction(nameof(Index));
    }
    // 📌 GET: Obtener Asistencias Confirmadas
    public async Task<IActionResult> ObtenerAsistenciasConfirmadas()
    {
        var response = await _httpClient.GetAsync("/api/asistencia");
        if (!response.IsSuccessStatusCode)
        {
            TempData["ErrorMessage"] = "No se pudieron cargar las asistencias confirmadas.";
            return PartialView("_AsistenciasConfirmadas", new List<Asistencia>());
        }

        var jsonData = await response.Content.ReadAsStringAsync();
        var asistencias = JsonConvert.DeserializeObject<List<Asistencia>>(jsonData) ?? new List<Asistencia>();

        // 🔹 Filtrar solo las asistencias con estado "Aceptada"
        var asistenciasConfirmadas = asistencias.Where(a => a.Estado == "Aceptada").ToList();

        return PartialView("_AsistenciasConfirmadas", asistenciasConfirmadas);
    }

}
