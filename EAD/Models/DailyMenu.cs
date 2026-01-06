using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EAD.Models;

public partial class DailyMenu
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Please select a day")]
    [Display(Name = "Day of Week")]
    public string DayOfWeek { get; set; } = null!;

    [Required(ErrorMessage = "Please select a meal type")]
    [Display(Name = "Meal Type")]
    public string MealType { get; set; } = null!;

    [Required(ErrorMessage = "Please select a dish")]
    [Display(Name = "Dish")]
    public int MealItemId { get; set; }

    public virtual MealItem MealItem { get; set; } = null!;
}
