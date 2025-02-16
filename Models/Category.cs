namespace COMP2139_Assignment1.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


    public class Category
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        public List<Product> Products { get; set; } = new();
    }
