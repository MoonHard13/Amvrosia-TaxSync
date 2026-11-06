using System.Text.Json;
using Microsoft.Extensions.Logging;
using MoonHard.AmvrosiaTaxSync.Domain.Models;
using MoonHard.AmvrosiaTaxSync.Infrastructure.SqlServer.SqlServer;

namespace MoonHard.AmvrosiaTaxSync.Cli;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        if (args.Length == 0 || args[0] is "--help" or "-h" or "help")
        {
            PrintHelp();
            return 0;
        }

        if (args[0] is not ("test" or "scan"))
        {
            Console.Error.WriteLine("Άγνωστη εντολή. Δες --help.");
            return 2;
        }

        try
        {
            var parameters = ParseParameters(args.Skip(1).ToArray());
            var auth = parameters.GetValueOrDefault("auth", "windows");
            if (auth is not ("windows" or "sql"))
                throw new ArgumentException("Το --auth πρέπει να είναι windows ή sql.");

            var options = new SqlConnectionOptions(
                parameters.GetValueOrDefault("server", ""),
                parameters.GetValueOrDefault("database", ""),
                auth == "windows",
                parameters.GetValueOrDefault("user"),
                Environment.GetEnvironmentVariable("AMVROSIA_SQL_PASSWORD"),
                int.Parse(parameters.GetValueOrDefault("port", "0")),
                int.Parse(parameters.GetValueOrDefault("timeout", "15")));

            options.Validate();

            using var loggerFactory = LoggerFactory.Create(_ => { });
            var service = new SqlSchemaService(loggerFactory.CreateLogger<SqlSchemaService>());
            using var cancellation = new CancellationTokenSource();
            Console.CancelKeyPress += (_, eventArgs) =>
            {
                eventArgs.Cancel = true;
                cancellation.Cancel();
            };

            if (args[0] == "test")
            {
                var result = await service.TestConnectionAsync(options, cancellation.Token);
                Console.WriteLine($"Επιτυχής σύνδεση στη βάση {result.Name}. SQL Server: {result.ServerVersion}");
                return 0;
            }

            var schema = await service.InspectAsync(options, cancellation.Token);
            Console.WriteLine(JsonSerializer.Serialize(schema, new JsonSerializerOptions { WriteIndented = true }));
            return 0;
        }
        catch (OperationCanceledException)
        {
            Console.Error.WriteLine("Η διαδικασία ακυρώθηκε.");
            return 3;
        }
        catch (Exception ex) when (ex is ArgumentException or FormatException or OverflowException)
        {
            Console.Error.WriteLine(ex.Message);
            return 2;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Αποτυχία: {ex.GetType().Name}: {ex.Message}");
            return 1;
        }
    }

    private static Dictionary<string, string> ParseParameters(string[] args)
    {
        var parsed = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        for (var index = 0; index < args.Length; index += 2)
        {
            if (!args[index].StartsWith("--", StringComparison.Ordinal) || index + 1 >= args.Length)
                throw new ArgumentException("Οι επιλογές πρέπει να έχουν μορφή --όνομα τιμή.");

            var key = args[index][2..];
            if (key is not ("server" or "database" or "auth" or "user" or "port" or "timeout"))
                throw new ArgumentException($"Άγνωστη παράμετρος: --{key}");

            if (!parsed.TryAdd(key, args[index + 1]))
                throw new ArgumentException($"Διπλή παράμετρος: --{key}");
        }

        return parsed;
    }

    private static void PrintHelp()
    {
        Console.WriteLine("""
            Amvrosia TaxSync by MoonHard — CLI
              test --server SERVER --database DB [--auth windows|sql] [--user USER] [--port PORT]
              scan --server SERVER --database DB [--auth windows|sql] [--user USER] [--port PORT]

            Η εντολή scan διαβάζει αποκλειστικά SQL metadata και επιστρέφει JSON.
            Για SQL Authentication όρισε AMVROSIA_SQL_PASSWORD στο περιβάλλον.
            Κανένας κωδικός δεν περνά ως command-line argument.
            Exit codes: 0 επιτυχία, 1 σφάλμα, 2 παράμετροι, 3 ακύρωση.
            """);
    }
}
