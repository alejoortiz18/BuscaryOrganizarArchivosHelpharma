using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace Indexador.Data
{
    public class CompressedRepository
    {
        private readonly string _connectionString =
            "Server=(localdb)\\MSSQLLocalDB;Database=FilesNas;Trusted_Connection=True;TrustServerCertificate=True;";

        public List<string> ObtenerComprimidos()
        {
            var resultado = new List<string>();

            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            var sql = @"
                SELECT RutaCompleta
                FROM ArchivosIndexados
                WHERE Extension LIKE '%zip%'
                   OR Extension LIKE '%rar%'
                   OR Extension LIKE '%7z%'
            ";

            using var cmd = new SqlCommand(sql, connection);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                var ruta = reader.GetString(0);

                if (!string.IsNullOrWhiteSpace(ruta))
                    resultado.Add(ruta);
            }

            Console.WriteLine($"[INFO] Comprimidos encontrados en BD: {resultado.Count}");

            return resultado;
        }
    }
}
