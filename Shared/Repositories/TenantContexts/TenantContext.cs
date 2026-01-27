using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Entities.Models;
using Helpers.Consumer;
using Microsoft.AspNetCore.Http;

namespace Repositories.TenantContexts
{

        public class TenantContext
    {
        public Guid? ApplicationId { get; }
        public bool IsSystemAdmin { get; }

        public TenantContext(IHttpContextAccessor http)
        {
            var user = http.HttpContext?.User;

            IsSystemAdmin =
                user?.HasClaim(ClaimTypes.Role, LevelPriority.System.ToString()) == true;

            if (!IsSystemAdmin)
            {
                var appId =
                    user?.FindFirst("ApplicationId")?.Value ??
                    (http.HttpContext?.Items["MstIntegration"] as MstIntegration)?.ApplicationId.ToString();

                if (!Guid.TryParse(appId, out var id))
                    throw new UnauthorizedAccessException("ApplicationId is required");

                ApplicationId = id;
            }
        }

        public Guid RequireApplicationIdFromBody(Guid? bodyAppId)
        {
            if (!IsSystemAdmin)
                return ApplicationId!.Value;

            if (!bodyAppId.HasValue || bodyAppId == Guid.Empty)
                throw new ArgumentException("ApplicationId is required for SystemAdmin");

            return bodyAppId.Value;
        }
    }
}
