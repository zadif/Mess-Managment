using System;
using System.Collections.Generic;

namespace EAD.Models;

public partial class DailyMenu
{
    public int Id { get; set; }

    public string DayOfWeek { get; set; } = null!;

    public string MealType { get; set; } = null!;

    public int MealItemId { get; set; }

    public virtual MealItem MealItem { get; set; } = null!;
}
