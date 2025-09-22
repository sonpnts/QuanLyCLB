using System;
using System.Collections.Generic;
using QuanLyClb.Domain.Enums;

namespace QuanLyClb.Domain.Authorization;

public static class PermissionRoleDefaults
{
    public static IReadOnlyDictionary<Permission, UserRole[]> DefaultRoleMappings { get; } =
        new Dictionary<Permission, UserRole[]>(EqualityComparer<Permission>.Default)
        {
            [Permission.ViewStudents] = new[]
            {
                UserRole.Admin,
                UserRole.Staff,
                UserRole.Coach,
                UserRole.Assistant
            },
            [Permission.ManageStudents] = new[]
            {
                UserRole.Admin,
                UserRole.Staff
            },
            [Permission.ViewClasses] = new[]
            {
                UserRole.Admin,
                UserRole.Staff,
                UserRole.Coach,
                UserRole.Assistant
            },
            [Permission.ManageClasses] = new[]
            {
                UserRole.Admin,
                UserRole.Coach
            },
            [Permission.ViewSchedules] = new[]
            {
                UserRole.Admin,
                UserRole.Staff,
                UserRole.Coach,
                UserRole.Assistant
            },
            [Permission.ManageSchedules] = new[]
            {
                UserRole.Admin,
                UserRole.Coach,
                UserRole.Assistant
            },
            [Permission.ViewAttendance] = new[]
            {
                UserRole.Admin,
                UserRole.Staff,
                UserRole.Coach,
                UserRole.Assistant
            },
            [Permission.ManageAttendance] = new[]
            {
                UserRole.Admin,
                UserRole.Coach,
                UserRole.Assistant
            },
            [Permission.ManageEnrollments] = new[]
            {
                UserRole.Admin,
                UserRole.Staff
            },
            [Permission.ManagePayments] = new[]
            {
                UserRole.Admin,
                UserRole.Staff
            },
            [Permission.ViewFinancialReports] = new[]
            {
                UserRole.Admin,
                UserRole.Staff
            },
            [Permission.ViewClassReports] = new[]
            {
                UserRole.Admin,
                UserRole.Staff,
                UserRole.Coach,
                UserRole.Assistant
            }
        };
}
