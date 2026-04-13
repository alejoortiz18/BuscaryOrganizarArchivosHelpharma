using System;
using System.Collections.Generic;
using System.IO;

namespace Indexador.Services
{
    public static class FileScannerService
    {
        public static IEnumerable<string> EscanearArchivos(string rutaBase, DateTime ultimaEjecucion)
        {
            var pendientes = new Stack<string>();
            pendientes.Push(rutaBase);

            while (pendientes.Count > 0)
            {
                var carpetaActual = pendientes.Pop();

                string[] subdirectorios = Array.Empty<string>();
                string[] archivos = Array.Empty<string>();

                // 🔥 Leer subdirectorios
                try
                {
                    subdirectorios = Directory.GetDirectories(carpetaActual);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Error accediendo a carpetas: {carpetaActual}");
                    Console.WriteLine(ex.Message);
                    continue;
                }

                // 🔥 Leer archivos
                try
                {
                    archivos = Directory.GetFiles(carpetaActual);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Error accediendo a archivos: {carpetaActual}");
                    Console.WriteLine(ex.Message);
                    continue;
                }

                // 🔁 Recorrer subcarpetas
                foreach (var dir in subdirectorios)
                {
                    pendientes.Push(dir);
                }

                // 📄 Procesar archivos
                foreach (var archivo in archivos)
                {
                    DateTime fechaModificacion;

                    try
                    {
                        fechaModificacion = File.GetLastWriteTime(archivo);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"⚠️ Error leyendo fecha archivo: {archivo}");
                        Console.WriteLine(ex.Message);
                        continue;
                    }

                    // 🔥 FILTRO CLAVE (INCREMENTAL)
                    if (fechaModificacion > ultimaEjecucion)
                    {
                        yield return archivo;
                    }
                }
            }
        }
    }
}