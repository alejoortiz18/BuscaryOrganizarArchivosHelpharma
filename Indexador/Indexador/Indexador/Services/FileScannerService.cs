using System;
using System.Collections.Generic;
using System.IO;

namespace Indexador.Services
{
    public static class FileScannerService
    {
        public static IEnumerable<string> EscanearArchivos(
            string rutaBase,
            string? ultimaRutaProcesada,
            DateTime ultimaEjecucion)
        {
            var pendientes = new Stack<string>();
            pendientes.Push(rutaBase);

            bool empezarAProcesar = string.IsNullOrEmpty(ultimaRutaProcesada);

            while (pendientes.Count > 0)
            {
                var carpetaActual = pendientes.Pop();

                string[] subdirectorios = Array.Empty<string>();
                string[] archivos = Array.Empty<string>();

                try
                {
                    subdirectorios = Directory.GetDirectories(carpetaActual);
                    Array.Sort(subdirectorios);
                }
                catch
                {
                    continue;
                }

                try
                {
                    archivos = Directory.GetFiles(carpetaActual);
                    Array.Sort(archivos);
                }
                catch
                {
                    continue;
                }

                foreach (var dir in subdirectorios)
                {
                    pendientes.Push(dir);
                }

                foreach (var archivo in archivos)
                {
                    // 🔥 SALTO HASTA LLEGAR AL ÚLTIMO
                    if (!empezarAProcesar)
                    {
                        if (archivo == ultimaRutaProcesada)
                        {
                            empezarAProcesar = true;
                        }
                        continue;
                    }

                    DateTime fechaMod;

                    try
                    {
                        fechaMod = File.GetLastWriteTime(archivo);
                    }
                    catch
                    {
                        continue;
                    }

                    if (fechaMod > ultimaEjecucion)
                    {
                        yield return archivo;
                    }
                }
            }
        }
    }
}