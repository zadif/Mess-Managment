using System;
using System.Collections.Generic;

namespace EAD.Models;

public partial class User
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public int UserType { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedOn { get; set; }

    public virtual ICollection<BillRecheckRequest> BillRecheckRequests { get; set; } = new List<BillRecheckRequest>();

    public virtual ICollection<Bill> Bills { get; set; } = new List<Bill>();

    public virtual ICollection<DailyConsumption> DailyConsumptions { get; set; } = new List<DailyConsumption>();
}
