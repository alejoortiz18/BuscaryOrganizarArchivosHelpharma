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
            _logger.LogInformation("Iniciando indexación inicial...");

            await _indexer.IndexarArchivosAsync();

            _logger.LogInformation("Indexación inicial completada");

            _watcher.IniciarWatcher();

            _logger.LogInformation("Watcher activo");

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
    }
}
