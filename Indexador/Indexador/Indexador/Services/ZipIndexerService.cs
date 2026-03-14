using Indexador.Models;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Text;

namespace Indexador.Services
{
    public static class ZipIndexerService
    {
        public static IEnumerable<ArchivoModel> ProcesarArchivo(ArchivoModel archivo)
        {
            // siempre devolver el archivo principal
            yield return archivo;

            if (archivo.Extension != ".zip")
                yield break;

            if (!File.Exists(archivo.RutaCompleta))
                yield break;

            ZipArchive? zip = null;

            try
            {
                zip = ZipFile.OpenRead(archivo.RutaCompleta);
            }
            catch
            {
                yield break;
            }

            foreach (var entry in zip.Entries)
            {
                if (string.IsNullOrWhiteSpace(entry.Name))
                    continue;

                var nombre = entry.Name;
                var ext = Path.GetExtension(nombre).ToLower();

                yield return new ArchivoModel
                {
                    RutaCompleta = archivo.RutaCompleta + "\\" + nombre,
                    NombreArchivo = nombre,
                    Extension = ext
                };
            }

            zip.Dispose();
        }
    }
}
