using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventoCalendario.Models
{
    public class Notificacion
    {
        private readonly List<IObservador> _observadores = new List<IObservador>();

        [Key]
        public int Id { get; set; }
        [Required]
        public string Contenido { get; set; }
        [Required]
        public DateTime FechaEnvio { get; set; }

        [ForeignKey("Usuario")]
        public int UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }

        [ForeignKey("Evento")]
        public int EventoId { get; set; }
        public Evento? Evento { get; set; }

        public void SuscribirUsuario(IObservador observador)
        {
            if (!_observadores.Contains(observador))
            {
                _observadores.Add(observador);
            }
        }

        public void DesuscribirUsuario(IObservador observador)
        {
            if (_observadores.Contains(observador))
            {
                _observadores.Remove(observador);
            }
        }

        public void Notificar(string mensaje)
        {
            foreach (var observador in _observadores)
            {
                observador.Actualizar(mensaje);
            }
        }
    }
}
