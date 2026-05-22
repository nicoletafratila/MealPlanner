using Common.Models;
using Identity.Api.Features.ContactUs.Commands.Send;
using Identity.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Identity.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContactUsController(ISender mediator) : ControllerBase
    {
        private readonly ISender _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));

        [HttpPost("send")]
        public async Task<CommandResponse?> SendAsync(
            [FromBody] ContactUsModel model,
            CancellationToken cancellationToken)
        {
            var command = new SendContactUsCommand { Model = model };
            return await _mediator.Send(command, cancellationToken);
        }
    }
}
