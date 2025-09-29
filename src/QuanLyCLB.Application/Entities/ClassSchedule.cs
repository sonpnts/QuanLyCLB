using System;
using System.Collections.Generic;
using QuanLyCLB.Application.Enums;

namespace QuanLyCLB.Application.Entities;

/// <summary>
/// Thực thể Lịch học cho biết một buổi học cụ thể của lớp đào tạo.
/// </summary>
public class ClassSchedule : AuditableEntity
{
    // Khóa chính và liên kết về lớp đào tạo
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TrainingClassId { get; set; }
    public TrainingClass? TrainingClass { get; set; }

    // Thông tin thời gian diễn ra buổi học theo tuần
    public DayOfWeek DayOfWeek { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }

    // Địa điểm tổ chức được tham chiếu qua bảng chi nhánh
    public Guid BranchId { get; set; }
    public Branch? Branch { get; set; }

    public bool IsActive { get; set; } = true;

    // Danh sách bản ghi điểm danh của buổi học
    public ICollection<AttendanceRecord> AttendanceRecords { get; set; } = new List<AttendanceRecord>();
}
