namespace Shared.Contracts;

// MQTT DTOs for Evacuation (CMS to Engine)

public class EvacuationTriggerMqttDto
{
    public string EvacuationAlertId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string TriggerType { get; set; } = string.Empty;
    public string TriggeredAt { get; set; } = string.Empty;
    public string ApplicationId { get; set; } = string.Empty;
}

public class EvacuationCompleteMqttDto
{
    public string EvacuationAlertId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string CompletedAt { get; set; } = string.Empty;
    public string CompletedBy { get; set; } = string.Empty;
    public string? CompletionNotes { get; set; }
}

public class EvacuationCancelMqttDto
{
    public string EvacuationAlertId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string CancelledAt { get; set; } = string.Empty;
}

// Assembly point refresh (CMS to Engine)
public class AssemblyPointRefreshDto
{
    public List<AssemblyPointMqttDto> AssemblyPoints { get; set; } = new();
}

public class AssemblyPointMqttDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? AreaShape { get; set; }
    public int PriorityOrder { get; set; }
}
