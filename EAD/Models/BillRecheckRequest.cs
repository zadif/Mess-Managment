using System;
using System.Collections.Generic;

namespace EAD.Models;

public partial class BillRecheckRequest
{
    public int Id { get; set; }

    public int BillId { get; set; }

    public int UserId { get; set; }

    public string? RequestMessage { get; set; }

    public DateTime RequestedOn { get; set; }

    public string Status { get; set; } = null!;

    public virtual Bill Bill { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
