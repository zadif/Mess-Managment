using System;
using System.Collections.Generic;

namespace EAD.Models;

public partial class MealItem
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public decimal Price { get; set; }

    public string? Category { get; set; }

    public virtual ICollection<DailyConsumption> DailyConsumptions { get; set; } = new List<DailyConsumption>();

    public virtual ICollection<DailyMenu> DailyMenus { get; set; } = new List<DailyMenu>();
}
