using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;
using WorkerServiceFiles.Models.ModelsFile;

namespace WorkerServiceFiles.Data
{
    public class SqlRepository
    {
        private readonly string _connectionString;

        public SqlRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("SqlServer")!;
        }

        private SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }

        public async Task InsertArchivoAsync(ArchivoModel archivo)
        {
            const string sql = @"
            INSERT INTO ArchivosIndexados
            (
                RutaCompleta,
                NombreArchivo,
                Extension,
                Prefijo,
                NumeroFactura,
                FechaCreacion
            )
            VALUES
            (
                @RutaCompleta,
                @NombreArchivo,
                @Extension,
                @Prefijo,
                @NumeroFactura,
                SYSDATETIME()
            );";

            try
            {
                using var connection = GetConnection();
                await connection.ExecuteAsync(sql, archivo);
            }
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
            {
                // duplicado por índice único
            }
        }

        public async Task DeleteArchivoAsync(string rutaCompleta)
        {
            const string sql = @"
        DELETE FROM ArchivosIndexados
        WHERE RutaHash = HASHBYTES('SHA2_256', @RutaCompleta);";

            using var connection = GetConnection();

            await connection.ExecuteAsync(sql, new { RutaCompleta = rutaCompleta });
        }

        public async Task<long> ObtenerTotalArchivosAsync()
        {
            const string sql = "SELECT COUNT_BIG(*) FROM ArchivosIndexados";

            using var connection = GetConnection();

            return await connection.ExecuteScalarAsync<long>(sql);
        }

        public async Task BulkInsertAsync(List<ArchivoModel> archivos)
        {
            using var connection = GetConnection();
            await connection.OpenAsync();

            using var bulkCopy = new SqlBulkCopy(connection)
            {
                DestinationTableName = "ArchivosIndexados",
                BatchSize = 500,
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

        public async Task BulkInsertStagingAsync(List<ArchivoModel> archivos)
        {
            using var connection = GetConnection();
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

        public async Task<HashSet<string>> ObtenerRutasExistentesAsync()
        {
            const string sql = "SELECT RutaCompleta FROM ArchivosIndexados";

            using var connection = GetConnection();

            var rutas = await connection.QueryAsync<string>(sql);

            return rutas.ToHashSet(StringComparer.OrdinalIgnoreCase);
        }

        public async Task EjecutarMergeAsync()
        {
            using var connection = GetConnection();

            await connection.ExecuteAsync(
                "sp_MergeArchivosIndexados",
                commandType: CommandType.StoredProcedure);
        }
    }
}
