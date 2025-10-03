using System;
using System.Collections.Generic;

namespace QuanLyCLB.Application.Entities;

/// <summary>
/// Thực thể Chi nhánh lưu thông tin địa điểm phục vụ cho việc điểm danh và tổ chức lớp học.
/// </summary>
public class Branch : AuditableEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public double AllowedRadiusMeters { get; set; }

    //public string? GooglePlaceId { get; set; }

    public string? GoogleMapsEmbedUrl { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<ClassSchedule> ClassSchedules { get; set; } = new List<ClassSchedule>();
}
