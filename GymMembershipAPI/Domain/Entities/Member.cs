namespace GymMembershipAPI.Domain.Entities;

public class Member
{
    public int Id { get; set; }
    public Guid PublicId { get; set; } = Guid.NewGuid();
    public string Name { get; set; }
    public string Email { get; set; }
    public string telefono { get; set; }
    
    //Relacion con tipo membresía
    public int MembershipTypeId { get; set; }
    public MembershipType MembershipType { get; set; }
    
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
    
    //Navegacion
    public ICollection<Booking>  Bookings { get; set; }
    public ICollection<RegisterAccess> RegisterAccesses { get; set; }
}