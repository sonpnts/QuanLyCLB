using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using QuanLyCLB.Application.DTOs;

namespace QuanLyCLB.Application.Interfaces;

public interface IBranchService
{
    Task<IReadOnlyCollection<BranchDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<BranchDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<BranchDto> CreateAsync(CreateBranchRequest request, CancellationToken cancellationToken = default);
    Task<BranchDto?> UpdateAsync(Guid id, UpdateBranchRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
