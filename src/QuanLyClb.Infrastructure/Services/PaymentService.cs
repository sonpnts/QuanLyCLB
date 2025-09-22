using Microsoft.EntityFrameworkCore;
using QuanLyClb.Application.DTOs;
using QuanLyClb.Application.Extensions;
using QuanLyClb.Application.Interfaces;
using QuanLyClb.Application.Requests;
using QuanLyClb.Infrastructure.Persistence;

namespace QuanLyClb.Infrastructure.Services;

public class PaymentService : IPaymentService
{
    private readonly ApplicationDbContext _dbContext;

    public PaymentService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<TuitionPaymentDto> RecordPaymentAsync(RecordPaymentRequest request, CancellationToken cancellationToken = default)
    {
        var studentExists = await _dbContext.Students.AnyAsync(s => s.Id == request.StudentId, cancellationToken);
        if (!studentExists)
        {
            throw new KeyNotFoundException($"Student {request.StudentId} not found");
        }

        var classExists = await _dbContext.Classes.AnyAsync(c => c.Id == request.ClassId, cancellationToken);
        if (!classExists)
        {
            throw new KeyNotFoundException($"Class {request.ClassId} not found");
        }

        if (request.CollectedById.HasValue)
        {
            var collectorExists = await _dbContext.Users.AnyAsync(u => u.Id == request.CollectedById, cancellationToken);
            if (!collectorExists)
            {
                throw new KeyNotFoundException($"Collector {request.CollectedById} not found");
            }
        }

        var payment = new Domain.Entities.TuitionPayment
        {
            StudentId = request.StudentId,
            ClassId = request.ClassId,
            Amount = request.Amount,
            CollectedById = request.CollectedById,
            Notes = request.Notes
        };

        _dbContext.TuitionPayments.Add(payment);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return payment.ToDto();
    }

    public async Task<decimal> CloseoutAsync(CloseoutPaymentRequest request, CancellationToken cancellationToken = default)
    {
        var payments = await _dbContext.TuitionPayments
            .Where(p => p.CollectedById == request.CollectorId && p.PaidAt >= request.From && p.PaidAt <= request.To)
            .ToListAsync(cancellationToken);

        return payments.Sum(p => p.Amount);
    }
}
