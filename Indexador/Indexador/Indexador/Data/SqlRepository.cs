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
            "Server=(localdb)\\MSSQLLocalDB;Database=FilesNas;Trusted_Connection=True;TrustServerCertificate=True;";

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

            foreach (var a in archivos)
            {
                table.Rows.Add(
                    a.RutaCompleta,
                    a.NombreArchivo,
                    a.Extension,
                    a.Prefijo,
                    a.NumeroFactura
                );
            }

            bulkCopy.ColumnMappings.Add("RutaCompleta", "RutaCompleta");
            bulkCopy.ColumnMappings.Add("NombreArchivo", "NombreArchivo");
            bulkCopy.ColumnMappings.Add("Extension", "Extension");
            bulkCopy.ColumnMappings.Add("Prefijo", "Prefijo");
            bulkCopy.ColumnMappings.Add("NumeroFactura", "NumeroFactura");

            await bulkCopy.WriteToServerAsync(table);
        }

        public async Task EjecutarMergeAsync()
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var sql = @"
                MERGE ArchivosIndexados AS target
                USING ArchivosIndexados_Staging AS source
                ON target.RutaHash = HASHBYTES('SHA2_256', source.RutaCompleta)

                WHEN NOT MATCHED THEN
                INSERT
                (
                    RutaCompleta,
                    NombreArchivo,
                    Extension,
                    Prefijo,
                    NumeroFactura
                )
                VALUES
                (
                    source.RutaCompleta,
                    source.NombreArchivo,
                    source.Extension,
                    source.Prefijo,
                    source.NumeroFactura
                );

                TRUNCATE TABLE ArchivosIndexados_Staging;
                ";

            using var cmd = new SqlCommand(sql, connection);
            await cmd.ExecuteNonQueryAsync();
        }
    }
}
