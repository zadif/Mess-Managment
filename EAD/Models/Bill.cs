using System;
using System.Collections.Generic;

namespace EAD.Models;

public partial class Bill
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string MonthYear { get; set; } = null!;

    public decimal TotalAmount { get; set; }

    public DateTime GeneratedOn { get; set; }

    public string Status { get; set; } = null!;

    public virtual ICollection<BillRecheckRequest> BillRecheckRequests { get; set; } = new List<BillRecheckRequest>();

    public virtual User User { get; set; } = null!;
}
