using System;
using System.Collections.Generic;

namespace EAD.Models;
using System.ComponentModel.DataAnnotations;

public partial class DailyMenu
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Name of day is required")]

    public string DayOfWeek { get; set; } = null!;

    public string MealType { get; set; } = null!;

    public int MealItemId { get; set; }

    public virtual MealItem MealItem { get; set; } = null!;
}
