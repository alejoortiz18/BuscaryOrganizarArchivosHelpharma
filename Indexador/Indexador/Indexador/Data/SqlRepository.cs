using Indexador.Models;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Indexador.Data
{
    public class SqlRepository
    {
        private readonly string _connectionString =
            "Server=(localdb)\\MSSQLLocalDB;Database=FilesNas;Trusted_Connection=True;TrustServerCertificate=True";

        public async Task BulkInsertAsync(List<ArchivoModel> archivos)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var bulkCopy = new SqlBulkCopy(connection)
            {
                DestinationTableName = "ArchivosIndexados_Staging",
                BatchSize = 5000,
                BulkCopyTimeout = 0
            };

            var table = new DataTable();

            table.Columns.Add("RutaCompleta", typeof(string));
            table.Columns.Add("NombreArchivo", typeof(string));
            table.Columns.Add("Extension", typeof(string));
            table.Columns.Add("Prefijo", typeof(string));
            table.Columns.Add("NumeroFactura", typeof(string));

            // 🔍 Construcción del DataTable con logging
            foreach (var a in archivos)
            {
                try
                {
                    table.Rows.Add(
                        a.RutaCompleta,
                        a.NombreArchivo,
                        a.Extension,
                        a.Prefijo,
                        a.NumeroFactura
                    );
                }
                catch (Exception ex)
                {
                    Console.WriteLine("❌ ERROR ARMANDO FILA:");
                    Console.WriteLine($"Ruta: {a.RutaCompleta}");
                    Console.WriteLine($"Nombre: {a.NombreArchivo}");
                    Console.WriteLine($"Extension: {a.Extension}");
                    Console.WriteLine($"Prefijo: {a.Prefijo}");
                    Console.WriteLine($"Numero: {a.NumeroFactura}");
                    Console.WriteLine(ex.Message);

                    throw;
                }
            }

            bulkCopy.ColumnMappings.Add("RutaCompleta", "RutaCompleta");
            bulkCopy.ColumnMappings.Add("NombreArchivo", "NombreArchivo");
            bulkCopy.ColumnMappings.Add("Extension", "Extension");
            bulkCopy.ColumnMappings.Add("Prefijo", "Prefijo");
            bulkCopy.ColumnMappings.Add("NumeroFactura", "NumeroFactura");

            // 🔥 DEBUG DEL ERROR REAL EN SQL BULK
            try
            {
                await bulkCopy.WriteToServerAsync(table);
            }
            catch (Exception ex)
            {
                Console.WriteLine("💣 ERROR EN BULK INSERT");

                int index = 0;

                foreach (DataRow row in table.Rows)
                {
                    try
                    {
                        var ruta = row["RutaCompleta"]?.ToString();
                        var nombre = row["NombreArchivo"]?.ToString();
                        var ext = row["Extension"]?.ToString();
                        var prefijo = row["Prefijo"]?.ToString();
                        var numero = row["NumeroFactura"]?.ToString();

                        // 🔴 Validaciones clave
                        if (ext != null && ext.Length > 20)
                        {
                            Console.WriteLine("🚨 EXTENSION DEMASIADO LARGA");
                            Console.WriteLine($"Index: {index}");
                            Console.WriteLine($"Extension: {ext}");
                            Console.WriteLine($"Length: {ext.Length}");
                            Console.WriteLine($"Archivo: {nombre}");
                            Console.WriteLine($"Ruta: {ruta}");
                            break;
                        }

                        if (nombre != null && nombre.Length > 500)
                        {
                            Console.WriteLine("🚨 NOMBRE DEMASIADO LARGO");
                            Console.WriteLine($"Nombre: {nombre}");
                            break;
                        }

                        if (ruta != null && ruta.Length > 1000)
                        {
                            Console.WriteLine("🚨 RUTA DEMASIADO LARGA");
                            Console.WriteLine($"Ruta: {ruta}");
                            break;
                        }
                    }
                    catch (Exception innerEx)
                    {
                        Console.WriteLine("⚠️ Error inspeccionando fila:");
                        Console.WriteLine(innerEx.Message);
                    }

                    index++;
                }

                Console.WriteLine("🔥 EXCEPCIÓN ORIGINAL:");
                Console.WriteLine(ex.Message);

                throw;
            }
        }

        public async Task EjecutarMergeAsync()
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var sql = @"
                    INSERT INTO ArchivosIndexados
                    (
                        RutaCompleta,
                        NombreArchivo,
                        Extension,
                        Prefijo,
                        NumeroFactura
                    )
                    SELECT
                        s.RutaCompleta,
                        s.NombreArchivo,
                        LEFT(s.Extension, 20),
                        s.Prefijo,
                        s.NumeroFactura
                    FROM ArchivosIndexados_Staging s
                    WHERE NOT EXISTS
                    (
                        SELECT 1
                        FROM ArchivosIndexados t
                        WHERE t.RutaHash = HASHBYTES('SHA2_256', s.RutaCompleta)
                    );

                    TRUNCATE TABLE ArchivosIndexados_Staging;
                    ";

            using var cmd = new SqlCommand(sql, connection);
            cmd.CommandTimeout = 0;

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task EjecutarMergeSPAsync()
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var cmd = new SqlCommand("sp_MergeArchivosIndexados", connection);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandTimeout = 0;

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<DateTime> ObtenerUltimaEjecucionAsync()
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var cmd = new SqlCommand("SELECT UltimaEjecucion FROM IndexadorControl WHERE Id = 1", connection);

            var result = await cmd.ExecuteScalarAsync();

            return result != null ? (DateTime)result : DateTime.MinValue;
        }

        public async Task ActualizarUltimaEjecucionAsync(DateTime fecha)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var cmd = new SqlCommand("UPDATE IndexadorControl SET UltimaEjecucion = @fecha WHERE Id = 1", connection);
            cmd.Parameters.AddWithValue("@fecha", fecha);

            await cmd.ExecuteNonQueryAsync();
        }
    }
}
