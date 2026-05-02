namespace NorthwindTraders.Application.DTOs.Auth;

// What the server sends back after a successful login
public class LoginResponseDto
{
    // The JWT token — client must send this in every future request
    // Authorization: Bearer {Token}
    public string Token { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}