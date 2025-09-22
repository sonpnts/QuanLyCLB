using Microsoft.EntityFrameworkCore;
using QuanLyClb.Application.DTOs;
using QuanLyClb.Application.Extensions;
using QuanLyClb.Application.Interfaces;
using QuanLyClb.Application.Requests;
using QuanLyClb.Infrastructure.Persistence;

namespace QuanLyClb.Infrastructure.Services;

public class StudentService : IStudentService
{
    private readonly ApplicationDbContext _dbContext;

    public StudentService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<StudentDto> CreateAsync(CreateStudentRequest request, CancellationToken cancellationToken = default)
    {
        var entity = new Domain.Entities.Student
        {
            FullName = request.FullName,
            Email = request.Email.Trim().ToLowerInvariant(),
            PhoneNumber = request.PhoneNumber,
            DateOfBirth = request.DateOfBirth,
            Notes = request.Notes
        };

        _dbContext.Students.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return entity.ToDto();
    }

    public async Task<StudentDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Students.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        return entity?.ToDto();
    }

    public async Task<IReadOnlyCollection<StudentDto>> SearchAsync(string? keyword, CancellationToken cancellationToken = default)
    {
        keyword = keyword?.Trim().ToLowerInvariant();
        var query = _dbContext.Students.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(s => s.FullName.ToLower().Contains(keyword) || s.Email.ToLower().Contains(keyword));
        }

        var students = await query.OrderBy(s => s.FullName).ToListAsync(cancellationToken);
        return students.Select(s => s.ToDto()).ToList();
    }

    public async Task<StudentDto> UpdateAsync(UpdateStudentRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Students.FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);
        if (entity is null)
        {
            throw new KeyNotFoundException($"Student {request.Id} not found");
        }

        entity.FullName = request.FullName;
        entity.Email = request.Email.Trim().ToLowerInvariant();
        entity.PhoneNumber = request.PhoneNumber;
        entity.DateOfBirth = request.DateOfBirth;
        entity.Notes = request.Notes;
        entity.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return entity.ToDto();
    }

    public async Task ChangeStatusAsync(ChangeStudentStatusRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Students.FirstOrDefaultAsync(s => s.Id == request.StudentId, cancellationToken);
        if (entity is null)
        {
            throw new KeyNotFoundException($"Student {request.StudentId} not found");
        }

        entity.Status = request.Status;
        entity.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
