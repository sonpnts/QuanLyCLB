using Microsoft.EntityFrameworkCore;
using QuanLyClb.Application.DTOs;
using QuanLyClb.Application.Extensions;
using QuanLyClb.Application.Interfaces;
using QuanLyClb.Application.Requests;
using QuanLyClb.Domain.Enums;
using QuanLyClb.Infrastructure.Persistence;

namespace QuanLyClb.Infrastructure.Services;

public class EnrollmentService : IEnrollmentService
{
    private readonly ApplicationDbContext _dbContext;

    public EnrollmentService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<EnrollmentDto> EnrollAsync(EnrollStudentRequest request, CancellationToken cancellationToken = default)
    {
        var student = await _dbContext.Students.FirstOrDefaultAsync(s => s.Id == request.StudentId, cancellationToken);
        if (student is null)
        {
            throw new KeyNotFoundException($"Student {request.StudentId} not found");
        }

        var trainingClass = await _dbContext.Classes.Include(c => c.Enrollments)
            .FirstOrDefaultAsync(c => c.Id == request.ClassId, cancellationToken);
        if (trainingClass is null)
        {
            throw new KeyNotFoundException($"Class {request.ClassId} not found");
        }

        if (trainingClass.Enrollments.Count >= trainingClass.Capacity)
        {
            throw new InvalidOperationException($"Class {trainingClass.Name} is full");
        }

        var enrollment = new Domain.Entities.Enrollment
        {
            StudentId = student.Id,
            ClassId = trainingClass.Id
        };

        student.CurrentClassId = trainingClass.Id;
        student.Status = Domain.Enums.StudentStatus.Active;
        trainingClass.Enrollments.Add(enrollment);

        await _dbContext.SaveChangesAsync(cancellationToken);
        return enrollment.ToDto();
    }

    public async Task TransferAsync(TransferStudentRequest request, CancellationToken cancellationToken = default)
    {
        var enrollment = await _dbContext.Enrollments.FirstOrDefaultAsync(e => e.StudentId == request.StudentId && e.ClassId == request.FromClassId, cancellationToken);
        if (enrollment is null)
        {
            throw new KeyNotFoundException("Enrollment not found");
        }

        var targetClass = await _dbContext.Classes.Include(c => c.Enrollments)
            .FirstOrDefaultAsync(c => c.Id == request.ToClassId, cancellationToken);
        if (targetClass is null)
        {
            throw new KeyNotFoundException($"Target class {request.ToClassId} not found");
        }

        if (targetClass.Enrollments.Count >= targetClass.Capacity)
        {
            throw new InvalidOperationException("Target class is full");
        }

        enrollment.Status = EnrollmentStatus.Transferred;
        enrollment.UpdatedAt = DateTime.UtcNow;

        var newEnrollment = new Domain.Entities.Enrollment
        {
            StudentId = request.StudentId,
            ClassId = targetClass.Id
        };

        targetClass.Enrollments.Add(newEnrollment);

        var student = await _dbContext.Students.FirstAsync(s => s.Id == request.StudentId, cancellationToken);
        student.CurrentClassId = targetClass.Id;
        student.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
