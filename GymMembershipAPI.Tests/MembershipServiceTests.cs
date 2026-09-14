using FluentAssertions;
using GymMembershipAPI.API.DTOs.Membership;
using GymMembershipAPI.API.Services;
using GymMembershipAPI.Domain.Entities;
using GymMembershipAPI.Domain.results;
using GymMembershipAPI.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace GymMembershipAPI.Tests;

public class MembershipServiceTests
{
    private readonly Mock<ILogger<MembershipService>> _logger = new();

    #region GetMethodsTests

    //GetByPublicIdAsync

    [Fact]
    public async Task GetByPublicIdAsync_ValidPublicId_ReturnSuccess()
    {
        await using var context = TestDbContextFactory.Create();
        var service = new MembershipService(_logger.Object, context);

        var member = new Member("testName", "testEmail@email.com", "8888-8888");
        context.Members.Add(member);
        await context.SaveChangesAsync();

        var membershipType = new MembershipType("Basico", 9.99m, 1);
        context.MembershipTypes.Add(membershipType);
        await context.SaveChangesAsync();

        var membership = new Membership(member.Id, membershipType.Id, DateTime.UtcNow.AddDays(-1),
            DateTime.UtcNow.AddDays(30), true);
        context.Memberships.Add(membership);
        await context.SaveChangesAsync();

        //Act
        var result = await service.GetByPublicIdAsync(membership.PublicId);

        //Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.MemberId.Should().Be(member.Id);
        result.Value.MembershipTypeId.Should().Be(membershipType.Id);
        result.Value.StartDate.Should().Be(membership.StartDate);
        result.Value.EndDate.Should().Be(membership.EndDate);
        result.Value.IsActive.Should().Be(membership.IsActive);

        //Has navigation entities
        result.Value.Member.Should().NotBeNull();
        result.Value.MembershipType.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByPublicIdAsync_InvalidPublicId_ReturnFailure()
    {
        await using var context = TestDbContextFactory.Create();
        var service = new MembershipService(_logger.Object, context);
        var inexistentGuid = Guid.NewGuid();

        //Act
        var result = await service.GetByPublicIdAsync(inexistentGuid);

        //Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be(MembershipErrors.NotFound.Code);
    }

    //GetActiveByMemberPublicId

    [Fact]
    public async Task GetActiveByMemberPublicIdAsync_ValidMemberPublicIdIgnoreInactive_ReturnSuccess()
    {
        await using var context = TestDbContextFactory.Create();
        var service = new MembershipService(_logger.Object, context);

        var member = new Member("testName", "testEmail@email.com", "8888-8888");
        context.Members.Add(member);
        await context.SaveChangesAsync();

        var membershipType = new MembershipType("Basico", 9.99m, 1);
        context.MembershipTypes.Add(membershipType);
        await context.SaveChangesAsync();

        var membership = new Membership(member.Id, membershipType.Id, DateTime.UtcNow.AddDays(-1),
            DateTime.UtcNow.AddDays(30), true);
        var membership2 = new Membership(member.Id, membershipType.Id, DateTime.UtcNow.AddDays(-30),
            DateTime.UtcNow.AddDays(-1), false);
        context.Memberships.Add(membership);
        await context.SaveChangesAsync();

        //Act
        var result = await service.GetActiveByMemberPublicId(member.PublicId);

        //Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Count.Should().Be(1);
        result.Value.First().MemberId.Should().Be(member.Id);
        result.Value.First().MembershipTypeId.Should().Be(membershipType.Id);
        result.Value.First().StartDate.Should().Be(membership.StartDate);
        result.Value.First().EndDate.Should().Be(membership.EndDate);
        result.Value.First().IsActive.Should().Be(membership.IsActive);
    }

    [Fact]
    public async Task GetActiveByMemberPublicIdAsync_InvalidMemberPublicId_ReturnFailure()
    {
        await using var context = TestDbContextFactory.Create();
        var service = new MembershipService(_logger.Object, context);
        var inexistentGuid = Guid.NewGuid();

        //Act
        var result = await service.GetActiveByMemberPublicId(inexistentGuid);

        //Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be(MembershipErrors.MemberNotFound.Code);
    }

    [Fact]
    public async Task GetActiveByMemberPublicIdAsync_ValidMemberPublicWithEmptyMatches_ReturnSuccess()
    {
        await using var context = TestDbContextFactory.Create();
        var service = new MembershipService(_logger.Object, context);

        var member = new Member("testName", "testEmail@email.com", "8888-8888");
        context.Members.Add(member);
        await context.SaveChangesAsync();

        //Act
        var result = await service.GetActiveByMemberPublicId(member.PublicId);

        //Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Count.Should().Be(0);
    }

    [Fact]
    public async Task GetHistoryByMemberPublicIdAsync_ValidMemberPublicId_ReturnSuccess()
    {
        await using var context = TestDbContextFactory.Create();
        var service = new MembershipService(_logger.Object, context);

        var member = new Member("testName", "testEmail@email.com", "8888-8888");
        context.Members.Add(member);
        await context.SaveChangesAsync();

        var membershipType = new MembershipType("Basico", 9.99m, 1);
        context.MembershipTypes.Add(membershipType);
        await context.SaveChangesAsync();

        var membership = new Membership(member.Id, membershipType.Id, DateTime.UtcNow.AddDays(-1),
            DateTime.UtcNow.AddDays(30), true);
        var membership2 = new Membership(member.Id, membershipType.Id, DateTime.UtcNow.AddDays(-30),
            DateTime.UtcNow.AddDays(-1), false);
        context.Memberships.Add(membership);
        context.Memberships.Add(membership2);
        await context.SaveChangesAsync();

        //Act
        var result = await service.GetHistoryByMemberPublicIdAsync(member.PublicId);

        //Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Count.Should().Be(2);
        result.Value.First().Id.Should().Be(membership.Id);
        result.Value.First().IsActive.Should().Be(membership.IsActive);
        result.Value[1].Id.Should().Be(membership2.Id);
        result.Value[1].IsActive.Should().Be(membership2.IsActive);
    }

    [Fact]
    public async Task GetHistoryByMemberPublicIdAsync_InvalidMemberPublicId_ReturnFailure()
    {
        await using var context = TestDbContextFactory.Create();
        var service = new MembershipService(_logger.Object, context);
        var inexistentGuid = Guid.NewGuid();

        //Act
        var result = await service.GetHistoryByMemberPublicIdAsync(inexistentGuid);

        //Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be(MembershipErrors.MemberNotFound.Code);
    }

    [Fact]
    public async Task GetHistoryByMemberPublicIdAsync_ValidMemberPublicWithEmptyMatches_ReturnSuccess()
    {
        await using var context = TestDbContextFactory.Create();
        var service = new MembershipService(_logger.Object, context);

        var member = new Member("testName", "testEmail@email.com", "8888-8888");
        context.Members.Add(member);
        await context.SaveChangesAsync();

        //Act
        var result = await service.GetHistoryByMemberPublicIdAsync(member.PublicId);

        //Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Count.Should().Be(0);
    }

    #endregion

    #region CreateAsyncTests

    [Fact]
    public async Task CreateAsync_ValidData_CreatesMembershipAndDeactivatesPrevious()
    {
        await using var context = TestDbContextFactory.Create();
        var service = new MembershipService(_logger.Object, context);

        var member = new Member("Juan Pérez", "juan@test.com", "555-1234");
        var oldType = new MembershipType("Mensual", 29.99m, 1);
        var newType = new MembershipType("Anual", 299.99m, 12);

        context.Members.Add(member);
        context.MembershipTypes.AddRange(oldType, newType);
        await context.SaveChangesAsync();

        var oldMembership = new Membership
        {
            MemberId = member.Id,
            MembershipTypeId = oldType.Id,
            StartDate = DateTime.UtcNow.AddMonths(-1),
            EndDate = DateTime.UtcNow.AddMonths(1),
            IsActive = true
        };
        context.Memberships.Add(oldMembership);
        await context.SaveChangesAsync();

        var dto = new MembershipRequestDto(member.PublicId, newType.PublicId);

        //Act
        var result = await service.CreateAsync(dto);

        //Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.IsActive.Should().BeTrue();
        result.Value.MemberId.Should().Be(member.Id);
        result.Value.MembershipTypeId.Should().Be(newType.Id);

        var allMemberships = await context.Memberships.Where(m => m.Member.Id == member.Id)
            .OrderByDescending(m => m.StartDate).ToListAsync();

        allMemberships.Count.Should().Be(2);

        allMemberships.First().IsActive.Should().BeTrue();
        allMemberships.First().MembershipTypeId.Should().Be(newType.Id);

        allMemberships[1].IsActive.Should().BeFalse();
        allMemberships[1].EndDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task CreateAsync_InexistentMemberPublicId_ReturnsFailure()
    {
        await using var context = TestDbContextFactory.Create();
        var service = new MembershipService(_logger.Object, context);

        var inexistentGuid = Guid.NewGuid();
        var dto = new MembershipRequestDto(inexistentGuid, inexistentGuid);

        //Act
        var result = await service.CreateAsync(dto);

        //Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be(MembershipErrors.MemberNotFound.Code);
    }

    [Fact]
    public async Task CreateAsync_InexistentMembershipTypePublicId_ReturnsFailure()
    {
        await using var context = TestDbContextFactory.Create();
        var service = new MembershipService(_logger.Object, context);
        var inexistentGuid = Guid.NewGuid();
        var member = new Member("testName", "testemail@test.com", "1111-1111");
        context.Members.Add(member);
        await context.SaveChangesAsync();
        var dto = new MembershipRequestDto(member.PublicId, inexistentGuid);

        //Act
        var result = await service.CreateAsync(dto);

        //Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be(MembershipErrors.MembershipTypeNotFound.Code);
    }

    #endregion

    #region CancelAsyncTests

    [Fact]
    public async Task CancelAsync_ValidData_CancelsMembershipAndReturnsSuccess()
    {
        await using var context = TestDbContextFactory.Create();
        var service = new MembershipService(_logger.Object, context);

        var member = new Member("Test", "test@test.com", "123");
        var type = new MembershipType("Test Type", 10m, 1);
        context.AddRange(member, type);
        await context.SaveChangesAsync();

        var publicId = Guid.NewGuid();
        var existentMembership = new Membership()
        {
            PublicId = publicId,
            MemberId = member.Id,
            MembershipTypeId = type.Id,
            StartDate = DateTime.UtcNow.AddMonths(-1),
            EndDate = DateTime.UtcNow.AddMonths(1),
            IsActive = true
        };
        context.Memberships.Add(existentMembership);
        await context.SaveChangesAsync();

        //Act
        var result = await service.CancelAsync(existentMembership.PublicId);

        //Assert
        result.IsSuccess.Should().BeTrue();

        var canceledMembership =
            await context.Memberships.FirstOrDefaultAsync(m => m.PublicId == existentMembership.PublicId);
        canceledMembership.Should().NotBeNull();
        canceledMembership.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task CancelAsync_InexistentMembershipPublicId_ReturnsFailure()
    {
        await using var context = TestDbContextFactory.Create();
        var service = new MembershipService(_logger.Object, context);
        var inexistentGuid = Guid.NewGuid();

        //Act
        var result = await service.CancelAsync(inexistentGuid);

        //Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be(MembershipErrors.NotFound.Code);
    }

    [Fact]
    public async Task CancelAsync_AlreadyCancelled_ReturnsFailure()
    {
        await using var context = TestDbContextFactory.Create();
        var service = new MembershipService(_logger.Object, context);

        var member = new Member("Test", "test@test.com", "123");
        var type = new MembershipType("Test Type", 10m, 1);
        context.AddRange(member, type);
        await context.SaveChangesAsync();


        var publicId = Guid.NewGuid();
        var existentMembership = new Membership()
        {
            PublicId = publicId,
            MemberId = member.Id,
            MembershipTypeId = type.Id,
            StartDate = DateTime.UtcNow.AddMonths(-1),
            EndDate = DateTime.UtcNow.AddMonths(1),
            IsActive = false
        };
        context.Memberships.Add(existentMembership);
        await context.SaveChangesAsync();

        //Act
        var result = await service.CancelAsync(existentMembership.PublicId);

        //Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be(MembershipErrors.AlreadyCancelled.Code);
    }

    #endregion
}