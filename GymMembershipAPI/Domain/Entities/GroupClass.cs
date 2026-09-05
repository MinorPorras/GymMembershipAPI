namespace GymMembershipAPI.Domain.Entities;

public class GroupClass
{
    public int Id { get; set; }
    public Guid PublicId { get; set; }
    public string Name { get; set; }
    public string Instructor { get; set; }
    public DateTime DateHour { get; set; }
    public int MaxMembers { get; set; }

    public ICollection<Booking> Bookings { get; set; }
}