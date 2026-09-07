
namespace GymMembershipAPI.Domain.Entities;

public class MembershipType
{
    public MembershipType() { }

    public int Id { get; set; }
    public Guid PublicId { get; set; } = Guid.NewGuid();
    public string Name { get; set; }
    public decimal Price { get; set; }
    public int DurationMonths { get; set; }

    public ICollection<Member> Members { get; set; }
}