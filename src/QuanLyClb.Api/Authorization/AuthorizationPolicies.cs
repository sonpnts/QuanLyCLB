using Microsoft.AspNetCore.Authorization;
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

    public static void Configure(AuthorizationOptions options)
    {
        options.AddPolicy(ViewStudents, policy =>
            policy.RequireRole(
                UserRole.Admin.ToString(),
                UserRole.Staff.ToString(),
                UserRole.Coach.ToString(),
                UserRole.Assistant.ToString()));

        options.AddPolicy(ManageStudents, policy =>
            policy.RequireRole(
                UserRole.Admin.ToString(),
                UserRole.Staff.ToString()));

        options.AddPolicy(ViewClasses, policy =>
            policy.RequireRole(
                UserRole.Admin.ToString(),
                UserRole.Staff.ToString(),
                UserRole.Coach.ToString(),
                UserRole.Assistant.ToString()));

        options.AddPolicy(ManageClasses, policy =>
            policy.RequireRole(
                UserRole.Admin.ToString(),
                UserRole.Coach.ToString()));

        options.AddPolicy(ViewSchedules, policy =>
            policy.RequireRole(
                UserRole.Admin.ToString(),
                UserRole.Staff.ToString(),
                UserRole.Coach.ToString(),
                UserRole.Assistant.ToString()));

        options.AddPolicy(ManageSchedules, policy =>
            policy.RequireRole(
                UserRole.Admin.ToString(),
                UserRole.Coach.ToString(),
                UserRole.Assistant.ToString()));

        options.AddPolicy(ViewAttendance, policy =>
            policy.RequireRole(
                UserRole.Admin.ToString(),
                UserRole.Staff.ToString(),
                UserRole.Coach.ToString(),
                UserRole.Assistant.ToString()));

        options.AddPolicy(ManageAttendance, policy =>
            policy.RequireRole(
                UserRole.Admin.ToString(),
                UserRole.Coach.ToString(),
                UserRole.Assistant.ToString()));

        options.AddPolicy(ManageEnrollments, policy =>
            policy.RequireRole(
                UserRole.Admin.ToString(),
                UserRole.Staff.ToString()));

        options.AddPolicy(ManagePayments, policy =>
            policy.RequireRole(
                UserRole.Admin.ToString(),
                UserRole.Staff.ToString()));

        options.AddPolicy(ViewFinancialReports, policy =>
            policy.RequireRole(
                UserRole.Admin.ToString(),
                UserRole.Staff.ToString()));

        options.AddPolicy(ViewClassReports, policy =>
            policy.RequireRole(
                UserRole.Admin.ToString(),
                UserRole.Staff.ToString(),
                UserRole.Coach.ToString(),
                UserRole.Assistant.ToString()));
    }
}
