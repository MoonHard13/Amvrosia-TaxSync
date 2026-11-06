using MoonHard.AmvrosiaTaxSync.Domain.Models;

namespace MoonHard.AmvrosiaTaxSync.Application.Contracts;

public interface ISqlSchemaService
{
    Task<DatabaseInfo> TestConnectionAsync(
        SqlConnectionOptions options, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SchemaObject>> InspectAsync(
        SqlConnectionOptions options, CancellationToken cancellationToken = default);
}
