using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EAD.Models;

public partial class User
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Name is required")]

    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "Email is required")]


    public string Email { get; set; } = null!;
    [Required(ErrorMessage = "Password is required")]
    [DataType(DataType.Password)]
    [StringLength(20, MinimumLength = 5, ErrorMessage = "Range should be between 5 to 20 characters")]


    public string Password { get; set; } = null!;

    public int UserType { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime? CreatedOn { get; set; }

    public virtual ICollection<BillRecheckRequest> BillRecheckRequests { get; set; } = new List<BillRecheckRequest>();

    public virtual ICollection<Bill> Bills { get; set; } = new List<Bill>();

    public virtual ICollection<DailyConsumption> DailyConsumptions { get; set; } = new List<DailyConsumption>();
}
