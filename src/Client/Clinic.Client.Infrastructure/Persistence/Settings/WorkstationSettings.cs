namespace Clinic.Client.Infrastructure.Persistence.Settings;

/// <summary>
/// Single-row workstation identity record. Row Id must be <see cref="SingleRowId"/>.
/// The workstation identifies itself to the Authority with this WorkstationId.
/// </summary>
public sealed class WorkstationSettings
{
    public const long SingleRowId = 1;

    public WorkstationSettings(Guid workstationId, string displayName)
    {
        WorkstationId = workstationId;
        DisplayName = displayName;
    }

    public long Id { get; set; } = SingleRowId;
    public Guid WorkstationId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
}