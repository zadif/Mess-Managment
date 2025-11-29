using System;
using System.Collections.Generic;

namespace EAD.Models;

public partial class Bill
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public decimal TotalAmount { get; set; }

    public DateTime GeneratedOn { get; set; }

    public bool IsPaid { get; set; }

    public bool VerifiedByAdmin { get; set; }

    public DateTime? PaidOn { get; set; }

    public virtual ICollection<BillRecheckRequest> BillRecheckRequests { get; set; } = new List<BillRecheckRequest>();

    public virtual ICollection<DailyConsumption> DailyConsumptions { get; set; } = new List<DailyConsumption>();

    public virtual User User { get; set; } = null!;
}
