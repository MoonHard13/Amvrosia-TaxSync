using System.Text.Json;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using MoonHard.AmvrosiaTaxSync.Infrastructure.SqlServer.Diagnostics;
using MoonHard.AmvrosiaTaxSync.Application.Services;
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

        if (args[0] is not ("test" or "scan" or "customers"))
        {
            Console.Error.WriteLine("Άγνωστη εντολή. Δες --help.");
            return 2;
        }

        using var cancellation = new CancellationTokenSource();
        Console.CancelKeyPress += (_, eventArgs) =>
        {
            eventArgs.Cancel = true;
            cancellation.Cancel();
        };

        try
        {
            var parameters = ParseParameters(args.Skip(1).ToArray());
            SqlConnectionOptions options;
            var overrideCertificate = true;
            if (parameters.TryGetValue("trust-server-certificate", out var overrideText) &&
                !bool.TryParse(overrideText, out overrideCertificate))
                throw new ArgumentException("Το --trust-server-certificate πρέπει να είναι true ή false.");

            if (parameters.TryGetValue("udl", out var udlPath))
            {
                if (parameters.Keys.Any(key => key is not ("udl" or "trust-server-certificate" or "search" or "page" or "page-size")))
                    throw new ArgumentException("Με --udl επιτρέπονται μόνο --search, --page, --page-size και --trust-server-certificate.");

                var imported = await new UdlConnectionFileReader().LoadAsync(udlPath, cancellation.Token);
                options = imported.Options with
                {
                    Password = Environment.GetEnvironmentVariable("AMVROSIA_SQL_PASSWORD")
                               ?? imported.Options.Password
                };
            }
            else
            {
                var auth = parameters.GetValueOrDefault("auth", "windows");
                if (auth is not ("windows" or "sql"))
                    throw new ArgumentException("Το --auth πρέπει να είναι windows ή sql.");

                options = new SqlConnectionOptions(
                    parameters.GetValueOrDefault("server", ""),
                    parameters.GetValueOrDefault("database", ""),
                    auth == "windows",
                    parameters.GetValueOrDefault("user"),
                    Environment.GetEnvironmentVariable("AMVROSIA_SQL_PASSWORD"),
                    int.Parse(parameters.GetValueOrDefault("port", "0")),
                    int.Parse(parameters.GetValueOrDefault("timeout", "15")));
            }

            options = options with { TrustServerCertificate = overrideCertificate };
            options.Validate();

            if (parameters.ContainsKey("trust-server-certificate") && overrideCertificate)
            {
                Console.Error.WriteLine(
                    "ΠΡΟΕΙΔΟΠΟΙΗΣΗ: Παράκαμψη επαλήθευσης πιστοποιητικού SQL Server. " +
                    "Μόνο για εξουσιοδοτημένη δοκιμή σε γνωστό server. Η σύνδεση εξακολουθεί να κρυπτογραφείται.");
            }

            using var loggerFactory = LoggerFactory.Create(_ => { });
            var service = new SqlSchemaService(loggerFactory.CreateLogger<SqlSchemaService>());

            if (args[0] == "test")
            {
                var result = await service.TestConnectionAsync(options, cancellation.Token);
                Console.WriteLine($"Επιτυχής σύνδεση στη βάση {result.Name}. SQL Server: {result.ServerVersion}");
                return 0;
            }

            if (args[0] == "customers")
            {
                var query = new CustomerQuery(
                    parameters.GetValueOrDefault("search", ""),
                    int.Parse(parameters.GetValueOrDefault("page", "1")),
                    int.Parse(parameters.GetValueOrDefault("page-size", "100")));

                var reader = new CustomerReadService(loggerFactory.CreateLogger<CustomerReadService>());
                var page = await reader.LoadAsync(options, query, cancellation.Token);
                Console.WriteLine(JsonSerializer.Serialize(page, new JsonSerializerOptions { WriteIndented = true }));
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
        catch (SqlException ex)
        {
            var diagnostic = SqlConnectionErrorFormatter.Diagnose(ex);
            Console.Error.WriteLine($"{diagnostic.Title} {diagnostic.Suggestion}");
            Console.Error.WriteLine(diagnostic.TechnicalDetails);
            return 1;
        }
        catch (Exception ex) when (ex is ArgumentException or FormatException or OverflowException or NotSupportedException)
        {
            Console.Error.WriteLine(ex.Message);
            return 2;
        }
        catch (Exception ex)
        {
            // Αποφεύγουμε λεπτομερείς εξαιρέσεις που μπορεί να εκθέτουν δεδομένα UDL.
            Console.Error.WriteLine($"Αποτυχία λειτουργίας: {ex.GetType().Name}");
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
            if (key is not ("server" or "database" or "auth" or "user" or "port" or "timeout" or "udl" or "trust-server-certificate" or "search" or "page" or "page-size"))
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
              test --udl "C:\Data\connection.udl"
              scan --udl "C:\Data\connection.udl"
              customers --udl "C:\Data\connection.udl" [--search TERM] [--page 1] [--page-size 100]
              customers --server SERVER --database DB [--auth windows|sql] [--user USER] [--search TERM]
              test --udl "C:\Data\connection.udl" --trust-server-certificate false

            Η εντολή scan διαβάζει αποκλειστικά SQL metadata και επιστρέφει JSON.
            Η εντολή customers διαβάζει TblSnCustomer χωρίς αλλαγές και επιστρέφει μία σελίδα JSON.
            Τα --search, --page, --page-size χρησιμοποιούνται μόνο με customers (10-500 εγγραφές).
            Το --udl υποστηρίζει OLE DB UDL SQL Server και UTF-16/UTF-8.
            Για SQL Authentication μπορείς να ορίσεις AMVROSIA_SQL_PASSWORD.
            Προεπιλογή: Encrypt=Mandatory, TrustServerCertificate=True.
            --trust-server-certificate false: επαλήθευση πιστοποιητικού SQL Server.
            Κανένας κωδικός δεν περνά ως command-line argument.
            Exit codes: 0 επιτυχία, 1 σφάλμα, 2 παράμετροι, 3 ακύρωση.
            """);
    }
}
