using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using BusinessLogic.Services.Interface;
using Helpers.Consumer.Mqtt;
using Microsoft.AspNetCore.Http;

namespace BusinessLogic.Services.Implementation
{
    public class AuditEmitter : IAuditEmitter
    {
        private readonly IMqttClientService _mqtt;
        private readonly IHttpContextAccessor _http;

        public AuditEmitter(
            IMqttClientService mqtt,
            IHttpContextAccessor http)
        {
            _mqtt = mqtt;
            _http = http;
        }

        private async Task Emit(
            string evt,
            string entity,
            string details,
            object? meta)
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

        public Task Created(string entity, object id, string details, object? meta = null)
            => Emit("CREATE", entity, details, meta);

        public Task Updated(string entity, object id, string details, object? meta = null)
            => Emit("UPDATE", entity, details, meta);

        public Task Deleted(string entity, object id, string details, object? meta = null)
            => Emit("DELETE", entity, details, meta);

        public Task Action(
            string action,
            string entity,
            string details,
            object? meta = null)
            => Emit(action, entity, details, meta);

        public Task Alarm(string details, object? meta = null)
            => Emit("ALARM", "System", details, meta);
    }

}