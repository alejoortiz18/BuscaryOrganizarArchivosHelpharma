using Indexador.Models;
using Indexador.Parsers;

namespace Indexador.Services
{
    public static class FileParserService
    {
        public static IEnumerable<ArchivoModel> ParsearArchivos(IEnumerable<string> rutas)
        {
            foreach (var ruta in rutas)
            {
                if (string.IsNullOrWhiteSpace(ruta))
                    continue;

                string nombre = Path.GetFileName(ruta);
                string extension = Path.GetExtension(nombre).ToLower();

                var (prefijo, numero) = FacturaParser.Extraer(nombre);

                yield return new ArchivoModel
                {
                    RutaCompleta = ruta,
                    NombreArchivo = nombre,
                    Extension = extension,
                    Prefijo = prefijo,
                    NumeroFactura = numero
                };
            }
        }
    }
}