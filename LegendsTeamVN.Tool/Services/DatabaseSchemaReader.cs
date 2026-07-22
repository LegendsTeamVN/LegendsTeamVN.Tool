using LegendsTeamVN.Tool.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace LegendsTeamVN.Tool.Services
{
    public class DatabaseSchemaReader
    {
        public async Task<List<TableInfo>> GetTablesAsync(string connectionString)
        {
            var tables = new List<TableInfo>();

            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            var query = @"
                SELECT 
                    TABLE_SCHEMA, 
                    TABLE_NAME 
                FROM INFORMATION_SCHEMA.TABLES 
                WHERE TABLE_TYPE = 'BASE TABLE' 
                ORDER BY TABLE_SCHEMA, TABLE_NAME";

            using var command = new SqlCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                tables.Add(new TableInfo
                {
                    Schema = reader.GetString(0),
                    Name = reader.GetString(1)
                });
            }

            return tables;
        }

        public async Task<TableInfo> GetTableDetailsAsync(string connectionString, string schema, string tableName)
        {
            var table = new TableInfo { Schema = schema, Name = tableName };

            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            // Query columns
            var columnsQuery = @"
                SELECT 
                    c.COLUMN_NAME, 
                    c.DATA_TYPE, 
                    c.IS_NULLABLE,
                    ISNULL(
                        (SELECT TOP 1 1 
                         FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE kcu
                         JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc ON kcu.CONSTRAINT_NAME = tc.CONSTRAINT_NAME
                         WHERE tc.CONSTRAINT_TYPE = 'PRIMARY KEY' 
                           AND kcu.TABLE_NAME = c.TABLE_NAME 
                           AND kcu.TABLE_SCHEMA = c.TABLE_SCHEMA 
                           AND kcu.COLUMN_NAME = c.COLUMN_NAME), 0) AS IS_PRIMARY_KEY
                FROM INFORMATION_SCHEMA.COLUMNS c
                WHERE c.TABLE_SCHEMA = @Schema AND c.TABLE_NAME = @TableName
                ORDER BY c.ORDINAL_POSITION";

            using var command = new SqlCommand(columnsQuery, connection);
            command.Parameters.AddWithValue("@Schema", schema);
            command.Parameters.AddWithValue("@TableName", tableName);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                table.Columns.Add(new ColumnInfo
                {
                    Name = reader.GetString(0),
                    DataType = reader.GetString(1),
                    IsNullable = reader.GetString(2) == "YES",
                    IsPrimaryKey = reader.GetInt32(3) == 1
                });
            }

            return table;
        }
    }
}
