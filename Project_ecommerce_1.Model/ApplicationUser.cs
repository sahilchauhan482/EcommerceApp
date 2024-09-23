using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_ecommerce_1.Model
{
    public class ApplicationUser:IdentityUser
    {
        [Required]
       public string Name { get; set; }
        [Display(Name = "Street Address")]
       public string StreetAddress { get; set; }
       public string City { get; set; }
       public string State { get; set; }
        [Display(Name = "Postal code")]
        public string PostalCode { get; set; }

        [ForeignKey("CompanyId")]
        [Display(Name = "Company")]
        public int? CompanyId { get; set; }

        public Company Company { get; set; }

        [NotMapped]
        public string Role { get; set; }
    }
}
