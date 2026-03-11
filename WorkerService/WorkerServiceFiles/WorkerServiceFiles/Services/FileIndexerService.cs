using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.IO.Compression;
using WorkerServiceFiles.Data;
using WorkerServiceFiles.Helper;
using WorkerServiceFiles.Models;
using WorkerServiceFiles.Models.ModelsFile;


namespace WorkerServiceFiles.Services;

public class FileIndexerService
{
    private readonly SqlRepository _repository;
    private readonly NasSettings _nasSettings;
    private readonly IndexerSettings _indexerSettings;

    private const int BatchSize = 500;

    public FileIndexerService(
        SqlRepository repository,
        IOptions<NasSettings> nasOptions,
        IOptions<IndexerSettings> indexerOptions)
    {
        _repository = repository;
        _nasSettings = nasOptions.Value;
        _indexerSettings = indexerOptions.Value;
    }


    public async Task IndexarArchivosAsync()
    {
        var rutaNas = _nasSettings.RutaNas;

        if (!Directory.Exists(rutaNas))
            throw new DirectoryNotFoundException($"No se encontró la ruta NAS: {rutaNas}");

        var cola = new BlockingCollection<ArchivoModel>(100000);

        var escritor = Task.Run(async () =>
        {
            var lote = new List<ArchivoModel>(BatchSize);
            int contadorLotes = 0;

            foreach (var archivo in cola.GetConsumingEnumerable())
            {
                lote.Add(archivo);

                if (lote.Count >= BatchSize)
                {
                    await _repository.BulkInsertStagingAsync(lote);
                    lote.Clear();

                    contadorLotes++;

                    if (contadorLotes % 20 == 0)
                        await _repository.EjecutarMergeAsync();
                }
            }

            if (lote.Count > 0)
                await _repository.BulkInsertStagingAsync(lote);

            await _repository.EjecutarMergeAsync();
        });

        var opciones = new ParallelOptions
        {
            MaxDegreeOfParallelism = Environment.ProcessorCount
        };

        Parallel.ForEach(
            Directory.EnumerateDirectories(rutaNas),
            opciones,
            carpeta =>
            {
                foreach (var rutaArchivo in EnumerarArchivosSeguro(carpeta))
                {
                    try
                    {
                        foreach (var archivo in CrearModeloArchivo(rutaArchivo))
                        {
                            cola.Add(archivo);
                        }
                    }
                    catch { }
                }
            });

        cola.CompleteAdding();

        await escritor;
    }

    public IEnumerable<ArchivoModel> CrearModeloArchivo(string rutaArchivo)
    {
        var nombreArchivo = Path.GetFileName(rutaArchivo);
        var extension = Path.GetExtension(rutaArchivo).ToLower();

        var resultado = FileNameParser.ExtraerFactura(nombreArchivo);

        // 1️⃣ Registrar el archivo físico (zip)
        yield return new ArchivoModel
        {
            RutaCompleta = rutaArchivo,
            NombreArchivo = nombreArchivo,
            Extension = extension,
            Prefijo = resultado.Prefijo,
            NumeroFactura = resultado.Numero
        };

        // 2️⃣ Si es ZIP indexar contenido interno
        if (extension == ".zip")
        {
            foreach (var archivoZip in EnumerarZip(rutaArchivo))
            {
                yield return archivoZip;
            }
        }
    }

    private IEnumerable<string> EnumerarArchivosSeguro(string ruta)
    {
        var pendientes = new Stack<string>();
        pendientes.Push(ruta);

        while (pendientes.Count > 0)
        {
            var actual = pendientes.Pop();

            IEnumerable<string> subdirectorios = Enumerable.Empty<string>();
            IEnumerable<string> archivos = Enumerable.Empty<string>();

            try
            {
                subdirectorios = Directory.EnumerateDirectories(actual);
            }
            catch { }

            try
            {
                archivos = Directory.EnumerateFiles(actual);
            }
            catch { }

            foreach (var archivo in archivos)
                yield return archivo;

            foreach (var dir in subdirectorios)
                pendientes.Push(dir);
        }
    }

    private IEnumerable<ArchivoModel> EnumerarZip(string rutaZip)
    {
        using var archive = ZipFile.OpenRead(rutaZip);

        foreach (var entry in archive.Entries)
        {
            if (string.IsNullOrEmpty(entry.Name))
                continue;

            var nombreArchivo = entry.Name;
            var extension = Path.GetExtension(nombreArchivo).ToLower();

            var resultado = FileNameParser.ExtraerFactura(nombreArchivo);

            yield return new ArchivoModel
            {
                RutaCompleta = $"{rutaZip}|{entry.FullName}",
                NombreArchivo = nombreArchivo,
                Extension = extension,
                Prefijo = resultado.Prefijo,
                NumeroFactura = resultado.Numero
            };
        }
    }
}