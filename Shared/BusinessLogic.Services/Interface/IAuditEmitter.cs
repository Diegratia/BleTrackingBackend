using System;
using System.Collections.Generic;
using System.Linq;
using BusinessLogic.Services.Implementation;

namespace BusinessLogic.Services.Interface
{
    public interface IAuditEmitter
    {
        void Created(string entity, object id, string details, object? meta = null);
        void Updated(string entity, object id, string details, object? meta = null);
        void Deleted(string entity, object id, string details, object? meta = null);
        void Action(AuditEmitter.AuditAction action, string entity, string details, object? meta = null);
        void Alarm(string details, object? meta = null);
    }
}