namespace GymMembershipAPI.Domain.Entities;

public class Member
{
    public Member() { }

    public int Id { get; set; }
    public Guid PublicId { get; set; } = Guid.NewGuid();
    public string Name { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public bool IsActive => Memberships.Any(m => m.EndDate >= DateTime.UtcNow && m.IsActive);

    //Relacion con tipo membresía
    public ICollection<Membership>  Memberships { get; set; } = new List<Membership>();
    
    //Navegacion
    public ICollection<Booking> Bookings { get; set; }
    public ICollection<RegisterAccess> RegisterAccesses { get; set; }
}