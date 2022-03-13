using Identity.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using static Identity.Dtos;
using static IdentityServer4.IdentityServerConstants;

namespace Identity.Controllers
{
    [ApiController]
    [Route("users")]
    [Authorize(Policy = LocalApi.PolicyName, Roles = Roles.Admin)]
    public class UserController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [HttpGet]
        public ActionResult<IEnumerable<UserDto>> Get()
        {
            var users = _userManager.Users
                .ToList()
                .Select(User => User.AsDto());

            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetByIdAsync(Guid id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());

            if (user == null)
            {
                return NotFound();
            }

            return user.AsDto();
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<UserDto>> PutAsync(Guid id, UpdateUserDto userDto)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());

            if (user == null)
            {
                return NotFound();
            }

            user.Email = userDto.Email;
            user.UserName = userDto.Email;
            user.Points = userDto.Points;

            await _userManager.UpdateAsync(user);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<UserDto>> DeleteAsync(Guid id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());

            if (user == null)
            {
                return NotFound();
            }

            await _userManager.DeleteAsync(user);

            return NoContent();
        }
    }
}
