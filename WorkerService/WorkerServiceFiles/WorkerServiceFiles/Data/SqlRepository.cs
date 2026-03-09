using Dapper;
using Microsoft.Data.SqlClient;
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
                // Duplicado por índice único (RutaHash)
                // Se ignora porque el archivo ya fue indexado
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


        public async Task<bool> ExisteArchivoAsync(string rutaCompleta)
        {
            const string sql = @"
                SELECT CASE 
                    WHEN EXISTS (
                        SELECT 1 
                        FROM ArchivosIndexados
                        WHERE RutaHash = HASHBYTES('SHA2_256', @RutaCompleta)
                    )
                    THEN 1 
                    ELSE 0 
                END";

            using var connection = GetConnection();

            return await connection.ExecuteScalarAsync<bool>(sql, new { RutaCompleta = rutaCompleta });
        }

        public async Task<long> ObtenerTotalArchivosAsync()
        {
            const string sql = "SELECT COUNT_BIG(*) FROM ArchivosIndexados";

            using var connection = GetConnection();

            return await connection.ExecuteScalarAsync<long>(sql);
        }
    }
}
