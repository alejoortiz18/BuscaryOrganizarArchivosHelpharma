using Indexador.Models;
using SharpCompress.Archives;

namespace Indexador.Services;

public static class CompressedIndexerService
{
    static readonly HashSet<string> extensionesComprimidas = new()
    {
        ".zip",
        ".rar",
        ".7z"
    };

    public static IEnumerable<ArchivoModel> ProcesarArchivo(ArchivoModel archivo)
    {
        var resultados = new List<ArchivoModel>();

        // siempre agregamos el archivo principal
        resultados.Add(archivo);

        if (!extensionesComprimidas.Contains(archivo.Extension))
            return resultados;

        if (!File.Exists(archivo.RutaCompleta))
            return resultados;

        try
        {
            using var archive = ArchiveFactory.Open(archivo.RutaCompleta);

            foreach (var entry in archive.Entries)
            {
                if (entry.IsDirectory)
                    continue;

                var nombre = Path.GetFileName(entry.Key);
                var ext = Path.GetExtension(nombre).ToLower();

                resultados.Add(new ArchivoModel
                {
                    RutaCompleta = archivo.RutaCompleta + "\\" + nombre,
                    NombreArchivo = nombre,
                    Extension = ext
                });
            }
        }
        catch
        {
            // comprimido corrupto o formato no soportado
        }

        return resultados;
    }
}