
namespace GymMembershipAPI.Domain.Entities;

public class MembershipType
{
    public MembershipType() { }

    public int Id { get; set; }
    public Guid PublicId { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public int MonthDurantion { get; set; }

    public ICollection<Member> Members { get; set; }
}