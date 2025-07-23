using AutoMapper;
using Duende.IdentityServer;
using Identity.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Identity.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize(IdentityServerConstants.LocalApi.PolicyName, Roles = "admin, member")]
    public class UserController : ControllerBase
    {
        private readonly UserManager<ApplicationUserModel> _userManager;
        private readonly IMapper _mapper;

        public UserController(UserManager<ApplicationUserModel> userManager, IMapper mapper)
        {
            _userManager = userManager;
            _mapper = mapper;
        }

        [HttpGet]
        [Authorize(IdentityServerConstants.LocalApi.PolicyName, Roles = "admin")]
        public IActionResult Get()
        {
            return Ok(_mapper.Map<IEnumerable<ApplicationUserModel>>(_userManager.Users.ToList()));
        }

        [HttpGet("{id}")]
        [Authorize(IdentityServerConstants.LocalApi.PolicyName, Roles = "admin, member")]
        public IActionResult Get(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest($"Bad request for the id = {id}");
            var user = _userManager.Users.FirstOrDefault(item => item.UserId == id);
            if (user == null)
                return BadRequest($"Bad request for the id = {id}");
            return Ok(_mapper.Map<ApplicationUserModel>(user));
        }

        [HttpPut]
        [Authorize(IdentityServerConstants.LocalApi.PolicyName, Roles = "admin")]
        public async Task<IActionResult> PutAsync(ApplicationUserModel user)
        {
            if (user == null)
                return BadRequest();

            var existingUser = _userManager.FindByIdAsync(user.UserId!).Result;
            if (existingUser == null)
            {
                return NotFound($"Could not find a user with id = {user.UserId}");
            }

            _mapper.Map(user, existingUser);
            await _userManager.UpdateAsync(existingUser);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(IdentityServerConstants.LocalApi.PolicyName, Roles = "admin")]
        public async Task<IActionResult> DeleteAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest();

            var existingUser = _userManager.FindByIdAsync(id).Result;
            if (existingUser == null)
            {
                return NotFound($"Could not find a user with id = {id}");
            }

            await _userManager.DeleteAsync(existingUser);
            return NoContent();
        }

    }
}