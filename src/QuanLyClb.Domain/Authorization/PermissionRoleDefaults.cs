using System.Collections.Generic;
using QuanLyClb.Domain.Enums;

namespace QuanLyClb.Domain.Authorization;

public static class PermissionRoleDefaults
{
    public static IReadOnlyDictionary<UserRole, Dictionary<string, PermissionAction[]>> DefaultRolePermissions { get; } =
        new Dictionary<UserRole, Dictionary<string, PermissionAction[]>>(EqualityComparer<UserRole>.Default)
        {
            [UserRole.Admin] = new()
            {
                ["Students"] = new[] { PermissionAction.View, PermissionAction.Create, PermissionAction.Update, PermissionAction.Delete },
                ["Classes"] = new[] { PermissionAction.View, PermissionAction.Create, PermissionAction.Update, PermissionAction.Delete },
                ["Schedules"] = new[] { PermissionAction.View, PermissionAction.Create, PermissionAction.Update, PermissionAction.Delete },
                ["Attendance"] = new[] { PermissionAction.View, PermissionAction.Create, PermissionAction.Update, PermissionAction.Delete },
                ["Enrollments"] = new[] { PermissionAction.View, PermissionAction.Create, PermissionAction.Update, PermissionAction.Delete },
                ["Payments"] = new[] { PermissionAction.View, PermissionAction.Create, PermissionAction.Update, PermissionAction.Delete },
                ["FinancialReports"] = new[] { PermissionAction.View },
                ["ClassReports"] = new[] { PermissionAction.View }
            },
            [UserRole.Staff] = new()
            {
                ["Students"] = new[] { PermissionAction.View, PermissionAction.Create, PermissionAction.Update, PermissionAction.Delete },
                ["Classes"] = new[] { PermissionAction.View },
                ["Schedules"] = new[] { PermissionAction.View },
                ["Attendance"] = new[] { PermissionAction.View },
                ["Enrollments"] = new[] { PermissionAction.View, PermissionAction.Create, PermissionAction.Update, PermissionAction.Delete },
                ["Payments"] = new[] { PermissionAction.View, PermissionAction.Create, PermissionAction.Update, PermissionAction.Delete },
                ["FinancialReports"] = new[] { PermissionAction.View },
                ["ClassReports"] = new[] { PermissionAction.View }
            },
            [UserRole.Coach] = new()
            {
                ["Students"] = new[] { PermissionAction.View },
                ["Classes"] = new[] { PermissionAction.View, PermissionAction.Create, PermissionAction.Update, PermissionAction.Delete },
                ["Schedules"] = new[] { PermissionAction.View, PermissionAction.Create, PermissionAction.Update, PermissionAction.Delete },
                ["Attendance"] = new[] { PermissionAction.View, PermissionAction.Create, PermissionAction.Update, PermissionAction.Delete },
                ["ClassReports"] = new[] { PermissionAction.View }
            },
            [UserRole.Assistant] = new()
            {
                ["Students"] = new[] { PermissionAction.View },
                ["Classes"] = new[] { PermissionAction.View },
                ["Schedules"] = new[] { PermissionAction.View, PermissionAction.Create, PermissionAction.Update, PermissionAction.Delete },
                ["Attendance"] = new[] { PermissionAction.View, PermissionAction.Create, PermissionAction.Update, PermissionAction.Delete },
                ["ClassReports"] = new[] { PermissionAction.View }
            }
        };
}
