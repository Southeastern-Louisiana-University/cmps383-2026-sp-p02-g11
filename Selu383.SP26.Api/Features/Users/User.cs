namespace Selu383.SP26.Api.Features.Users;

public class User
{
    public int Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string HashedPassword { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public Role? Role { get; set; }
}