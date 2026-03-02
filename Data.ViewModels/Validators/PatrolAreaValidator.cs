using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;

namespace Data.ViewModels.Validators
{
    public class PatrolAreaCreateValidator : AbstractValidator<PatrolAreaCreateDto>
    {
        public PatrolAreaCreateValidator()
        {
            RuleFor(patrolArea => patrolArea.FloorplanId)
                .NotEmpty().WithMessage("Floorplan Id is required.");
            RuleFor(patrolArea => patrolArea.FloorId)
                .NotEmpty().WithMessage("Floor Id is required.");
            RuleFor(patrolArea => patrolArea.Name)
                .MaximumLength(255).WithMessage("Name cannot exceed 255 characters.")
                .When(x => x.Name != null);
            RuleFor(patrolArea => patrolArea.AreaShape)
                .NotEmpty().WithMessage("AreaShape is required.");
        }
    }
    
    public class PatrolAreaUpdateValidator : AbstractValidator<PatrolAreaUpdateDto>
{
    public PatrolAreaUpdateValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(255).When(x => x.Name != null);
    }
}
}

