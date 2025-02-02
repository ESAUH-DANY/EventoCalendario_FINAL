using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EventoCalendario.Models
{
    public class Calendario
    {
        [Key]
        public int Id { get; set; }
        public List<Evento> Eventos { get; set; } = new List<Evento>();
    }

}
