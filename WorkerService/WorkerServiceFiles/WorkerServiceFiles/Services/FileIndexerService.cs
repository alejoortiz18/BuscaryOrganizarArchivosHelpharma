using Microsoft.Extensions.Options;
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

        foreach (var rutaArchivo in EnumerarArchivosSeguro(rutaNas))
        {
            try
            {
                await ProcesarArchivo(rutaArchivo);
            }
            catch
            {
                // Ignora errores de archivos individuales
            }
        }
    }

    private async Task ProcesarArchivo(string rutaArchivo)
    {
        var nombreArchivo = Path.GetFileName(rutaArchivo);
        var extension = Path.GetExtension(rutaArchivo).ToLower();

        string? prefijo = null;
        string? numeroFactura = null;

        if (_indexerSettings.ExtensionesFactura.Contains(extension))
        {
            var resultado = FileNameParser.ExtraerFactura(nombreArchivo);
            prefijo = resultado.Prefijo;
            numeroFactura = resultado.Numero;
        }

        var archivo = new ArchivoModel
        {
            RutaCompleta = rutaArchivo,
            NombreArchivo = nombreArchivo,
            Extension = extension,
            Prefijo = prefijo,
            NumeroFactura = numeroFactura
        };

        await _repository.InsertArchivoAsync(archivo);
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
            catch
            {
                // Ignorar carpetas sin acceso
            }

            try
            {
                archivos = Directory.GetFiles(actual);
            }
            catch
            {
                // Ignorar archivos inaccesibles
            }

            foreach (var archivo in archivos)
                yield return archivo;

            foreach (var dir in subdirectorios)
                pendientes.Push(dir);
        }
    }
}