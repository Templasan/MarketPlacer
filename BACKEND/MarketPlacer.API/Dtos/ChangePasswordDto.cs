namespace MarketPlacer.API.Dtos;

public class ChangePasswordDto
{
    [System.Text.Json.Serialization.JsonPropertyName("currentPassword")]
    public string CurrentPassword { get; set; } = string.Empty;

    [System.Text.Json.Serialization.JsonPropertyName("newPassword")]
    public string NewPassword { get; set; } = string.Empty;
}