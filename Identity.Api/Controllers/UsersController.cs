using AutoMapper;
using Common.ViewModels;
using Identity.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using static Duende.IdentityServer.IdentityServerConstants;

namespace Identity.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize(LocalApi.PolicyName, Roles = "admin, member")]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;

        public UsersController(UserManager<ApplicationUser> userManager, IMapper mapper)
        {
            _userManager = userManager;
            _mapper = mapper;
        }

        [HttpGet]
        [Authorize(LocalApi.PolicyName, Roles = "admin")]
        public IActionResult Get()
        {
            return Ok(_mapper.Map<IEnumerable<ApplicationUserModel>>(_userManager.Users.ToList()));
        }

        [HttpGet("{id}")]
        [Authorize(LocalApi.PolicyName, Roles = "admin, member")]
        public IActionResult Get(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest($"Bad request for the id = {id}");
            var user = _userManager.Users.FirstOrDefault(item => item.Id == id);
            if (user == null)
                return BadRequest($"Bad request for the id = {id}");
            return Ok(_mapper.Map<ApplicationUserModel>(user));
        }

        [HttpPut]
        [Authorize(LocalApi.PolicyName, Roles = "admin")]
        public async Task<IActionResult> UpdateAsync(ApplicationUserModel user)
        {
            if (user == null)
                return BadRequest();

            var existingUser = _userManager.FindByIdAsync(user.UserId).Result;
            if (existingUser == null)
            {
                return NotFound($"Could not find a user with id = {user.UserId}");
            }

            _mapper.Map(user, existingUser);
            await _userManager.UpdateAsync(existingUser);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(LocalApi.PolicyName, Roles = "admin")]
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