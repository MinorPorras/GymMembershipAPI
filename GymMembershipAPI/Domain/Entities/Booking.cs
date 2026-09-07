namespace GymMembershipAPI.Domain.Entities;

public class Booking
{
    public int Id { get; set; }
    public Guid PublicId { get; set; } = Guid.NewGuid();
    public int MemberId { get; set; }
    public Member Member { get; set; }
    
    public int GroupClassId { get; set; }
    public GroupClass GroupClass { get; set; }
    
    public DateTime BookingDate { get; set; }
    public string State { get; set; } // Confirmada, Cancelada, Asistio
}