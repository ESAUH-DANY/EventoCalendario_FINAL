using System;
using System.ComponentModel.DataAnnotations;

namespace EventoCalendario.Models
{
    public class UsuarioObservador : IObservador
    {
        [Key]
        public int Id { get; set; }

        public string Nombre { get; set; }

        public void Actualizar(string mensaje)
        {
            Console.WriteLine($"🔔 Usuario {Nombre} recibió la notificación: {mensaje}");
        }
    }
}
