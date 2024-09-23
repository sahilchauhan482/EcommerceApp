using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_ecommerce_1.Model.ViewModels
{
    public class OrderHeaderVM
    {
        
            public int OrderId { get; set; }
            public string Status { get; set; }
            public DateTime OrderDate { get; set; }
            public string CustomerName { get; set; }
            public string PhoneNumber { get; set; }
            public string CustomerEmail { get; set; }
            
        
    }
}
