using Common.Models;
using Identity.Api.Features.ApplicationUser.Commands.Update;
using Identity.Api.Features.ApplicationUser.Queries.GetEdit;
using Identity.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Identity.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = Common.Constants.MealPlanner.PolicyScope, Roles = "admin,member")]
    public class ApplicationUserController(ISender mediator) : ControllerBase
    {
        [HttpGet("edit")]
        public async Task<ApplicationUserEditModel> GetEditAsync(string username)
        {
            GetEditQuery query = new()
            {
                Name = username
            };
            return await mediator.Send(query);
        }

        [HttpPut]
        public async Task<CommandResponse?> PutAsync(ApplicationUserEditModel model)
        {
            UpdateCommand command = new()
            {
                Model = model
            };
            return await mediator.Send(command);
        }

        //[HttpGet]
        //[Authorize(IdentityServerConstants.LocalApi.PolicyName, Roles = "admin")]
        //public IActionResult Get()
        //{
        //    return Ok(mapper.Map<IEnumerable<ApplicationUserEditModel>>(userManager.Users.ToList()));
        //}

        //[HttpGet("{id}")]
        //[Authorize(IdentityServerConstants.LocalApi.PolicyName, Roles = "admin, member")]
        //public IActionResult Get(string id)
        //{
        //    if (string.IsNullOrWhiteSpace(id))
        //        return BadRequest($"Bad request for the id = {id}");
        //    var user = userManager.Users.FirstOrDefault(item => item.UserId == id);
        //    if (user == null)
        //        return BadRequest($"Bad request for the id = {id}");
        //    return Ok(mapper.Map<ApplicationUserEditModel>(user));
        //}

        //[HttpPut]
        //[Authorize(IdentityServerConstants.LocalApi.PolicyName, Roles = "admin")]
        //public async Task<IActionResult> PutAsync(ApplicationUserEditModel user)
        //{
        //    if (user == null || string.IsNullOrWhiteSpace(user.UserId))
        //        return BadRequest();

        //    var existingUser = userManager.FindByIdAsync(user.UserId).Result;
        //    if (existingUser == null)
        //    {
        //        return NotFound($"Could not find a user with id = {user.UserId}");
        //    }

        //    mapper.Map(user, existingUser);
        //    await userManager.UpdateAsync(existingUser);
        //    return NoContent();
        //}

        //[HttpDelete("{id}")]
        //[Authorize(IdentityServerConstants.LocalApi.PolicyName, Roles = "admin")]
        //public async Task<IActionResult> DeleteAsync(string id)
        //{
        //    if (string.IsNullOrWhiteSpace(id))
        //        return BadRequest();

        //    var existingUser = userManager.FindByIdAsync(id).Result;
        //    if (existingUser == null)
        //    {
        //        return NotFound($"Could not find a user with id = {id}");
        //    }

        //    await userManager.DeleteAsync(existingUser);
        //    return NoContent();
        //}
    }
}