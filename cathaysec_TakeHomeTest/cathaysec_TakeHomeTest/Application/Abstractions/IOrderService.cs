using CathaySec.Api.Models;

namespace CathaySec.Api.Application;

public interface IOrderService
{
    Task<OrderResponse> CreateAsync(CreateOrderRequest request, CancellationToken cancellationToken);
    Task<OrderResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}
