using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;

namespace Data.ViewModels.Validators
{
    public class FloorplanMaskedAreaCreateValidator : AbstractValidator<FloorplanMaskedAreaCreateDto>
    {
        public FloorplanMaskedAreaCreateValidator()
        {
            RuleFor(maskedArea => maskedArea.FloorplanId)
                .NotEmpty().WithMessage("Floorplan Id is required.");
            RuleFor(maskedArea => maskedArea.FloorId)
                .NotEmpty().WithMessage("Floor Id is required.");
            RuleFor(maskedArea => maskedArea.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(255).WithMessage("Name cannot exceed 255 characters.");
            RuleFor(maskedArea => maskedArea.AreaShape)
                .NotEmpty().WithMessage("AreaShape is required.");
            RuleFor(maskedArea => maskedArea.RestrictedStatus)
                .NotEmpty().WithMessage("RestrictedStatus is required.");
            // RuleFor(maskedArea => maskedArea.AllowFloorChange)
            //     .NotEmpty().WithMessage("AllowFloorChange is required.");
        }


    }
    
    // public class FloorplanMaskedAreaUpdateValidator : AbstractValidator<FloorplanMaskedAreaUpdateDto>
    // {
    //     public FloorplanMaskedAreaUpdateValidator()
    //     {
    //         RuleFor(maskedArea => maskedArea.FloorplanId)
    //             .NotEmpty().WithMessage("Floorplan Id is required.");
    //         RuleFor(maskedArea => maskedArea.FloorId)
    //             .NotEmpty().WithMessage("Floor Id is required.");
    //         RuleFor(maskedArea => maskedArea.Name)
    //             .NotEmpty().WithMessage("Name is required.")
    //             .MaximumLength(255).WithMessage("Name cannot exceed 255 characters.");
    //         RuleFor(maskedArea => maskedArea.AreaShape)
    //             .NotEmpty().WithMessage("AreaShape is required.");
    //         RuleFor(maskedArea => maskedArea.RestrictedStatus)
    //             .NotEmpty().WithMessage("RestrictedStatus is required.");
    //         RuleFor(maskedArea => maskedArea.AllowFloorChange)
    //             .NotEmpty().WithMessage("AllowFloorChange is required.");
    //     }
    // }
}

