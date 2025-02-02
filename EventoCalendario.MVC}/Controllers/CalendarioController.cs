using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using EventoCalendario.Models;

namespace EventoCalendario.WebApp.Controllers
{
    public class CalendarioController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl;

        public CalendarioController(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiBaseUrl = configuration["ApiSettings:BaseUrl"] + "calendario";
        }

        public async Task<IActionResult> Index(int? mes, int? anio, string tipo, string usuario)
        {
            try
            {
                var fechaActual = DateTime.Now;
                var mesMostrado = mes ?? fechaActual.Month;
                var anioMostrado = anio ?? fechaActual.Year;

                if (mesMostrado < 1) { mesMostrado = 12; anioMostrado--; }
                if (mesMostrado > 12) { mesMostrado = 1; anioMostrado++; }

                var eventosRequestUrl = $"{_apiBaseUrl}/eventos";
                var usuariosRequestUrl = $"{_apiBaseUrl}/usuarios";

                var eventosResponse = await _httpClient.GetAsync(eventosRequestUrl);
                var usuariosResponse = await _httpClient.GetAsync(usuariosRequestUrl);

                List<Evento> eventos = new List<Evento>();
                List<Usuario> usuarios = new List<Usuario>();

                // 🔹 Obtener eventos
                if (eventosResponse.IsSuccessStatusCode)
                {
                    var eventosData = await eventosResponse.Content.ReadAsStringAsync();
                    eventos = JsonConvert.DeserializeObject<List<Evento>>(eventosData) ?? new List<Evento>();
                }

                // 🔹 Obtener usuarios
                if (usuariosResponse.IsSuccessStatusCode)
                {
                    var usuariosData = await usuariosResponse.Content.ReadAsStringAsync();
                    usuarios = JsonConvert.DeserializeObject<List<Usuario>>(usuariosData) ?? new List<Usuario>();
                }

                // 🔹 Aplicar filtros en la WebApp
                if (!string.IsNullOrEmpty(tipo))
                {
                    eventos = eventos.Where(e => e.Tipo == tipo).ToList();
                }

                if (!string.IsNullOrEmpty(usuario))
                {
                    eventos = eventos.Where(e => e.UsuarioId.ToString() == usuario).ToList();
                }

                // 🔹 Guardar valores en ViewBag para la vista
                ViewBag.Mes = mesMostrado;
                ViewBag.Anio = anioMostrado;
                ViewBag.DiasEnMes = DateTime.DaysInMonth(anioMostrado, mesMostrado);
                ViewBag.PrimerDiaSemana = (int)new DateTime(anioMostrado, mesMostrado, 1).DayOfWeek;
                ViewBag.Eventos = eventos;
                ViewBag.Usuarios = usuarios; // Agregar usuarios

                ViewBag.TipoSeleccionado = tipo;
                ViewBag.UsuarioSeleccionado = usuario;

                return View(eventos);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERROR en WebApp: {ex.Message}");
                TempData["ErrorMessage"] = $"Error al cargar el calendario: {ex.Message}";
                return View(new List<Evento>());
            }
        }


    }
}
