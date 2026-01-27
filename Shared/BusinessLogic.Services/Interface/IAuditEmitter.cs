using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusinessLogic.Services.Implementation;

namespace BusinessLogic.Services.Interface
{
        public interface IAuditEmitter
    {
        Task Created(string entity, object id, string details, object? meta = null);
        Task Updated(string entity, object id, string details, object? meta = null);
        Task Deleted(string entity, object id, string details, object? meta = null);
        public Task Action(
           AuditEmitter.AuditAction action,
           string entity,
           string details,
           object? meta = null);
        Task Alarm(string details, object? meta = null);
    }

}