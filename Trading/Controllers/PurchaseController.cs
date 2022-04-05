using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Trading.Contracts;
using Trading.StateMachines;

namespace Trading.Controllers
{
    [ApiController]
    [Route("purchase")]
    [Authorize]
    public class PurchaseController : ControllerBase
    {
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IRequestClient<GetPurchaseState> _purchaseClient;

        public PurchaseController(IPublishEndpoint publishEndpoint, IRequestClient<GetPurchaseState> purchaseClient)
        {
            this._publishEndpoint = publishEndpoint;
            this._purchaseClient = purchaseClient;
        }

        [HttpGet("status/{idempotencyId}")]
        public async Task<ActionResult<PurchaseDto>> GetStatusAsync(Guid idempotencyId)
        {
            var response = await _purchaseClient.GetResponse<PurchaseState>(
                new GetPurchaseState(idempotencyId)
            );

            var purchaseState = response.Message;

            var purchase = new PurchaseDto(
                purchaseState.UserId,
                purchaseState.ItemId,
                purchaseState.PurchaseTotal,
                purchaseState.Quantity,
                purchaseState.CurrentState,
                purchaseState.ErrorMessage,
                purchaseState.Received,
                purchaseState.LastUpdated
            );

            return Ok(purchase);
        }

        [HttpPost]
        public async Task<IActionResult> PostAsync(SubmitPurchaseDto purchaseDto)
        {
            var userId = User.FindFirstValue("sub");

            var message = new PurchaseRequested(
                    Guid.Parse(userId),
                    purchaseDto.ItemId.Value,
                    purchaseDto.IdempotencyId.Value,
                    purchaseDto.Quantity
                );

            await _publishEndpoint.Publish(message);

            return AcceptedAtAction(nameof(GetStatusAsync), new { purchaseDto.IdempotencyId }, new { purchaseDto.IdempotencyId });
        }
    }
}
