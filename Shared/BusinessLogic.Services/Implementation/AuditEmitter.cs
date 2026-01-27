using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using BusinessLogic.Services.Interface;
using Helpers.Consumer.Mqtt;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace BusinessLogic.Services.Implementation
{
    public class AuditEmitter : IAuditEmitter
    {
        private readonly IMqttClientService _mqtt;
        private readonly IHttpContextAccessor _http;
        private readonly ILogger<AuditEmitter> _logger;

        public AuditEmitter(
            IMqttClientService mqtt,
            IHttpContextAccessor http,
            ILogger<AuditEmitter> logger)
        {
            _mqtt = mqtt;
            _http = http;
            _logger = logger;
        }

        private async Task Emit(
            string evt,
            string entity,
            string details,
            object? meta)
        {
            try
            {
                var user = _http.HttpContext?.User;

                var payload = new
                {
                    @event = evt,
                    entity,
                    eventTime = DateTime.UtcNow,
                    serverTime = DateTime.UtcNow,
                    actor = new
                    {
                        name = user?.Identity?.Name ?? "System",
                        role = user?.FindFirst(ClaimTypes.Role)?.Value ?? "System",
                        type = user == null ? "System" : "User"
                    },
                    details,
                    metadata = meta
                };

                await _mqtt.PublishAsync(
                    "audit/action",
                    JsonSerializer.Serialize(payload)
                );
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"[AUDIT] Publish failed: {ex.Message}");
            }
        }

            public enum AuditAction
        {
            LOGIN,
            LOGOUT,
            DOWNLOAD,
            UPLOAD,
            APPROVE,
            REJECT,
            SYNC,
            ALARM
        }

        public Task Created(string entity, object id, string details, object? meta = null)
            => Emit("CREATE", entity, details, meta);

        public Task Updated(string entity, object id, string details, object? meta = null)
            => Emit("UPDATE", entity, details, meta);

        public Task Deleted(string entity, object id, string details, object? meta = null)
            => Emit("DELETE", entity, details, meta);

        public Task Action(
            AuditAction action,
            string entity,
            string details,
            object? meta = null)
            => Emit(action.ToString(), entity, details, meta);

        public Task Alarm(string details, object? meta = null)
            => Emit("ALARM", "System", details, meta);
    }

}