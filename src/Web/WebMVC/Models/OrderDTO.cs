using System;
using System.ComponentModel.DataAnnotations;

namespace HMS.WebMVC.Models
{
    public class OrderDTO
    {
        [Required]
        public string OrderNumber { get; set; }
    }
}