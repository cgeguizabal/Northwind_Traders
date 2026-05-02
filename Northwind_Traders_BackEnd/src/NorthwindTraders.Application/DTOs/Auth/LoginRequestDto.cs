namespace NorthwindTraders.Application.DTOs.Auth;

// What the client sends to log in
public class LoginRequestDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}