using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using QuanLyCLB.API.Data;
using QuanLyCLB.API.Models;
using QuanLyCLB.API.DTOs;
using System.Security.Claims;

namespace QuanLyCLB.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AttendanceController : ControllerBase
    {
        private readonly QuanLyCLBContext _context;
        private readonly IWebHostEnvironment _environment;

        public AttendanceController(QuanLyCLBContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        [HttpGet("class/{classId}")]
        public async Task<ActionResult<IEnumerable<AttendanceDto>>> GetAttendanceByClass(int classId, DateTime? date = null)
        {
            var query = _context.Attendances
                .Include(a => a.Student)
                .Include(a => a.Class)
                .Include(a => a.RecordedBy)
                .Where(a => a.ClassId == classId);

            if (date.HasValue)
            {
                query = query.Where(a => a.AttendanceDate.Date == date.Value.Date);
            }

            var attendances = await query
                .Select(a => new AttendanceDto
                {
                    Id = a.Id,
                    Student = new StudentDto
                    {
                        Id = a.Student.Id,
                        FullName = a.Student.FullName,
                        Email = a.Student.Email,
                        Status = a.Student.Status.ToString()
                    },
                    Class = new ClassDto
                    {
                        Id = a.Class.Id,
                        Name = a.Class.Name
                    },
                    RecordedBy = new UserDto
                    {
                        Id = a.RecordedBy.Id,
                        FullName = a.RecordedBy.FullName,
                        Role = a.RecordedBy.Role.ToString()
                    },
                    AttendanceDate = a.AttendanceDate,
                    Status = a.Status.ToString(),
                    Notes = a.Notes,
                    PhotoUrl = a.PhotoUrl,
                    CreatedAt = a.CreatedAt
                })
                .OrderBy(a => a.AttendanceDate)
                .ThenBy(a => a.Student.FullName)
                .ToListAsync();

            return Ok(attendances);
        }

        [HttpGet("student/{studentId}")]
        public async Task<ActionResult<IEnumerable<AttendanceDto>>> GetAttendanceByStudent(int studentId)
        {
            var attendances = await _context.Attendances
                .Include(a => a.Student)
                .Include(a => a.Class)
                .Include(a => a.RecordedBy)
                .Where(a => a.StudentId == studentId)
                .Select(a => new AttendanceDto
                {
                    Id = a.Id,
                    Student = new StudentDto
                    {
                        Id = a.Student.Id,
                        FullName = a.Student.FullName,
                        Email = a.Student.Email,
                        Status = a.Student.Status.ToString()
                    },
                    Class = new ClassDto
                    {
                        Id = a.Class.Id,
                        Name = a.Class.Name
                    },
                    RecordedBy = new UserDto
                    {
                        Id = a.RecordedBy.Id,
                        FullName = a.RecordedBy.FullName,
                        Role = a.RecordedBy.Role.ToString()
                    },
                    AttendanceDate = a.AttendanceDate,
                    Status = a.Status.ToString(),
                    Notes = a.Notes,
                    PhotoUrl = a.PhotoUrl,
                    CreatedAt = a.CreatedAt
                })
                .OrderByDescending(a => a.AttendanceDate)
                .ToListAsync();

            return Ok(attendances);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Trainer,Assistant")]
        public async Task<ActionResult<AttendanceDto>> CreateAttendance(CreateAttendanceDto createAttendanceDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int recordedById))
            {
                return Unauthorized();
            }

            // Check if attendance already exists for this student, class and date
            var existingAttendance = await _context.Attendances
                .FirstOrDefaultAsync(a => a.StudentId == createAttendanceDto.StudentId && 
                                        a.ClassId == createAttendanceDto.ClassId && 
                                        a.AttendanceDate.Date == createAttendanceDto.AttendanceDate.Date);
            
            if (existingAttendance != null)
            {
                return BadRequest("Attendance already recorded for this student on this date");
            }

            var attendance = new Attendance
            {
                StudentId = createAttendanceDto.StudentId,
                ClassId = createAttendanceDto.ClassId,
                RecordedById = recordedById,
                AttendanceDate = createAttendanceDto.AttendanceDate,
                Status = (AttendanceStatus)createAttendanceDto.Status,
                Notes = createAttendanceDto.Notes
            };

            _context.Attendances.Add(attendance);
            await _context.SaveChangesAsync();

            // Load the created attendance with related data
            var createdAttendance = await _context.Attendances
                .Include(a => a.Student)
                .Include(a => a.Class)
                .Include(a => a.RecordedBy)
                .FirstAsync(a => a.Id == attendance.Id);

            var attendanceDto = new AttendanceDto
            {
                Id = createdAttendance.Id,
                Student = new StudentDto
                {
                    Id = createdAttendance.Student.Id,
                    FullName = createdAttendance.Student.FullName,
                    Email = createdAttendance.Student.Email,
                    Status = createdAttendance.Student.Status.ToString()
                },
                Class = new ClassDto
                {
                    Id = createdAttendance.Class.Id,
                    Name = createdAttendance.Class.Name
                },
                RecordedBy = new UserDto
                {
                    Id = createdAttendance.RecordedBy.Id,
                    FullName = createdAttendance.RecordedBy.FullName,
                    Role = createdAttendance.RecordedBy.Role.ToString()
                },
                AttendanceDate = createdAttendance.AttendanceDate,
                Status = createdAttendance.Status.ToString(),
                Notes = createdAttendance.Notes,
                PhotoUrl = createdAttendance.PhotoUrl,
                CreatedAt = createdAttendance.CreatedAt
            };

            return CreatedAtAction(nameof(GetAttendanceByClass), new { classId = attendance.ClassId }, attendanceDto);
        }

        [HttpPost("bulk")]
        [Authorize(Roles = "Admin,Trainer,Assistant")]
        public async Task<ActionResult> CreateBulkAttendance(BulkAttendanceDto bulkAttendanceDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int recordedById))
            {
                return Unauthorized();
            }

            var attendances = new List<Attendance>();

            foreach (var studentAttendance in bulkAttendanceDto.Students)
            {
                // Check if attendance already exists
                var existingAttendance = await _context.Attendances
                    .FirstOrDefaultAsync(a => a.StudentId == studentAttendance.StudentId && 
                                            a.ClassId == bulkAttendanceDto.ClassId && 
                                            a.AttendanceDate.Date == bulkAttendanceDto.AttendanceDate.Date);
                
                if (existingAttendance == null)
                {
                    var attendance = new Attendance
                    {
                        StudentId = studentAttendance.StudentId,
                        ClassId = bulkAttendanceDto.ClassId,
                        RecordedById = recordedById,
                        AttendanceDate = bulkAttendanceDto.AttendanceDate,
                        Status = (AttendanceStatus)studentAttendance.Status,
                        Notes = studentAttendance.Notes
                    };
                    attendances.Add(attendance);
                }
            }

            if (attendances.Any())
            {
                _context.Attendances.AddRange(attendances);
                await _context.SaveChangesAsync();
            }

            return Ok(new { Message = $"Created {attendances.Count} attendance records", Count = attendances.Count });
        }

        [HttpPost("{id}/upload-photo")]
        [Authorize(Roles = "Admin,Trainer,Assistant")]
        public async Task<ActionResult> UploadAttendancePhoto(int id, [FromForm] IFormFile photo)
        {
            var attendance = await _context.Attendances.FindAsync(id);
            if (attendance == null)
            {
                return NotFound();
            }

            if (photo == null || photo.Length == 0)
            {
                return BadRequest("No photo provided");
            }

            // Validate file type
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var fileExtension = Path.GetExtension(photo.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(fileExtension))
            {
                return BadRequest("Invalid file type. Only images are allowed.");
            }

            // Create uploads directory if it doesn't exist
            var uploadsPath = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, "uploads", "attendance");
            Directory.CreateDirectory(uploadsPath);

            // Generate unique filename
            var fileName = $"attendance_{id}_{DateTime.UtcNow.Ticks}{fileExtension}";
            var filePath = Path.Combine(uploadsPath, fileName);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await photo.CopyToAsync(stream);
            }

            // Update attendance record with photo URL
            attendance.PhotoUrl = $"/uploads/attendance/{fileName}";
            await _context.SaveChangesAsync();

            return Ok(new { PhotoUrl = attendance.PhotoUrl });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Trainer,Assistant")]
        public async Task<ActionResult> UpdateAttendance(int id, CreateAttendanceDto updateAttendanceDto)
        {
            var attendance = await _context.Attendances.FindAsync(id);
            if (attendance == null)
            {
                return NotFound();
            }

            attendance.Status = (AttendanceStatus)updateAttendanceDto.Status;
            attendance.Notes = updateAttendanceDto.Notes;

            await _context.SaveChangesAsync();
            return Ok(new { Message = "Attendance updated successfully" });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteAttendance(int id)
        {
            var attendance = await _context.Attendances.FindAsync(id);
            if (attendance == null)
            {
                return NotFound();
            }

            // Delete photo file if exists
            if (!string.IsNullOrEmpty(attendance.PhotoUrl))
            {
                var photoPath = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, attendance.PhotoUrl.TrimStart('/'));
                if (System.IO.File.Exists(photoPath))
                {
                    System.IO.File.Delete(photoPath);
                }
            }

            _context.Attendances.Remove(attendance);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}