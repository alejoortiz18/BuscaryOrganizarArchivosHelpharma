using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.IO.Compression;
using WorkerServiceFiles.Helper;
using WorkerServiceFiles.Models;
using WorkerServiceFiles.Models.ModelsFile;

namespace WorkerServiceFiles.Services;

public class FileIndexerService
{
    private readonly NasSettings _nasSettings;
    private readonly IndexerSettings _indexerSettings;

    private const int BatchSize = 500;
    private const long MaxZipSizeBytes = 200_000_000; // 200 MB

    public FileIndexerService(
        IOptions<NasSettings> nasOptions,
        IOptions<IndexerSettings> indexerOptions)
    {
        _nasSettings = nasOptions.Value;
        _indexerSettings = indexerOptions.Value;
    }

    public async Task IndexarArchivosAsync(Func<List<ArchivoModel>, Task> bulkInsert, Func<Task> merge)
    {
        var rutaNas = _nasSettings.RutaNas;

        if (!Directory.Exists(rutaNas))
            throw new DirectoryNotFoundException($"No se encontró la ruta NAS: {rutaNas}");

        var cola = new BlockingCollection<ArchivoModel>(100000);

        // 🔹 Escritor (consume cola y guarda en DB)
        var escritor = Task.Run(async () =>
        {
            var lote = new List<ArchivoModel>(BatchSize);
            int contadorLotes = 0;

            foreach (var archivo in cola.GetConsumingEnumerable())
            {
                lote.Add(archivo);

                if (lote.Count >= BatchSize)
                {
                    await bulkInsert(lote);
                    lote.Clear();

                    contadorLotes++;

                    if (contadorLotes % 20 == 0)
                        await merge();
                }
            }

            if (lote.Count > 0)
                await bulkInsert(lote);

            await merge();
        });

        var opciones = new ParallelOptions
        {
            MaxDegreeOfParallelism = Environment.ProcessorCount
        };

        // 🔹 Procesamiento paralelo REAL async
        await Parallel.ForEachAsync(
            Directory.EnumerateDirectories(rutaNas),
            opciones,
            async (carpeta, ct) =>
            {
                foreach (var rutaArchivo in EnumerarArchivosSeguro(carpeta))
                {
                    try
                    {
                        foreach (var archivo in CrearModeloArchivo(rutaArchivo))
                        {
                            cola.Add(archivo, ct);
                        }
                    }
                    catch (Exception)
                    {
                        // Aquí puedes loguear si quieres
                    }
                }

                await Task.CompletedTask;
            });

        cola.CompleteAdding();

        await escritor;
    }

    public IEnumerable<ArchivoModel> CrearModeloArchivo(string rutaArchivo)
    {
        var nombreArchivo = Path.GetFileName(rutaArchivo);
        var extension = Path.GetExtension(rutaArchivo).ToLower();

        // 🔹 Filtrar extensiones válidas
        if (!_indexerSettings.ExtensionesFactura.Contains(extension))
            yield break;

        // 🔹 Evitar archivos basura
        if (nombreArchivo.StartsWith("~$") || nombreArchivo.EndsWith(".tmp"))
            yield break;

        var resultado = FileNameParser.ExtraerFactura(nombreArchivo);

        // 1️⃣ Archivo físico
        yield return new ArchivoModel
        {
            RutaCompleta = rutaArchivo,
            NombreArchivo = nombreArchivo,
            Extension = extension,
            Prefijo = resultado.Prefijo,
            NumeroFactura = resultado.Numero
        };

        // 2️⃣ ZIP
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
        if (!File.Exists(rutaZip))
            yield break;

        // 🔹 Evitar ZIPs demasiado grandes
        try
        {
            var fileInfo = new FileInfo(rutaZip);
            if (fileInfo.Length > MaxZipSizeBytes)
                yield break;
        }
        catch
        {
            yield break;
        }

        ZipArchive? archive = null;

        try
        {
            archive = ZipFile.OpenRead(rutaZip);
        }
        catch
        {
            yield break;
        }

        using (archive)
        {
            foreach (var entry in archive.Entries)
            {
                if (string.IsNullOrEmpty(entry.Name))
                    continue;

                var nombreArchivo = entry.Name;
                var extension = Path.GetExtension(nombreArchivo).ToLower();

                if (!_indexerSettings.ExtensionesFactura.Contains(extension))
                    continue;

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
}