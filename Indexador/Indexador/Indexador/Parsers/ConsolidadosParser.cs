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

                var ruta = linea.Trim();

                string nombre;
                string extension;

                // CASO 1: archivo dentro de zip
                if (ruta.Contains("|"))
                {
                    var partes = ruta.Split('|');

                    var nombreInterno = partes[1];

                    nombre = Path.GetFileName(nombreInterno);
                    extension = Path.GetExtension(nombre).ToLower();
                }
                else
                {
                    // CASO 2: archivo normal
                    nombre = Path.GetFileName(ruta);
                    extension = Path.GetExtension(nombre).ToLower();
                }

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
