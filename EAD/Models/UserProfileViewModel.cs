using Microsoft.AspNetCore.Mvc;

namespace EAD.Models
{
    public class UserProfileViewModel
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public int UserType { get; set; }
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }


    }
}
