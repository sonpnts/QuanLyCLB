using System;
using System.Collections.Generic;
using QuanLyClb.Domain.Enums;

namespace QuanLyClb.Api.Authorization;

public static class AuthorizationPolicies
{
    public const string ViewStudents = "ViewStudents";
    public const string ManageStudents = "ManageStudents";
    public const string ViewClasses = "ViewClasses";
    public const string ManageClasses = "ManageClasses";
    public const string ViewSchedules = "ViewSchedules";
    public const string ManageSchedules = "ManageSchedules";
    public const string ViewAttendance = "ViewAttendance";
    public const string ManageAttendance = "ManageAttendance";
    public const string ManageEnrollments = "ManageEnrollments";
    public const string ManagePayments = "ManagePayments";
    public const string ViewFinancialReports = "ViewFinancialReports";
    public const string ViewClassReports = "ViewClassReports";

    public static IReadOnlyDictionary<string, UserRole[]> DefaultRoleMappings { get; } =
        new Dictionary<string, UserRole[]>(StringComparer.OrdinalIgnoreCase)
        {
            [ViewStudents] = new[]
            {
                UserRole.Admin,
                UserRole.Staff,
                UserRole.Coach,
                UserRole.Assistant
            },
            [ManageStudents] = new[]
            {
                UserRole.Admin,
                UserRole.Staff
            },
            [ViewClasses] = new[]
            {
                UserRole.Admin,
                UserRole.Staff,
                UserRole.Coach,
                UserRole.Assistant
            },
            [ManageClasses] = new[]
            {
                UserRole.Admin,
                UserRole.Coach
            },
            [ViewSchedules] = new[]
            {
                UserRole.Admin,
                UserRole.Staff,
                UserRole.Coach,
                UserRole.Assistant
            },
            [ManageSchedules] = new[]
            {
                UserRole.Admin,
                UserRole.Coach,
                UserRole.Assistant
            },
            [ViewAttendance] = new[]
            {
                UserRole.Admin,
                UserRole.Staff,
                UserRole.Coach,
                UserRole.Assistant
            },
            [ManageAttendance] = new[]
            {
                UserRole.Admin,
                UserRole.Coach,
                UserRole.Assistant
            },
            [ManageEnrollments] = new[]
            {
                UserRole.Admin,
                UserRole.Staff
            },
            [ManagePayments] = new[]
            {
                UserRole.Admin,
                UserRole.Staff
            },
            [ViewFinancialReports] = new[]
            {
                UserRole.Admin,
                UserRole.Staff
            },
            [ViewClassReports] = new[]
            {
                UserRole.Admin,
                UserRole.Staff,
                UserRole.Coach,
                UserRole.Assistant
            }
        };
}
