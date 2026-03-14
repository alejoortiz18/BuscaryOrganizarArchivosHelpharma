using Indexador.Models;
using SharpCompress.Archives;

namespace Indexador.Services
{
    public class CompressedProcessorService
    {
        public List<ArchivoModel> Procesar(string rutaZip)
        {
            var resultados = new List<ArchivoModel>();

            if (!File.Exists(rutaZip))
            {
                Console.WriteLine($"[WARN] Archivo no encontrado: {rutaZip}");
                return resultados;
            }

            try
            {
                using var archive = ArchiveFactory.Open(rutaZip);

                int contador = 0;

                foreach (var entry in archive.Entries)
                {
                    if (entry.IsDirectory)
                        continue;

                    var nombre = Path.GetFileName(entry.Key);
                    var extension = Path.GetExtension(nombre)?.ToLower() ?? "";

                    resultados.Add(new ArchivoModel
                    {
                        RutaCompleta = rutaZip + "\\" + nombre,
                        NombreArchivo = nombre,
                        Extension = extension,
                        Prefijo = null,
                        NumeroFactura = null
                    });

                    contador++;
                }

                Console.WriteLine($"[ZIP OK] {rutaZip} -> {contador} archivos internos");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ZIP ERROR] {rutaZip} -> {ex.Message}");
            }

            return resultados;
        }
    }
}