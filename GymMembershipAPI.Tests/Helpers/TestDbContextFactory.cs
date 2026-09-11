using GymMembershipAPI.Infraestructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GymMembershipAPI.Tests.Helpers;

public static class TestDbContextFactory
{
    public static GymDbContext Create()
    {
        var options = new DbContextOptionsBuilder<GymDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var context = new GymDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }
}