using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_ecommerce_1.Model.ViewModels
{
    public class ShoppingCartVM
    {
        public IEnumerable<ShoppingCart> Listcart { get; set; }
        public OrderHeader orderHeader { get; set; }
        public IEnumerable<Address> UserAddresses { get; set; }
    }
}
