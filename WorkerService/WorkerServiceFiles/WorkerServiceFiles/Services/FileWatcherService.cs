using Microsoft.Extensions.Options;
using WorkerServiceFiles.Data;
using WorkerServiceFiles.Models;

namespace WorkerServiceFiles.Services
{
    public class FileWatcherService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly NasSettings _nasSettings;

        private FileSystemWatcher? _watcher;

        public FileWatcherService(
            IServiceScopeFactory scopeFactory,
            IOptions<NasSettings> nasOptions)
        {
            _scopeFactory = scopeFactory;
            _nasSettings = nasOptions.Value;
        }

        public void IniciarWatcher()
        {
            _watcher = new FileSystemWatcher(_nasSettings.RutaNas)
            {
                IncludeSubdirectories = true,
                EnableRaisingEvents = true,
                NotifyFilter =
                    NotifyFilters.FileName |
                    NotifyFilters.Size |
                    NotifyFilters.LastWrite,

                InternalBufferSize = 65536
            };

            _watcher.Created += OnCreated;
            _watcher.Deleted += OnDeleted;
            _watcher.Renamed += OnRenamed;
            _watcher.Error += OnError;

            Console.WriteLine($"Watcher iniciado en {_nasSettings.RutaNas}");
        }

        private async void OnCreated(object sender, FileSystemEventArgs e)
        {
            if (Directory.Exists(e.FullPath))
                return;

            await EsperarArchivoDisponible(e.FullPath);

            await ProcesarArchivo(e.FullPath);
        }

        private async void OnDeleted(object sender, FileSystemEventArgs e)
        {
            if (Directory.Exists(e.FullPath))
                return;

            using var scope = _scopeFactory.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<SqlRepository>();

            await repository.DeleteArchivoAsync(e.FullPath);
        }

        private async void OnRenamed(object sender, RenamedEventArgs e)
        {
            if (Directory.Exists(e.FullPath))
                return;

            using var scope = _scopeFactory.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<SqlRepository>();

            await repository.DeleteArchivoAsync(e.OldFullPath);

            await EsperarArchivoDisponible(e.FullPath);

            await ProcesarArchivo(e.FullPath);
        }

        private void OnError(object sender, ErrorEventArgs e)
        {
            Console.WriteLine("Watcher error: " + e.GetException().Message);

            _watcher?.Dispose();
            IniciarWatcher();
        }

        private async Task ProcesarArchivo(string rutaArchivo)
        {
            if (!File.Exists(rutaArchivo))
                return;

            using var scope = _scopeFactory.CreateScope();

            var repository = scope.ServiceProvider.GetRequiredService<SqlRepository>();
            var indexerService = scope.ServiceProvider.GetRequiredService<FileIndexerService>();

            foreach (var archivo in indexerService.CrearModeloArchivo(rutaArchivo))
            {
                await repository.InsertArchivoAsync(archivo);
            }
        }

        private async Task EsperarArchivoDisponible(string ruta)
        {
            int intentos = 0;

            while (intentos < 10)
            {
                try
                {
                    using var stream = File.Open(ruta, FileMode.Open, FileAccess.Read, FileShare.None);
                    return;
                }
                catch
                {
                    intentos++;
                    await Task.Delay(500);
                }
            }
        }

        public async Task IndexacionInicialAsync()
        {
            using var scope = _scopeFactory.CreateScope();

            var indexer = scope.ServiceProvider.GetRequiredService<FileIndexerService>();
            var repository = scope.ServiceProvider.GetRequiredService<SqlRepository>();

            await indexer.IndexarArchivosAsync(
                async (lote) =>
                {
                    await repository.BulkInsertStagingAsync(lote);
                },
                async () =>
                {
                    await repository.EjecutarMergeAsync();
                });
        }
    }
}