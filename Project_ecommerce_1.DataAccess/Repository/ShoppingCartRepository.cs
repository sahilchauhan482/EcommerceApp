using Project_ecommerce_1.Data;
using Project_ecommerce_1.DataAccess.Repository.IRepository;
using Project_ecommerce_1.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_ecommerce_1.DataAccess.Repository
{
    internal class ShoppingCartRepository:Repository<ShoppingCart>,IShoppingCartRepository
    {
        private readonly ApplicationDbContext _context;
        public ShoppingCartRepository(ApplicationDbContext context):base(context) 
        {
            _context = context;
        }
    }
}
