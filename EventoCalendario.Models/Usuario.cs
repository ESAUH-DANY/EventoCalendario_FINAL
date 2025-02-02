using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace EventoCalendario.Models
{
    public class Usuario
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [StringLength(100)]
        public string Nombre { get; set; }
        [Required]
        [EmailAddress]
        public string Correo { get; set; }
        [Required]
        [StringLength(50)]
        public string Rol { get; set; }
        public List<Evento> EventosCreados { get; set; } = new List<Evento>();
        [JsonIgnore]
        public ICollection<Asistencia> Asistencias { get; set; } = new List<Asistencia>();
    }
}
