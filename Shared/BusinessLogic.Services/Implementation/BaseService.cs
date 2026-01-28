using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Helpers.Consumer;
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
        protected string EmailFormToken
        {
            get
            {
                var email = Http.HttpContext?.User.FindFirst(ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(email))
                    throw new Exception("Email missing in token");

                return email;
            }
        }


        protected void SetCreateAudit(BaseModelWithTime entity)
        {
            entity.Id = Guid.NewGuid();
            // entity.Status = 1;

            entity.CreatedBy = UsernameFormToken;
            entity.CreatedAt = DateTime.UtcNow;

            entity.UpdatedBy = UsernameFormToken;
            entity.UpdatedAt = DateTime.UtcNow;
        }

        protected void SetCreateAudit(BaseModelOnlyIdWithTime entity)
        {
            entity.Id = Guid.NewGuid();
            // entity.Status = 1;

            entity.CreatedBy = UsernameFormToken;
            entity.CreatedAt = DateTime.UtcNow;

            entity.UpdatedBy = UsernameFormToken;
            entity.UpdatedAt = DateTime.UtcNow;
        }

        protected void SetCreateAudit(BaseModelWithTimeApp entity)
        {
            entity.Id = Guid.NewGuid();
            entity.Status = 1;

            entity.CreatedBy = UsernameFormToken;
            entity.CreatedAt = DateTime.UtcNow;

            entity.UpdatedBy = UsernameFormToken;
            entity.UpdatedAt = DateTime.UtcNow;
        }

        protected void SetUpdateAudit(BaseModelWithTime entity)
        {
            entity.UpdatedBy = UsernameFormToken;
            entity.UpdatedAt = DateTime.UtcNow;
        }
        protected void SetUpdateAudit(BaseModelOnlyIdWithTime entity)
        {
            entity.UpdatedBy = UsernameFormToken;
            entity.UpdatedAt = DateTime.UtcNow;
        }
        protected void SetUpdateAudit(BaseModelWithTimeApp entity)
        {
            entity.UpdatedBy = UsernameFormToken;
            entity.UpdatedAt = DateTime.UtcNow;
        }

        protected void SetDeleteAudit(BaseModelWithTime entity)
        {
            // entity.Status = 0;
            entity.UpdatedBy = UsernameFormToken;
            entity.UpdatedAt = DateTime.UtcNow;
        }
        protected void SetDeleteAudit(BaseModelOnlyIdWithTime entity)
        {
            // entity.Status = 0;
            entity.UpdatedBy = UsernameFormToken;
            entity.UpdatedAt = DateTime.UtcNow;
        }
        protected void SetDeleteAudit(BaseModelWithTimeApp entity)
        {
            entity.Status = 0;
            entity.UpdatedBy = UsernameFormToken;
            entity.UpdatedAt = DateTime.UtcNow;
        }

         public static class FileMimePresets
        {
            public static readonly string[] Documents =
            {
                "application/pdf",
                "application/msword",
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
            };

            public static readonly string[] Videos =
            {
                "video/mp4",
                "video/webm",
                "video/quicktime"
            };
        }

    }
}