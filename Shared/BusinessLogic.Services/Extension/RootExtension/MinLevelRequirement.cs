using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shared.Contracts;
using Microsoft.AspNetCore.Authorization;

namespace BusinessLogic.Services.Extension.RootExtension
{
    public class MinLevelRequirement : IAuthorizationRequirement
    {
        public LevelPriority MinLevel { get; }
        public MinLevelRequirement(LevelPriority minLevel)
        {
            MinLevel = minLevel;
        }

    }
    
        public class MinLevelAttribute : AuthorizeAttribute
    {
        public MinLevelAttribute(LevelPriority level)
        {
            Policy = $"Min{level}";
        }
    }

}