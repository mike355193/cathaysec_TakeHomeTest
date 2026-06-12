using CathaySec.Api.Domain;

namespace CathaySec.Api.Application;

public interface IStockMasterDataClient
{
    Task<IReadOnlyList<Stock>> GetListedStocksAsync(CancellationToken cancellationToken);
}
