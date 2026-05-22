using Common.Models;
using Identity.Shared.Models;
using MediatR;

namespace Identity.Api.Features.ContactUs.Commands.Send
{
    public class SendContactUsCommand : IRequest<CommandResponse?>
    {
        public ContactUsModel? Model { get; set; }
    }
}
