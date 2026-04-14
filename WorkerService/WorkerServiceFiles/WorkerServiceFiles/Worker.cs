using System.Diagnostics;
using WorkerServiceFiles.Helper;
using WorkerServiceFiles.Models;
using WorkerServiceFiles.Services;

namespace WorkerServiceFiles
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly FileWatcherService _watcher;
        private readonly NasSettings _nasSettings;

        public Worker(
          ILogger<Worker> logger,
          FileWatcherService watcher,
          Microsoft.Extensions.Options.IOptions<NasSettings> nasOptions)
        {
            _logger = logger;
            _watcher = watcher;
            _nasSettings = nasOptions.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                _logger.LogInformation("Conectando a NAS...");

                using var nas = new NasConnection(
                     @"\\192.168.0.69",
                    new System.Net.NetworkCredential(
                    "radicacion",
                    _nasSettings.Password,
                    "ServiciosRelease"
)
                );

                await EsperarDisponibilidadNAS(_nasSettings.RutaNas, stoppingToken);

                _logger.LogInformation("Iniciando indexación inicial...");

                await _watcher.IndexacionInicialAsync();

                _logger.LogInformation("Indexación inicial completada");

                _watcher.IniciarWatcher();

                _logger.LogInformation("Watcher activo, escuchando cambios...");

                while (!stoppingToken.IsCancellationRequested)
                {
                    await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error crítico en el servicio IndexadorNAS");
            }
        }


        private async Task EsperarDisponibilidadNAS(string rutaNas, CancellationToken token)
        {
            _logger.LogInformation("Esperando acceso a NAS: {ruta}", rutaNas);

            int intentos = 0;

            while (!Directory.Exists(rutaNas))
            {
                intentos++;

                if (intentos > 30)
                {
                    throw new Exception("NAS no disponible después de múltiples intentos");
                }

                _logger.LogWarning("NAS no disponible aún: {ruta}", rutaNas);

                await Task.Delay(10000, token);
            }

            _logger.LogInformation("NAS disponible");
        }
    }
}