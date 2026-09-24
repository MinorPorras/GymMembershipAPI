using FluentAssertions;
using GymMembershipAPI.API.DTOs.RegisterAccess;
using GymMembershipAPI.API.Services;
using GymMembershipAPI.Domain.Entities;
using GymMembershipAPI.Domain.Results;
using GymMembershipAPI.Tests.Helpers;
using Microsoft.Extensions.Logging;
using Moq;

namespace GymMembershipAPI.Tests;

public class RegisterAccessServiceTests
{
    private readonly Mock<ILogger<RegisterAccessService>> _logger = new();

    #region CreateAsyncTests

    [Fact]
    public async Task CreateAsync_WithValidData_ReturnSuccess()
    {
        //Arrange
        await using var context = TestDbContextFactory.Create();
        var service = new RegisterAccessService(_logger.Object, context);

        var member = new Member("Test Member", "member@email.com", "8888-8888");
        context.Members.Add(member);

        var membershipType = new MembershipType("Test Type", 9.99m, 1);
        context.MembershipTypes.Add(membershipType);

        await context.SaveChangesAsync();

        var membership = new Membership
        {
            MemberId = member.Id,
            MembershipTypeId = membershipType.Id,
            StartDate = DateTime.UtcNow.AddDays(-1),
            EndDate = DateTime.UtcNow.AddDays(30),
            IsActive = true
        };
        context.Memberships.Add(membership);
        await context.SaveChangesAsync();


        var accessDate = DateTime.UtcNow;
        var dto = new RegisterAccessRequestDto(member.PublicId);

        //Act
        var result = await service.RegisterAsync(dto, CancellationToken.None);

        //Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();

        result.Value.MemberId.Should().Be(member.Id);
        result.Value.AccessDate.Should().BeCloseTo(accessDate, TimeSpan.FromSeconds(1));
        result.Value.AllowAccess.Should().BeTrue();
        result.Value.PublicId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task RegisterAsync_InexistentMember_ReturnFailure()
    {
        await using var context = TestDbContextFactory.Create();
        var service = new RegisterAccessService(_logger.Object, context);

        var inexistentGuid = Guid.NewGuid();
        var dto = new RegisterAccessRequestDto(inexistentGuid);

        //Act
        var result = await service.RegisterAsync(dto, CancellationToken.None);

        //Assert
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be(RegisterAccessErrors.NotFound.Code);

        var savedEntities = context.RegisterAccesses.Count();
        savedEntities.Should().Be(0);
    }

    [Fact]
    public async Task RegisterAsync_MemberWithoutMembership_ReturnSuccessButAllowAccessFalse()
    {
        await using var context = TestDbContextFactory.Create();
        var service = new RegisterAccessService(_logger.Object, context);

        var member = new Member("Test Member", "member@email.com", "8888-8888");
        context.Members.Add(member);
        await context.SaveChangesAsync();

        var accessDate = DateTime.UtcNow;
        var dto = new RegisterAccessRequestDto(member.PublicId);

        //Act
        var result = await service.RegisterAsync(dto, CancellationToken.None);

        //Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.MemberId.Should().Be(member.Id);
        result.Value.AccessDate.Should().BeCloseTo(accessDate, TimeSpan.FromSeconds(1));
        result.Value.AllowAccess.Should().BeFalse();

        var savedEntities = context.RegisterAccesses.Count();
        savedEntities.Should().Be(1);
    }

    #endregion


    #region GetMethodTests

    [Fact]
    public async Task GetByPublicIdAsync_WithValidData_ReturnSuccess()
    {
        await using var context = TestDbContextFactory.Create();
        var service = new RegisterAccessService(_logger.Object, context);
        var entity = new RegisterAccess
        {
            MemberId = 1,
            AccessDate = DateTime.UtcNow,
            AllowAccess = true
        };
        context.Add(entity);
        await context.SaveChangesAsync();

        //Act
        var result = await service.GetByPublicIdAsync(entity.PublicId, CancellationToken.None);

        //Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.MemberId.Should().Be(entity.MemberId);
        result.Value.AccessDate.Should().Be(entity.AccessDate);
        result.Value.AllowAccess.Should().BeTrue();
        result.Value.PublicId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetByPublicIdAsync_InvalidPublicId_ReturnFailure()
    {
        await using var context = TestDbContextFactory.Create();
        var service = new RegisterAccessService(_logger.Object, context);
        var inexistentGuid = Guid.NewGuid();

        //Act
        var result = await service.GetByPublicIdAsync(inexistentGuid, CancellationToken.None);

        //Assert
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be(RegisterAccessErrors.NotFound.Code);
    }

    [Fact]
    public async Task GetAll_WithExistentData_ReturnSuccess()
    {
        await using var context = TestDbContextFactory.Create();
        var service = new RegisterAccessService(_logger.Object, context);
        var entity = new RegisterAccess
        {
            MemberId = 1,
            AccessDate = DateTime.UtcNow,
            AllowAccess = true
        };
        context.Add(entity);
        await context.SaveChangesAsync();

        //Act
        var result = await service.GetAllAsync(CancellationToken.None);

        //Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Count.Should().Be(1);
    }

    [Fact]
    public async Task GetAll_NoMatches_ReturnSuccessAndEmptyList()
    {
        await using var context = TestDbContextFactory.Create();
        var service = new RegisterAccessService(_logger.Object, context);

        //Act
        var result = await service.GetAllAsync(CancellationToken.None);

        //Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Count.Should().Be(0);
    }

    [Fact]
    public async Task GetByDateAsync_WithExistentData_ReturnSuccess()
    {
        await using var context = TestDbContextFactory.Create();
        var service = new RegisterAccessService(_logger.Object, context);
        var actualDate = DateTime.UtcNow;
        var entity = new RegisterAccess
        {
            MemberId = 1,
            AccessDate = actualDate,
            AllowAccess = true
        };
        context.Add(entity);
        await context.SaveChangesAsync();

        //Act
        var result = await service.GetByDateAsync(actualDate, CancellationToken.None);

        //Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Count.Should().Be(1);
    }

    [Fact]
    public async Task GetByDateAsync_NoMatches_ReturnSuccessAndEmptyList()
    {
        await using var context = TestDbContextFactory.Create();
        var service = new RegisterAccessService(_logger.Object, context);
        var actualDate = DateTime.UtcNow;

        //Act
        var result = await service.GetByDateAsync(actualDate, CancellationToken.None);

        //Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Count.Should().Be(0);
    }

    [Fact]
    public async Task GetByMemberPublicIdAsync_ValidPublicId_ReturnSuccess()
    {
        await using var context = TestDbContextFactory.Create();
        var service = new RegisterAccessService(_logger.Object, context);
        var member = new Member("Test Member", "member@email.com", "8888-8888");
        context.Members.Add(member);
        await context.SaveChangesAsync();

        var actualDate = DateTime.UtcNow;
        var entity = new RegisterAccess
        {
            MemberId = member.Id,
            AccessDate = actualDate,
            AllowAccess = true
        };
        context.Add(entity);
        await context.SaveChangesAsync();

        //Act
        var result = await service.GetByMemberPublicIdAsync(member.PublicId, CancellationToken.None);

        //Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Count.Should().Be(1);
    }

    [Fact]
    public async Task GetByMemberPublicIdAsync_ValidPublicIdButNoAccesses_ReturnSuccessValid()
    {
        await using var context = TestDbContextFactory.Create();
        var service = new RegisterAccessService(_logger.Object, context);
        var member = new Member("Test Member", "member@email.com", "8888-8888");
        context.Members.Add(member);
        await context.SaveChangesAsync();

        //Act
        var result = await service.GetByMemberPublicIdAsync(member.PublicId, CancellationToken.None);

        //Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Count.Should().Be(0);
    }

    [Fact]
    public async Task GetByMemberPublicIdAsync_InvalidPublicId_ReturnSuccessAndEmptyList()
    {
        await using var context = TestDbContextFactory.Create();
        var service = new RegisterAccessService(_logger.Object, context);
        var inexistentGuid = Guid.NewGuid();

        //Act
        var result = await service.GetByMemberPublicIdAsync(inexistentGuid, CancellationToken.None);

        //Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Count.Should().Be(0);
    }

    #endregion
}