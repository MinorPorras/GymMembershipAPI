namespace GymMembershipAPI.Domain.Entities;

public class Booking
{
    public int Id { get; set; }
    public Guid PublicId { get; set; } = Guid.NewGuid();
    public int MemberId { get; set; }
    public Member Member { get; set; } = null;

    public int GroupClassId { get; set; }
    public GroupClass GroupClass { get; set; } = null;

    public DateTime CreatedAt { get; set; }

    // Confirmada, Cancelada, Asistio
    public string State { get; set; } = "Confirmada";
    
    // Concurrency Token with xmin in POSTGREsql
    public uint Version { get; set; }
}