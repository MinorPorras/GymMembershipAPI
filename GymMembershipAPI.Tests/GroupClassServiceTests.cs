using FluentAssertions;
using GymMembershipAPI.API.DTOs.GroupClass;
using GymMembershipAPI.API.Services;
using GymMembershipAPI.Domain.Entities;
using GymMembershipAPI.Domain.Results;
using GymMembershipAPI.Tests.Helpers;
using Microsoft.Extensions.Logging;
using Moq;

namespace GymMembershipAPI.Tests;

public class GroupClassServiceTests
{
    private readonly Mock<ILogger<GroupClassService>> _logger = new();

    #region GetMethods

    [Fact]
    public async Task GetByPublicIdAsync_ValidData_ReturnsSuccess()
    {
        await using var context = TestDbContextFactory.Create();
        var service = new GroupClassService(_logger.Object, context);

        var groupClass = new GroupClass()
        {
            Name = "Test Group",
            Instructor = "Test Instructor",
            DateHour = DateTime.UtcNow.AddDays(3),
            MaxMembers = 2
        };
        context.GroupClasses.Add(groupClass);
        await context.SaveChangesAsync();

        //Act
        var result = await service.GetByPublicIdAsync(groupClass.PublicId);

        //Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().BeEquivalentTo(groupClass);
    }

    [Fact]
    public async Task GetByPublicIdAsync_InvalidPublicId_ReturnsFailure()
    {
        await using var context = TestDbContextFactory.Create();
        var service = new GroupClassService(_logger.Object, context);
        var inexistentGuid = Guid.NewGuid();

        //Act
        var result = await service.GetByPublicIdAsync(inexistentGuid);

        //Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be(GroupClassErrors.NotFound.Code);
    }

    [Fact]
    public async Task GetAllAsync_WithData_ReturnsSuccessAndList()
    {
        await using var context = TestDbContextFactory.Create();
        var service = new GroupClassService(_logger.Object, context);

        var groupClass = new GroupClass()
        {
            Name = "Test Group",
            Instructor = "Test Instructor",
            DateHour = DateTime.UtcNow.AddDays(3),
            MaxMembers = 2
        };
        context.GroupClasses.Add(groupClass);
        await context.SaveChangesAsync();

        //Act
        var result = await service.GetAllAsync();

        //Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Count.Should().Be(1);
        result.Value[0].Should().BeEquivalentTo(groupClass);
    }

    [Fact]
    public async Task GetAllAsync_EmptyContext_ReturnsSuccess()
    {
        await using var context = TestDbContextFactory.Create();
        var service = new GroupClassService(_logger.Object, context);

        //Act
        var result = await service.GetAllAsync();

        //Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Count.Should().Be(0);
    }

    [Fact]
    public async Task GetByDateAsync_ReturnsSuccess()
    {
        await using var context = TestDbContextFactory.Create();
        var service = new GroupClassService(_logger.Object, context);

        var searchedDate = DateTime.UtcNow;

        var groupClass = new GroupClass()
        {
            Name = "Test Group",
            Instructor = "Test Instructor",
            DateHour = searchedDate,
            MaxMembers = 2
        };
        context.GroupClasses.Add(groupClass);
        var groupClass2 = new GroupClass()
        {
            Name = "Test Group2",
            Instructor = "Test Instructor2",
            DateHour = searchedDate.AddDays(-3),
            MaxMembers = 2
        };
        context.GroupClasses.Add(groupClass2);
        await context.SaveChangesAsync();

        //Act
        var result = await service.GetByDateAsync(searchedDate);

        //Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Count.Should().Be(1);
        result.Value[0].Should().BeEquivalentTo(groupClass);
    }

    #endregion

    #region CreateAsyncTests

    [Fact]
    public async Task CreateAsync_ValidData_ReturnsSuccess()
    {
        await using var context = TestDbContextFactory.Create();
        var service = new GroupClassService(_logger.Object, context);
        var dto = new GroupClassRequestDto("test", "testInstructor", DateTime.UtcNow.AddDays(3), 1);

        //Act
        var result = await service.CreateAsync(dto);

        //Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be(dto.Name);
        result.Value.Instructor.Should().Be(dto.Instructor);
        result.Value.DateHour.Should().Be(dto.DateHour);
        result.Value.MaxMembers.Should().Be(dto.MaxMembers);
    }

    [Fact]
    public async Task CreateAsync_DuplicatedName_ReturnsFailure()
    {
        await using var context = TestDbContextFactory.Create();
        var service = new GroupClassService(_logger.Object, context);

        var duplicatedName = "Test Group";
        var groupClass = new GroupClass()
        {
            Name = duplicatedName,
            Instructor = "Test Instructor",
            DateHour = DateTime.UtcNow.AddDays(3),
            MaxMembers = 2
        };
        context.GroupClasses.Add(groupClass);
        await context.SaveChangesAsync();

        var dto = new GroupClassRequestDto(duplicatedName, "testInstructor", DateTime.UtcNow.AddDays(3), 1);

        //Act
        var result = await service.CreateAsync(dto);

        //Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be(GroupClassErrors.NameAlreadyExists.Code);

        var count = context.GroupClasses.Count();
        count.Should().Be(1);
    }

    [Fact]
    public async Task CreateAsync_DateOlderThanToday_ReturnsFailure()
    {
        await using var context = TestDbContextFactory.Create();
        var service = new GroupClassService(_logger.Object, context);
        var dto = new GroupClassRequestDto("test", "testInstructor", DateTime.UtcNow.AddDays(-1), 1);

        //Act
        var result = await service.CreateAsync(dto);

        //Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be(GroupClassErrors.InvalidDate.Code);
    }

    #endregion

    #region UpdateAsyncTests

    [Fact]
    public async Task UpdateAsync_ValidData_ReturnsSuccess()
    {
        await using var context = TestDbContextFactory.Create();
        var service = new GroupClassService(_logger.Object, context);

        var originalClass = new GroupClass()
        {
            Name = "Test Group",
            Instructor = "Test Instructor",
            DateHour = DateTime.UtcNow.AddDays(3),
            MaxMembers = 2
        };
        context.GroupClasses.Add(originalClass);
        await context.SaveChangesAsync();

        var dto = new GroupClassRequestDto("test", "testInstructor", DateTime.UtcNow.AddDays(3), 1);

        //Act
        var result = await service.UpdateAsync(originalClass.PublicId, dto);

        //Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be(dto.Name);
        result.Value.Instructor.Should().Be(dto.Instructor);
        result.Value.DateHour.Should().Be(dto.DateHour);
        result.Value.MaxMembers.Should().Be(dto.MaxMembers);
    }

    [Fact]
    public async Task UpdateAsync_InexistentPublicId_ReturnsFailure()
    {
        await using var context = TestDbContextFactory.Create();
        var service = new GroupClassService(_logger.Object, context);

        var inexistentPublicId = Guid.NewGuid();
        var dto = new GroupClassRequestDto("test", "testInstructor", DateTime.UtcNow.AddDays(3), 1);

        //Act
        var result = await service.UpdateAsync(inexistentPublicId, dto);

        //Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be(GroupClassErrors.NotFound.Code);
    }

    #endregion

    #region DeleteAsyncTests

    [Fact]
    public async Task DeleteAsync_ValidData_ReturnSuccess()
    {
        await using var context = TestDbContextFactory.Create();
        var service = new GroupClassService(_logger.Object, context);

        var originalClass = new GroupClass()
        {
            Name = "Test Group",
            Instructor = "Test Instructor",
            DateHour = DateTime.UtcNow.AddDays(3),
            MaxMembers = 2
        };
        context.GroupClasses.Add(originalClass);
        await context.SaveChangesAsync();

        //Act
        var result = await service.DeleteAsync(originalClass.PublicId);

        //Assert
        result.IsSuccess.Should().BeTrue();

        var count = context.GroupClasses.Count();
        count.Should().Be(0);
    }

    [Fact]
    public async Task DeleteAsync_InexistentPublicId_ReturnsSuccess()
    {
        await using var context = TestDbContextFactory.Create();
        var service = new GroupClassService(_logger.Object, context);

        var originalClass = new GroupClass()
        {
            Name = "Test Group",
            Instructor = "Test Instructor",
            DateHour = DateTime.UtcNow.AddDays(3),
            MaxMembers = 2
        };
        context.GroupClasses.Add(originalClass);
        await context.SaveChangesAsync();

        var inexistentPublicId = Guid.NewGuid();

        //Act
        var result = await service.DeleteAsync(inexistentPublicId);

        //Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be(GroupClassErrors.NotFound.Code);

        var count = context.GroupClasses.Count();
        count.Should().Be(1);
    }

    #endregion
}