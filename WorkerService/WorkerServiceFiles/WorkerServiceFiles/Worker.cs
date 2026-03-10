using System.Diagnostics;
using System.Net;
using WorkerServiceFiles.Helper;
using WorkerServiceFiles.Services;

namespace WorkerServiceFiles
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly FileIndexerService _indexer;
        private readonly FileWatcherService _watcher;

        public Worker(
            ILogger<Worker> logger,
            FileIndexerService indexer,
            FileWatcherService watcher)
        {
            _logger = logger;
            _indexer = indexer;
            _watcher = watcher;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                var rutaNas = @"\\192.168.0.69\Informes";

                _logger.LogInformation("Conectando sesión SMB con NAS...");

                Process.Start(new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = "/c net use \\\\192.168.0.69\\Informes /user:ServiciosRelease\\radicacion h3lph@rm@,+",
                    CreateNoWindow = true,
                    UseShellExecute = false
                });

                await Task.Delay(2000);

                _logger.LogInformation("Esperando acceso a NAS: {ruta}", rutaNas);

                while (!Directory.Exists(rutaNas))
                {
                    _logger.LogWarning("NAS no disponible aún: {ruta}", rutaNas);
                    await Task.Delay(10000, stoppingToken);
                }

                _logger.LogInformation("NAS disponible, iniciando indexación...");

                await _indexer.IndexarArchivosAsync();

                _logger.LogInformation("Indexación inicial completada");

                _watcher.IniciarWatcher();

                _logger.LogInformation("Watcher activo");

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
    }
}
