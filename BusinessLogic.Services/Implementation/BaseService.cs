using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace BusinessLogic.Services.Implementation
{
    public abstract class BaseService
    {
        protected readonly IHttpContextAccessor Http;

        protected BaseService(IHttpContextAccessor http)
        {
            Http = http;
        }

        protected Guid AppId
        {
            get
            {
                var appId = Http.HttpContext?.User.FindFirst("ApplicationId")?.Value;
                

                if (string.IsNullOrEmpty(appId))
                    throw new Exception("ApplicationId missing in token");

                return Guid.Parse(appId);
            }
        }

        protected string UsernameFormToken
        {
            get
            {
                var username = Http.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? "System";
                if (string.IsNullOrEmpty(username))
                    throw new Exception("Username missing in token");

                return username;
            }
        }
    }
}