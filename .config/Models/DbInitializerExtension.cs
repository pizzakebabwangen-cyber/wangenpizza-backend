using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WangenPizza.Context;

namespace WangenPizza.Models
{
    public static class DbInitializerExtension
    {
        /// <summary>
        /// Migrate + Seed beim Start. Bei SQL-Ausfall darf die App nicht mit IIS 500.30 sterben —
        /// sonst ist admin.pizzawangen.ch komplett tot. Fehler loggen, Prozess starten lassen.
        /// </summary>
        public static async Task<IApplicationBuilder> UseItToSeedSqlServer(this IApplicationBuilder app)
        {
            ArgumentNullException.ThrowIfNull(app, nameof(app));

            using var scope = app.ApplicationServices.CreateScope();
            var services = scope.ServiceProvider;
            var logger = services.GetRequiredService<ILoggerFactory>()
                .CreateLogger("WangenPizza.DbInitializer");

            try
            {
                var context = services.GetRequiredService<ApplicationDbContext>();
                var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
                var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

                await DbInitializer.Initialize(context, userManager, roleManager);
                logger.LogInformation("Datenbank-Migration/Seed abgeschlossen.");
            }
            catch (Exception ex)
            {
                logger.LogCritical(
                    ex,
                    "Datenbank beim Start nicht erreichbar oder Migrate/Seed fehlgeschlagen. " +
                    "ConnectionStrings:DefultConnection und SQL Server prüfen; ggf. dotnet ef database update manuell. " +
                    "Die Anwendung startet trotzdem — Seiten mit DB-Zugriff können fehlschlagen, bis die DB wieder OK ist.");
            }

            return app;
        }
    }
}
