namespace NorthwindTraders.Application.DTOs.Auth;

public class ChangePasswordRequestDto
{
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}
