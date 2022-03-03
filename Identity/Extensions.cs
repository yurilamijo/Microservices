using Identity.Entities;
using static Identity.Dtos;

namespace Identity
{
    public static class Extensions
    {
        public static UserDto AsDto(this ApplicationUser user)
        {
            return new UserDto(user.Id, user.UserName, user.Email, user.Points, user.CreatedOn);
        }
    }
}
