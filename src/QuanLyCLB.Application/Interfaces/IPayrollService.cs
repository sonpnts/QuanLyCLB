using QuanLyCLB.Application.DTOs;

namespace QuanLyCLB.Application.Interfaces;

public interface IPayrollService
{
    Task<PayrollPeriodDto> GeneratePayrollAsync(GeneratePayrollRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<PayrollPeriodDto>> GetPayrollsAsync(Guid coachId, CancellationToken cancellationToken = default);
    Task<PayrollPeriodDto?> GetPayrollByIdAsync(Guid payrollId, CancellationToken cancellationToken = default);
}
