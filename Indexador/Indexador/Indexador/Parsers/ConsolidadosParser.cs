using Indexador.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Indexador.Parsers
{
    public static class ConsolidadosParser
    {
        public static IEnumerable<ArchivoModel> Parsear(IEnumerable<string> lineas)
        {
            foreach (var linea in lineas)
            {
                if (string.IsNullOrWhiteSpace(linea))
                    continue;

                // ignorar archivos internos del zip
                if (linea.StartsWith("->"))
                    continue;

                var ruta = linea.Trim();

                var nombre = Path.GetFileName(ruta);
                var extension = Path.GetExtension(nombre).ToLower();

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
