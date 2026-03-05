using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;

namespace Data.ViewModels.Validators
{
    public class PatrolRouteCreateValidator : AbstractValidator<PatrolRouteCreateDto>
    {
        public PatrolRouteCreateValidator()
        {
            RuleFor(patrolRoute => patrolRoute.RouteAreas)
                .NotEmpty().WithMessage("RouteAreas is required.");
            RuleFor(patrolRoute => patrolRoute.Name)
                .MaximumLength(255).WithMessage("Name cannot exceed 255 characters.")
                .When(x => x.Name != null);
        }
    }
    
        public class PatrolRouteUpdateValidator : AbstractValidator<PatrolRouteUpdateDto>
    {
        public PatrolRouteUpdateValidator()
        {
            RuleFor(x => x.Name)
                .MaximumLength(255).When(x => x.Name != null);
        }
    }
}

