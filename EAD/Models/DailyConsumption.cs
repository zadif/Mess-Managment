using System;
using System.Collections.Generic;

namespace EAD.Models;

public partial class DailyConsumption
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int MealItemId { get; set; }

    public DateOnly ConsumptionDate { get; set; }

    public int Quantity { get; set; }

    public bool IsBilled { get; set; }

    public int? BillId { get; set; }

    public virtual Bill? Bill { get; set; }

    public virtual MealItem MealItem { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
