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

                    var factura = ExtraerFactura(nombre);

                    resultados.Add(new ArchivoModel
                    {
                        RutaCompleta = rutaZip + "\\" + nombre,
                        NombreArchivo = nombre,
                        Extension = extension,
                        Prefijo = factura.Prefijo,
                        NumeroFactura = factura.Numero
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

        private (string? Prefijo, string? Numero) ExtraerFactura(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                return (null, null);

            var match = System.Text.RegularExpressions.Regex.Match(
                nombre,
                @"([A-Za-z]{1,5})[-_]?(\d{3,})"
            );

            if (match.Success)
            {
                var prefijo = match.Groups[1].Value.ToUpper();
                var numero = match.Groups[2].Value;

                return (prefijo, numero);
            }

            return (null, null);
        }
    }
}