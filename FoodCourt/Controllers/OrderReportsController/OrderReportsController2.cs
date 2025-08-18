


using ClosedXML.Excel;
using Domain.Dtos.OrderReportsDto;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Persistence.Data;
using System.Data;
using System.Security.Claims;


namespace FoodCourt.Controllers.OrderReportsController2
{
    public class OrderDto
    {
        public int orderID { get; set; }
        public string customerId { get; set; }
        public string customerName {  get; set; }
        public string customerEmail { get; set; }
        public string customerPhone { get; set; }
        public DateTime CreatedAt { get; set; }
        public string date { get; set; }
        public string time { get; set; }
        public decimal dFee { get; set; }
        public decimal pFee { get; set; }
        public double dKm { get; set; }
        public decimal total { get; set; }
    }
    public class OrderItemDto
    {
        public int OrderId { get; set; }
        public int ItemId { get; set; }
        public string ItemName { get; set; }   // from Item.Name
        public int Quantity { get; set; }
        public decimal ItemPrice { get; set; } // stored from Item.Price
        public decimal TotalPrice { get; set; } // ItemPrice * Quantity + AddOns + Combos
    }

    [ApiController]
    [Route("api/[controller]")]
    public class OrderReportsController2: ControllerBase
    {
        FoodCourtDbContext _dbContext;
        IdentityContext _identityContext;
        IValidator<ExportOrdersDetailsDto> _exportOrdersDetailsDto;


        public OrderReportsController2(
            FoodCourtDbContext dbContext , 
            IdentityContext identityContext,
            IValidator<ExportOrdersDetailsDto> exportOrdersDetailsDto
            )
        {
            _dbContext = dbContext;
            _identityContext = identityContext;
            _exportOrdersDetailsDto = exportOrdersDetailsDto;
        }

        [HttpPost]
        [Authorize(Roles = "Chef")]
        public IActionResult ExportOrdersDetails([FromBody] ExportOrdersDetailsDto dto)
        {
            var currentUser = HttpContext.User;

            //check validation 
            var validationResult = _exportOrdersDetailsDto.Validate(dto);
            if (!validationResult.IsValid) 
                return BadRequest(validationResult.Errors);


            string chefId = currentUser.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            List<string>wbNames = new List<string> { "Orders Details"  , "Orders Items" };

            // "4e698927-1d39-41d0-8981-063eb6196d38"
            // "250b3722-6f00-4cc2-8129-4bb6e76d5776"
            List<DataTable> dataTables = GenerateTables("250b3722-6f00-4cc2-8129-4bb6e76d5776", dto);

            using (XLWorkbook wb = new XLWorkbook())
            {
                int i = 0;
                foreach(var table in dataTables)
                {
                    wb.AddWorksheet(table, wbNames[i]);
                    i++;
                }

                using (MemoryStream ms = new MemoryStream())
                {
                    wb.SaveAs(ms);
                    return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                                               "Orders-Details.xlsx");
                }
            }

        }

        [NonAction]
        public List<DataTable> GenerateTables(string chefId, ExportOrdersDetailsDto dto)
        {
         
            List<DataTable> dataTables = new List<DataTable>();

            //fetch chef resturant Id
            var restId = _dbContext.Restaurants
           .Where(r => r.ChefId == chefId)
           .Select(r => r.Id)
           .FirstOrDefault();

            //fetch resturant orders
           var ordersList = _dbContext.Orders
           .Where(o=>o.RestaurantId == restId && o.CreatedAt >= dto.from && o.CreatedAt < dto.to)
          .Select(order => new 
          {
                order.Id,
                order.CustomerId,
                order.CreatedAt,
                order.DeliveryFee,
                order.PlatformFee,
                order.DistanceKm,
                order.SubTotal
          })
          .ToList();

            //list of customers Ids
            var customerIds = ordersList.Select(o=>o.CustomerId).Distinct().ToList();

            //restuarnt orders customers Names
            var customerNames = _identityContext.Users
                .Where(u => customerIds.Contains(u.Id))
                .Select(u => new { u.Id, u.DisplayName , u.Email, u.PhoneNumber })
                .ToDictionary(u => u.Id, u => new {
                    u.DisplayName,
                    u.Email,
                    u.PhoneNumber
                });

            // Merge results
            var orders = ordersList.Select(o => new OrderDto
            {
                orderID = o.Id,
                customerId = o.CustomerId,
                customerName = customerNames[o.CustomerId].DisplayName,
                customerEmail = customerNames[o.CustomerId].Email,
                customerPhone = customerNames[o.CustomerId].PhoneNumber,
                CreatedAt = o.CreatedAt,
                date = o.CreatedAt.ToString("MM/dd/yyyy"),
                time = o.CreatedAt.ToString("HH:mm"),
                dFee = o.DeliveryFee,
                pFee = o.PlatformFee,
                dKm = o.DistanceKm,
                total = o.SubTotal

            }).ToList();

            //restaurant orders ids
            var ordresIds = orders.Select(o=>o.orderID)
                .ToList();

          // orders items
          var ordItems = _dbContext.OrderItems
                .Where(oi=>ordresIds.Contains(oi.OrderId))
                 .Select(oi => new OrderItemDto
                  {
                       OrderId = oi.OrderId,
                       ItemId = oi.ItemId,
                       ItemName = oi.Item.Name,
                       Quantity = oi.Quantity,
                       ItemPrice = oi.ItemPrice,
                       TotalPrice = oi.TotalPrice
                  })
                .ToList();

            //Ordres Summery
            var totalOrdres = ordresIds.Count();
            var totalSales = _dbContext.Orders
                .Where(o => o.RestaurantId == restId && o.CreatedAt >= dto.from && o.CreatedAt < dto.to)
                .GroupBy(x => true)
                .Select(x => new
                {
                    total = x.Sum(y => y.SubTotal),
                    PlatformFee = x.Sum(y => y.PlatformFee),
                    DeliveryFee = x.Sum(y => y.DeliveryFee)   
                }).FirstOrDefault();

            var revenues = totalSales.total - totalSales.PlatformFee - totalSales.DeliveryFee;
            var avgOrderValue = revenues/totalOrdres;

            //peak Order Day
            var peakOrderTime = (from o in orders
                                 group o by o.CreatedAt.Hour into g
                                 select new
                                 {
                                     Time = g.Key,
                                     OrdersCount = g.Count()
                                 })
                                 .OrderBy(x => x.OrdersCount)
                                 .Take(1);

            //top selling item
            var topSellingItem = (from item in ordItems
                                 group item by item.ItemName into g
                                 select new
                                 {
                                     ItemName = g.Key,
                                     TotalQuantity = g.Sum(x=>x.Quantity),
                                     
                                 })
                                 .OrderByDescending(x=>x.TotalQuantity)
                                 .Take(1);
            //Ordres Summery
            //generate Tables
          dataTables.Add(OrdersData(orders));
          dataTables.Add(OrdersItems(ordItems));

          return dataTables;

        }
        [NonAction]
        public DataTable OrdersItems(List<OrderItemDto> ordItems)
        {

            var dataTable = new DataTable();
            dataTable.Columns.Add("Order Id", typeof(int));
            dataTable.Columns.Add("Item Id", typeof(int));
            dataTable.Columns.Add("Item Name", typeof(string));
            dataTable.Columns.Add("Quantity", typeof(int));
            dataTable.Columns.Add("Item Price", typeof(decimal));
            dataTable.Columns.Add("Total Price", typeof(decimal));

            foreach (var item in ordItems)
            {

                dataTable.Rows.Add(
                    item.OrderId,
                    item.ItemId,
                    item.ItemName,
                    item.Quantity,
                    item.ItemPrice,
                    item.TotalPrice
                    );
            }

            return dataTable;
        }

        [NonAction]
        public DataTable OrdersData(List<OrderDto> orders)
        {

            var dataTable = new DataTable();
            dataTable.Columns.Add("Order Id", typeof(int));
            dataTable.Columns.Add("CustomerId", typeof(string));
            dataTable.Columns.Add("CustomerName", typeof(string));
            dataTable.Columns.Add("CustomerEmail", typeof(string));
            dataTable.Columns.Add("CustomerPhone", typeof(string));
            dataTable.Columns.Add("Date", typeof(string));
            dataTable.Columns.Add("Time", typeof(string));
            dataTable.Columns.Add("DeliveryFee", typeof(decimal));
            dataTable.Columns.Add("PlatformFee", typeof(decimal));
            dataTable.Columns.Add("DistanceKm", typeof(float));
            dataTable.Columns.Add("Total", typeof(decimal));

            foreach (var order in orders)
            {

                dataTable.Rows.Add(
                    order.orderID,
                    order.customerId,
                    order.customerName,
                    order.customerEmail,
                    order.customerPhone,
                    order.date,
                    order.time,
                    order.dFee,
                    order.pFee,
                    order.dKm,
                    order.total
                    );
            }

            return dataTable;
        }


    }
}


//Sales By Category
//Category      Orders	Revenue	    Percentage
//Appetizers	120	    $1,800.00	11.4%
//Mains	        250	    $10,250.00	65.2%
//Desserts	    80	    $1,120.00	7.1%
//Beverages	    200	    $2,560.50	16.3%


//Details Order
//Order ID	    Date	Time	    Type	    Items	                        Total	Payment
//1001	        Aug 1	6:45 PM     Dine-in	    Cheeseburger, Fries	            $18.50	Credit Card
//1002	        Aug 1	7:15 PM     Delivery	Pasta Alfredo, Garlic Bread	    $22.00	Online Payment
//1003	        Aug 2	12:30 PM    Takeaway	Caesar Salad	                $9.50	Cash
//1004	        Aug 2	7:50 PM     Dine-in	    Steak, Mashed Potatoes	        $28.00	Credit Card


//Summery
//Metric                          value
//Total Orders	                   452
//Total Sales Revenue	           $15,730.50
//Average Order Value	           $34.80
//Peak Order Time	               7 PM – 8 PM

//Top-Selling Item	               Cheeseburger
//Most Common Payment Method	   Credit Card





