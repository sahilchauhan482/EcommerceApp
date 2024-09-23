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
    public class CategoryRepository:Repository<Category>,ICategoryRepository

    {
        private readonly ApplicationDbContext _context;
        public CategoryRepository(ApplicationDbContext context):base(context) 
        {
          _context = context;
        }
    }
}
