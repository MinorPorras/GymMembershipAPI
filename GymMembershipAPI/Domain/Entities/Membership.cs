namespace GymMembershipAPI.Domain.Entities;

public class Membership
{
    public Membership(){}
    public Membership(int memberId, int MembershipTypeId, DateTime startDate, DateTime endDate, bool isActive)
    {
        MemberId = memberId;
        this.MembershipTypeId = MembershipTypeId;
        StartDate = startDate;
        EndDate = endDate;
        IsActive = isActive;
    }
    
    public int Id { get; set; }
    public Guid PublicId { get; set; } = Guid.NewGuid();

    public int MemberId { get; set; }
    public Member? Member { get; set; } = null;

    public int MembershipTypeId { get; set; }
    public MembershipType? MembershipType { get; set; } = null;

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
}