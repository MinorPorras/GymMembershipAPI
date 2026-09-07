namespace GymMembershipAPI.Domain.Entities;

public class RegisterAccess
{
    public int Id { get; set; }
    public Guid PublicId { get; set; } = Guid.NewGuid();
    public int MemberId { get; set; }
    public Member Member { get; set; }
    
    public DateTime AccessDate { get; set; }
    public bool AllowAccess { get; set; }
}