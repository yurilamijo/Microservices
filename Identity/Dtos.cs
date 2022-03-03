using System.ComponentModel.DataAnnotations;

namespace Identity
{
    public class Dtos
    {
        public record UserDto(
            Guid Id, 
            string 
            Username, 
            string Email, 
            decimal Points, 
            DateTimeOffset CreatedDate
        );
    
        public record UpdateUserDto(
            [Required][EmailAddress]string Email, 
            [Range(0, 1000)]decimal Points
        );
    }
}
