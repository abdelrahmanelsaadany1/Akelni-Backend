using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class StripeSettings
    {
        public string SecretKey { set; get; }
        public string PublishKey { set; get; }
    }
}
