﻿using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    public class Product
    {
        public Product()
        {
            TempPiece = 1;
        }


        [Key]
        public int Id { get; set; }
        [Required]
        [MaxLength(50)]
        [MinLength(5)]
      
        public string Name { get; set; }
        public string Description { get; set; }

        [Range(0, int.MaxValue)]
        public double Price { get; set; }

        public string Image { get; set; }

        [Display (Name = "Category Type")]
        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public virtual Category Category { get; set; }

        [NotMapped]
        [Range(1,1000 , ErrorMessage ="piece must be greater than 0 ")]
        public int TempPiece { get; set; }
    }
}
