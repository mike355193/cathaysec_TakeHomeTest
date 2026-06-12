using CathaySec.Api.Application;
using CathaySec.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;
using CathaySec.Api.Swagger.Examples;

namespace CathaySec.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/orders")]
public sealed class OrdersController(IOrderService orders) : ControllerBase
{
    /// <summary>
    /// 建立股票買進或賣出委託單。
    /// </summary>
    /// <remarks>
    /// 建立股票買進或賣出委託；股票代號必須存在於股票清單。
    /// </remarks>
    /// <param name="request">股票代號、買賣方向、委託價格與委託股數。</param>
    /// <param name="cancellationToken">HTTP 請求取消權杖。</param>
    /// <returns>建立完成的委託單。</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<OrderResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [SwaggerRequestExample(typeof(CreateOrderRequest), typeof(CreateOrderRequestExample))]
    [SwaggerResponseExample(StatusCodes.Status201Created, typeof(CreateOrderResponseExample))]
    [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(OrderValidationErrorResponseExample))]
    public async Task<ActionResult<OrderResponse>> Create(
        CreateOrderRequest request, CancellationToken cancellationToken)
    {
        var order = await orders.CreateAsync(request, cancellationToken);
        Response.Headers.Location = Url.ActionLink(nameof(GetById), values: new { id = order.Id });
        return StatusCode(StatusCodes.Status201Created, order);
    }

    /// <summary>
    /// 依委託單識別碼查詢委託內容與狀態。
    /// </summary>
    /// <remarks>
    /// 使用委託單唯一識別碼查詢股票、方向、價格、股數與目前狀態。
    /// </remarks>
    /// <param name="id">委託單唯一識別碼，格式為 UUID，例如 11111111-1111-1111-1111-111111111111。</param>
    /// <param name="cancellationToken">HTTP 請求取消權杖。</param>
    /// <returns>指定委託單的完整資料。</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<OrderResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(GetOrderResponseExample))]
    [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(OrderNotFoundResponseExample))]
    public async Task<ActionResult<OrderResponse>> GetById(
        Guid id, CancellationToken cancellationToken) =>
        Ok(await orders.GetByIdAsync(id, cancellationToken));
}
