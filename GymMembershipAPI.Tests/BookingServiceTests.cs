using FluentAssertions;
using GymMembershipAPI.API.DTOs.Booking;
using GymMembershipAPI.API.Services;
using GymMembershipAPI.Domain.Entities;
using GymMembershipAPI.Domain.Interfaces;
using GymMembershipAPI.Domain.Results;
using GymMembershipAPI.Infraestructure.Data;
using GymMembershipAPI.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;
using Moq;

namespace GymMembershipAPI.Tests;

public class BookingServiceTests
{
    private readonly Mock<ILogger<BookingService>> _logger = new();

    #region GetMethodTests

    [Fact]
    public async Task GetByPublicIdAsync_ValidData_ReturnsSuccess()
    {
        await using var context = TestDbContextFactory.Create();
        var service = new BookingService(_logger.Object, context);

        var member = new Member("name", "test@email.com", "1111-1111");
        context.Members.Add(member);
        await context.SaveChangesAsync();

        var groupClass = new GroupClass()
        {
            Name = "test",
            Instructor = "instructor",
            DateHour = DateTime.UtcNow.AddDays(3),
            MaxMembers = 1
        };
        context.GroupClasses.Add(groupClass);
        await context.SaveChangesAsync();

        var existingBooking = new Booking()
        {
            MemberId = member.Id,
            GroupClassId = groupClass.Id,
            CreatedAt = DateTime.UtcNow.AddDays(-3),
            State = "Confirmada"
        };
        context.Bookings.Add(existingBooking);
        await context.SaveChangesAsync();

        //Act
        var result = await service.GetByPublicIdAsync(existingBooking.PublicId);

        //Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(existingBooking);
        result.Value.Member.Should().BeEquivalentTo(member);
        result.Value.GroupClass.Should().BeEquivalentTo(groupClass);
    }

    [Fact]
    public async Task GetByPrivateIdAsync_InexistentPublicId_ReturnsFailure()
    {
        await using var context = TestDbContextFactory.Create();
        var service = new BookingService(_logger.Object, context);
        var inexistentGuid = Guid.NewGuid();

        //Act
        var result = await service.GetByPublicIdAsync(inexistentGuid);

        //Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(BookingErrors.NotFound.Code);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsSuccessAndList()
    {
        await using var context = TestDbContextFactory.Create();
        var service = new BookingService(_logger.Object, context);

        var member = new Member("name", "test@email.com", "1111-1111");
        context.Members.Add(member);
        await context.SaveChangesAsync();

        var groupClass = new GroupClass()
        {
            Name = "test",
            Instructor = "instructor",
            DateHour = DateTime.UtcNow.AddDays(3),
            MaxMembers = 2
        };
        context.GroupClasses.Add(groupClass);
        await context.SaveChangesAsync();

        var booking1 = new Booking()
        {
            MemberId = member.Id,
            GroupClassId = groupClass.Id,
            CreatedAt = DateTime.UtcNow.AddDays(-3),
            State = "Confirmada"
        };
        var booking2 = new Booking()
        {
            MemberId = member.Id,
            GroupClassId = groupClass.Id,
            CreatedAt = DateTime.UtcNow.AddDays(-3),
            State = "Confirmada"
        };
        context.Bookings.AddRange(booking1, booking2);
        await context.SaveChangesAsync();

        //Act
        var result = await service.GetAllAsync();

        //Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNullOrEmpty();
        result.Value.Count.Should().Be(2);
        result.Value.Should().BeEquivalentTo([booking1, booking2]);
    }

    [Fact]
    public async Task GetAllAsync_EmptyList_ReturnsSuccessAndList()
    {
        await using var context = TestDbContextFactory.Create();
        var service = new BookingService(_logger.Object, context);

        //Act
        var result = await service.GetAllAsync();

        //Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Count.Should().Be(0);
    }

    [Fact]
    public async Task GetByMemberPublicIdAsync_ValidData_ReturnsSuccess()
    {
        await using var context = TestDbContextFactory.Create();
        var service = new BookingService(_logger.Object, context);

        var member1 = new Member("name", "test@email.com", "1111-1111");
        var member2 = new Member("name2", "test2@email.com", "2222-2222");

        context.Members.AddRange(member1, member2);
        await context.SaveChangesAsync();

        var groupClass = new GroupClass()
        {
            Name = "test",
            Instructor = "instructor",
            DateHour = DateTime.UtcNow.AddDays(3),
            MaxMembers = 2
        };
        context.GroupClasses.Add(groupClass);
        await context.SaveChangesAsync();

        var booking1 = new Booking()
        {
            MemberId = member1.Id,
            GroupClassId = groupClass.Id,
            CreatedAt = DateTime.UtcNow.AddDays(-3),
            State = "Confirmada"
        };
        var booking2 = new Booking()
        {
            MemberId = member2.Id,
            GroupClassId = groupClass.Id,
            CreatedAt = DateTime.UtcNow.AddDays(-3),
            State = "Confirmada"
        };
        context.Bookings.AddRange(booking1, booking2);
        await context.SaveChangesAsync();

        //Act
        var result = await service.GetByMemberPublicIdAsync(member1.PublicId);

        //Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNullOrEmpty();
        result.Value.Count.Should().Be(1);
        result.Value.Should().BeEquivalentTo([booking1]);
    }

    [Fact]
    public async Task GetByMemberPublicIdAsync_InvalidPublicId_ReturnsSuccessButEmptyList()
    {
        await using var context = TestDbContextFactory.Create();
        var service = new BookingService(_logger.Object, context);

        var member = new Member("name", "test@email.com", "1111-1111");
        context.Members.Add(member);
        await context.SaveChangesAsync();

        var groupClass = new GroupClass()
        {
            Name = "test",
            Instructor = "instructor",
            DateHour = DateTime.UtcNow.AddDays(3),
            MaxMembers = 1
        };
        context.GroupClasses.Add(groupClass);
        await context.SaveChangesAsync();

        var existingBooking = new Booking()
        {
            MemberId = member.Id,
            GroupClassId = groupClass.Id,
            CreatedAt = DateTime.UtcNow.AddDays(-3),
            State = "Confirmada"
        };
        context.Bookings.Add(existingBooking);
        await context.SaveChangesAsync();

        var inexistentGuid = Guid.NewGuid();

        //Act
        var result = await service.GetByMemberPublicIdAsync(inexistentGuid);

        //Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Count.Should().Be(0);
    }

    [Fact]
    public async Task GetByClassPublicIdAsync_ValidData_ReturnsSuccess()
    {
        await using var context = TestDbContextFactory.Create();
        var service = new BookingService(_logger.Object, context);

        var member1 = new Member("name", "test@email.com", "1111-1111");
        var member2 = new Member("name2", "test2@email.com", "2222-2222");

        context.Members.AddRange(member1, member2);
        await context.SaveChangesAsync();

        var groupClass = new GroupClass()
        {
            Name = "test",
            Instructor = "instructor",
            DateHour = DateTime.UtcNow.AddDays(3),
            MaxMembers = 2
        };
        context.GroupClasses.Add(groupClass);
        await context.SaveChangesAsync();

        var booking1 = new Booking()
        {
            MemberId = member1.Id,
            GroupClassId = groupClass.Id,
            CreatedAt = DateTime.UtcNow.AddDays(-3),
            State = "Confirmada"
        };
        var booking2 = new Booking()
        {
            MemberId = member2.Id,
            GroupClassId = groupClass.Id,
            CreatedAt = DateTime.UtcNow.AddDays(-3),
            State = "Confirmada"
        };
        context.Bookings.AddRange(booking1, booking2);
        await context.SaveChangesAsync();

        //Act
        var result = await service.GetByClassPublicIdAsync(groupClass.PublicId);

        //Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNullOrEmpty();
        result.Value.Count.Should().Be(2);
        result.Value.Should().BeEquivalentTo([booking1, booking2]);
    }

    [Fact]
    public async Task GetByClassPublicIdAsync_InvalidPublicId_ReturnsSuccessButEmptyList()
    {
        await using var context = TestDbContextFactory.Create();
        var service = new BookingService(_logger.Object, context);

        var member = new Member("name", "test@email.com", "1111-1111");
        context.Members.Add(member);
        await context.SaveChangesAsync();

        var groupClass = new GroupClass()
        {
            Name = "test",
            Instructor = "instructor",
            DateHour = DateTime.UtcNow.AddDays(3),
            MaxMembers = 1
        };
        context.GroupClasses.Add(groupClass);
        await context.SaveChangesAsync();

        var existingBooking = new Booking()
        {
            MemberId = member.Id,
            GroupClassId = groupClass.Id,
            CreatedAt = DateTime.UtcNow.AddDays(-3),
            State = "Confirmada"
        };
        context.Bookings.Add(existingBooking);
        await context.SaveChangesAsync();

        var inexistentGuid = Guid.NewGuid();

        //Act
        var result = await service.GetByClassPublicIdAsync(inexistentGuid);

        //Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Count.Should().Be(0);
    }

    #endregion

    #region CreateAsyncTests

    [Fact]
    public async Task CreateAsync_ValidData_ReturnsSuccess()
    {
        await using var context = TestDbContextFactory.Create();
        var service = new BookingService(_logger.Object, context);

        var member = new Member("name", "test@email.com", "1111-1111");
        context.Members.Add(member);
        await context.SaveChangesAsync();

        var type = new MembershipType("Basic", 9.99m, 1);

        var membership = new Membership()
        {
            MemberId = member.Id,
            MembershipTypeId = type.Id,
            StartDate = DateTime.UtcNow.AddDays(-3),
            EndDate = DateTime.UtcNow.AddDays(27),
            IsActive = true
        };
        context.Memberships.Add(membership);
        await context.SaveChangesAsync();

        var groupClass = new GroupClass()
        {
            Name = "test",
            Instructor = "instructor",
            DateHour = DateTime.UtcNow.AddDays(3),
            MaxMembers = 1
        };
        context.GroupClasses.Add(groupClass);
        await context.SaveChangesAsync();

        var dto = new BookingRequestDto(member.PublicId, groupClass.PublicId);

        //Act 
        var result = await service.CreateAsync(dto);

        //Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.MemberId.Should().Be(member.Id);
        result.Value.GroupClassId.Should().Be(groupClass.Id);
        result.Value.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        result.Value.State.Should().Be("Confirmada");
    }

    [Fact]
    public async Task CreateAsync_InexistentMemberPublicId_ReturnsFailure()
    {
        await using var context = TestDbContextFactory.Create();
        var service = new BookingService(_logger.Object, context);

        var inexistentMemberPublicId = Guid.NewGuid();
        var inexistentClassPublicId = Guid.NewGuid();

        var dto = new BookingRequestDto(inexistentMemberPublicId, inexistentClassPublicId);
        //Act
        var result = await service.CreateAsync(dto);

        //Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be(BookingErrors.MemberNotFound.Code);
    }

    [Fact]
    public async Task CreateAsync_MemberWithoutMembership_ReturnsFailure()
    {
        await using var context = TestDbContextFactory.Create();
        var service = new BookingService(_logger.Object, context);

        var member = new Member("name", "test@email.com", "1111-1111");
        context.Members.Add(member);
        await context.SaveChangesAsync();

        var inexistentClassPublicId = Guid.NewGuid();

        var dto = new BookingRequestDto(member.PublicId, inexistentClassPublicId);

        //Act
        var result = await service.CreateAsync(dto);

        //Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be(BookingErrors.NoActiveMembership.Code);
    }

    [Fact]
    public async Task CreateAsync_MemberWithoutActiveMembership_ReturnsFailure()
    {
        await using var context = TestDbContextFactory.Create();
        var service = new BookingService(_logger.Object, context);

        var member = new Member("name", "test@email.com", "1111-1111");
        context.Members.Add(member);
        await context.SaveChangesAsync();

        var type = new MembershipType("Basic", 9.99m, 1);

        var membership = new Membership()
        {
            MemberId = member.Id,
            MembershipTypeId = type.Id,
            StartDate = DateTime.UtcNow.AddDays(-27),
            EndDate = DateTime.UtcNow.AddDays(-1),
            IsActive = false
        };
        context.Memberships.Add(membership);
        await context.SaveChangesAsync();

        var inexistentClassPublicId = Guid.NewGuid();

        var dto = new BookingRequestDto(member.PublicId, inexistentClassPublicId);

        //Act
        var result = await service.CreateAsync(dto);

        //Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be(BookingErrors.NoActiveMembership.Code);
    }

    [Fact]
    public async Task CreateAsync_InexistentGroupClassPublicId_ReturnsFailure()
    {
        await using var context = TestDbContextFactory.Create();
        var service = new BookingService(_logger.Object, context);

        var member = new Member("name", "test@email.com", "1111-1111");
        context.Members.Add(member);
        await context.SaveChangesAsync();

        var type = new MembershipType("Basic", 9.99m, 1);

        var membership = new Membership()
        {
            MemberId = member.Id,
            MembershipTypeId = type.Id,
            StartDate = DateTime.UtcNow.AddDays(-3),
            EndDate = DateTime.UtcNow.AddDays(27),
            IsActive = true
        };
        context.Memberships.Add(membership);
        await context.SaveChangesAsync();

        var inexistentClassPublicId = Guid.NewGuid();
        var dto = new BookingRequestDto(member.PublicId, inexistentClassPublicId);

        //Act
        var result = await service.CreateAsync(dto);

        //Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be(BookingErrors.GroupClassNotFound.Code);
    }

    [Fact]
    public async Task CreateAsync_MaxMembersReached_ReturnsFailure()
    {
        await using var context = TestDbContextFactory.Create();
        var service = new BookingService(_logger.Object, context);

        var member = new Member("name", "test@email.com", "1111-1111");
        var member2 = new Member("name2", "test2@gmail.com", "2222-2222");
        context.Members.AddRange([member, member2]);
        await context.SaveChangesAsync();

        var groupClass = new GroupClass()
        {
            Name = "test",
            Instructor = "instructor",
            DateHour = DateTime.UtcNow.AddDays(3),
            MaxMembers = 1
        };
        context.GroupClasses.Add(groupClass);
        await context.SaveChangesAsync();

        var type = new MembershipType("Basic", 9.99m, 1);

        var membership = new Membership()
        {
            MemberId = member2.Id,
            MembershipTypeId = type.Id,
            StartDate = DateTime.UtcNow.AddDays(-3),
            EndDate = DateTime.UtcNow.AddDays(27),
            IsActive = true
        };
        context.Memberships.Add(membership);
        await context.SaveChangesAsync();

        var existingBooking = new Booking()
        {
            MemberId = member.Id,
            GroupClassId = groupClass.Id,
            CreatedAt = DateTime.UtcNow.AddDays(-3),
            State = "Confirmada"
        };
        context.Bookings.Add(existingBooking);
        await context.SaveChangesAsync();

        var dto = new BookingRequestDto(member2.PublicId, groupClass.PublicId);

        //Act
        var result = await service.CreateAsync(dto);

        //Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be(BookingErrors.ClassIsFull.Code);

        var actualBookings = context.Bookings.Where(b => b.GroupClassId == groupClass.Id && b.State == "Confirmada")
            .ToList();
        actualBookings.Count.Should().Be(1);
    }

    [Fact]
    public async Task CreateAsync_AlreadyBookedClass_ReturnsFailure()
    {
        await using var context = TestDbContextFactory.Create();
        var service = new BookingService(_logger.Object, context);

        var member = new Member("name", "test@email.com", "1111-1111");
        context.Members.Add(member);
        await context.SaveChangesAsync();

        var groupClass = new GroupClass()
        {
            Name = "test",
            Instructor = "instructor",
            DateHour = DateTime.UtcNow.AddDays(3),
            MaxMembers = 2
        };
        context.GroupClasses.Add(groupClass);
        await context.SaveChangesAsync();

        var type = new MembershipType("Basic", 9.99m, 1);

        var membership = new Membership()
        {
            MemberId = member.Id,
            MembershipTypeId = type.Id,
            StartDate = DateTime.UtcNow.AddDays(-3),
            EndDate = DateTime.UtcNow.AddDays(27),
            IsActive = true
        };
        context.Memberships.Add(membership);
        await context.SaveChangesAsync();

        var existingBooking = new Booking()
        {
            MemberId = member.Id,
            GroupClassId = groupClass.Id,
            CreatedAt = DateTime.UtcNow.AddDays(-3),
            State = "Confirmada"
        };
        context.Bookings.Add(existingBooking);
        await context.SaveChangesAsync();

        var dto = new BookingRequestDto(member.PublicId, groupClass.PublicId);

        //Act
        var result = await service.CreateAsync(dto);

        //Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be(BookingErrors.AlreadyBooked.Code);

        var actualBookings = context.Bookings.Where(b => b.GroupClassId == groupClass.Id && b.State == "Confirmada")
            .ToList();
        actualBookings.Count.Should().Be(1);
    }

    [Fact]
    public async Task CreateAsync_WhenDbUpdateConcurrencyExceptionOccurs_ReturnsFailure()
    {
        // 1. Configurar opciones compartidas de InMemory
        var options = new DbContextOptionsBuilder<GymDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        Member member;
        GroupClass groupClass;

        // Sembrar datos en DOS PASOS para garantizar que los IDs se resuelvan correctamente
        await using (var seedContext = new GymDbContext(options))
        {
            seedContext.Database.EnsureCreated();

            member = new Member("test", "test@gmail.com", "1111-1111");
            seedContext.Members.Add(member);

            var type = new MembershipType("Basic", 9.99m, 1);
            seedContext.MembershipTypes.Add(type);

            await seedContext.SaveChangesAsync(); 
            
            var membership = new Membership
            {
                MemberId = member.Id,
                MembershipTypeId = type.Id,
                StartDate = DateTime.UtcNow.AddDays(-1),
                EndDate = DateTime.UtcNow.AddDays(30),
                IsActive = true
            };
            seedContext.Memberships.Add(membership);

            groupClass = new GroupClass
            {
                Name = "Yoga",
                Instructor = "Ana",
                DateHour = DateTime.UtcNow.AddDays(2),
                MaxMembers = 10
            };
            seedContext.GroupClasses.Add(groupClass);

            await seedContext.SaveChangesAsync();
        }

        // Usar el contexto que lanza la excepción
        await using var throwingContext = new ConcurrencyThrowingDbContext(options);

        var service = new BookingService(_logger.Object, throwingContext);
        var dto = new BookingRequestDto(member.PublicId, groupClass.PublicId);

        // Act
        var result = await service.CreateAsync(dto);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be(BookingErrors.ClassIsFull.Code);
    }

    private class ConcurrencyThrowingDbContext : GymDbContext
    {
        public ConcurrencyThrowingDbContext(DbContextOptions<GymDbContext> options) : base(options)
        {
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            throw new DbUpdateConcurrencyException("Simulated concurrency conflict");
        }
    }

    #endregion

    #region CancelAsyncTests

    [Fact]
    public async Task CancelAsync_ValidPublicId_ReturnsSuccess()
    {
        await using var context = TestDbContextFactory.Create();
        var service = new BookingService(_logger.Object, context);
        var member = new Member("name", "test@email.com", "1111-1111");
        context.Members.Add(member);
        await context.SaveChangesAsync();

        var groupClass = new GroupClass()
        {
            Name = "test",
            Instructor = "instructor",
            DateHour = DateTime.UtcNow.AddDays(3),
            MaxMembers = 2
        };
        context.GroupClasses.Add(groupClass);
        await context.SaveChangesAsync();

        var type = new MembershipType("Basic", 9.99m, 1);

        var membership = new Membership()
        {
            MemberId = member.Id,
            MembershipTypeId = type.Id,
            StartDate = DateTime.UtcNow.AddDays(-3),
            EndDate = DateTime.UtcNow.AddDays(27),
            IsActive = true
        };
        context.Memberships.Add(membership);
        await context.SaveChangesAsync();

        var existingBooking = new Booking()
        {
            MemberId = member.Id,
            GroupClassId = groupClass.Id,
            CreatedAt = DateTime.UtcNow.AddDays(-3),
            State = "Confirmada"
        };
        context.Bookings.Add(existingBooking);
        await context.SaveChangesAsync();

        //Act
        var result = await service.CancelAsync(existingBooking.PublicId);

        //Assert
        result.IsSuccess.Should().BeTrue();

        var canceledBooking = await context.Bookings.FirstOrDefaultAsync(b => b.PublicId == existingBooking.PublicId);
        canceledBooking.Should().NotBeNull();
        canceledBooking.MemberId.Should().Be(member.Id);
        canceledBooking.GroupClassId.Should().Be(groupClass.Id);
        canceledBooking.State.Should().Be("Cancelled");
    }

    [Fact]
    public async Task CancelAsync_InvalidPublicId_ReturnsFailure()
    {
        await using var context = TestDbContextFactory.Create();
        var service = new BookingService(_logger.Object, context);
        var inexistentPublicId = Guid.NewGuid();

        //Act
        var result = await service.CancelAsync(inexistentPublicId);

        //Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be(BookingErrors.NotFound.Code);
    }

    [Fact]
    public async Task CancelAsync_AlreadyCanceledBooking_ReturnsFailure()
    {
        await using var context = TestDbContextFactory.Create();
        var service = new BookingService(_logger.Object, context);
        var member = new Member("name", "test@email.com", "1111-1111");
        context.Members.Add(member);
        await context.SaveChangesAsync();

        var groupClass = new GroupClass()
        {
            Name = "test",
            Instructor = "instructor",
            DateHour = DateTime.UtcNow.AddDays(3),
            MaxMembers = 2
        };
        context.GroupClasses.Add(groupClass);
        await context.SaveChangesAsync();

        var type = new MembershipType("Basic", 9.99m, 1);

        var membership = new Membership()
        {
            MemberId = member.Id,
            MembershipTypeId = type.Id,
            StartDate = DateTime.UtcNow.AddDays(-3),
            EndDate = DateTime.UtcNow.AddDays(27),
            IsActive = true
        };
        context.Memberships.Add(membership);
        await context.SaveChangesAsync();

        var existingBooking = new Booking()
        {
            MemberId = member.Id,
            GroupClassId = groupClass.Id,
            CreatedAt = DateTime.UtcNow.AddDays(-3),
            State = "Cancelled"
        };
        context.Bookings.Add(existingBooking);
        await context.SaveChangesAsync();

        //Act
        var result = await service.CancelAsync(existingBooking.PublicId);

        //Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be(BookingErrors.AlreadyCancelled.Code);
    }

    #endregion
}