using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EAD.Models;

public partial class MealItem
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Dish name is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters")]
    [Display(Name = "Dish Name")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "Description is required")]
    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string Description { get; set; } = null!;

    [Required(ErrorMessage = "Price is required")]
    [Range(0.01, 10000, ErrorMessage = "Price must be between 0.01 and 10,000")]
    [DataType(DataType.Currency)]
    [Display(Name = "Price (PKR)")]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "Please select a category")]
    public string? Category { get; set; }

    public virtual ICollection<DailyConsumption> DailyConsumptions { get; set; } = new List<DailyConsumption>();

    public virtual ICollection<DailyMenu> DailyMenus { get; set; } = new List<DailyMenu>();
}
