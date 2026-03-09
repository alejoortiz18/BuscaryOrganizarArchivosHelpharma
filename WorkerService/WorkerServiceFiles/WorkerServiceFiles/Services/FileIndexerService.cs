using Microsoft.Extensions.Options;
using WorkerServiceFiles.Data;
using WorkerServiceFiles.Helper;
using WorkerServiceFiles.Models;
using WorkerServiceFiles.Models.ModelsFile;
using System.IO.Compression;


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

        var lote = new List<ArchivoModel>(BatchSize);

        foreach (var rutaArchivo in EnumerarArchivosSeguro(rutaNas))
        {
            try
            {
                foreach (var archivo in CrearModeloArchivo(rutaArchivo))
                {
                    lote.Add(archivo);
                }

                if (lote.Count >= BatchSize)
                {
                    await _repository.BulkInsertStagingAsync(lote);
                    await _repository.EjecutarMergeAsync();
                    lote.Clear();
                }
            }
            catch
            {
                // ignorar errores individuales
            }
        }

        if (lote.Count > 0)
            await _repository.BulkInsertStagingAsync(lote);
            await _repository.EjecutarMergeAsync();
            lote.Clear();
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

            string[] subdirectorios = Array.Empty<string>();
            string[] archivos = Array.Empty<string>();

            try
            {
                subdirectorios = Directory.GetDirectories(actual);
            }
            catch { }

            try
            {
                archivos = Directory.GetFiles(actual);
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