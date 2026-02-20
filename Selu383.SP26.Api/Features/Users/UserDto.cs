using System.ComponentModel.DataAnnotations;

namespace Selu383.SP26.Api.Features.Users;

public class UserDto
{
	public int Id { get; set; }
	[Required]
	public string UserName { get; set; } = string.Empty;
	[Required]
	public required string[] Roles { get; set; }
}