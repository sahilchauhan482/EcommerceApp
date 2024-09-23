using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_ecommerce_1.Model
{
    public class Company
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Display(Name = "Street Address")]
        [Required]
        public string StreetAdress { get; set; }
        [Required]
        public string City { get; set; }
        [Required]
        public string State { get; set; }
        [Display(Name = "Postal code")]
        [Required]
        public string PostalCode { get; set; }
        [Display(Name ="Phone Number")]
        [Required]
        public string PhoneNumber { get; set; }
        [Display(Name = "Is Authorized Company")]
        [Required]
        public bool IsAuthorizedCompany { get; set; }
    }
}
