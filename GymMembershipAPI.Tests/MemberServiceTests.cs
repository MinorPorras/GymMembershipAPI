using FluentAssertions;
using GymMembershipAPI.API.DTOs.Members;
using GymMembershipAPI.API.Services;
using GymMembershipAPI.Domain.Entities;
using GymMembershipAPI.Domain.Interfaces;
using GymMembershipAPI.Domain.results;
using GymMembershipAPI.Domain.Results;
using GymMembershipAPI.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace GymMembershipAPI.Tests;

public class MemberServiceTests
{
    private readonly Mock<ILogger<MemberService>> _logger = new();
    private readonly Mock<IMembershipTypeService> _membershipTypeService = new();
    private readonly Mock<IMembershipService> _membershipService = new();

    #region GetMethodsTests

    [Fact]
    public async Task GetAllAsync_WithData_ReturnsSuccess()
    {
        await using var context = TestDbContextFactory.Create();
        var service = new MemberService(context, _membershipTypeService.Object, _logger.Object,
            _membershipService.Object);
        var member = new Member("Test", "test@email.com", "1111-1111");
        context.Members.Add(member);
        await context.SaveChangesAsync();

        //Act
        var result = await service.GetAllAsync();

        //Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Count.Should().BeGreaterThan(0);
        result.Value[0].Name.Should().Be("Test");
        result.Value[0].Email.Should().Be(member.Email);
        result.Value[0].Phone.Should().Be(member.Phone);
        result.Value[0].IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task GetAllAsync_WithEmptyData_ReturnsSuccessAndEmptyList()
    {
        await using var context = TestDbContextFactory.Create();
        var service = new MemberService(context, _membershipTypeService.Object, _logger.Object,
            _membershipService.Object);

        //Act
        var result = await service.GetAllAsync();

        //Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Count.Should().Be(0);
    }

    [Fact]
    public async Task GetByPublicIdAsync_WithValidData_ReturnsSuccess()
    {
        await using var context = TestDbContextFactory.Create();
        var service = new MemberService(context, _membershipTypeService.Object, _logger.Object,
            _membershipService.Object);
        var member = new Member("Test", "test@email.com", "1111-1111");
        context.Members.Add(member);
        await context.SaveChangesAsync();

        //Act
        var result = await service.GetByPublicIdAsync(member.PublicId);

        //Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be(member.Name);
        result.Value.Email.Should().Be(member.Email);
        result.Value.Phone.Should().Be(member.Phone);
        result.Value.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task GetByPublicIdAsync_InvalidPublicId_ReturnsFailure()
    {
        await using var context = TestDbContextFactory.Create();
        var service = new MemberService(context, _membershipTypeService.Object, _logger.Object,
            _membershipService.Object);
        var inexistentGuid = Guid.NewGuid();

        //Act
        var result = await service.GetByPublicIdAsync(inexistentGuid);

        //Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be(MemberErrors.NotFound.Code);
    }

    #endregion

    #region CreateAsyncTests

    [Fact]
    public async Task CreateAsync_WithValidDto_ReturnsSuccess()
    {
        await using var context = TestDbContextFactory.Create();
        var service = new MemberService(context, _membershipTypeService.Object, _logger.Object,
            _membershipService.Object);

        var type = new MembershipType("Basic", 9.99m, 1);
        context.MembershipTypes.Add(type);
        await context.SaveChangesAsync();

        var dto = new MemberCreateDto("testName", "testEmail@gmail.com", "testPhone", type.PublicId);

        //Act
        var result = await service.CreateAsync(dto);

        //Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be(dto.Name);
        result.Value.Email.Should().Be(dto.Email);
        result.Value.Phone.Should().Be(dto.Phone);
        result.Value.IsActive.Should().BeTrue();

        var initialMembership = await context.Memberships.FirstOrDefaultAsync(x => x.MemberId == result.Value.Id);
        initialMembership.Should().NotBeNull();
        initialMembership.MembershipTypeId.Should().Be(type.Id);
        initialMembership.StartDate.Should().BeCloseTo(DateTime.UtcNow, new TimeSpan(0, 0, 0, 10));
        initialMembership.EndDate.Should()
            .BeCloseTo(DateTime.UtcNow.AddMonths(type.DurationMonths), new TimeSpan(0, 0, 0, 10));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task CreateAsync_NullOrEmptyName_ReturnsFailure(string name)
    {
        await using var context = TestDbContextFactory.Create();
        var service = new MemberService(context, _membershipTypeService.Object, _logger.Object,
            _membershipService.Object);

        var type = new MembershipType("Basic", 9.99m, 1);
        context.MembershipTypes.Add(type);
        await context.SaveChangesAsync();

        var dto = new MemberCreateDto(name, "testEmail@gmail.com", "testPhone", type.PublicId);

        //Act
        var result = await service.CreateAsync(dto);

        //Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be(MemberErrors.EmptyOrNullName.Code);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task CreateAsync_NullOrEmptyEmail_ReturnsFailure(string email)
    {
        await using var context = TestDbContextFactory.Create();
        var service = new MemberService(context, _membershipTypeService.Object, _logger.Object,
            _membershipService.Object);

        var type = new MembershipType("Basic", 9.99m, 1);
        context.MembershipTypes.Add(type);
        await context.SaveChangesAsync();

        var dto = new MemberCreateDto("testName", email, "testPhone", type.PublicId);

        //Act
        var result = await service.CreateAsync(dto);

        //Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be(MemberErrors.EmptyOrNullEmail.Code);
    }

    [Fact]
    public async Task CreateAsync_AlreadyExistentEmail_ReturnsFailure()
    {
        await using var context = TestDbContextFactory.Create();
        var service = new MemberService(context, _membershipTypeService.Object, _logger.Object,
            _membershipService.Object);

        var type = new MembershipType("Basic", 9.99m, 1);
        context.MembershipTypes.Add(type);
        await context.SaveChangesAsync();
        const string repeatedMail = "test@gmail.com";

        var otherMember = new Member("testName1", repeatedMail, "1111-1111");
        context.Members.Add(otherMember);
        await context.SaveChangesAsync();

        var dto = new MemberCreateDto("newName", repeatedMail, "1111-1111", type.PublicId);

        //Act
        var result = await service.CreateAsync(dto);

        //Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be(MemberErrors.EmailAlreadyExists.Code);
    }

    [Fact]
    public async Task CreateAsync_InvalidEmailFormat_ReturnsFailure()
    {
        await using var context = TestDbContextFactory.Create();
        var service = new MemberService(context, _membershipTypeService.Object, _logger.Object,
            _membershipService.Object);

        var type = new MembershipType("Basic", 9.99m, 1);
        context.MembershipTypes.Add(type);
        await context.SaveChangesAsync();

        var dto = new MemberCreateDto("testName", "falseEmail", "testPhone", type.PublicId);

        //Act
        var result = await service.CreateAsync(dto);

        //Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be(MemberErrors.InvalidEmailFormat.Code);
    }

    [Fact]
    public async Task CreateAsync_InvalidMembershipTypePublicId_ReturnsFailure()
    {
        await using var context = TestDbContextFactory.Create();
        var service = new MemberService(context, _membershipTypeService.Object, _logger.Object,
            _membershipService.Object);
        var inexistentGuid = Guid.NewGuid();
        var dto = new MemberCreateDto("testName", "test@gmail.com", "testPhone", inexistentGuid);

        //Act
        var result = await service.CreateAsync(dto);

        //Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be(MembershipTypeErrors.NotFound.Code);
    }

    #endregion

    #region UpdateAsyncTests

    [Fact]
    public async Task UpdateAsync_ValidData_ReturnsSuccess()
    {
        await using var context = TestDbContextFactory.Create();
        var service = new MemberService(context, _membershipTypeService.Object, _logger.Object,
            _membershipService.Object);
        var existentMember = new Member("test", "test@gmail.com", "1111-1111");
        context.Members.Add(existentMember);
        await context.SaveChangesAsync();

        var updatedMember = new MemberUpdateDto("test2", "test2@gmail.com", "1212-1212");

        //Act
        var result = await service.UpdateAsync(existentMember.PublicId, updatedMember);

        //Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().Be(existentMember.Id);
        result.Value.Name.Should().Be(updatedMember.Name);
        result.Value.Email.Should().Be(updatedMember.Email);
        result.Value.Phone.Should().Be(updatedMember.Phone);
    }

    [Fact]
    public async Task UpdateAsync_DuplicatedEmailFromOtherUser_ReturnsFailure()
    {
        await using var context = TestDbContextFactory.Create();
        var service = new MemberService(context, _membershipTypeService.Object, _logger.Object,
            _membershipService.Object);
        
        var existentMember = new Member("test", "test@gmail.com", "1111-1111");
        context.Members.Add(existentMember);
        var secondMember = new Member("dupTest", "test2@gmail.com", "2222-2222");
        context.Members.Add(secondMember);
        await context.SaveChangesAsync();

        var updatedMember = new MemberUpdateDto("test2", "test2@gmail.com", "1212-1212");
        
        //Act
        var result = await service.UpdateAsync(existentMember.PublicId, updatedMember);
        
        //Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be(MemberErrors.EmailAlreadyExists.Code);
    }

    [Fact]
    public async Task UpdateAsync_InvalidGuid_ReturnsFailure()
    {
        await using var context = TestDbContextFactory.Create();
        var service = new MemberService(context, _membershipTypeService.Object, _logger.Object,
            _membershipService.Object);
        var existentGuid = Guid.NewGuid();
        var updatedMember = new MemberUpdateDto("test", "test@gmail.com", "1111-1111");
        
        //Act
        var result = await service.UpdateAsync(existentGuid, updatedMember);
        
        //Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be(MemberErrors.NotFound.Code);
    }

    #endregion

    #region DeleteAsyncTests

    [Fact]
    public async Task DeleteAsync_ValidPublicId_ReturnsSuccess()
    {
        await using var context = TestDbContextFactory.Create();
        var service = new MemberService(context, _membershipTypeService.Object, _logger.Object,
            _membershipService.Object);
        
        var member = new Member("test", "test@gmail.com", "1111-1111");
        context.Members.Add(member);
        await context.SaveChangesAsync();
        
        //Act
        var result = await service.DeleteAsync(member.PublicId);
        
        //Assert
        result.IsSuccess.Should().BeTrue();
        var count = context.Members.Count();
        count.Should().Be(0);
    }

    [Fact]
    public async Task DeleteAsync_InvalidGuid_ReturnsFailure()
    {
        await using var context = TestDbContextFactory.Create();
        var service = new MemberService(context, _membershipTypeService.Object, _logger.Object,
            _membershipService.Object);
        
        var member = new Member("test", "test@gmail.com", "1111-1111");
        context.Members.Add(member);
        await context.SaveChangesAsync();
        
        var invalidGuid = Guid.NewGuid();
        
        //Act
        var result = await service.DeleteAsync(invalidGuid);
        
        //Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be(MemberErrors.NotFound.Code);
        
        var count  = context.Members.Count();
        count.Should().Be(1);
    }
    #endregion
}