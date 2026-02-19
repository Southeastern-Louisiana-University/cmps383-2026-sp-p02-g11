namespace Selu383.SP26.Api.Features.Users;

public class UserDto
{
    public string UserName { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
    public int Id { get; set; }
}