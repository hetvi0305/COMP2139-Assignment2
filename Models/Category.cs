namespace COMP2139_Assignment1.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


    public class Category
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)] 
        public string? Name { get; set; }
        
        [StringLength(500)] 
        public string? Description { get; set; }

        public List<Product>? Products { get; set; } = new();
    }
