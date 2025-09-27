namespace QuanLyCLB.Application.Entities;

/// <summary>
/// Base entity that provides standard audit fields for creation and updates.
/// </summary>
public abstract class AuditableEntity
{
    /// <summary>
    /// UTC timestamp when the record was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Identifier of the user that created the record.
    /// </summary>
    public Guid? CreatedByUserId { get; set; }

    /// <summary>
    /// UTC timestamp when the record was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Identifier of the user that performed the latest update.
    /// </summary>
    public Guid? UpdatedByUserId { get; set; }
}
