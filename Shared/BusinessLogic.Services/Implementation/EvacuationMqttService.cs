using System;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Threading.Tasks;
using Helpers.Consumer.Mqtt;
using Microsoft.Extensions.Logging;
using BusinessLogic.Services.Interface;
using Shared.Contracts;

namespace BusinessLogic.Services.Implementation
{
    public class EvacuationMqttService
    {
        private readonly IMqttClientService _mqttClient;
        private readonly IEvacuationTransactionService _transactionService;
        private readonly ILogger<EvacuationMqttService> _logger;

        private readonly ConcurrentDictionary<Guid, TaskCompletionSource<int>> _pendingRequests = new();

        public EvacuationMqttService(
            IMqttClientService mqttClient,
            IEvacuationTransactionService transactionService,
            ILogger<EvacuationMqttService> logger)
        {
            _mqttClient = mqttClient;
            _transactionService = transactionService;
            _logger = logger;

            // Subscribe to evacuation topics
            SubscribeToTopics();
        }

        private async void SubscribeToTopics()
        {
            await _mqttClient.SubscribeAsync("evacuation/detection");
            await _mqttClient.SubscribeAsync("evacuation/summary");
            await _mqttClient.SubscribeAsync("evacuation/total-response");

            // Register message handler
            _mqttClient.OnMessageReceived += HandleMessageAsync;

            _logger.LogInformation("[EvacuationMQTT] Subscribed to evacuation topics");
        }

        public async Task HandleMessageAsync(string topic, string payload)
        {
            try
            {
                _logger.LogDebug($"[EvacuationMQTT] Received message on {topic}");

                switch (topic)
                {
                    case var t when t.StartsWith("evacuation/detection"):
                        await HandleDetectionAsync(payload);
                        break;

                    case var t when t.StartsWith("evacuation/summary"):
                        await HandleSummaryAsync(payload);
                        break;

                    case var t when t.StartsWith("evacuation/total-response"):
                        HandleTotalResponseAsync(payload);
                        break;

                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[EvacuationMQTT] Error handling message on {topic}");
            }
        }

        private async Task HandleDetectionAsync(string payload)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var detection = JsonSerializer.Deserialize<EvacuationDetectionMqttDto>(payload, options);
                if (detection == null)
                {
                    _logger.LogWarning("[EvacuationMQTT] Failed to deserialize detection payload");
                    return;
                }

                var dto = new EvacuationDetectionDto
                {
                    EvacuationAlertId = Guid.Parse(detection.EvacuationAlertId),
                    AssemblyPointId = Guid.Parse(detection.AssemblyPointId),
                    DetectedAt = DateTime.Parse(detection.DetectedAt),
                    Persons = detection.Persons?.Select(p => new PersonDetectionDto
                    {
                        PersonCategory = Enum.Parse<EvacuationPersonCategory>(p.PersonCategory, true),
                        MemberId = p.MemberId != null ? Guid.Parse(p.MemberId) : null,
                        VisitorId = p.VisitorId != null ? Guid.Parse(p.VisitorId) : null,
                        SecurityId = p.SecurityId != null ? Guid.Parse(p.SecurityId) : null,
                        CardId = p.CardId != null ? Guid.Parse(p.CardId) : null
                    }).ToList() ?? new()
                };

                await _transactionService.ProcessDetectionAsync(dto);
                _logger.LogInformation($"[EvacuationMQTT] Processed detection for alert {dto.EvacuationAlertId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[EvacuationMQTT] Error handling detection");
            }
        }

        private async Task HandleSummaryAsync(string payload)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var summary = JsonSerializer.Deserialize<EvacuationSummaryMqttDto>(payload, options);
                if (summary == null)
                {
                    _logger.LogWarning("[EvacuationMQTT] Failed to deserialize summary payload");
                    return;
                }

                var alertId = Guid.Parse(summary.EvacuationAlertId);
                var dto = new EvacuationSummaryUpdateDto
                {
                    TotalRequired = summary.TotalRequired,
                    TotalEvacuated = summary.TotalEvacuated,
                    TotalConfirmed = summary.TotalConfirmed,
                    TotalRemaining = summary.TotalRemaining
                };

                await _transactionService.UpdateSummaryAsync(alertId, dto);
                _logger.LogInformation($"[EvacuationMQTT] Updated summary for alert {alertId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[EvacuationMQTT] Error handling summary");
            }
        }

        private void HandleTotalResponseAsync(string payload)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var response = JsonSerializer.Deserialize<EvacuationTotalResponseDto>(payload, options);
                if (response == null)
                {
                    _logger.LogWarning("[EvacuationMQTT] Failed to deserialize total response payload");
                    return;
                }

                if (_pendingRequests.TryRemove(response.RequestId, out var tcs))
                {
                    tcs.TrySetResult(response.TotalActive);
                    _logger.LogDebug($"[EvacuationMQTT] Received total response: {response.TotalActive}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[EvacuationMQTT] Error handling total response");
            }
        }

        /// <summary>
        /// Request total active persons from engine
        /// </summary>
        public async Task<int> RequestTotalActiveAsync(Guid applicationId)
        {
            var requestId = Guid.NewGuid();
            var tcs = new TaskCompletionSource<int>();
            _pendingRequests[requestId] = tcs;

            var requestDto = new EvacuationTotalRequestDto
            {
                RequestId = requestId,
                ApplicationId = applicationId
            };

            await _mqttClient.PublishAsync("evacuation/request-total", JsonSerializer.Serialize(requestDto));
            _logger.LogInformation($"[EvacuationMQTT] Requested total active for application {applicationId}, requestId: {requestId}");

            // Wait for response with timeout
            using var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(5));

            try
            {
                return await tcs.Task.WaitAsync(cts.Token);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("[EvacuationMQTT] Request total timeout");
                _pendingRequests.TryRemove(requestId, out _);
                throw new TimeoutException("Engine did not respond within 5 seconds");
            }
        }

        public async Task PublishAssemblyPointsRefreshAsync(Guid applicationId, Guid floorplanId, AssemblyPointRefreshDto dto)
        {
            var payload = JsonSerializer.Serialize(new
            {
                applicationId = applicationId.ToString(),
                floorplanId = floorplanId.ToString(),
                assemblyPoints = dto.AssemblyPoints
            });

            await _mqttClient.PublishAsync("engine/refresh/evacuation-assembly-points", payload);
            _logger.LogInformation($"[EvacuationMQTT] Published assembly points refresh for floorplan {floorplanId}");
        }

        public async Task PublishEvacuationTriggerAsync(Guid applicationId, EvacuationTriggerMqttDto dto)
        {
            var topic = $"evacuation/trigger/{applicationId}";
            var payload = JsonSerializer.Serialize(dto);

            await _mqttClient.PublishAsync(topic, payload);
            _logger.LogInformation($"[EvacuationMQTT] Published evacuation trigger for application {applicationId}");
        }

        public async Task PublishEvacuationCompleteAsync(Guid applicationId, EvacuationCompleteMqttDto dto)
        {
            var topic = $"evacuation/complete/{applicationId}";
            var payload = JsonSerializer.Serialize(dto);

            await _mqttClient.PublishAsync(topic, payload);
            _logger.LogInformation($"[EvacuationMQTT] Published evacuation complete for application {applicationId}");
        }

        public async Task PublishEvacuationCancelAsync(Guid applicationId, EvacuationCancelMqttDto dto)
        {
            var topic = $"evacuation/cancel/{applicationId}";
            var payload = JsonSerializer.Serialize(dto);

            await _mqttClient.PublishAsync(topic, payload);
            _logger.LogInformation($"[EvacuationMQTT] Published evacuation cancel for application {applicationId}");
        }

        public async Task PublishEvacuationStatusAsync(Guid applicationId, EvacuationStatusMqttDto dto)
        {
            var topic = $"evacuation/status/{applicationId}";
            var payload = JsonSerializer.Serialize(dto);

            await _mqttClient.PublishAsync(topic, payload);
            _logger.LogDebug($"[EvacuationMQTT] Published evacuation status for application {applicationId}");
        }
    }

    // MQTT DTOs
    public class EvacuationDetectionMqttDto
    {
        public string EvacuationAlertId { get; set; } = string.Empty;
        public string AssemblyPointId { get; set; } = string.Empty;
        public string DetectedAt { get; set; } = string.Empty;
        public List<PersonDetectionMqttDto>? Persons { get; set; }
    }

    public class PersonDetectionMqttDto
    {
        public string PersonCategory { get; set; } = string.Empty;
        public string? MemberId { get; set; }
        public string? VisitorId { get; set; }
        public string? SecurityId { get; set; }
        public string? CardId { get; set; }
    }

    public class EvacuationSummaryMqttDto
    {
        public string EvacuationAlertId { get; set; } = string.Empty;
        public string Timestamp { get; set; } = string.Empty;
        public int TotalRequired { get; set; }
        public int TotalEvacuated { get; set; }
        public int TotalConfirmed { get; set; }
        public int TotalRemaining { get; set; }
    }

    public class EvacuationTotalRequestDto
    {
        public Guid RequestId { get; set; }
        public Guid ApplicationId { get; set; }
    }

    public class EvacuationTotalResponseDto
    {
        public Guid RequestId { get; set; }
        public Guid ApplicationId { get; set; }
        public int TotalActive { get; set; }
    }

    public class AssemblyPointRefreshDto
    {
        public List<AssemblyPointMqttDto> AssemblyPoints { get; set; } = new();
    }

    public class AssemblyPointMqttDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? FloorplanMaskedAreaId { get; set; }
        public string? AreaShape { get; set; }
        public int PriorityOrder { get; set; }
    }

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

    public class EvacuationStatusMqttDto
    {
        public string EvacuationAlertId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Timestamp { get; set; } = string.Empty;
        public int TotalRequired { get; set; }
        public int TotalEvacuated { get; set; }
        public int TotalConfirmed { get; set; }
        public int TotalRemaining { get; set; }
    }
}
