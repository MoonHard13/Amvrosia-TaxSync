using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using MoonHard.AmvrosiaTaxSync.Application.Contracts;
using MoonHard.AmvrosiaTaxSync.Application.Services;
using MoonHard.AmvrosiaTaxSync.Domain.Models;

namespace MoonHard.AmvrosiaTaxSync.Infrastructure.SqlServer.SqlServer;

public sealed class CustomerReadService(ILogger<CustomerReadService> logger) : ICustomerReadService
{
    private static readonly string[] RequiredColumns =
    [
        "CustCode", "CustTitle", "CustName", "CustAFM", "CustInactive", "CustomerOID"
    ];

    public async Task<CustomerPage> LoadAsync(
        SqlConnectionOptions options,
        CustomerQuery query,
        CancellationToken cancellationToken = default)
    {
        options.Validate();
        query.Validate();

        await using var connection = new SqlConnection(SqlConnectionFactory.BuildConnectionString(options));
        await connection.OpenAsync(cancellationToken);

        var schema = await ResolveSchemaAsync(connection, cancellationToken);
        var table = $"{Quote(schema)}.[TblSnCustomer]";
        var pattern = string.IsNullOrWhiteSpace(query.Search)
            ? null
            : $"%{EscapeLike(query.Search.Trim())}%";
        var afmText = "NULLIF(LTRIM(RTRIM(CONVERT(nvarchar(40), [CustAFM]))), N'')";

        // Η μοναδική δυναμική SQL ονομασία προέρχεται από το SQL Server catalog.
        var filter = $"""
            (@Search IS NULL OR
             CONVERT(nvarchar(100), [CustCode]) LIKE @Search ESCAPE N'~' OR
             CONVERT(nvarchar(400), [CustTitle]) LIKE @Search ESCAPE N'~' OR
             CONVERT(nvarchar(400), [CustName]) LIKE @Search ESCAPE N'~' OR
             CONVERT(nvarchar(40), [CustAFM]) LIKE @Search ESCAPE N'~')
            AND (@OnlyWithAfm = 0 OR {afmText} IS NOT NULL)
            """;

        var statsSql = $"""
            SELECT COUNT_BIG(*),
                   COALESCE(SUM(CASE WHEN {afmText} IS NOT NULL
                                     THEN CONVERT(bigint, 1) ELSE CONVERT(bigint, 0) END), 0),
                   COALESCE(SUM(CASE WHEN {afmText} IS NULL
                                     THEN CONVERT(bigint, 1) ELSE CONVERT(bigint, 0) END), 0)
            FROM {table};
            """;

        CustomerTotals totals;
        await using (var stats = connection.CreateCommand())
        {
            stats.CommandText = statsSql;
            stats.CommandTimeout = 60;
            await using var reader = await stats.ExecuteReaderAsync(cancellationToken);
            if (!await reader.ReadAsync(cancellationToken))
                throw new DataException("Ο SQL Server δεν επέστρεψε σύνολα πελατών.");
            totals = new CustomerTotals(reader.GetInt64(0), reader.GetInt64(1), reader.GetInt64(2));
        }

        long matched;
        await using (var count = connection.CreateCommand())
        {
            count.CommandText = $"SELECT COUNT_BIG(*) FROM {table} WHERE {filter};";
            count.CommandTimeout = 60;
            AddSearchParameter(count, pattern);
            count.Parameters.Add("@OnlyWithAfm", SqlDbType.Bit).Value = query.OnlyWithAfm;
            matched = Convert.ToInt64(await count.ExecuteScalarAsync(cancellationToken));
        }

        var results = new List<CustomerRecord>(query.PageSize);
        var offset = ((long)query.Page - 1L) * query.PageSize;
        if (offset < matched)
        {
            await using var command = connection.CreateCommand();
            command.CommandTimeout = 60;
            command.CommandText = $"""
                SELECT CONVERT(nvarchar(100), [CustCode]),
                       CONVERT(nvarchar(400), [CustTitle]),
                       CONVERT(nvarchar(400), [CustName]),
                       CONVERT(nvarchar(40), [CustAFM]),
                       TRY_CONVERT(int, [CustInactive])
                FROM {table}
                WHERE {filter}
                ORDER BY [CustCode], [CustomerOID]
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
                """;
            AddSearchParameter(command, pattern);
            command.Parameters.Add("@OnlyWithAfm", SqlDbType.Bit).Value = query.OnlyWithAfm;
            command.Parameters.Add("@Offset", SqlDbType.BigInt).Value = offset;
            command.Parameters.Add("@PageSize", SqlDbType.Int).Value = query.PageSize;

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                var code = GetText(reader, 0);
                var title = GetText(reader, 1);
                var name = GetText(reader, 2);
                var afm = GetText(reader, 3).Trim();

                results.Add(new CustomerRecord(
                    code, title, name, afm,
                    !reader.IsDBNull(4) && reader.GetInt32(4) != 0,
                    GreekAfmValidator.Check(afm)));
            }
        }

        logger.LogInformation(
            "Customer listing completed. Page={Page} PageSize={PageSize} Total={Total} Matched={Matched} OnlyWithAfm={OnlyWithAfm}.",
            query.Page, query.PageSize, totals.Total, matched, query.OnlyWithAfm);

        return new CustomerPage(results, totals, matched, query.Page, query.PageSize);
    }

    private static async Task<string> ResolveSchemaAsync(
        SqlConnection connection,
        CancellationToken cancellationToken)
    {
        await using var tableQuery = connection.CreateCommand();
        tableQuery.CommandText = """
            SELECT TOP (2) s.name
            FROM sys.tables t
            INNER JOIN sys.schemas s ON s.schema_id = t.schema_id
            WHERE t.name = N'TblSnCustomer'
            ORDER BY CASE WHEN s.name = N'dbo' THEN 0 ELSE 1 END, s.name;
            """;

        var schemas = new List<string>();
        await using (var reader = await tableQuery.ExecuteReaderAsync(cancellationToken))
        {
            while (await reader.ReadAsync(cancellationToken))
                schemas.Add(reader.GetString(0));
        }

        if (schemas.Count == 0)
            throw new InvalidOperationException("Δεν βρέθηκε πίνακας TblSnCustomer στη συνδεδεμένη βάση.");
        if (schemas.Count > 1)
            throw new InvalidOperationException("Βρέθηκαν πολλαπλοί πίνακες TblSnCustomer σε διαφορετικά schemas.");

        await using var columns = connection.CreateCommand();
        columns.CommandText = """
            SELECT c.name
            FROM sys.columns c
            INNER JOIN sys.tables t ON t.object_id = c.object_id
            INNER JOIN sys.schemas s ON s.schema_id = t.schema_id
            WHERE t.name = N'TblSnCustomer' AND s.name = @Schema;
            """;
        columns.Parameters.Add("@Schema", SqlDbType.NVarChar, 128).Value = schemas[0];

        var discovered = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        await using (var reader = await columns.ExecuteReaderAsync(cancellationToken))
        {
            while (await reader.ReadAsync(cancellationToken))
                discovered.Add(reader.GetString(0));
        }

        var missing = RequiredColumns.Where(column => !discovered.Contains(column)).ToArray();
        if (missing.Length > 0)
            throw new InvalidOperationException(
                $"Ο TblSnCustomer δεν περιέχει απαραίτητα πεδία: {string.Join(", ", missing)}.");

        return schemas[0];
    }

    private static string Quote(string identifier) =>
        $"[{identifier.Replace("]", "]]", StringComparison.Ordinal)}]";

    private static string EscapeLike(string value) =>
        value.Replace("~", "~~", StringComparison.Ordinal)
             .Replace("%", "~%", StringComparison.Ordinal)
             .Replace("_", "~_", StringComparison.Ordinal)
             .Replace("[", "~[", StringComparison.Ordinal);

    private static void AddSearchParameter(SqlCommand command, string? pattern) =>
        command.Parameters.Add("@Search", SqlDbType.NVarChar, 204).Value =
            pattern is null ? DBNull.Value : pattern;

    private static string GetText(SqlDataReader reader, int ordinal) =>
        reader.IsDBNull(ordinal) ? "" : reader.GetString(ordinal).Trim();
}
