using FluentAssertions;
using GymMembershipAPI.API.DTOs.MembershipType;
using GymMembershipAPI.API.Services;
using GymMembershipAPI.Domain.Results;
using GymMembershipAPI.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace GymMembershipAPI.Tests;

public class MembershipTypeServiceTests
{
    private readonly Mock<ILogger<MembershipTypeService>> _loggerMock = new();

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_WithValidData_ReturnsSuccess()
    {
        //ARRANGE
        await using var context = TestDbContextFactory.Create();
        var service = new MembershipTypeService(context, _loggerMock.Object);

        var dto = new MembershipTypeRequestDto(
            Name: "Mensual básico",
            Price: 29.99m,
            DurationMonths: 1
        );

        //ACT
        var result = await service.CreateAsync(dto);

        //ASSERT
        result.IsSuccess.Should().BeTrue();

        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be(dto.Name);
        result.Value.Price.Should().Be(dto.Price);
        result.Value.DurationMonths.Should().Be(dto.DurationMonths);
        result.Value.PublicId.Should().NotBeEmpty();

        //Verificar que realmente se guardó en la Db
        var savedEntity =
            await context.MembershipTypes.FirstOrDefaultAsync(x => x.PublicId == result.Value.PublicId);
        savedEntity.Should().NotBeNull();
        savedEntity.Name.Should().Be(result.Value.Name);
        savedEntity.Price.Should().Be(result.Value.Price);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task CreateAsync_WithInvalidName_ReturnFailure(string invalidName)
    {
        //Assert
        await using var context = TestDbContextFactory.Create();
        var service = new MembershipTypeService(context, _loggerMock.Object);

        var dto = new MembershipTypeRequestDto(invalidName, 29.99m, 1);

        // Act
        var result = await service.CreateAsync(dto);

        //Assert
        result.IsFailure.Should().BeTrue();

        result.Value.Should().BeNull();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be(MembershipTypeErrors.InvalidName.Code);

        var savedEntity =
            await context.MembershipTypes.FirstOrDefaultAsync(x => x.Name == invalidName);
        savedEntity.Should().BeNull();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100000)]
    public async Task CreateAsync_WithInvalidPrice_ReturnFailure(decimal invalidPrice)
    {
        //Arrange
        await using var context = TestDbContextFactory.Create();
        var service = new MembershipTypeService(context, _loggerMock.Object);

        var dto = new MembershipTypeRequestDto("TestName", invalidPrice, 1);

        //Act
        var result = await service.CreateAsync(dto);

        // Assert
        result.IsFailure.Should().BeTrue();

        result.Value.Should().BeNull();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be(MembershipTypeErrors.InvalidPrice.Code);

        var savedEntity =
            await context.MembershipTypes.FirstOrDefaultAsync(x => x.Price == invalidPrice);
        savedEntity.Should().BeNull();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100000)]
    public async Task CreateAsync_WithInvalidDurationMonths_ReturnFailure(int invalidDurationMonths)
    {
        //Arrange
        await using var context = TestDbContextFactory.Create();
        var service = new MembershipTypeService(context, _loggerMock.Object);

        var dto = new MembershipTypeRequestDto("TestName", 19.99m, invalidDurationMonths);

        //Act
        var result = await service.CreateAsync(dto);

        //Assert
        result.IsFailure.Should().BeTrue();

        result.Value.Should().BeNull();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be(MembershipTypeErrors.InvalidMonthDuration.Code);

        var savedEntity =
            await context.MembershipTypes.FirstOrDefaultAsync(x => x.DurationMonths == invalidDurationMonths);
        savedEntity.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateName_ReturnsFailure()
    {
        //Arrange
        await using var context = TestDbContextFactory.Create();
        var service = new MembershipTypeService(context, _loggerMock.Object);
        var original = new MembershipTypeRequestDto("TestName", 19.99m, 1);
        var duplicateNameDto = new MembershipTypeRequestDto("TestName", 29.99m, 12);

        await service.CreateAsync(original);

        //Act
        var result = await service.CreateAsync(duplicateNameDto);

        //Assert
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be(MembershipTypeErrors.AlreadyExists.Code);

        var savedEntity = await context.MembershipTypes.FirstOrDefaultAsync(x => x.Name == original.Name);
        savedEntity.Should().NotBeNull();
        savedEntity.Price.Should().Be(original.Price);
        savedEntity.DurationMonths.Should().Be(original.DurationMonths);

        var entityCount = await context.MembershipTypes.CountAsync();
        entityCount.Should().Be(1);
    }

    #endregion

    #region UpdateAsyncTests

    [Fact]
    public async Task UpdateAsync_WithValidData_ReturnsSuccess()
    {
        // Arrange
        await using var context = TestDbContextFactory.Create();
        var service = new MembershipTypeService(context, _loggerMock.Object);
        var addedEntity = new MembershipTypeRequestDto("TestName", 19.99m, 1);
        var updatedEntity = new MembershipTypeRequestDto("NewTestName", 29.99m, 2);

        var createdResult = await service.CreateAsync(addedEntity);

        //Act
        var result = await service.UpdateAsync(createdResult.Value.PublicId, updatedEntity);

        //Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.PublicId.Should().Be(createdResult.Value.PublicId);
        result.Value.Name.Should().Be(updatedEntity.Name);
        result.Value.Price.Should().Be(updatedEntity.Price);
        result.Value.DurationMonths.Should().Be(updatedEntity.DurationMonths);

        var savedEntity = await context.MembershipTypes.FirstOrDefaultAsync(x => x.Name == updatedEntity.Name);
        savedEntity.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateAsync_WithDuplicateName_ReturnsFailure()
    {
        //Arrange
        await using var context = TestDbContextFactory.Create();
        var service = new MembershipTypeService(context, _loggerMock.Object);
        var mainEntity = new MembershipTypeRequestDto("TestName", 19.99m, 1);
        var addedEntity2 = new MembershipTypeRequestDto("TestName2", 29.99m, 1);
        var updatedEntity = new MembershipTypeRequestDto("TestName2", 29.99m, 2);
        var createdResult = await service.CreateAsync(mainEntity);
        await service.CreateAsync(addedEntity2);

        //Act 
        var result = await service.UpdateAsync(createdResult.Value.PublicId, updatedEntity);

        //Assert
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be(MembershipTypeErrors.AlreadyExists.Code);

        var savedEntity = await context.MembershipTypes.FirstOrDefaultAsync(x => x.Name == mainEntity.Name);
        savedEntity.Should().NotBeNull();
        savedEntity.Name.Should().Be(mainEntity.Name);
        savedEntity.Price.Should().Be(mainEntity.Price);
        savedEntity.DurationMonths.Should().Be(mainEntity.DurationMonths);
    }

    [Fact]
    public async Task UpdateAsync_WithNonExistingPublicId_ReturnsFailure()
    {
        await using var context = TestDbContextFactory.Create();
        var service = new MembershipTypeService(context, _loggerMock.Object);
        var nonExistentId = Guid.NewGuid();
        var updateDto = new MembershipTypeRequestDto("TestName", 19.99m, 1);

        //Act
        var result = await service.UpdateAsync(nonExistentId, updateDto);

        //Assert
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be(MembershipTypeErrors.NotFound.Code);
    }

    #endregion

    #region GetMethods

    [Fact]
    public async Task GetAll_WithDataInDb_ReturnsSuccess()
    {
        await using var context = TestDbContextFactory.Create();
        var service = new MembershipTypeService(context, _loggerMock.Object);
        var dto1 = new MembershipTypeRequestDto("TestName", 19.99m, 1);
        var dto2 = new MembershipTypeRequestDto("TestName2", 29.99m, 2);
        await service.CreateAsync(dto1);
        await service.CreateAsync(dto2);

        //Act
        var result = await service.GetAllAsync();

        //Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Count.Should().Be(2);
        result.Value[0].Name.Should().Be("TestName");
        result.Value[0].Price.Should().Be(19.99m);
        result.Value[0].DurationMonths.Should().Be(1);
        result.Value[1].Name.Should().Be("TestName2");
        result.Value[1].Price.Should().Be(29.99m);
        result.Value[1].DurationMonths.Should().Be(2);
    }

    [Fact]
    public async Task Getall_WithEmptyDb_ReturnsSuccess()
    {
        await using var context = TestDbContextFactory.Create();
        var service = new MembershipTypeService(context, _loggerMock.Object);

        //Act
        var result = await service.GetAllAsync();

        //Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Count.Should().Be(0);
    }

    [Fact]
    public async Task GetById_ValidPublicId_ReturnsSuccess()
    {
        await using var context = TestDbContextFactory.Create();
        var service = new MembershipTypeService(context, _loggerMock.Object);
        var data = new MembershipTypeRequestDto("TestName", 19.99m, 1);
        var createdData = await service.CreateAsync(data);

        //Act
        var result = await service.GetByPublicIdAsync(createdData.Value.PublicId);

        //Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.PublicId.Should().Be(createdData.Value.PublicId);
    }

    [Fact]
    public async Task GetById_InvalidPublicId_ReturnsFailure()
    {
        await using var context = TestDbContextFactory.Create();
        var service = new MembershipTypeService(context, _loggerMock.Object);
        var inexistentId = Guid.NewGuid();

        //Act
        var result = await service.GetByPublicIdAsync(inexistentId);

        //Assert
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be(MembershipTypeErrors.NotFound.Code);
    }

    #endregion

    #region DeleteAsyncTests

    [Fact]
    public async Task DeleteAsync_ValidPublicId_ReturnsSuccess()
    {
        await using var context = TestDbContextFactory.Create();
        var service = new MembershipTypeService(context, _loggerMock.Object);
        var data = new MembershipTypeRequestDto("TestName", 19.99m, 1);
        var createdData = await service.CreateAsync(data);

        //Act
        var result = await service.DeleteAsync(createdData.Value.PublicId);

        //Assert
        result.IsSuccess.Should().BeTrue();

        var existingEntity = await context.MembershipTypes.FirstOrDefaultAsync(x => x.Name == data.Name);
        existingEntity.Should().BeNull();
    }


    [Fact]
    public async Task DeleteAsync_InvalidPublicId_ReturnsFailureAndNotDeleteAnything()
    {
        await using var context = TestDbContextFactory.Create();
        var service = new MembershipTypeService(context, _loggerMock.Object);
        var data = new MembershipTypeRequestDto("TestName", 19.99m, 1);
        var createdData = await service.CreateAsync(data);
        var nonExistentId = Guid.NewGuid();

        //Act
        var result = await service.DeleteAsync(nonExistentId);

        //Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be(MembershipTypeErrors.NotFound.Code);

        var existingEntity = await context.MembershipTypes.FirstOrDefaultAsync(x => x.Name == data.Name);
        existingEntity.Should().NotBeNull();
        existingEntity.PublicId.Should().Be(createdData.Value.PublicId);
        existingEntity.Name.Should().Be(data.Name);
        existingEntity.Price.Should().Be(data.Price);
        existingEntity.DurationMonths.Should().Be(1);
    }

    #endregion
}