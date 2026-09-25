using FluentAssertions;
using GymMembershipAPI.API.DTOs.Members;
using GymMembershipAPI.API.Services;
using GymMembershipAPI.Domain.Entities;
using GymMembershipAPI.Domain.Interfaces;
using GymMembershipAPI.Domain.Results;
using GymMembershipAPI.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace GymMembershipAPI.Tests;

public class MemberServiceTests
{
    private readonly Mock<ILogger<MemberService>> _logger = new();

    #region GetMethodsTests

    [Fact]
    public async Task GetAllPaginatedAsync_WithData_ReturnsSuccessWithCorrectMetadata()
    {
        // Arrange
        await using var context = TestDbContextFactory.Create();
        var service = new MemberService(context, _logger.Object);

        var member = new Member("Test", "test@email.com", "1111-1111");
        context.Members.Add(member);
        await context.SaveChangesAsync();

        // Act: Pasamos page 1 y pageSize 10
        var result = await service.GetAllAsync(page: 1, pageSize: 10, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();

        // 1. Verificamos los Metadatos (La gran diferencia con el test anterior)
        result.Value.TotalRecords.Should().Be(1);
        result.Value.CurrentPage.Should().Be(1);
        result.Value.PageSize.Should().Be(10);
        result.Value.TotalPages.Should().Be(1);

        // 2. Verificamos los Datos
        result.Value.Data.Should().HaveCount(1);
        var firstMember = result.Value.Data.First();
        
        firstMember.Name.Should().Be("Test");
        firstMember.Email.Should().Be(member.Email);
        firstMember.Phone.Should().Be(member.Phone);
        firstMember.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task GetAllPaginatedAsync_WithEmptyData_ReturnsSuccessAndEmptyList()
    {
        // Arrange
        await using var context = TestDbContextFactory.Create();
        var service = new MemberService(context, _logger.Object);

        // Act
        var result = await service.GetAllAsync(page: 1, pageSize: 10, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();

        // Metadatos en vacío
        result.Value.TotalRecords.Should().Be(0);
        result.Value.TotalPages.Should().Be(0); // Math.Ceiling(0 / 10.0) es 0

        // Datos vacíos
        result.Value.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllPaginatedAsync_WithMultiplePages_ReturnsCorrectSubsetOfData()
    {
        // Arrange
        await using var context = TestDbContextFactory.Create();
        var service = new MemberService(context, _logger.Object);

        for (int i = 1; i <= 15; i++)
        {
            context.Members.Add(new Member($"Member {i}", $"test{i}@email.com", "1111-1111"));
        }
        await context.SaveChangesAsync();

        // Act 1: Pedimos la Página 1 con tamaño 10
        var resultPage1 = await service.GetAllAsync(page: 1, pageSize: 10, CancellationToken.None);

        // Assert 1
        resultPage1.Value.TotalRecords.Should().Be(15);
        resultPage1.Value.TotalPages.Should().Be(2); // 15 / 10 = 1.5 -> Ceil = 2
        resultPage1.Value.Data.Should().HaveCount(10); // Solo debe traer 10
        resultPage1.Value.Data.First().Name.Should().Be("Member 1");

        // Act 2: Pedimos la Página 2 con tamaño 10
        var resultPage2 = await service.GetAllAsync(page: 2, pageSize: 10, CancellationToken.None);

        // Assert 2
        resultPage2.Value.Data.Should().HaveCount(5); // Solo deben quedar 5
        resultPage2.Value.Data.First().Name.Should().Be("Member 11"); // Debe empezar donde terminó la anterior
    }

    [Fact]
    public async Task GetByPublicIdAsync_WithValidData_ReturnsSuccess()
    {
        await using var context = TestDbContextFactory.Create();
        var service = new MemberService(context, _logger.Object);

        var member = new Member("Test", "test@email.com", "1111-1111");
        context.Members.Add(member);
        await context.SaveChangesAsync();

        //Act
        var result = await service.GetByPublicIdAsync(member.PublicId, CancellationToken.None);

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
        var service = new MemberService(context, _logger.Object);

        var inexistentGuid = Guid.NewGuid();

        //Act
        var result = await service.GetByPublicIdAsync(inexistentGuid, CancellationToken.None);

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
        var service = new MemberService(context, _logger.Object);

        var type = new MembershipType("Basic", 9.99m, 1);
        context.MembershipTypes.Add(type);
        await context.SaveChangesAsync();

        var dto = new MemberCreateDto("testName", "testEmail@gmail.com", "testPhone", type.PublicId);

        //Act
        var result = await service.CreateAsync(dto, CancellationToken.None);

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

    [Fact]
    public async Task CreateAsync_AlreadyExistentEmail_ReturnsFailure()
    {
        await using var context = TestDbContextFactory.Create();
        var service = new MemberService(context, _logger.Object);


        var type = new MembershipType("Basic", 9.99m, 1);
        context.MembershipTypes.Add(type);
        await context.SaveChangesAsync();
        const string repeatedMail = "test@gmail.com";

        var otherMember = new Member("testName1", repeatedMail, "1111-1111");
        context.Members.Add(otherMember);
        await context.SaveChangesAsync();

        var dto = new MemberCreateDto("newName", repeatedMail, "1111-1111", type.PublicId);

        //Act
        var result = await service.CreateAsync(dto, CancellationToken.None);

        //Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be(MemberErrors.EmailAlreadyExists.Code);
    }

    [Fact]
    public async Task CreateAsync_InvalidMembershipTypePublicId_ReturnsFailure()
    {
        await using var context = TestDbContextFactory.Create();
        var service = new MemberService(context, _logger.Object);

        var inexistentGuid = Guid.NewGuid();
        var dto = new MemberCreateDto("testName", "test@gmail.com", "testPhone", inexistentGuid);

        //Act
        var result = await service.CreateAsync(dto, CancellationToken.None);

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
        var service = new MemberService(context, _logger.Object);

        var existentMember = new Member("test", "test@gmail.com", "1111-1111");
        context.Members.Add(existentMember);
        await context.SaveChangesAsync();

        var updatedMember = new MemberUpdateDto("test2", "test2@gmail.com", "1212-1212");

        //Act
        var result = await service.UpdateAsync(existentMember.PublicId, updatedMember, CancellationToken.None);

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
        var service = new MemberService(context, _logger.Object);
        
        var existentMember = new Member("test", "test@gmail.com", "1111-1111");
        context.Members.Add(existentMember);
        var secondMember = new Member("dupTest", "test2@gmail.com", "2222-2222");
        context.Members.Add(secondMember);
        await context.SaveChangesAsync();

        var updatedMember = new MemberUpdateDto("test2", "test2@gmail.com", "1212-1212");
        
        //Act
        var result = await service.UpdateAsync(existentMember.PublicId, updatedMember, CancellationToken.None);
        
        //Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be(MemberErrors.EmailAlreadyExists.Code);
    }

    [Fact]
    public async Task UpdateAsync_InvalidGuid_ReturnsFailure()
    {
        await using var context = TestDbContextFactory.Create();
        var service = new MemberService(context, _logger.Object);

        var existentGuid = Guid.NewGuid();
        var updatedMember = new MemberUpdateDto("test", "test@gmail.com", "1111-1111");
        
        //Act
        var result = await service.UpdateAsync(existentGuid, updatedMember, CancellationToken.None);
        
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
        var service = new MemberService(context, _logger.Object);

        
        var member = new Member("test", "test@gmail.com", "1111-1111");
        context.Members.Add(member);
        await context.SaveChangesAsync();
        
        //Act
        var result = await service.DeleteAsync(member.PublicId, CancellationToken.None);
        
        //Assert
        result.IsSuccess.Should().BeTrue();
        var count = context.Members.Count();
        count.Should().Be(0);
    }

    [Fact]
    public async Task DeleteAsync_InvalidGuid_ReturnsFailure()
    {
        await using var context = TestDbContextFactory.Create();
        var service = new MemberService(context, _logger.Object);
        
        var member = new Member("test", "test@gmail.com", "1111-1111");
        context.Members.Add(member);
        await context.SaveChangesAsync();
        
        var invalidGuid = Guid.NewGuid();
        
        //Act
        var result = await service.DeleteAsync(invalidGuid, CancellationToken.None);
        
        //Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be(MemberErrors.NotFound.Code);
        
        var count  = context.Members.Count();
        count.Should().Be(1);
    }
    #endregion
}