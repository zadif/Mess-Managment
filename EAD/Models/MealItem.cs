using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EAD.Models;

public partial class MealItem
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Name is required")]

    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "Description is required")]

    public string Description { get; set; } = null!;

    [Required(ErrorMessage = "Price is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than zero")]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "Name is required")]

    public string Category { get; set; }

    public virtual ICollection<DailyConsumption> DailyConsumptions { get; set; } = new List<DailyConsumption>();
}
