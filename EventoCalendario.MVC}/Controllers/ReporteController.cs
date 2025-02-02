using EventoCalendario.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

public class ReporteController : Controller
{
    private readonly HttpClient _httpClient;
    private readonly string _apiBaseUrl = "https://localhost:7129/api/reporte"; // Dirección base de la API de reportes
    private readonly string _apiEventoUrl = "https://localhost:7129/api/evento"; // Dirección base de la API de eventos

    public ReporteController(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    // ✅ Obtener todos los reportes y eventos desde la API
    public async Task<IActionResult> Index()
    {
        var reportes = new List<Reporte>();
        var eventos = new List<Evento>();

        // Obtener los reportes
        var reporteResponse = await _httpClient.GetAsync(_apiBaseUrl);
        if (reporteResponse.IsSuccessStatusCode)
        {
            var jsonData = await reporteResponse.Content.ReadAsStringAsync();
            reportes = JsonConvert.DeserializeObject<List<Reporte>>(jsonData);
        }
        else
        {
            TempData["ErrorMessage"] = "Error al cargar los reportes.";
        }

        // Obtener los eventos disponibles
        var eventoResponse = await _httpClient.GetAsync(_apiEventoUrl);
        if (eventoResponse.IsSuccessStatusCode)
        {
            var jsonEventos = await eventoResponse.Content.ReadAsStringAsync();
            eventos = JsonConvert.DeserializeObject<List<Evento>>(jsonEventos);
        }
        else
        {
            TempData["ErrorMessage"] = "Error al cargar los eventos.";
        }

        ViewData["Eventos"] = eventos;
        return View(reportes);
    }

    // ✅ Generar un nuevo reporte mediante la API
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GenerarReporte(int eventoId, string tipo, string contenido)
    {
        if (eventoId == 0 || string.IsNullOrEmpty(tipo) || string.IsNullOrEmpty(contenido))
        {
            TempData["ErrorMessage"] = "Debe seleccionar un evento, tipo y contenido.";
            return RedirectToAction(nameof(Index));
        }

        var reporte = new
        {
            EventoId = eventoId,
            Tipo = tipo,
            Contenido = contenido,
            UsuarioId = 1
        };

        var jsonContent = new StringContent(JsonConvert.SerializeObject(reporte), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync($"{_apiBaseUrl}/generar", jsonContent);

        if (response.IsSuccessStatusCode)
        {
            TempData["SuccessMessage"] = "Reporte generado exitosamente.";
        }
        else
        {
            TempData["ErrorMessage"] = "No se pudo generar el reporte.";
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> VerDetallesReporte(int id)
    {
        var apiUrl = $"{_apiBaseUrl}/{id}";  // URL completa de la solicitud

        var response = await _httpClient.GetAsync(apiUrl);
        if (!response.IsSuccessStatusCode)
        {
            TempData["ErrorMessage"] = "No se encontró el reporte.";
            return RedirectToAction(nameof(Index));
        }

        var jsonData = await response.Content.ReadAsStringAsync();
        dynamic reporteData = JsonConvert.DeserializeObject<dynamic>(jsonData);

        // Asegúrate de que los datos estén correctamente deserializados
        ViewBag.ReporteId = reporteData.ReporteId;
        ViewBag.Evento = (string)reporteData.Evento;

        // Verificar si el valor de FechaGeneracion es nulo
        if (reporteData.FechaGeneracion != null)
        {
            ViewBag.FechaGeneracion = ((DateTime)reporteData.FechaGeneracion).ToString("dd/MM/yyyy HH:mm"); // Formato de fecha
        }
        else
        {
            ViewBag.FechaGeneracion = "Fecha no disponible"; // Asigna un valor predeterminado si es null
        }

        ViewBag.NumeroDeAsistentes = reporteData.NumeroDeAsistentes ?? 0; // Si es null, asigna 0
        ViewBag.TotalPersonasEnEvento = reporteData.TotalPersonasEnEvento ?? 0; // Si es null, asigna 0
        ViewBag.Asistentes = reporteData.Asistentes != null ? reporteData.Asistentes.ToObject<List<string>>() : new List<string>(); // Asignar lista vacía si es null
        ViewBag.Tipo = (string)reporteData.Tipo;
        ViewBag.Contenido = (string)reporteData.Contenido;

        return View("VerDetallesReporte");
    }





}
