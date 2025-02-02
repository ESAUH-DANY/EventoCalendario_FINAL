using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EventoCalendario.Models
{
    public class Reporte
    {
        [Key]
        public int Id { get; set; } // Identificador único del reporte

        [Required]
        public string Tipo { get; set; } // Tipo de reporte: Asistencia, Popularidad

        [Required]
        public DateTime FechaGeneracion { get; set; } // Fecha de generación del reporte

        // Relación con Evento: Un reporte está asociado a un evento
        public int EventoId { get; set; }
        public Evento? Evento { get; set; }

        // Lista de asistencias utilizadas en el reporte
        public List<Asistencia> Asistencias { get; set; } = new List<Asistencia>();

        // Usuario que generó el reporte
        public int UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }

        // Contenido del reporte
        public string Contenido { get; set; }
    }
}
