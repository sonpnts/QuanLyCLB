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
    public class ClassesController : ControllerBase
    {
        private readonly QuanLyCLBContext _context;

        public ClassesController(QuanLyCLBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ClassDto>>> GetClasses()
        {
            var classes = await _context.Classes
                .Include(c => c.Trainer)
                .Include(c => c.Assistant)
                .Include(c => c.Enrollments)
                .Select(c => new ClassDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    MaxStudents = c.MaxStudents,
                    FeePerMonth = c.FeePerMonth,
                    Status = c.Status.ToString(),
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    Trainer = new UserDto
                    {
                        Id = c.Trainer.Id,
                        FullName = c.Trainer.FullName,
                        Email = c.Trainer.Email,
                        Role = c.Trainer.Role.ToString()
                    },
                    Assistant = c.Assistant != null ? new UserDto
                    {
                        Id = c.Assistant.Id,
                        FullName = c.Assistant.FullName,
                        Email = c.Assistant.Email,
                        Role = c.Assistant.Role.ToString()
                    } : null,
                    CurrentStudentCount = c.Enrollments.Count(e => e.Status == EnrollmentStatus.Active),
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();

            return Ok(classes);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ClassDto>> GetClass(int id)
        {
            var classEntity = await _context.Classes
                .Include(c => c.Trainer)
                .Include(c => c.Assistant)
                .Include(c => c.Enrollments)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (classEntity == null)
            {
                return NotFound();
            }

            var classDto = new ClassDto
            {
                Id = classEntity.Id,
                Name = classEntity.Name,
                Description = classEntity.Description,
                MaxStudents = classEntity.MaxStudents,
                FeePerMonth = classEntity.FeePerMonth,
                Status = classEntity.Status.ToString(),
                StartDate = classEntity.StartDate,
                EndDate = classEntity.EndDate,
                Trainer = new UserDto
                {
                    Id = classEntity.Trainer.Id,
                    FullName = classEntity.Trainer.FullName,
                    Email = classEntity.Trainer.Email,
                    Role = classEntity.Trainer.Role.ToString()
                },
                Assistant = classEntity.Assistant != null ? new UserDto
                {
                    Id = classEntity.Assistant.Id,
                    FullName = classEntity.Assistant.FullName,
                    Email = classEntity.Assistant.Email,
                    Role = classEntity.Assistant.Role.ToString()
                } : null,
                CurrentStudentCount = classEntity.Enrollments.Count(e => e.Status == EnrollmentStatus.Active),
                CreatedAt = classEntity.CreatedAt
            };

            return Ok(classDto);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Trainer")]
        public async Task<ActionResult<ClassDto>> CreateClass(CreateClassDto createClassDto)
        {
            // Verify trainer exists
            var trainer = await _context.Users.FindAsync(createClassDto.TrainerId);
            if (trainer == null || trainer.Role != UserRole.Trainer)
            {
                return BadRequest("Invalid trainer");
            }

            // Verify assistant exists if provided
            User? assistant = null;
            if (createClassDto.AssistantId.HasValue)
            {
                assistant = await _context.Users.FindAsync(createClassDto.AssistantId.Value);
                if (assistant == null || assistant.Role != UserRole.Assistant)
                {
                    return BadRequest("Invalid assistant");
                }
            }

            var classEntity = new Class
            {
                Name = createClassDto.Name,
                Description = createClassDto.Description,
                MaxStudents = createClassDto.MaxStudents,
                FeePerMonth = createClassDto.FeePerMonth,
                StartDate = createClassDto.StartDate,
                EndDate = createClassDto.EndDate,
                TrainerId = createClassDto.TrainerId,
                AssistantId = createClassDto.AssistantId
            };

            _context.Classes.Add(classEntity);
            await _context.SaveChangesAsync();

            // Load the created class with related data
            var createdClass = await _context.Classes
                .Include(c => c.Trainer)
                .Include(c => c.Assistant)
                .FirstAsync(c => c.Id == classEntity.Id);

            var classDto = new ClassDto
            {
                Id = createdClass.Id,
                Name = createdClass.Name,
                Description = createdClass.Description,
                MaxStudents = createdClass.MaxStudents,
                FeePerMonth = createdClass.FeePerMonth,
                Status = createdClass.Status.ToString(),
                StartDate = createdClass.StartDate,
                EndDate = createdClass.EndDate,
                Trainer = new UserDto
                {
                    Id = createdClass.Trainer.Id,
                    FullName = createdClass.Trainer.FullName,
                    Email = createdClass.Trainer.Email,
                    Role = createdClass.Trainer.Role.ToString()
                },
                Assistant = createdClass.Assistant != null ? new UserDto
                {
                    Id = createdClass.Assistant.Id,
                    FullName = createdClass.Assistant.FullName,
                    Email = createdClass.Assistant.Email,
                    Role = createdClass.Assistant.Role.ToString()
                } : null,
                CurrentStudentCount = 0,
                CreatedAt = createdClass.CreatedAt
            };

            return CreatedAtAction(nameof(GetClass), new { id = classEntity.Id }, classDto);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Trainer")]
        public async Task<IActionResult> UpdateClass(int id, UpdateClassDto updateClassDto)
        {
            var classEntity = await _context.Classes.FindAsync(id);
            if (classEntity == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(updateClassDto.Name))
                classEntity.Name = updateClassDto.Name;
            
            if (!string.IsNullOrEmpty(updateClassDto.Description))
                classEntity.Description = updateClassDto.Description;
            
            if (updateClassDto.MaxStudents.HasValue)
                classEntity.MaxStudents = updateClassDto.MaxStudents.Value;
            
            if (updateClassDto.FeePerMonth.HasValue)
                classEntity.FeePerMonth = updateClassDto.FeePerMonth.Value;
            
            if (updateClassDto.StartDate.HasValue)
                classEntity.StartDate = updateClassDto.StartDate.Value;
            
            if (updateClassDto.EndDate.HasValue)
                classEntity.EndDate = updateClassDto.EndDate;
            
            if (updateClassDto.TrainerId.HasValue)
                classEntity.TrainerId = updateClassDto.TrainerId.Value;
            
            if (updateClassDto.AssistantId.HasValue)
                classEntity.AssistantId = updateClassDto.AssistantId;
            
            if (updateClassDto.Status.HasValue)
                classEntity.Status = (ClassStatus)updateClassDto.Status.Value;

            classEntity.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteClass(int id)
        {
            var classEntity = await _context.Classes.FindAsync(id);
            if (classEntity == null)
            {
                return NotFound();
            }

            _context.Classes.Remove(classEntity);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost("{id}/copy")]
        [Authorize(Roles = "Admin,Trainer")]
        public async Task<ActionResult<ClassDto>> CopyClass(int id, [FromBody] string newName)
        {
            var originalClass = await _context.Classes
                .Include(c => c.Trainer)
                .Include(c => c.Assistant)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (originalClass == null)
            {
                return NotFound();
            }

            var copiedClass = new Class
            {
                Name = newName,
                Description = originalClass.Description,
                MaxStudents = originalClass.MaxStudents,
                FeePerMonth = originalClass.FeePerMonth,
                StartDate = DateTime.UtcNow,
                EndDate = originalClass.EndDate?.AddYears(1),
                TrainerId = originalClass.TrainerId,
                AssistantId = originalClass.AssistantId,
                Status = ClassStatus.Active
            };

            _context.Classes.Add(copiedClass);
            await _context.SaveChangesAsync();

            // Load the created class with related data
            var createdClass = await _context.Classes
                .Include(c => c.Trainer)
                .Include(c => c.Assistant)
                .FirstAsync(c => c.Id == copiedClass.Id);

            var classDto = new ClassDto
            {
                Id = createdClass.Id,
                Name = createdClass.Name,
                Description = createdClass.Description,
                MaxStudents = createdClass.MaxStudents,
                FeePerMonth = createdClass.FeePerMonth,
                Status = createdClass.Status.ToString(),
                StartDate = createdClass.StartDate,
                EndDate = createdClass.EndDate,
                Trainer = new UserDto
                {
                    Id = createdClass.Trainer.Id,
                    FullName = createdClass.Trainer.FullName,
                    Email = createdClass.Trainer.Email,
                    Role = createdClass.Trainer.Role.ToString()
                },
                Assistant = createdClass.Assistant != null ? new UserDto
                {
                    Id = createdClass.Assistant.Id,
                    FullName = createdClass.Assistant.FullName,
                    Email = createdClass.Assistant.Email,
                    Role = createdClass.Assistant.Role.ToString()
                } : null,
                CurrentStudentCount = 0,
                CreatedAt = createdClass.CreatedAt
            };

            return CreatedAtAction(nameof(GetClass), new { id = copiedClass.Id }, classDto);
        }
    }
}