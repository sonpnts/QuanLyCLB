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
    public class PaymentsController : ControllerBase
    {
        private readonly QuanLyCLBContext _context;

        public PaymentsController(QuanLyCLBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PaymentDto>>> GetPayments()
        {
            var payments = await _context.Payments
                .Include(p => p.Student)
                .Include(p => p.Class)
                .ThenInclude(c => c!.Trainer)
                .Select(p => new PaymentDto
                {
                    Id = p.Id,
                    Student = new StudentDto
                    {
                        Id = p.Student.Id,
                        FullName = p.Student.FullName,
                        Email = p.Student.Email,
                        PhoneNumber = p.Student.PhoneNumber,
                        Status = p.Student.Status.ToString()
                    },
                    Class = p.Class != null ? new ClassDto
                    {
                        Id = p.Class.Id,
                        Name = p.Class.Name,
                        FeePerMonth = p.Class.FeePerMonth,
                        Trainer = new UserDto
                        {
                            Id = p.Class.Trainer.Id,
                            FullName = p.Class.Trainer.FullName,
                            Role = p.Class.Trainer.Role.ToString()
                        }
                    } : null,
                    Amount = p.Amount,
                    PaymentType = p.PaymentType.ToString(),
                    Status = p.Status.ToString(),
                    PaymentDate = p.PaymentDate,
                    DueDate = p.DueDate,
                    PaymentMethod = p.PaymentMethod,
                    TransactionId = p.TransactionId,
                    Notes = p.Notes,
                    CreatedAt = p.CreatedAt
                })
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return Ok(payments);
        }

        [HttpGet("student/{studentId}")]
        public async Task<ActionResult<IEnumerable<PaymentDto>>> GetPaymentsByStudent(int studentId)
        {
            var payments = await _context.Payments
                .Include(p => p.Student)
                .Include(p => p.Class)
                .ThenInclude(c => c!.Trainer)
                .Where(p => p.StudentId == studentId)
                .Select(p => new PaymentDto
                {
                    Id = p.Id,
                    Student = new StudentDto
                    {
                        Id = p.Student.Id,
                        FullName = p.Student.FullName,
                        Email = p.Student.Email,
                        PhoneNumber = p.Student.PhoneNumber,
                        Status = p.Student.Status.ToString()
                    },
                    Class = p.Class != null ? new ClassDto
                    {
                        Id = p.Class.Id,
                        Name = p.Class.Name,
                        FeePerMonth = p.Class.FeePerMonth,
                        Trainer = new UserDto
                        {
                            Id = p.Class.Trainer.Id,
                            FullName = p.Class.Trainer.FullName,
                            Role = p.Class.Trainer.Role.ToString()
                        }
                    } : null,
                    Amount = p.Amount,
                    PaymentType = p.PaymentType.ToString(),
                    Status = p.Status.ToString(),
                    PaymentDate = p.PaymentDate,
                    DueDate = p.DueDate,
                    PaymentMethod = p.PaymentMethod,
                    TransactionId = p.TransactionId,
                    Notes = p.Notes,
                    CreatedAt = p.CreatedAt
                })
                .OrderByDescending(p => p.DueDate)
                .ToListAsync();

            return Ok(payments);
        }

        [HttpGet("overdue")]
        public async Task<ActionResult<IEnumerable<PaymentDto>>> GetOverduePayments()
        {
            var overduePayments = await _context.Payments
                .Include(p => p.Student)
                .Include(p => p.Class)
                .ThenInclude(c => c!.Trainer)
                .Where(p => p.Status == PaymentStatus.Pending && p.DueDate < DateTime.UtcNow)
                .Select(p => new PaymentDto
                {
                    Id = p.Id,
                    Student = new StudentDto
                    {
                        Id = p.Student.Id,
                        FullName = p.Student.FullName,
                        Email = p.Student.Email,
                        PhoneNumber = p.Student.PhoneNumber,
                        Status = p.Student.Status.ToString()
                    },
                    Class = p.Class != null ? new ClassDto
                    {
                        Id = p.Class.Id,
                        Name = p.Class.Name,
                        FeePerMonth = p.Class.FeePerMonth,
                        Trainer = new UserDto
                        {
                            Id = p.Class.Trainer.Id,
                            FullName = p.Class.Trainer.FullName,
                            Role = p.Class.Trainer.Role.ToString()
                        }
                    } : null,
                    Amount = p.Amount,
                    PaymentType = p.PaymentType.ToString(),
                    Status = p.Status.ToString(),
                    PaymentDate = p.PaymentDate,
                    DueDate = p.DueDate,
                    PaymentMethod = p.PaymentMethod,
                    TransactionId = p.TransactionId,
                    Notes = p.Notes,
                    CreatedAt = p.CreatedAt
                })
                .OrderBy(p => p.DueDate)
                .ToListAsync();

            // Update status to Overdue
            var overdueIds = overduePayments.Select(p => p.Id).ToList();
            if (overdueIds.Any())
            {
                var paymentsToUpdate = await _context.Payments
                    .Where(p => overdueIds.Contains(p.Id))
                    .ToListAsync();
                
                foreach (var payment in paymentsToUpdate)
                {
                    payment.Status = PaymentStatus.Overdue;
                    payment.UpdatedAt = DateTime.UtcNow;
                }
                
                await _context.SaveChangesAsync();
            }

            return Ok(overduePayments);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Trainer")]
        public async Task<ActionResult<PaymentDto>> CreatePayment(CreatePaymentDto createPaymentDto)
        {
            // Verify student exists
            var student = await _context.Students.FindAsync(createPaymentDto.StudentId);
            if (student == null)
            {
                return BadRequest("Student not found");
            }

            // Verify class exists if provided
            if (createPaymentDto.ClassId.HasValue)
            {
                var classExists = await _context.Classes.AnyAsync(c => c.Id == createPaymentDto.ClassId.Value);
                if (!classExists)
                {
                    return BadRequest("Class not found");
                }
            }

            var payment = new Payment
            {
                StudentId = createPaymentDto.StudentId,
                ClassId = createPaymentDto.ClassId,
                Amount = createPaymentDto.Amount,
                PaymentType = (PaymentType)createPaymentDto.PaymentType,
                DueDate = createPaymentDto.DueDate,
                PaymentDate = DateTime.UtcNow,
                PaymentMethod = createPaymentDto.PaymentMethod,
                TransactionId = createPaymentDto.TransactionId,
                Notes = createPaymentDto.Notes
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            // Load the created payment with related data
            var createdPayment = await _context.Payments
                .Include(p => p.Student)
                .Include(p => p.Class)
                .ThenInclude(c => c!.Trainer)
                .FirstAsync(p => p.Id == payment.Id);

            var paymentDto = new PaymentDto
            {
                Id = createdPayment.Id,
                Student = new StudentDto
                {
                    Id = createdPayment.Student.Id,
                    FullName = createdPayment.Student.FullName,
                    Email = createdPayment.Student.Email,
                    PhoneNumber = createdPayment.Student.PhoneNumber,
                    Status = createdPayment.Student.Status.ToString()
                },
                Class = createdPayment.Class != null ? new ClassDto
                {
                    Id = createdPayment.Class.Id,
                    Name = createdPayment.Class.Name,
                    FeePerMonth = createdPayment.Class.FeePerMonth,
                    Trainer = new UserDto
                    {
                        Id = createdPayment.Class.Trainer.Id,
                        FullName = createdPayment.Class.Trainer.FullName,
                        Role = createdPayment.Class.Trainer.Role.ToString()
                    }
                } : null,
                Amount = createdPayment.Amount,
                PaymentType = createdPayment.PaymentType.ToString(),
                Status = createdPayment.Status.ToString(),
                PaymentDate = createdPayment.PaymentDate,
                DueDate = createdPayment.DueDate,
                PaymentMethod = createdPayment.PaymentMethod,
                TransactionId = createdPayment.TransactionId,
                Notes = createdPayment.Notes,
                CreatedAt = createdPayment.CreatedAt
            };

            return CreatedAtAction(nameof(GetPayments), paymentDto);
        }

        [HttpPost("process")]
        [Authorize(Roles = "Admin,Trainer")]
        public async Task<ActionResult> ProcessPayment(ProcessPaymentDto processPaymentDto)
        {
            var payment = await _context.Payments.FindAsync(processPaymentDto.PaymentId);
            if (payment == null)
            {
                return NotFound();
            }

            if (payment.Status == PaymentStatus.Paid)
            {
                return BadRequest("Payment is already processed");
            }

            payment.Status = PaymentStatus.Paid;
            payment.PaymentMethod = processPaymentDto.PaymentMethod;
            payment.TransactionId = processPaymentDto.TransactionId;
            payment.PaymentDate = DateTime.UtcNow;
            payment.Notes = processPaymentDto.Notes;
            payment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { Message = "Payment processed successfully", PaymentId = payment.Id });
        }

        [HttpPost("generate-monthly")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> GenerateMonthlyPayments([FromBody] DateTime month)
        {
            var firstDayOfMonth = new DateTime(month.Year, month.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

            // Get all active enrollments
            var activeEnrollments = await _context.Enrollments
                .Include(e => e.Student)
                .Include(e => e.Class)
                .Where(e => e.Status == EnrollmentStatus.Active)
                .ToListAsync();

            var paymentsToCreate = new List<Payment>();

            foreach (var enrollment in activeEnrollments)
            {
                // Check if payment already exists for this month
                var existingPayment = await _context.Payments
                    .AnyAsync(p => p.StudentId == enrollment.StudentId && 
                                  p.ClassId == enrollment.ClassId && 
                                  p.PaymentType == PaymentType.MonthlyFee &&
                                  p.DueDate.Year == month.Year && 
                                  p.DueDate.Month == month.Month);

                if (!existingPayment)
                {
                    var payment = new Payment
                    {
                        StudentId = enrollment.StudentId,
                        ClassId = enrollment.ClassId,
                        Amount = enrollment.Class.FeePerMonth,
                        PaymentType = PaymentType.MonthlyFee,
                        DueDate = lastDayOfMonth,
                        PaymentDate = DateTime.UtcNow,
                        Notes = $"Monthly fee for {month:yyyy-MM}"
                    };
                    paymentsToCreate.Add(payment);
                }
            }

            if (paymentsToCreate.Any())
            {
                _context.Payments.AddRange(paymentsToCreate);
                await _context.SaveChangesAsync();
            }

            return Ok(new { 
                Message = $"Generated {paymentsToCreate.Count} monthly payments for {month:yyyy-MM}",
                Count = paymentsToCreate.Count 
            });
        }

        [HttpGet("report")]
        public async Task<ActionResult<PaymentReportDto>> GetPaymentReport(DateTime fromDate, DateTime toDate)
        {
            var payments = await _context.Payments
                .Include(p => p.Student)
                .Include(p => p.Class)
                .ThenInclude(c => c!.Trainer)
                .Where(p => p.CreatedAt >= fromDate && p.CreatedAt <= toDate)
                .ToListAsync();

            var report = new PaymentReportDto
            {
                FromDate = fromDate,
                ToDate = toDate,
                TotalPayments = payments.Count,
                TotalAmount = payments.Sum(p => p.Amount),
                PaidAmount = payments.Where(p => p.Status == PaymentStatus.Paid).Sum(p => p.Amount),
                PendingAmount = payments.Where(p => p.Status == PaymentStatus.Pending).Sum(p => p.Amount),
                OverdueAmount = payments.Where(p => p.Status == PaymentStatus.Overdue).Sum(p => p.Amount),
                Payments = payments.Select(p => new PaymentDto
                {
                    Id = p.Id,
                    Student = new StudentDto
                    {
                        Id = p.Student.Id,
                        FullName = p.Student.FullName,
                        Email = p.Student.Email,
                        Status = p.Student.Status.ToString()
                    },
                    Class = p.Class != null ? new ClassDto
                    {
                        Id = p.Class.Id,
                        Name = p.Class.Name,
                        FeePerMonth = p.Class.FeePerMonth,
                        Trainer = new UserDto
                        {
                            Id = p.Class.Trainer.Id,
                            FullName = p.Class.Trainer.FullName,
                            Role = p.Class.Trainer.Role.ToString()
                        }
                    } : null,
                    Amount = p.Amount,
                    PaymentType = p.PaymentType.ToString(),
                    Status = p.Status.ToString(),
                    PaymentDate = p.PaymentDate,
                    DueDate = p.DueDate,
                    PaymentMethod = p.PaymentMethod,
                    TransactionId = p.TransactionId,
                    Notes = p.Notes,
                    CreatedAt = p.CreatedAt
                }).ToList()
            };

            return Ok(report);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeletePayment(int id)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment == null)
            {
                return NotFound();
            }

            _context.Payments.Remove(payment);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}