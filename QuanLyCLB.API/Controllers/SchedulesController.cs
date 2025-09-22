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
    public class SchedulesController : ControllerBase
    {
        private readonly QuanLyCLBContext _context;

        public SchedulesController(QuanLyCLBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ScheduleDto>>> GetSchedules()
        {
            var schedules = await _context.Schedules
                .Include(s => s.Class)
                .ThenInclude(c => c.Trainer)
                .Include(s => s.User)
                .Where(s => s.IsActive)
                .Select(s => new ScheduleDto
                {
                    Id = s.Id,
                    Class = new ClassDto
                    {
                        Id = s.Class.Id,
                        Name = s.Class.Name,
                        Status = s.Class.Status.ToString(),
                        Trainer = new UserDto
                        {
                            Id = s.Class.Trainer.Id,
                            FullName = s.Class.Trainer.FullName,
                            Role = s.Class.Trainer.Role.ToString()
                        }
                    },
                    User = new UserDto
                    {
                        Id = s.User.Id,
                        FullName = s.User.FullName,
                        Email = s.User.Email,
                        Role = s.User.Role.ToString()
                    },
                    DayOfWeek = s.DayOfWeek.ToString(),
                    StartTime = s.StartTime,
                    EndTime = s.EndTime,
                    Location = s.Location,
                    EffectiveFrom = s.EffectiveFrom,
                    EffectiveTo = s.EffectiveTo,
                    IsActive = s.IsActive,
                    Notes = s.Notes,
                    CreatedAt = s.CreatedAt
                })
                .OrderBy(s => s.DayOfWeek)
                .ThenBy(s => s.StartTime)
                .ToListAsync();

            return Ok(schedules);
        }

        [HttpGet("class/{classId}")]
        public async Task<ActionResult<IEnumerable<ScheduleDto>>> GetSchedulesByClass(int classId)
        {
            var schedules = await _context.Schedules
                .Include(s => s.Class)
                .ThenInclude(c => c.Trainer)
                .Include(s => s.User)
                .Where(s => s.ClassId == classId && s.IsActive)
                .Select(s => new ScheduleDto
                {
                    Id = s.Id,
                    Class = new ClassDto
                    {
                        Id = s.Class.Id,
                        Name = s.Class.Name,
                        Status = s.Class.Status.ToString(),
                        Trainer = new UserDto
                        {
                            Id = s.Class.Trainer.Id,
                            FullName = s.Class.Trainer.FullName,
                            Role = s.Class.Trainer.Role.ToString()
                        }
                    },
                    User = new UserDto
                    {
                        Id = s.User.Id,
                        FullName = s.User.FullName,
                        Email = s.User.Email,
                        Role = s.User.Role.ToString()
                    },
                    DayOfWeek = s.DayOfWeek.ToString(),
                    StartTime = s.StartTime,
                    EndTime = s.EndTime,
                    Location = s.Location,
                    EffectiveFrom = s.EffectiveFrom,
                    EffectiveTo = s.EffectiveTo,
                    IsActive = s.IsActive,
                    Notes = s.Notes,
                    CreatedAt = s.CreatedAt
                })
                .OrderBy(s => s.DayOfWeek)
                .ThenBy(s => s.StartTime)
                .ToListAsync();

            return Ok(schedules);
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<ScheduleDto>>> GetSchedulesByUser(int userId)
        {
            var schedules = await _context.Schedules
                .Include(s => s.Class)
                .ThenInclude(c => c.Trainer)
                .Include(s => s.User)
                .Where(s => s.UserId == userId && s.IsActive)
                .Select(s => new ScheduleDto
                {
                    Id = s.Id,
                    Class = new ClassDto
                    {
                        Id = s.Class.Id,
                        Name = s.Class.Name,
                        Status = s.Class.Status.ToString(),
                        Trainer = new UserDto
                        {
                            Id = s.Class.Trainer.Id,
                            FullName = s.Class.Trainer.FullName,
                            Role = s.Class.Trainer.Role.ToString()
                        }
                    },
                    User = new UserDto
                    {
                        Id = s.User.Id,
                        FullName = s.User.FullName,
                        Email = s.User.Email,
                        Role = s.User.Role.ToString()
                    },
                    DayOfWeek = s.DayOfWeek.ToString(),
                    StartTime = s.StartTime,
                    EndTime = s.EndTime,
                    Location = s.Location,
                    EffectiveFrom = s.EffectiveFrom,
                    EffectiveTo = s.EffectiveTo,
                    IsActive = s.IsActive,
                    Notes = s.Notes,
                    CreatedAt = s.CreatedAt
                })
                .OrderBy(s => s.DayOfWeek)
                .ThenBy(s => s.StartTime)
                .ToListAsync();

            return Ok(schedules);
        }

        [HttpGet("weekly")]
        public async Task<ActionResult<WeeklyScheduleDto>> GetWeeklySchedule(DateTime? weekStart = null)
        {
            var startDate = weekStart ?? DateTime.UtcNow.Date.AddDays(-(int)DateTime.UtcNow.DayOfWeek);
            var endDate = startDate.AddDays(6);

            var schedules = await _context.Schedules
                .Include(s => s.Class)
                .ThenInclude(c => c.Trainer)
                .Include(s => s.User)
                .Where(s => s.IsActive && 
                           s.EffectiveFrom <= endDate && 
                           (s.EffectiveTo == null || s.EffectiveTo >= startDate))
                .ToListAsync();

            var weeklySchedule = new WeeklyScheduleDto
            {
                WeekStartDate = startDate,
                Days = new List<DayScheduleDto>()
            };

            for (int i = 0; i < 7; i++)
            {
                var currentDate = startDate.AddDays(i);
                var dayOfWeek = currentDate.DayOfWeek;
                
                var daySchedules = schedules
                    .Where(s => s.DayOfWeek == dayOfWeek)
                    .Select(s => new ScheduleDto
                    {
                        Id = s.Id,
                        Class = new ClassDto
                        {
                            Id = s.Class.Id,
                            Name = s.Class.Name,
                            Status = s.Class.Status.ToString(),
                            Trainer = new UserDto
                            {
                                Id = s.Class.Trainer.Id,
                                FullName = s.Class.Trainer.FullName,
                                Role = s.Class.Trainer.Role.ToString()
                            }
                        },
                        User = new UserDto
                        {
                            Id = s.User.Id,
                            FullName = s.User.FullName,
                            Email = s.User.Email,
                            Role = s.User.Role.ToString()
                        },
                        DayOfWeek = s.DayOfWeek.ToString(),
                        StartTime = s.StartTime,
                        EndTime = s.EndTime,
                        Location = s.Location,
                        EffectiveFrom = s.EffectiveFrom,
                        EffectiveTo = s.EffectiveTo,
                        IsActive = s.IsActive,
                        Notes = s.Notes,
                        CreatedAt = s.CreatedAt
                    })
                    .OrderBy(s => s.StartTime)
                    .ToList();

                weeklySchedule.Days.Add(new DayScheduleDto
                {
                    DayOfWeek = dayOfWeek,
                    Date = currentDate,
                    Schedules = daySchedules
                });
            }

            return Ok(weeklySchedule);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Trainer")]
        public async Task<ActionResult<ScheduleDto>> CreateSchedule(CreateScheduleDto createScheduleDto)
        {
            // Verify class exists
            var classEntity = await _context.Classes.FindAsync(createScheduleDto.ClassId);
            if (classEntity == null)
            {
                return BadRequest("Class not found");
            }

            // Verify user exists and has appropriate role
            var user = await _context.Users.FindAsync(createScheduleDto.UserId);
            if (user == null || (user.Role != UserRole.Trainer && user.Role != UserRole.Assistant))
            {
                return BadRequest("User not found or invalid role");
            }

            // Check for scheduling conflicts
            var conflictingSchedule = await _context.Schedules
                .AnyAsync(s => s.UserId == createScheduleDto.UserId &&
                              s.DayOfWeek == (DayOfWeek)createScheduleDto.DayOfWeek &&
                              s.IsActive &&
                              ((s.StartTime <= createScheduleDto.StartTime && s.EndTime > createScheduleDto.StartTime) ||
                               (s.StartTime < createScheduleDto.EndTime && s.EndTime >= createScheduleDto.EndTime) ||
                               (s.StartTime >= createScheduleDto.StartTime && s.EndTime <= createScheduleDto.EndTime)));

            if (conflictingSchedule)
            {
                return BadRequest("User has a conflicting schedule at this time");
            }

            var schedule = new Schedule
            {
                ClassId = createScheduleDto.ClassId,
                UserId = createScheduleDto.UserId,
                DayOfWeek = (DayOfWeek)createScheduleDto.DayOfWeek,
                StartTime = createScheduleDto.StartTime,
                EndTime = createScheduleDto.EndTime,
                Location = createScheduleDto.Location,
                EffectiveFrom = createScheduleDto.EffectiveFrom,
                EffectiveTo = createScheduleDto.EffectiveTo,
                Notes = createScheduleDto.Notes
            };

            _context.Schedules.Add(schedule);
            await _context.SaveChangesAsync();

            // Load the created schedule with related data
            var createdSchedule = await _context.Schedules
                .Include(s => s.Class)
                .ThenInclude(c => c.Trainer)
                .Include(s => s.User)
                .FirstAsync(s => s.Id == schedule.Id);

            var scheduleDto = new ScheduleDto
            {
                Id = createdSchedule.Id,
                Class = new ClassDto
                {
                    Id = createdSchedule.Class.Id,
                    Name = createdSchedule.Class.Name,
                    Status = createdSchedule.Class.Status.ToString(),
                    Trainer = new UserDto
                    {
                        Id = createdSchedule.Class.Trainer.Id,
                        FullName = createdSchedule.Class.Trainer.FullName,
                        Role = createdSchedule.Class.Trainer.Role.ToString()
                    }
                },
                User = new UserDto
                {
                    Id = createdSchedule.User.Id,
                    FullName = createdSchedule.User.FullName,
                    Email = createdSchedule.User.Email,
                    Role = createdSchedule.User.Role.ToString()
                },
                DayOfWeek = createdSchedule.DayOfWeek.ToString(),
                StartTime = createdSchedule.StartTime,
                EndTime = createdSchedule.EndTime,
                Location = createdSchedule.Location,
                EffectiveFrom = createdSchedule.EffectiveFrom,
                EffectiveTo = createdSchedule.EffectiveTo,
                IsActive = createdSchedule.IsActive,
                Notes = createdSchedule.Notes,
                CreatedAt = createdSchedule.CreatedAt
            };

            return CreatedAtAction(nameof(GetSchedules), scheduleDto);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Trainer")]
        public async Task<ActionResult> UpdateSchedule(int id, UpdateScheduleDto updateScheduleDto)
        {
            var schedule = await _context.Schedules.FindAsync(id);
            if (schedule == null)
            {
                return NotFound();
            }

            if (updateScheduleDto.UserId.HasValue)
            {
                var user = await _context.Users.FindAsync(updateScheduleDto.UserId.Value);
                if (user == null || (user.Role != UserRole.Trainer && user.Role != UserRole.Assistant))
                {
                    return BadRequest("Invalid user or role");
                }
                schedule.UserId = updateScheduleDto.UserId.Value;
            }

            if (updateScheduleDto.DayOfWeek.HasValue)
                schedule.DayOfWeek = (DayOfWeek)updateScheduleDto.DayOfWeek.Value;
            
            if (updateScheduleDto.StartTime.HasValue)
                schedule.StartTime = updateScheduleDto.StartTime.Value;
            
            if (updateScheduleDto.EndTime.HasValue)
                schedule.EndTime = updateScheduleDto.EndTime.Value;
            
            if (!string.IsNullOrEmpty(updateScheduleDto.Location))
                schedule.Location = updateScheduleDto.Location;
            
            if (updateScheduleDto.EffectiveFrom.HasValue)
                schedule.EffectiveFrom = updateScheduleDto.EffectiveFrom.Value;
            
            if (updateScheduleDto.EffectiveTo.HasValue)
                schedule.EffectiveTo = updateScheduleDto.EffectiveTo;
            
            if (updateScheduleDto.IsActive.HasValue)
                schedule.IsActive = updateScheduleDto.IsActive.Value;
            
            if (!string.IsNullOrEmpty(updateScheduleDto.Notes))
                schedule.Notes = updateScheduleDto.Notes;

            schedule.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteSchedule(int id)
        {
            var schedule = await _context.Schedules.FindAsync(id);
            if (schedule == null)
            {
                return NotFound();
            }

            _context.Schedules.Remove(schedule);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPut("{id}/deactivate")]
        [Authorize(Roles = "Admin,Trainer")]
        public async Task<ActionResult> DeactivateSchedule(int id)
        {
            var schedule = await _context.Schedules.FindAsync(id);
            if (schedule == null)
            {
                return NotFound();
            }

            schedule.IsActive = false;
            schedule.EffectiveTo = DateTime.UtcNow;
            schedule.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(new { Message = "Schedule deactivated successfully" });
        }
    }
}