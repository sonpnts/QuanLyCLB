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
    public class StudentsController : ControllerBase
    {
        private readonly QuanLyCLBContext _context;

        public StudentsController(QuanLyCLBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<StudentDto>>> GetStudents()
        {
            var students = await _context.Students
                .Select(s => new StudentDto
                {
                    Id = s.Id,
                    FullName = s.FullName,
                    Email = s.Email,
                    PhoneNumber = s.PhoneNumber,
                    DateOfBirth = s.DateOfBirth,
                    Address = s.Address,
                    ParentName = s.ParentName,
                    ParentPhone = s.ParentPhone,
                    Status = s.Status.ToString(),
                    CreatedAt = s.CreatedAt
                })
                .ToListAsync();

            return Ok(students);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<StudentDto>> GetStudent(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student == null)
            {
                return NotFound();
            }

            var studentDto = new StudentDto
            {
                Id = student.Id,
                FullName = student.FullName,
                Email = student.Email,
                PhoneNumber = student.PhoneNumber,
                DateOfBirth = student.DateOfBirth,
                Address = student.Address,
                ParentName = student.ParentName,
                ParentPhone = student.ParentPhone,
                Status = student.Status.ToString(),
                CreatedAt = student.CreatedAt
            };

            return Ok(studentDto);
        }

        [HttpPost]
        public async Task<ActionResult<StudentDto>> CreateStudent(CreateStudentDto createStudentDto)
        {
            if (await _context.Students.AnyAsync(s => s.Email == createStudentDto.Email))
            {
                return BadRequest("Email already exists");
            }

            var student = new Student
            {
                FullName = createStudentDto.FullName,
                Email = createStudentDto.Email,
                PhoneNumber = createStudentDto.PhoneNumber,
                DateOfBirth = createStudentDto.DateOfBirth,
                Address = createStudentDto.Address,
                ParentName = createStudentDto.ParentName,
                ParentPhone = createStudentDto.ParentPhone
            };

            _context.Students.Add(student);
            await _context.SaveChangesAsync();

            var studentDto = new StudentDto
            {
                Id = student.Id,
                FullName = student.FullName,
                Email = student.Email,
                PhoneNumber = student.PhoneNumber,
                DateOfBirth = student.DateOfBirth,
                Address = student.Address,
                ParentName = student.ParentName,
                ParentPhone = student.ParentPhone,
                Status = student.Status.ToString(),
                CreatedAt = student.CreatedAt
            };

            return CreatedAtAction(nameof(GetStudent), new { id = student.Id }, studentDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStudent(int id, UpdateStudentDto updateStudentDto)
        {
            var student = await _context.Students.FindAsync(id);
            if (student == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(updateStudentDto.FullName))
                student.FullName = updateStudentDto.FullName;
            
            if (!string.IsNullOrEmpty(updateStudentDto.PhoneNumber))
                student.PhoneNumber = updateStudentDto.PhoneNumber;
            
            if (!string.IsNullOrEmpty(updateStudentDto.Address))
                student.Address = updateStudentDto.Address;
            
            if (!string.IsNullOrEmpty(updateStudentDto.ParentName))
                student.ParentName = updateStudentDto.ParentName;
            
            if (!string.IsNullOrEmpty(updateStudentDto.ParentPhone))
                student.ParentPhone = updateStudentDto.ParentPhone;
            
            if (updateStudentDto.Status.HasValue)
                student.Status = (StudentStatus)updateStudentDto.Status.Value;

            student.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteStudent(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student == null)
            {
                return NotFound();
            }

            _context.Students.Remove(student);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<StudentDto>>> GetActiveStudents()
        {
            var students = await _context.Students
                .Where(s => s.Status == StudentStatus.Active)
                .Select(s => new StudentDto
                {
                    Id = s.Id,
                    FullName = s.FullName,
                    Email = s.Email,
                    PhoneNumber = s.PhoneNumber,
                    DateOfBirth = s.DateOfBirth,
                    Address = s.Address,
                    ParentName = s.ParentName,
                    ParentPhone = s.ParentPhone,
                    Status = s.Status.ToString(),
                    CreatedAt = s.CreatedAt
                })
                .ToListAsync();

            return Ok(students);
        }
    }
}