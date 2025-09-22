using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using QuanLyCLB.API.Data;
using QuanLyCLB.API.Models;
using QuanLyCLB.API.DTOs;

namespace QuanLyCLB.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EnrollmentsController : ControllerBase
    {
        private readonly QuanLyCLBContext _context;

        public EnrollmentsController(QuanLyCLBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<EnrollmentDto>>> GetEnrollments()
        {
            var enrollments = await _context.Enrollments
                .Include(e => e.Student)
                .Include(e => e.Class)
                .ThenInclude(c => c.Trainer)
                .Select(e => new EnrollmentDto
                {
                    Id = e.Id,
                    Student = new StudentDto
                    {
                        Id = e.Student.Id,
                        FullName = e.Student.FullName,
                        Email = e.Student.Email,
                        PhoneNumber = e.Student.PhoneNumber,
                        DateOfBirth = e.Student.DateOfBirth,
                        Status = e.Student.Status.ToString()
                    },
                    Class = new ClassDto
                    {
                        Id = e.Class.Id,
                        Name = e.Class.Name,
                        FeePerMonth = e.Class.FeePerMonth,
                        Status = e.Class.Status.ToString(),
                        Trainer = new UserDto
                        {
                            Id = e.Class.Trainer.Id,
                            FullName = e.Class.Trainer.FullName,
                            Email = e.Class.Trainer.Email,
                            Role = e.Class.Trainer.Role.ToString()
                        }
                    },
                    EnrollmentDate = e.EnrollmentDate,
                    Status = e.Status.ToString(),
                    StartDate = e.StartDate,
                    EndDate = e.EndDate,
                    Notes = e.Notes,
                    CreatedAt = e.CreatedAt
                })
                .ToListAsync();

            return Ok(enrollments);
        }

        [HttpGet("class/{classId}")]
        public async Task<ActionResult<IEnumerable<EnrollmentDto>>> GetEnrollmentsByClass(int classId)
        {
            var enrollments = await _context.Enrollments
                .Include(e => e.Student)
                .Include(e => e.Class)
                .ThenInclude(c => c.Trainer)
                .Where(e => e.ClassId == classId && e.Status == EnrollmentStatus.Active)
                .Select(e => new EnrollmentDto
                {
                    Id = e.Id,
                    Student = new StudentDto
                    {
                        Id = e.Student.Id,
                        FullName = e.Student.FullName,
                        Email = e.Student.Email,
                        PhoneNumber = e.Student.PhoneNumber,
                        DateOfBirth = e.Student.DateOfBirth,
                        Status = e.Student.Status.ToString()
                    },
                    Class = new ClassDto
                    {
                        Id = e.Class.Id,
                        Name = e.Class.Name,
                        FeePerMonth = e.Class.FeePerMonth,
                        Status = e.Class.Status.ToString(),
                        Trainer = new UserDto
                        {
                            Id = e.Class.Trainer.Id,
                            FullName = e.Class.Trainer.FullName,
                            Email = e.Class.Trainer.Email,
                            Role = e.Class.Trainer.Role.ToString()
                        }
                    },
                    EnrollmentDate = e.EnrollmentDate,
                    Status = e.Status.ToString(),
                    StartDate = e.StartDate,
                    EndDate = e.EndDate,
                    Notes = e.Notes,
                    CreatedAt = e.CreatedAt
                })
                .ToListAsync();

            return Ok(enrollments);
        }

        [HttpPost]
        public async Task<ActionResult<EnrollmentDto>> CreateEnrollment(CreateEnrollmentDto createEnrollmentDto)
        {
            // Check if student exists
            var student = await _context.Students.FindAsync(createEnrollmentDto.StudentId);
            if (student == null)
            {
                return BadRequest("Student not found");
            }

            // Check if class exists
            var classEntity = await _context.Classes.FindAsync(createEnrollmentDto.ClassId);
            if (classEntity == null)
            {
                return BadRequest("Class not found");
            }

            // Check if student is already enrolled in this class
            var existingEnrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.StudentId == createEnrollmentDto.StudentId && 
                                        e.ClassId == createEnrollmentDto.ClassId && 
                                        e.Status == EnrollmentStatus.Active);
            
            if (existingEnrollment != null)
            {
                return BadRequest("Student is already enrolled in this class");
            }

            // Check class capacity
            var currentEnrollmentCount = await _context.Enrollments
                .CountAsync(e => e.ClassId == createEnrollmentDto.ClassId && e.Status == EnrollmentStatus.Active);
            
            if (currentEnrollmentCount >= classEntity.MaxStudents)
            {
                return BadRequest("Class is at maximum capacity");
            }

            var enrollment = new Enrollment
            {
                StudentId = createEnrollmentDto.StudentId,
                ClassId = createEnrollmentDto.ClassId,
                StartDate = createEnrollmentDto.StartDate,
                EndDate = createEnrollmentDto.EndDate,
                Notes = createEnrollmentDto.Notes
            };

            _context.Enrollments.Add(enrollment);
            await _context.SaveChangesAsync();

            // Load the created enrollment with related data
            var createdEnrollment = await _context.Enrollments
                .Include(e => e.Student)
                .Include(e => e.Class)
                .ThenInclude(c => c.Trainer)
                .FirstAsync(e => e.Id == enrollment.Id);

            var enrollmentDto = new EnrollmentDto
            {
                Id = createdEnrollment.Id,
                Student = new StudentDto
                {
                    Id = createdEnrollment.Student.Id,
                    FullName = createdEnrollment.Student.FullName,
                    Email = createdEnrollment.Student.Email,
                    PhoneNumber = createdEnrollment.Student.PhoneNumber,
                    DateOfBirth = createdEnrollment.Student.DateOfBirth,
                    Status = createdEnrollment.Student.Status.ToString()
                },
                Class = new ClassDto
                {
                    Id = createdEnrollment.Class.Id,
                    Name = createdEnrollment.Class.Name,
                    FeePerMonth = createdEnrollment.Class.FeePerMonth,
                    Status = createdEnrollment.Class.Status.ToString(),
                    Trainer = new UserDto
                    {
                        Id = createdEnrollment.Class.Trainer.Id,
                        FullName = createdEnrollment.Class.Trainer.FullName,
                        Email = createdEnrollment.Class.Trainer.Email,
                        Role = createdEnrollment.Class.Trainer.Role.ToString()
                    }
                },
                EnrollmentDate = createdEnrollment.EnrollmentDate,
                Status = createdEnrollment.Status.ToString(),
                StartDate = createdEnrollment.StartDate,
                EndDate = createdEnrollment.EndDate,
                Notes = createdEnrollment.Notes,
                CreatedAt = createdEnrollment.CreatedAt
            };

            return CreatedAtAction(nameof(GetEnrollments), enrollmentDto);
        }

        [HttpPost("transfer")]
        public async Task<ActionResult> TransferStudent(TransferStudentDto transferDto)
        {
            // Find current enrollment
            var currentEnrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.StudentId == transferDto.StudentId && 
                                        e.ClassId == transferDto.FromClassId && 
                                        e.Status == EnrollmentStatus.Active);
            
            if (currentEnrollment == null)
            {
                return BadRequest("Current enrollment not found");
            }

            // Check if target class exists
            var targetClass = await _context.Classes.FindAsync(transferDto.ToClassId);
            if (targetClass == null)
            {
                return BadRequest("Target class not found");
            }

            // Check target class capacity
            var targetClassEnrollmentCount = await _context.Enrollments
                .CountAsync(e => e.ClassId == transferDto.ToClassId && e.Status == EnrollmentStatus.Active);
            
            if (targetClassEnrollmentCount >= targetClass.MaxStudents)
            {
                return BadRequest("Target class is at maximum capacity");
            }

            // Update current enrollment status
            currentEnrollment.Status = EnrollmentStatus.Transferred;
            currentEnrollment.EndDate = DateTime.UtcNow;
            currentEnrollment.Notes = transferDto.Notes;
            currentEnrollment.UpdatedAt = DateTime.UtcNow;

            // Create new enrollment
            var newEnrollment = new Enrollment
            {
                StudentId = transferDto.StudentId,
                ClassId = transferDto.ToClassId,
                StartDate = DateTime.UtcNow,
                Notes = $"Transferred from class {transferDto.FromClassId}. {transferDto.Notes}"
            };

            _context.Enrollments.Add(newEnrollment);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Student transferred successfully" });
        }

        [HttpPut("{id}/complete")]
        public async Task<ActionResult> CompleteEnrollment(int id)
        {
            var enrollment = await _context.Enrollments.FindAsync(id);
            if (enrollment == null)
            {
                return NotFound();
            }

            enrollment.Status = EnrollmentStatus.Completed;
            enrollment.EndDate = DateTime.UtcNow;
            enrollment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(new { Message = "Enrollment completed successfully" });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteEnrollment(int id)
        {
            var enrollment = await _context.Enrollments.FindAsync(id);
            if (enrollment == null)
            {
                return NotFound();
            }

            _context.Enrollments.Remove(enrollment);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}