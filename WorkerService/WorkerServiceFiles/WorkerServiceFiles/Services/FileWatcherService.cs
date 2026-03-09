using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkerServiceFiles.Data;
using WorkerServiceFiles.Helper;
using WorkerServiceFiles.Models;
using WorkerServiceFiles.Models.ModelsFile;

namespace WorkerServiceFiles.Services
{
    public class FileWatcherService
    {
        private readonly SqlRepository _repository;
        private readonly NasSettings _nasSettings;
        private readonly IndexerSettings _indexerSettings;
        private readonly FileIndexerService _indexerService;

        private FileSystemWatcher? _watcher;

        public FileWatcherService(
            SqlRepository repository,
            FileIndexerService indexerService,
            IOptions<NasSettings> nasOptions,
            IOptions<IndexerSettings> indexerOptions)
        {
            _repository = repository;
            _nasSettings = nasOptions.Value;
            _indexerSettings = indexerOptions.Value;
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
        }

        private async void OnCreated(object sender, FileSystemEventArgs e)
        {
            await ProcesarArchivo(e.FullPath);
        }

        private async void OnDeleted(object sender, FileSystemEventArgs e)
        {
            await _repository.DeleteArchivoAsync(e.FullPath);
        }

        private async void OnRenamed(object sender, RenamedEventArgs e)
        {
            await _repository.DeleteArchivoAsync(e.OldFullPath);
            await ProcesarArchivo(e.FullPath);
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
    }
}
