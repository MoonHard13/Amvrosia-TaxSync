using Microsoft.Data.SqlClient;
using MoonHard.AmvrosiaTaxSync.Domain.Models;

namespace MoonHard.AmvrosiaTaxSync.Infrastructure.SqlServer.SqlServer;

public static class SqlConnectionFactory
{
    public static string BuildConnectionString(SqlConnectionOptions options)
    {
        options.Validate();

        var builder = new SqlConnectionStringBuilder
        {
            DataSource = options.Port == 0
                ? options.Server.Trim()
                : $"{options.Server.Trim()},{options.Port}",
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
