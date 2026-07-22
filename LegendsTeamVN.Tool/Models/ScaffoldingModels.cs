namespace LegendsTeamVN.Tool.Models
{
    public class DatabaseConnectionModel
    {
        public string ConnectionString { get; set; } = string.Empty;
    }

    public class ColumnInfo
    {
        public string Name { get; set; } = string.Empty;
        public string DataType { get; set; } = string.Empty;
        public bool IsNullable { get; set; }
        public bool IsPrimaryKey { get; set; }

        public string SystemType => MapToCSharpType(DataType, IsNullable);

        private string MapToCSharpType(string sqlType, bool isNullable)
        {
            var csharpType = sqlType.ToLower() switch
            {
                "bigint" => "long",
                "binary" => "byte[]",
                "bit" => "bool",
                "char" => "string",
                "date" => "DateTime",
                "datetime" => "DateTime",
                "datetime2" => "DateTime",
                "datetimeoffset" => "DateTimeOffset",
                "decimal" => "decimal",
                "float" => "double",
                "image" => "byte[]",
                "int" => "int",
                "money" => "decimal",
                "nchar" => "string",
                "ntext" => "string",
                "numeric" => "decimal",
                "nvarchar" => "string",
                "real" => "float",
                "smalldatetime" => "DateTime",
                "smallint" => "short",
                "smallmoney" => "decimal",
                "text" => "string",
                "time" => "TimeSpan",
                "timestamp" => "byte[]",
                "tinyint" => "byte",
                "uniqueidentifier" => "Guid",
                "varbinary" => "byte[]",
                "varchar" => "string",
                "xml" => "string",
                _ => "string"
            };

            if (isNullable && csharpType != "string" && csharpType != "byte[]")
            {
                return csharpType + "?";
            }

            return csharpType;
        }
    }

    public class TableInfo
    {
        public string Schema { get; set; } = "dbo";
        public string Name { get; set; } = string.Empty;
        public List<ColumnInfo> Columns { get; set; } = new();
    }

    public class ScaffoldRequestModel
    {
        public string ConnectionString { get; set; } = string.Empty;
        public List<string> SelectedTables { get; set; } = new();
    }
}
