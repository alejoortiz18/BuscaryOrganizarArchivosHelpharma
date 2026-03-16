using Microsoft.Extensions.Options;
using WorkerServiceFiles.Data;
using WorkerServiceFiles.Models;

namespace WorkerServiceFiles.Services
{
    public class FileWatcherService
    {
        private readonly SqlRepository _repository;
        private readonly NasSettings _nasSettings;
        private readonly FileIndexerService _indexerService;

        private FileSystemWatcher? _watcher;

        public FileWatcherService(
            SqlRepository repository,
            FileIndexerService indexerService,
            IOptions<NasSettings> nasOptions)
        {
            _repository = repository;
            _nasSettings = nasOptions.Value;
            _indexerService = indexerService;
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

            await _repository.DeleteArchivoAsync(e.FullPath);
        }

        private async void OnRenamed(object sender, RenamedEventArgs e)
        {
            if (Directory.Exists(e.FullPath))
                return;

            await _repository.DeleteArchivoAsync(e.OldFullPath);

            await EsperarArchivoDisponible(e.FullPath);

            await ProcesarArchivo(e.FullPath);
        }

        private void OnError(object sender, ErrorEventArgs e)
        {
            Console.WriteLine("Watcher error: " + e.GetException().Message);

            // reiniciar watcher
            _watcher?.Dispose();

            IniciarWatcher();
        }

        private async Task ProcesarArchivo(string rutaArchivo)
        {
            if (!File.Exists(rutaArchivo))
                return;

            foreach (var archivo in _indexerService.CrearModeloArchivo(rutaArchivo))
            {
                await _repository.InsertArchivoAsync(archivo);
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
    }
}