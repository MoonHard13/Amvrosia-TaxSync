using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using MoonHard.AmvrosiaTaxSync.Application.Contracts;
using MoonHard.AmvrosiaTaxSync.Domain.Models;

namespace MoonHard.AmvrosiaTaxSync.Infrastructure.SqlServer.SqlServer;

public sealed class SqlSchemaService(ILogger<SqlSchemaService> logger) : ISqlSchemaService
{
    private const string MetadataQuery = """
        SELECT s.name AS SchemaName,
               o.name AS ObjectName,
               CASE WHEN o.type = 'U' THEN N'Πίνακας' ELSE N'View' END AS ObjectKind,
               c.name AS ColumnName,
               t.name AS TypeName,
               c.is_nullable,
               CAST(CASE WHEN EXISTS (
                   SELECT 1 FROM sys.index_columns ic
                   JOIN sys.indexes i ON i.object_id = ic.object_id
                                      AND i.index_id = ic.index_id
                   WHERE i.is_primary_key = 1
                     AND ic.object_id = c.object_id
                     AND ic.column_id = c.column_id
               ) THEN 1 ELSE 0 END AS bit) AS IsPrimaryKey,
               CAST(CASE WHEN EXISTS (
                   SELECT 1 FROM sys.foreign_key_columns fkc
                   WHERE fkc.parent_object_id = c.object_id
                     AND fkc.parent_column_id = c.column_id
               ) THEN 1 ELSE 0 END AS bit) AS IsForeignKey
        FROM sys.objects o
        INNER JOIN sys.schemas s ON s.schema_id = o.schema_id
        INNER JOIN sys.columns c ON c.object_id = o.object_id
        INNER JOIN sys.types t ON t.user_type_id = c.user_type_id
        WHERE o.type IN ('U', 'V') AND o.is_ms_shipped = 0
        ORDER BY s.name, o.name, c.column_id;
        """;

    public async Task<DatabaseInfo> TestConnectionAsync(
        SqlConnectionOptions options, CancellationToken cancellationToken = default)
    {
        options.Validate();
        if (options.TrustServerCertificate)
            logger.LogWarning("SQL connection requested without server certificate identity verification.");

        await using var connection = new SqlConnection(BuildConnectionString(options));
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandTimeout = 30;
        command.CommandText = """
            SELECT DB_NAME(), CONVERT(nvarchar(128), SERVERPROPERTY('ProductVersion'));
            """;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            throw new InvalidOperationException("Ο SQL Server δεν επέστρεψε στοιχεία σύνδεσης.");

        var info = new DatabaseInfo(reader.GetString(0), reader.GetString(1));
        logger.LogInformation("SQL connection test completed successfully.");
        return info;
    }

    public async Task<IReadOnlyList<SchemaObject>> InspectAsync(
        SqlConnectionOptions options, CancellationToken cancellationToken = default)
    {
        options.Validate();
        if (options.TrustServerCertificate)
            logger.LogWarning("SQL schema inspection requested without server certificate identity verification.");

        await using var connection = new SqlConnection(BuildConnectionString(options));
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandTimeout = 60;
        command.CommandText = MetadataQuery;

        var collected = new Dictionary<(string Schema, string Name), (string Kind, List<SchemaColumn> Columns)>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            var key = (reader.GetString(0), reader.GetString(1));
            if (!collected.TryGetValue(key, out var entry))
            {
                entry = (reader.GetString(2), []);
                collected.Add(key, entry);
            }

            entry.Columns.Add(new SchemaColumn(
                reader.GetString(3),
                reader.GetString(4),
                reader.GetBoolean(5),
                reader.GetBoolean(6),
                reader.GetBoolean(7)));
        }

        var result = collected.Select(item => new SchemaObject(
            item.Key.Schema,
            item.Key.Name,
            item.Value.Kind,
            item.Value.Columns)).ToArray();

        logger.LogInformation("Read-only schema discovery completed; objects: {Count}.", result.Length);
        return result;
    }

    private static string BuildConnectionString(SqlConnectionOptions options)
    {
        var builder = new SqlConnectionStringBuilder
        {
            DataSource = options.Port == 0 ? options.Server.Trim() : $"{options.Server.Trim()},{options.Port}",
            InitialCatalog = options.Database.Trim(),
            IntegratedSecurity = options.WindowsAuthentication,
            Encrypt = SqlConnectionEncryptOption.Mandatory,
            TrustServerCertificate = options.TrustServerCertificate,
            ConnectTimeout = options.TimeoutSeconds,
            PersistSecurityInfo = false,
            ApplicationName = "Amvrosia TaxSync by MoonHard"
        };

        if (!options.WindowsAuthentication)
        {
            builder.UserID = options.Username;
            builder.Password = options.Password;
        }

        return builder.ConnectionString;
    }
}
