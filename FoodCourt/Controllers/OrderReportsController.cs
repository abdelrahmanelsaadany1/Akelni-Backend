using Domain.Dtos.OrderReportsDto;
using Domain.Entities.Identity;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Abstractions.IServices;
using System.Security.Claims;

namespace FoodCourt.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderReportsController : ControllerBase
    {
        private readonly IOrderReportService _orderReportService;
        private readonly IValidator<ExportOrdersDetailsDto> _validator;

        public OrderReportsController(
            IOrderReportService orderReportService,
            IValidator<ExportOrdersDetailsDto> validator)
        {
            _orderReportService = orderReportService;
            _validator = validator;
        }

        [HttpPost("export")]
        [Authorize(Roles = "Chef")]
        public async Task<IActionResult> ExportOrdersDetails([FromBody] ExportOrdersDetailsDto dto)
        {
            var validationResult = await _validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors);

            var chefId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(chefId))
                return Unauthorized("Chef ID not found");

            try
            {
                //temp
                var excelData = await _orderReportService.ExportOrdersToExcelAsync("250b3722-6f00-4cc2-8129-4bb6e76d5776", dto);

                //original
                //var excelData = await _orderReportService.ExportOrdersToExcelAsync(chefId, dto);

                return File(excelData,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "Orders-Details.xlsx");
            }
            catch (Exception ex)
            {
                // Log exception here
                return StatusCode(500, "An error occurred while generating the report");
            }
        }

    }
}