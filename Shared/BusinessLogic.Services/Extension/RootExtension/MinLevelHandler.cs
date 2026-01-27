using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Helpers.Consumer;
using Microsoft.AspNetCore.Authorization;

namespace BusinessLogic.Services.Extension.RootExtension
{
    public class MinLevelHandler
    : AuthorizationHandler<MinLevelRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            MinLevelRequirement requirement)
        {
            var levelClaim = context.User.FindFirst("level");
            if (levelClaim == null)
                return Task.CompletedTask;

            if (!int.TryParse(levelClaim.Value, out var userLevel))
                return Task.CompletedTask;

            if (userLevel <= (int)requirement.MinLevel)
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    

}

}