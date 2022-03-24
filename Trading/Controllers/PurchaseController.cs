using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Trading.Contracts;

namespace Trading.Controllers
{
    [ApiController]
    [Route("purchase")]
    [Authorize]
    public class PurchaseController : ControllerBase
    {
        private readonly IPublishEndpoint _publishEndpoint;

        public PurchaseController(IPublishEndpoint publishEndpoint)
        {
            this._publishEndpoint = publishEndpoint;
        }

        [HttpPost]
        public async Task<IActionResult> PostAsync(SubmitPurchaseDto purchaseDto)
        {
            var userId = User.FindFirstValue("sub");
            var correlationId = Guid.NewGuid();

            var message = new PurchaseRequested(
                    Guid.Parse(userId),
                    purchaseDto.ItemId.Value,
                    correlationId,
                    purchaseDto.Quantity
                );

            await _publishEndpoint.Publish(message);

            return Accepted();
        }
    }
}
