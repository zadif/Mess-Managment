
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EAD.Models;

public partial class User
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Name is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters")]
    [Display(Name = "Full Name")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address")]
    [Display(Name = "Email Address")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = null!;

    [Required(ErrorMessage = "Please select a user type")]
    [Display(Name = "User Type")]
    public int UserType { get; set; }

    [Display(Name = "Active Account")]
    public bool IsActive { get; set; } = false;

    public DateTime? CreatedOn { get; set; }

    public virtual ICollection<BillRecheckRequest> BillRecheckRequests { get; set; } = new List<BillRecheckRequest>();

    public virtual ICollection<Bill> Bills { get; set; } = new List<Bill>();

    public virtual ICollection<DailyConsumption> DailyConsumptions { get; set; } = new List<DailyConsumption>();
}
