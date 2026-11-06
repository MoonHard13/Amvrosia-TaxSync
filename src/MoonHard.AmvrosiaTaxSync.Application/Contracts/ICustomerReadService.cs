using MoonHard.AmvrosiaTaxSync.Domain.Models;

namespace MoonHard.AmvrosiaTaxSync.Application.Contracts;

public interface ICustomerReadService
{
    Task<CustomerPage> LoadAsync(
        SqlConnectionOptions options,
        CustomerQuery query,
        CancellationToken cancellationToken = default);
}
