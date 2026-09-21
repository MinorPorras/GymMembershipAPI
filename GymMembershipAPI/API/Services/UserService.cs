using GymMembershipAPI.API.DTOs.User;
using GymMembershipAPI.Domain.Entities;
using GymMembershipAPI.Domain.Enums;
using GymMembershipAPI.Domain.Interfaces;
using GymMembershipAPI.Domain.Results;
using GymMembershipAPI.Infraestructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GymMembershipAPI.API.Services;

public class UserService : IUserService
{
    private readonly GymDbContext _context;
    private readonly ILogger<UserService> _logger;

    public UserService(GymDbContext context, ILogger<UserService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<User>> GetByPublicIdAsync(Guid publicId)
    {
        var user = await _context.Users
            .Include(u => u.Member)
            .FirstOrDefaultAsync(u => u.PublicId == publicId);
        return user == null
            ? Result<User>.Failure(AuthErrors.UserNotFound)
            : Result<User>.Success(user);
    }

    public async Task<Result<User>> CreateAsync(UserCreateRequestDto dto)
    {
        var emailExists = await _context.Users.AnyAsync(u => u.Email == dto.Email);
        if (emailExists) return Result<User>.Failure(AuthErrors.UserAlreadyExists);

        if (!Enum.TryParse<UserRole>(dto.Role, ignoreCase: true, out var parsedRole))
            return Result<User>.Failure(
                Error.Unknown("El rol proporcionado no es válido. Use 'Admin', 'Staff' o 'Member'."));


        int? memberId = null;
        if (dto.MemberPublicId.HasValue)
        {
            var member = await _context.Members
                .FirstOrDefaultAsync(m => m.PublicId == dto.MemberPublicId);

            if (member == null)
                return Result<User>.Failure(MemberErrors.NotFound);

            memberId = member.Id;
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password, 12);

        var user = parsedRole == UserRole.Member && memberId.HasValue
            ? User.CreateMemberUser(dto.Email, passwordHash, memberId.Value)
            : User.CreateStaffUser(dto.Email, passwordHash, parsedRole);

        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return Result<User>.Success(user);
    }
}