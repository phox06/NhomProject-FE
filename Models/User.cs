using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace NhomProject.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; } 

        [Required]
        [StringLength(50)]
        public string Username { get; set; }

        [Required]
        [StringLength(200)] 
        public string Password { get; set; }

        [Required]
        [StringLength(100)]
        public string FullName { get; set; }

        [Required]
        [StringLength(100)]
        [EmailAddress]
        public string Email { get; set; }

        [StringLength(15)]
        public string Phone { get; set; }

        [StringLength(200)]
        public string Address { get; set; }

        public DateTime CreatedDate { get; set; }
        public bool IsActive { get; set; }

      
        public virtual ICollection<Order> Orders { get; set; }
    }
}