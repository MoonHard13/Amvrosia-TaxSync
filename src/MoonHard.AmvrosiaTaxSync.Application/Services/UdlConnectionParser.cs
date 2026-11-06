using System.Data.Common;
using MoonHard.AmvrosiaTaxSync.Domain.Models;

namespace MoonHard.AmvrosiaTaxSync.Application.Services;

public sealed record UdlImportResult(SqlConnectionOptions Options, bool ContainsPassword);

public static class UdlConnectionParser
{
    public static UdlImportResult Parse(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new FormatException("Το αρχείο UDL είναι κενό.");

        var lines = text.TrimStart('\uFEFF').Replace("\r", "").Split('\n');
        if (lines.Length < 2 || !string.Equals(lines[0].Trim(), "[oledb]", StringComparison.OrdinalIgnoreCase))
            throw new FormatException("Το επιλεγμένο αρχείο δεν έχει έγκυρη μορφή UDL.");

        var connectionText = string.Join("", lines.Skip(1)
            .Select(line => line.Trim())
            .Where(line => line.Length > 0 && !line.StartsWith(';')));

        DbConnectionStringBuilder builder = new();
        try
        {
            builder.ConnectionString = connectionText;
        }
        catch (ArgumentException)
        {
            throw new FormatException("Η σύνταξη των στοιχείων σύνδεσης UDL δεν αναγνωρίζεται.");
        }

        var provider = Read(builder, "Provider");
        if (string.IsNullOrWhiteSpace(provider) ||
            !(provider.StartsWith("SQLOLEDB", StringComparison.OrdinalIgnoreCase) ||
              provider.StartsWith("SQLNCLI", StringComparison.OrdinalIgnoreCase) ||
              provider.StartsWith("MSOLEDBSQL", StringComparison.OrdinalIgnoreCase)))
        {
            throw new NotSupportedException(
                "Το UDL πρέπει να χρησιμοποιεί SQL Server OLE DB provider (SQLOLEDB, SQLNCLI ή MSOLEDBSQL).");
        }

        var server = Read(builder, "Data Source", "Server", "Address");
        var database = Read(builder, "Initial Catalog", "Database");
        var username = Read(builder, "User ID", "UID");
        var password = Read(builder, "Password", "PWD");

        if (string.IsNullOrWhiteSpace(server) || string.IsNullOrWhiteSpace(database))
            throw new FormatException("Το UDL πρέπει να περιέχει Data Source και Initial Catalog.");

        var authText = Read(builder, "Integrated Security", "Trusted_Connection");
        var windowsAuth = authText is null
            ? string.IsNullOrWhiteSpace(username)
            : authText.Equals("SSPI", StringComparison.OrdinalIgnoreCase) ||
              authText.Equals("true", StringComparison.OrdinalIgnoreCase) ||
              authText.Equals("yes", StringComparison.OrdinalIgnoreCase) ||
              authText == "1";

        if (authText is not null &&
            !(windowsAuth || authText.Equals("false", StringComparison.OrdinalIgnoreCase) ||
              authText.Equals("no", StringComparison.OrdinalIgnoreCase) ||
              authText == "0"))
            throw new FormatException("Μη αναγνωρίσιμη επιλογή Integrated Security στο UDL.");

        var port = 0;
        var delimiter = server.LastIndexOf(',');
        if (delimiter > 0 && int.TryParse(server[(delimiter + 1)..].Trim(), out var parsedPort))
        {
            port = parsedPort;
            server = server[..delimiter].Trim();
        }

        var timeoutText = Read(builder, "Connect Timeout", "Connection Timeout", "Timeout");
        var timeout = 15;
        if (!string.IsNullOrEmpty(timeoutText) && !int.TryParse(timeoutText, out timeout))
            throw new FormatException("Μη έγκυρο Connect Timeout στο UDL.");

        // Το UDL επιτρέπεται να μην περιέχει password, ώστε να εισαχθεί από τον χειριστή.
        var options = new SqlConnectionOptions(server, database, windowsAuth,
            username, password, port, timeout);

        if (port is < 0 or > 65535 || timeout is < 1 or > 120)
            throw new FormatException("Το UDL περιέχει μη έγκυρη θύρα ή timeout.");

        return new UdlImportResult(options, !string.IsNullOrEmpty(password));
    }

    private static string? Read(DbConnectionStringBuilder values, params string[] keys)
    {
        foreach (var key in keys)
        {
            if (values.TryGetValue(key, out var value))
                return value?.ToString();
        }

        return null;
    }
}
