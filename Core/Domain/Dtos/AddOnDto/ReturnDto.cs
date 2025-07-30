using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dtos.AddOnDto
{
    public class ReturnDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal AdditionalPrice { get; set; }
    }
}
