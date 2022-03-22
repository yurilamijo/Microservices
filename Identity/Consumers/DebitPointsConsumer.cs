using Identity.Entities;
using Identity.Exceptions;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using static Contracts.IdentityContracts;

namespace Identity.Consumers
{
    public class DebitPointsConsumer : IConsumer<DebitPoints>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public DebitPointsConsumer(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task Consume(ConsumeContext<DebitPoints> context)
        {
            var message = context.Message;

            var user = await _userManager.FindByIdAsync(message.UserId.ToString());

            if (user == null)
            {
                throw new UnknownUserException(message.UserId);
            }

            if (user.Points - message.Points < 0)
            {
                throw new InsufficientFundsException(message.UserId, message.Points);
            }

            user.Points -= message.Points;

            await _userManager.UpdateAsync(user);

            await context.Publish(new PointsDebited(message.CorrelationId));
        }
    }
}
