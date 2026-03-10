using System;
using System.Security.Claims;
using System.Text.Json;
using BusinessLogic.Services.Background;
using BusinessLogic.Services.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace BusinessLogic.Services.Implementation
{
    public class AuditEmitter : IAuditEmitter
    {
        private readonly IMqttPubQueue _queue;
        private readonly IHttpContextAccessor _http;
        private readonly ILogger<AuditEmitter> _logger;

        public AuditEmitter(
            IMqttPubQueue queue,
            IHttpContextAccessor http,
            ILogger<AuditEmitter> logger)
        {
            _queue = queue;
            _http = http;
            _logger = logger;
        }

        private void Publish(string evt, string entity, string details, object? meta)
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

                _queue.Enqueue("audit/action", JsonSerializer.Serialize(payload));
            }
            catch (Exception ex)
            {
                _logger.LogWarning("[AUDIT] Enqueue failed: {Message}", ex.Message);
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
            ALARM,
            ACTION,
            SESSION_START,
            SESSION_STOP,
            FORGOT_PASSWORD,
            RESET_PASSWORD
        }

        public void Created(string entity, object id, string details, object? meta = null)
        {
            Publish("CREATE", entity, details, meta);
        }

        public void Updated(string entity, object id, string details, object? meta = null)
        {
            Publish("UPDATE", entity, details, meta);
        }

        public void Deleted(string entity, object id, string details, object? meta = null)
        {
            Publish("DELETE", entity, details, meta);

        }

        public void Action(AuditAction action, string entity, string details, object? meta = null)
        {
            Publish(action.ToString(), entity, details, meta);
        }

        public void Alarm(string details, object? meta = null)
        {
            Publish("ALARM", "System", details, meta);
        }
    }
}
