
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace EventoCalendario.Models
{
    public class Evento
    {
        [Key]
        public int Id { get; set; } // Identificador único del evento

        [Required(ErrorMessage = "El nombre del evento es obligatorio.")]
        public string Nombre { get; set; } // Nombre del evento

        public string Descripcion { get; set; } // Breve descripción del evento

        [Required(ErrorMessage = "La fecha del evento es obligatoria.")]
        [DataType(DataType.DateTime)]
        public DateTime Fecha { get; set; } // Fecha y hora del evento

        [Required(ErrorMessage = "La ubicación del evento es obligatoria.")]
        public string Ubicacion { get; set; } // Ubicación donde se realizará el evento

        [Required(ErrorMessage = "El tipo de evento es obligatorio.")]
        public string Tipo { get; set; } // Tipo del evento (Ej: Conferencia, Taller, etc.)

        [Required]
        public string Estado { get; set; } = "Activo"; // Estado del evento (Activo, Cancelado)

        [Required(ErrorMessage = "Debe seleccionar un usuario como creador del evento.")]
        public int UsuarioId { get; set; } // Identificador del usuario creador

        [ForeignKey("UsuarioId")]
        [JsonIgnore]
        public Usuario? Creador { get; set; } // Permite valores nulos en la creación


        [JsonIgnore] // No se enviará ni pedirá en Swagger
        public ICollection<Asistencia> Asistencias { get; set; } // Relación con asistencias
        public bool EsEliminado { get; set; } = false;

        // Constructor para inicializar la lista de asistencias y evitar errores de referencia nula
        public Evento()
        {
            Asistencias = new List<Asistencia>();
        }
    }
}
