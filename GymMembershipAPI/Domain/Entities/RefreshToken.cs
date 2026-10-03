using System.ComponentModel.DataAnnotations;

namespace GymMembershipAPI.Domain.Entities;

public class RefreshToken
{
    [Key] public int Id { get; set; }
    public int UserId { get; set; }

    [Required] [MaxLength(256)] public string Token { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? RevokedAt { get; set; }

    public int? ReplacedByRefreshTokenId { get; set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsRevoked => RevokedAt != null;
    public bool IsActive => !IsExpired && !IsRevoked;

    public User User { get; set; } = null;
}