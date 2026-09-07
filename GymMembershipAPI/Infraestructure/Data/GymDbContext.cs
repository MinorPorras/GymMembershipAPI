using GymMembershipAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GymMembershipAPI.Infraestructure.Data;

public class GymDbContext : DbContext
{
    //Constructor 
    public GymDbContext(DbContextOptions<GymDbContext> options) : base(options)
    {
    }

    // Representan las tablas de la base de datos
    public DbSet<Member> Members { get; set; }
    public DbSet<GroupClass> GroupClasses { get; set; }
    public DbSet<MembershipType> MembershipTypes { get; set; }
    public DbSet<Booking> Bookings { get; set; }
    public DbSet<RegisterAccess> RegisterAccesses { get; set; }

    // Configuraciones de relaciones y reglas con FLUENT API
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<MembershipType>(entity =>
        {
            entity.HasIndex(e => e.PublicId).IsUnique();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Price).HasPrecision(18, 2);
        });

        modelBuilder.Entity<Member>(entity =>
        {
            entity.HasIndex(e => e.PublicId).IsUnique();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(150);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(150);

            entity.HasMany(e => e.RegisterAccesses)
                .WithOne(e => e.Member)
                .HasForeignKey(e => e.MemberId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Membership>(entity =>
        {
            entity.HasIndex(e => e.PublicId).IsUnique();
            
            entity.HasOne(e => e.Member)
                .WithMany(e => e.Memberships)
                .HasForeignKey(e => e.MemberId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.MembershipType)
                .WithMany()
                .HasForeignKey(e => e.MembershipTypeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasIndex(e => e.PublicId).IsUnique();
            entity.HasIndex(e => new { e.MemberId, e.GroupClassId }).IsUnique();

            entity.HasOne(e => e.Member)
                .WithMany(e => e.Bookings)
                .HasForeignKey(e => e.MemberId);

            entity.HasOne(e => e.GroupClass)
                .WithMany(e => e.Bookings)
                .HasForeignKey(e => e.GroupClassId);
        });
    }
}