using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.Abstractions.ICategoryService;
using Services.CategoryService;

namespace FoodCourt.Controllers.OrderController
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {

        private readonly IOrderService _OrderService;

        public OrderController(IOrderService IOrderService)
        {
            _OrderService = IOrderService;
        }
        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAllOrders()
        {
            var Orders = await _OrderService.GetAllOrdersAsync();
            return Ok(Orders);
        }


    }
}
