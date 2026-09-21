using GymMembershipAPI.Domain.Enums;

namespace GymMembershipAPI.Domain.Entities;

public class User
{
    private User(string email, string passwordHash, UserRole role, int? memberId = null)
    {
        Email = email;
        PasswordHash = passwordHash;
        Role = role;
        MemberId = memberId;
    }

    public int Id { get; set; }
    public Guid PublicId { get; set; } = Guid.NewGuid();
    public string Email { get; set; }
    public string PasswordHash { get; set; }

    public UserRole Role { get; set; }

    public int? MemberId { get; set; }
    public Member? Member { get; set; }

    //Factories
    public static User CreateMemberUser(string email, string passwordHash, int memberId) 
        => new (email, passwordHash, UserRole.Member, memberId);

    public static User CreateStaffUser(string email, string passwordHash, UserRole role)
    {
        if (role != UserRole.Admin && role != UserRole.Staff)
            throw new ArgumentException("Solo se pueden crear usuarios de Staff o Admin sin memberID");

        return new User(email, passwordHash, role, null);
    }
}