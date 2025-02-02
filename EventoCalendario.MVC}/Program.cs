using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EventoCalendario.MVC_
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Agregar HttpClient para consumir la API
            builder.Services.AddHttpClient("EventoCalendarioAPI", client =>
            {
                client.BaseAddress = new Uri("https://localhost:7129/api/"); // Asegúrate que sea la URL correcta de tu API
            });

            // Agregar servicios de MVC
            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            // Configurar el pipeline HTTP
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
