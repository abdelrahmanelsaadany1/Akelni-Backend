using Domain.Dtos.OrderReportsDto;
using FluentValidation;
using FluentValidation.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Validations.ExportOrdersDetailsValidation
{
    public class ExportOrdersDetailsValidation:AbstractValidator<ExportOrdersDetailsDto>
    {
        public ExportOrdersDetailsValidation()
        {
            RuleFor(x => x.from)
                 .NotEmpty().WithMessage("From date is required.");

            RuleFor(x => x.to)
                .NotEmpty().WithMessage("To date is required.");

            RuleFor(x => x)
              .Must(x => x.from <= x.to)
              .WithMessage("'From' date must be earlier than or equal to 'To' date.");
         
        }
    }
}
