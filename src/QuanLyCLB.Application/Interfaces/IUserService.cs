using System;
using System.Collections.Generic;
using System.Threading;
using QuanLyCLB.Application.DTOs;

namespace QuanLyCLB.Application.Interfaces;

public interface IUserService
{
    Task<IReadOnlyCollection<UserDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<UserDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<UserDto> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default);

    Task<UserDto?> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    Task<UserDto?> ChangeEmailAsync(Guid id, ChangeUserEmailRequest request, CancellationToken cancellationToken = default);

    Task<UserDto?> UpdateFullNameAsync(Guid id, UpdateUserNameRequest request, CancellationToken cancellationToken = default);

    Task SendPasswordResetOtpAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default);

    Task<bool> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default);
}
