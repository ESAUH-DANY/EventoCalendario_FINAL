using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventoCalendario.Models
{
    public class Asistencia
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UsuarioId { get; set; }  // 🔹 Ahora se almacena solo el ID

        [Required]
        public int EventoId { get; set; }   // 🔹 Ahora se almacena solo el ID

        [ForeignKey("UsuarioId")]
        public Usuario? Usuario { get; set; } // Se carga en consultas, pero no es obligatorio en la creación

        [ForeignKey("EventoId")]
        public Evento? Evento { get; set; } // Se carga en consultas, pero no es obligatorio en la creación

        [Required]
        public string Estado { get; set; }
    }

}
