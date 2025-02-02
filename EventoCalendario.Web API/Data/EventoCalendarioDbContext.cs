using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EventoCalendario.Models;

    public class EventoCalendarioDbContext : DbContext
    {
        public EventoCalendarioDbContext (DbContextOptions<EventoCalendarioDbContext> options)
            : base(options)
        {
        }

        public DbSet<EventoCalendario.Models.Usuario> Usuario { get; set; } = default!;

public DbSet<EventoCalendario.Models.Asistencia> Asistencia { get; set; } = default!;

public DbSet<EventoCalendario.Models.Calendario> Calendario { get; set; } = default!;

public DbSet<EventoCalendario.Models.Evento> Evento { get; set; } = default!;

public DbSet<EventoCalendario.Models.Notificacion> Notificacion { get; set; } = default!;



public DbSet<EventoCalendario.Models.Reporte> Reporte { get; set; } = default!;
    }
