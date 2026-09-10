namespace GymMembershipAPI.Domain.Entities;

public class Booking
{
    public int Id { get; set; }
    public Guid PublicId { get; set; } = Guid.NewGuid();
    public int MemberId { get; set; }
    public Member Member { get; set; } = null;

    public int GroupClassId { get; set; }
    public GroupClass GroupClass { get; set; } = null;

    public DateTime BookingDate { get; set; }

    // Confirmada, Cancelada, Asistio
    public string State { get; set; } = "Confirmada";
    
    //TODO: Add Concurrency Token
}